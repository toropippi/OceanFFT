using UnityEngine;

public class Phillips : MonoBehaviour
{
    public const uint meshSize = 256;
    const float g = 9.81f;              // gravitational constant
    const float A = 1e-7f;              // wave scale factor
    public const float patchSize = 100;        // patch size
    const float windSpeed = 100.0f;
    const float windDir = Mathf.PI * 1.234f;
    const float dirDepend = 0.07f;
    public const uint spectrumW = meshSize + 4;
    public const uint spectrumH = meshSize + 1;

    void Start()
    {
        
    }


    // Generates Gaussian random number with mean 0 and standard deviation 1.
    float Gauss()
    {
        float u1 = Random.Range(0.000001f, 1f);
        float u2 = Random.Range(0f, 1f);
        return Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Cos(2f * Mathf.PI * u2);
    }


    //float phillips()
    // Phillips spectrum
    // (Kx, Ky) - normalized wave vector
    // Vdir - wind angle in radians
    // V - wind speed
    // A - constant
    float Generate_Phillips(float Kx, float Ky, float Vdir, float V, float A, float dir_depend)
    {
        float k_squared = Kx * Kx + Ky * Ky;

        if (k_squared == 0.0f)
        {
            return 0.0f;
        }

        // largest possible wave from constant wind of velocity v
        float L = V * V / g;

        float k_x = Kx / Mathf.Sqrt(k_squared);
        float k_y = Ky / Mathf.Sqrt(k_squared);
        float w_dot_k = k_x * Mathf.Cos(Vdir) + k_y * Mathf.Sin(Vdir);

        float phillips = A * Mathf.Exp(-1.0f / (k_squared * L * L)) / (k_squared * k_squared) * w_dot_k * w_dot_k;

        // filter out waves moving opposite to wind
        if (w_dot_k < 0.0f)
        {
            phillips *= dir_depend;
        }

        // damp out waves with very small length w << l
        float w = L / 10000;
        phillips *= Mathf.Exp(-k_squared * w * w);
        return phillips;
    }

    // Generate base heightfield in frequency space
    public Vector2[] Generate_h0()
    {
        Vector2[] h0 = new Vector2[spectrumH * spectrumW];
        for (uint y = 0; y <= meshSize; y++)
        {
            for (uint x = 0; x <= meshSize; x++)
            {
                float kx = (-(int)meshSize / 2.0f + x) * (2.0f * Mathf.PI / patchSize);
                float ky = (-(int)meshSize / 2.0f + y) * (2.0f * Mathf.PI / patchSize);

                float P = Mathf.Sqrt(Generate_Phillips(kx, ky, windDir, windSpeed, A, dirDepend));

                if (kx == 0.0f && ky == 0.0f)
                {
                    P = 0.0f;
                }

                //float Er = urand()*2.0f-1.0f;
                //float Ei = urand()*2.0f-1.0f;
                float Er = Gauss();
                float Ei = Gauss();
                //P=0.00009f;

                float h0_re = Er * P * Mathf.Sqrt(2.0f);
                float h0_im = Ei * P * Mathf.Sqrt(2.0f);

                uint i = y * spectrumW + x;
                h0[i].x = h0_re;
                h0[i].y = h0_im;
            }
        }
        return h0;
    }

}
