using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplexNoise
{
    private static float F2 = 0.5f * (Mathf.Sqrt(3.0f) - 1.0f);
    private static float G2 = (3.0f - Mathf.Sqrt(3.0f)) / 6.0f;

    public static float Generate(int x, int y, LocationMapData locationMapData)
    {
        float noiseValue = 0f;

        try
        {
            if (locationMapData == null)
            {
                throw new ArgumentNullException("locationMapData", "LocationMapData parameter cannot be null");
            }


        float s = (x + y) * F2;
        int i = Mathf.FloorToInt(x + s);
        int j = Mathf.FloorToInt(y + s);

        float t = (i + j) * G2;
        float X0 = i - t;
        float Y0 = j - t;
        float x0 = x - X0;
        float y0 = y - Y0;

        int i1, j1;
        if (x0 > y0)
        {
            i1 = 1;
            j1 = 0;
        }
        else
        {
            i1 = 0;
            j1 = 1;
        }

        float x1 = x0 - i1 + G2;
        float y1 = y0 - j1 + G2;
        float x2 = x0 - 1.0f + 2.0f * G2;
        float y2 = y0 - 1.0f + 2.0f * G2;

        int ii = i & 255;
        int jj = j & 255;
        int gi0 = locationMapData.Perm[ii + locationMapData.Perm[jj]] % 12;
        int gi1 = locationMapData.Perm[ii + i1 + locationMapData.Perm[jj + j1]] % 12;
        int gi2 = locationMapData.Perm[ii + 1 + locationMapData.Perm[jj + 1]] % 12;

        Vector3 g0 = locationMapData.Grad3[gi0];
        Vector3 g1 = locationMapData.Grad3[gi1];
        Vector3 g2 = locationMapData.Grad3[gi2];


        float n0, n1, n2;
        float t0 = 0.5f - x0 * x0 - y0 * y0;
        if (t0 < 0)
            n0 = 0.0f;
        else
        {
            t0 *= t0;
            n0 = t0 * t0 * Vector3.Dot(g0, new Vector3(x0, y0, 0f));
        }

        float t1 = 0.5f - x1 * x1 - y1 * y1;
        if (t1 < 0)
            n1 = 0.0f;
        else
        {
            t1 *= t1;
            n1 = t1 * t1 * Vector3.Dot(g1, new Vector3(x1, y1, 0f));
        }

        float t2 = 0.5f - x2 * x2 - y2 * y2;
        if (t2 < 0)
            n2 = 0.0f;
        else
        {
            t2 *= t2;
            n2 = t2 * t2 * Vector3.Dot(g2, new Vector3(x2, y2, 0f));
        }

        noiseValue += 70.0f * (n0 + n1 + n2);
        Debug.Log("NoiseValue: " + noiseValue);

            return noiseValue;
        }
        catch (Exception ex)
        {
            Debug.LogError($"SimplexNoise.Generate Error: {ex.Message}");
            return 0f;
        }
    }
}
