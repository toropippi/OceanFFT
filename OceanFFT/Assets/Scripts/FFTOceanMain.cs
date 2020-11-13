using UnityEngine;

public class FFTOceanMain : MonoBehaviour
{
    [SerializeField] ComputeShader shader;
    [SerializeField] Shader renderingShader;
    Material renderingShader_Material;

    [SerializeField] FFT fFT;
    [SerializeField] Phillips phillips;
    int kernel_GenerateSpectrumKernel;
    Vector2[] h_h0;
    ComputeBuffer d_h0, d_ht, d_ht_dmy;
    int cnt;

    private void Awake()
    {
        kernel_GenerateSpectrumKernel = shader.FindKernel("GenerateSpectrumKernel");
        renderingShader_Material = new Material(renderingShader);
        cnt = 0;
    }

    void SetArgs() 
    {
        //引数をセット
        shader.SetBuffer(kernel_GenerateSpectrumKernel, "h0", d_h0);
        shader.SetBuffer(kernel_GenerateSpectrumKernel, "ht", d_ht);
        shader.SetInt("in_width", (int)Phillips.spectrumW);
        shader.SetInt("out_width", (int)Phillips.meshSize);
        shader.SetInt("out_height", (int)Phillips.meshSize);
        shader.SetFloat("patchSize", Phillips.patchSize);
        // GPUバッファをマテリアルに設定
        renderingShader_Material.SetBuffer("d_ht", d_ht);
    }

    void Start()
    {
        h_h0 = phillips.Generate_h0();//CPU側メモリ確保。サイズはsizeof(Vector2)*260*257
        d_h0 = new ComputeBuffer(h_h0.Length, sizeof(float) * 2);//GPU側メモリ確保 サイズはsizeof(Vector2)*260*257
        d_ht = new ComputeBuffer((int)(Phillips.meshSize * Phillips.meshSize), sizeof(float) * 2);//GPU側メモリ確保 サイズはsizeof(Vector2)*256*256
        d_ht_dmy = new ComputeBuffer(d_ht.count, sizeof(float) * 2);
        d_h0.SetData(h_h0);
        SetArgs();
    }

     void Update()
    {
        //引数をセット
        shader.SetFloat("t", 0.01f * cnt);
        shader.Dispatch(kernel_GenerateSpectrumKernel, 1, 256, 1);//d_h0からd_htを計算
        fFT.FFT2D_256_Dispatch(d_ht, d_ht_dmy);//d_htから高さデータを計算
        cnt++;
    }

    void OnRenderObject()
    {
        // レンダリングを開始
        renderingShader_Material.SetPass(0);
        // n個のオブジェクトをレンダリング
        Graphics.DrawProceduralNow(MeshTopology.Points, 256 * 256);
    }

    private void OnDestroy()
    {
        //解放
        d_h0.Release();
        d_ht.Release();
        d_ht_dmy.Release();
    }

}