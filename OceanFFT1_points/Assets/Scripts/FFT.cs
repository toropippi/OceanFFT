using UnityEngine;

public class FFT : MonoBehaviour
{
    const bool DEBUG = false;
    [SerializeField] ComputeShader shader;
    ComputeBuffer buffer_dbg;
    int kernel_FFT2Dfunc256inv, kernel_DFT2Dfunc256inv;

    void Awake()
    {
        kernel_FFT2Dfunc256inv = shader.FindKernel("FFT2Dfunc256inv");
        kernel_DFT2Dfunc256inv = shader.FindKernel("DFT2Dfunc256inv");
    }

    void Start()
    {
        if (DEBUG) buffer_dbg = new ComputeBuffer(1, sizeof(float) * 2);
    }

    //bufferが入力、FFTの結果がbufferに出力
    public void FFT2D_256_Dispatch(ComputeBuffer buffer, ComputeBuffer buffer_dmy)
    {
        if (DEBUG) DEBUG_func1(buffer);
        //①引数をセット
        shader.SetBuffer(kernel_FFT2Dfunc256inv, "buffer", buffer);
        shader.SetBuffer(kernel_FFT2Dfunc256inv, "buffer_dmy", buffer_dmy);
        // GPUで計算
        shader.Dispatch(kernel_FFT2Dfunc256inv, 256, 1, 1);

        //②引数をセット
        shader.SetBuffer(kernel_FFT2Dfunc256inv, "buffer", buffer_dmy);
        shader.SetBuffer(kernel_FFT2Dfunc256inv, "buffer_dmy", buffer);
        // GPUで計算
        shader.Dispatch(kernel_FFT2Dfunc256inv, 256, 1, 1);

        if (DEBUG) DEBUG_func2(buffer);
    }

    //bufferが入力、FFTの結果がbuffer_dmyに出力
    public void DFT2D_256_Dispatch(ComputeBuffer buffer, ComputeBuffer buffer_dmy)
    {
        //引数をセット
        shader.SetBuffer(kernel_DFT2Dfunc256inv, "buffer", buffer);
        shader.SetBuffer(kernel_DFT2Dfunc256inv, "buffer_dmy", buffer_dmy);
        // GPUで計算
        shader.Dispatch(kernel_DFT2Dfunc256inv, 256, 1, 1);
    }



    //FFTとDFTを比較
    void DEBUG_func1(ComputeBuffer buffer) 
    {
        if (buffer_dbg.count!= buffer.count)
            buffer_dbg = new ComputeBuffer(buffer.count, sizeof(float) * 2);
        DFT2D_256_Dispatch(buffer, buffer_dbg);
    }

    //FFTとDFTを比較
    void DEBUG_func2(ComputeBuffer buffer)
    {
        Vector2[] bfr = new Vector2[buffer.count];
        Vector2[] bfr_dbg = new Vector2[buffer.count];
        buffer.GetData(bfr);
        buffer_dbg.GetData(bfr_dbg);
        float rss = 0.0f;
        for(int i=0;i< buffer.count;i++)
        {
            Vector2 v2 = bfr[i] - bfr_dbg[i];
            rss += v2.sqrMagnitude;
        }
        Debug.Log(rss);
    }

}
