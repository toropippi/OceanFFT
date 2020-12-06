using UnityEngine;

public class FFTOceanMain : MonoBehaviour
{
    [SerializeField] ComputeShader shaderGenerateSpectrum;
    [SerializeField] ComputeShader shaderSetNormal;
    [SerializeField] Shader renderingShader;
    Material renderingShader_Material;
    [SerializeField] FFT fFT;
    [SerializeField] Phillips phillips;
    int kernel_GenerateSpectrumKernel, kernel_SetNormal;
    Vector2[] h_h0;
    public ComputeBuffer d_h0, d_ht, d_ht_dx, d_ht_dz, d_ht_dmy;
    RenderTexture normal_Tex;
    int cnt;

    private void Awake()
    {
        kernel_GenerateSpectrumKernel = shaderGenerateSpectrum.FindKernel("GenerateSpectrumKernel");
        kernel_SetNormal = shaderSetNormal.FindKernel("SetNormal");
        renderingShader_Material = new Material(renderingShader);
        cnt = 0;
    }

    void Start()
    {
        normal_Tex = CreateRenderTeture();
        h_h0 = phillips.Generate_h0();//CPU側メモリ確保。サイズはsizeof(Vector2)*256*256
        d_h0 = new ComputeBuffer(h_h0.Length, sizeof(float) * 2);//GPU側メモリ確保 サイズはsizeof(Vector2)*256*256
        d_ht = new ComputeBuffer((int)(Phillips.meshSize * Phillips.meshSize), sizeof(float) * 2);//GPU側メモリ確保 サイズはsizeof(Vector2)*256*256
        d_ht_dx = new ComputeBuffer((int)(Phillips.meshSize * Phillips.meshSize), sizeof(float) * 2);//x偏微分用
        d_ht_dz = new ComputeBuffer((int)(Phillips.meshSize * Phillips.meshSize), sizeof(float) * 2);//z偏微分用
        d_ht_dmy = new ComputeBuffer(d_ht.count, sizeof(float) * 2);
        d_h0.SetData(h_h0);
        SetArgs();
    }

    RenderTexture CreateRenderTeture()
    {
        var tex = new RenderTexture((int)Phillips.meshSize, (int)Phillips.meshSize, 0, RenderTextureFormat.ARGBFloat);
        tex.enableRandomWrite = true;
        tex.Create();
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Repeat;
        return tex;
    }

    void SetArgs()
    {
        //ComputeShader引数をセット
        shaderGenerateSpectrum.SetBuffer(kernel_GenerateSpectrumKernel, "h0", d_h0);
        shaderGenerateSpectrum.SetBuffer(kernel_GenerateSpectrumKernel, "ht", d_ht);
        shaderGenerateSpectrum.SetBuffer(kernel_GenerateSpectrumKernel, "ht_dx", d_ht_dx);
        shaderGenerateSpectrum.SetBuffer(kernel_GenerateSpectrumKernel, "ht_dz", d_ht_dz);
        shaderGenerateSpectrum.SetInt("N", (int)Phillips.meshSize);
        shaderGenerateSpectrum.SetInt("seasizeLx", (int)Phillips.seasizeLx);
        shaderGenerateSpectrum.SetInt("seasizeLz", (int)Phillips.seasizeLz);
        shaderSetNormal.SetBuffer(kernel_SetNormal, "ht_dx", d_ht_dx);
        shaderSetNormal.SetBuffer(kernel_SetNormal, "ht_dz", d_ht_dz);
        shaderSetNormal.SetTexture(kernel_SetNormal, "tex", normal_Tex);
        shaderSetNormal.SetInt("N", (int)Phillips.meshSize);
        // GPUバッファをマテリアルに設定
        renderingShader_Material.SetBuffer("d_ht", d_ht);
        renderingShader_Material.SetTexture("_MainTex", normal_Tex);
        // その他Shader 定数関連
        renderingShader_Material.SetInt("N", (int)Phillips.meshSize);
        renderingShader_Material.SetFloat("halfN", 0.5f * Phillips.meshSize);
        renderingShader_Material.SetFloat("dx", 1.0f * Phillips.seasizeLx / Phillips.meshSize);
        renderingShader_Material.SetFloat("dz", 1.0f * Phillips.seasizeLz / Phillips.meshSize);
    }


    void Update()
    {
        Calc_Spectrum();
        CalcFFT_ht();
        CalcFFT_ht_dxz();
        SetNormal();
        cnt++;
        //このあとレンダリング
    }

    void Calc_Spectrum()
    {
        //引数tをセット
        shaderGenerateSpectrum.SetFloat("t", 0.03f * cnt);
        shaderGenerateSpectrum.Dispatch(kernel_GenerateSpectrumKernel, 1, 256, 1);//d_h0からd_htを計算、偏微分成分も
    }

    void CalcFFT_ht()
    {
        fFT.FFT2D_256_Dispatch(d_ht, d_ht_dmy);//d_htから高さデータを計算
    }

    void CalcFFT_ht_dxz()
    {
        fFT.FFT2D_256_Dispatch(d_ht_dx, d_ht_dmy);//d_ht_dxから高さデータを計算、d_ht_dxに結果が入る
        fFT.FFT2D_256_Dispatch(d_ht_dz, d_ht_dmy);//d_ht_dzから高さデータを計算、d_ht_dzに結果が入る
    }
    void SetNormal()
    {
        shaderSetNormal.Dispatch(kernel_SetNormal, 1, 256, 1);//偏微分bufferから値を抽出し法線ベクトルを計算してtextureに保存
    }

    void OnRenderObject()
    {
        // レンダリングを開始
        renderingShader_Material.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Quads, (int)(Phillips.meshSize * Phillips.meshSize) * 4);
    }

    private void OnDestroy()
    {
        //解放
        d_h0.Release();
        d_ht.Release();
        d_ht_dx.Release();
        d_ht_dz.Release();
        d_ht_dmy.Release();
    }

}