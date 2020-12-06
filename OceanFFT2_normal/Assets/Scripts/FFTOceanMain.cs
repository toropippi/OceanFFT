using UnityEngine;

public class FFTOceanMain : MonoBehaviour
{
    [SerializeField] ComputeShader shaderGenerateSpectrum;
    [SerializeField] ComputeShader shaderSetNormal;
    [SerializeField] Shader renderingShader;
    Material renderingShader_Material;

    [SerializeField] FFT fFT;
    [SerializeField] Phillips phillips;
    int kernel_GenerateSpectrumKernel, kernel_Center_difference;
    Vector2[] h_h0;
    public ComputeBuffer d_h0, d_ht, d_ht_dmy;
    RenderTexture normal_Tex;
    int cnt;

    private void Awake()
    {
        kernel_GenerateSpectrumKernel = shaderGenerateSpectrum.FindKernel("GenerateSpectrumKernel");
        kernel_Center_difference = shaderSetNormal.FindKernel("Center_difference");
        renderingShader_Material = new Material(renderingShader);
        cnt = 0;
    }

    void Start()
    {
        normal_Tex = new RenderTexture((int)Phillips.meshSize, (int)Phillips.meshSize, 0, RenderTextureFormat.ARGBFloat);
        normal_Tex.enableRandomWrite = true;
        normal_Tex.Create();
        normal_Tex.filterMode = FilterMode.Bilinear;
        normal_Tex.wrapMode = TextureWrapMode.Repeat;

        h_h0 = phillips.Generate_h0();//CPU側メモリ確保。サイズはsizeof(Vector2)*256*256
        d_h0 = new ComputeBuffer(h_h0.Length, sizeof(float) * 2);//GPU側メモリ確保 サイズはsizeof(Vector2)*256*256
        d_ht = new ComputeBuffer((int)(Phillips.meshSize * Phillips.meshSize), sizeof(float) * 2);//GPU側メモリ確保 サイズはsizeof(Vector2)*256*256
        d_ht_dmy = new ComputeBuffer(d_ht.count, sizeof(float) * 2);
        d_h0.SetData(h_h0);
        SetArgs();
    }

    void SetArgs()
    {
        //ComputeShader引数をセット
        shaderGenerateSpectrum.SetBuffer(kernel_GenerateSpectrumKernel, "h0", d_h0);
        shaderGenerateSpectrum.SetBuffer(kernel_GenerateSpectrumKernel, "ht", d_ht);
        shaderGenerateSpectrum.SetInt("N", (int)Phillips.meshSize);
        shaderGenerateSpectrum.SetInt("seasizeLx", (int)Phillips.seasizeLx);
        shaderGenerateSpectrum.SetInt("seasizeLz", (int)Phillips.seasizeLz);
        shaderSetNormal.SetBuffer(kernel_Center_difference, "ht", d_ht);
        shaderSetNormal.SetTexture(kernel_Center_difference, "tex", normal_Tex);
        shaderSetNormal.SetInt("N", (int)Phillips.meshSize);
        shaderSetNormal.SetFloat("rdx", 1.0f * Phillips.meshSize / Phillips.seasizeLx);
        shaderSetNormal.SetFloat("rdz", 1.0f * Phillips.meshSize / Phillips.seasizeLz);
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
        //引数tをセット
        shaderGenerateSpectrum.SetFloat("t", 0.03f * cnt);
        shaderGenerateSpectrum.Dispatch(kernel_GenerateSpectrumKernel, 1, 256, 1);//d_h0からd_htを計算
        fFT.FFT2D_256_Dispatch(d_ht, d_ht_dmy);//d_htから高さデータを計算

        //続いて差分から法線を計算しテクスチャに書き込み
        shaderSetNormal.Dispatch(kernel_Center_difference, 1, 256, 1);
        cnt++;
        //このあとレンダリング
    }

    void OnRenderObject()
    {
        // レンダリングを開始
        renderingShader_Material.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Quads, 256 * 256 * 4);
    }

    private void OnDestroy()
    {
        //解放
        d_h0.Release();
        d_ht.Release();
        d_ht_dmy.Release();
    }

}