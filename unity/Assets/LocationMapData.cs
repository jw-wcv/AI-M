using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationMapData : IEnumerable<NoiseMapDataItem>
{
    public Vector3[] Grad3;
    public int[] P;
    public int[] Perm;
    public Vector3[] GradP;

    public IEnumerator<NoiseMapDataItem> GetEnumerator()
{
    for (int i = 0; i < Grad3.Length; i++)
    {
        yield return new NoiseMapDataItem
        {
            Grad3 = Grad3[i],
            P = P[i],
            Perm = Perm,
            GradP = GradP[i]
        };
    }
}


    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}