using UnityEngine;

public class FFT : MonoBehaviour
{
    [SerializeField] ComputeShader shader;
    int kernel_FFT2Dfunc256inv;

    void Awake()
    {
        kernel_FFT2Dfunc256inv = shader.FindKernel("FFT2Dfunc256inv");
    }

    public void FFT2D_256_Dispatch(ComputeBuffer buffer, ComputeBuffer buffer_dmy)
    {
        //引数をセット
        shader.SetBuffer(kernel_FFT2Dfunc256inv, "buffer", buffer);
        shader.SetBuffer(kernel_FFT2Dfunc256inv, "buffer_dmy", buffer_dmy);
        // GPUで計算
        shader.Dispatch(kernel_FFT2Dfunc256inv, 256, 1, 1);

        //引数をセット
        shader.SetBuffer(kernel_FFT2Dfunc256inv, "buffer", buffer_dmy);
        shader.SetBuffer(kernel_FFT2Dfunc256inv, "buffer_dmy", buffer);
        // GPUで計算
        shader.Dispatch(kernel_FFT2Dfunc256inv, 256, 1, 1);
    }

}
