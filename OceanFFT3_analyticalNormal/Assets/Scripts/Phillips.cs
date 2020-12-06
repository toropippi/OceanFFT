using UnityEngine;

public class Phillips : MonoBehaviour
{
    public const uint meshSize = 256;
    public const uint seasizeLx = meshSize * 5 / 2;
    public const uint seasizeLz = meshSize * 5 / 2;
    const float G = 9.81f;              // gravitational constant
    const float A = 0.000001f;              // wave scale factor  A - constant
    const float windSpeed = 30.0f;
    const float windDir = Mathf.PI * 1.234f; //wind angle in radians

    // Generates Gaussian random number with mean 0 and standard deviation 1.
    Vector2 Gauss()
    {
        var u1 = Random.value;
        var u2 = Random.value;
        var logU1 = -2f * Mathf.Log(u1);
        var sqrt = (logU1 <= 0f) ? 0f : Mathf.Sqrt(logU1);
        var theta = Mathf.PI * 2.0f * u2;
        var z0 = sqrt * Mathf.Cos(theta);
        var z1 = sqrt * Mathf.Sin(theta);
        return new Vector2(z0, z1);
    }

    // Phillips spectrum
    // (Kx, Ky) - normalized wave vector
    float Generate_Phillips(float Kx, float Ky)
    {
        float k2mag = Kx * Kx + Ky * Ky;

        if (k2mag == 0.0f)
        {
            return 0.0f;
        }
		float k4mag = k2mag * k2mag;

        // largest possible wave from constant wind of velocity v
        float L = windSpeed * windSpeed / G;

        float k_x = Kx / Mathf.Sqrt(k2mag);
        float k_y = Ky / Mathf.Sqrt(k2mag);
        float w_dot_k = k_x * Mathf.Cos(windDir) + k_y * Mathf.Sin(windDir);

        float phillips = A * Mathf.Exp(-1.0f / (k2mag * L * L)) / k4mag * w_dot_k * w_dot_k;

        // damp out waves with very small length w << l
        float l2 = (L / 1000) * (L / 1000);
        phillips *= Mathf.Exp(-k2mag * l2);
        return phillips;
    }

    // Generate base heightfield in frequency space
    public Vector2[] Generate_h0()
    {
        Vector2[] h0 = new Vector2[meshSize * meshSize];
        for (uint y = 0; y < meshSize; y++)
        {
            for (uint x = 0; x < meshSize; x++)
            {
                float kx = (-(int)meshSize / 2.0f + x) * (2.0f * Mathf.PI / seasizeLx);
                float ky = (-(int)meshSize / 2.0f + y) * (2.0f * Mathf.PI / seasizeLz);
                float P = Generate_Phillips(kx, ky);
                if (kx == 0.0f && ky == 0.0f)
                {
                    P = 0.0f;
                }

                uint i = y * meshSize + x;
                h0[i] = Gauss() * Mathf.Sqrt(P * 0.5f);
            }
        }
        return h0;
    }

}
