using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralMapGenerator : MonoBehaviour
{
    public Terrain terrain; // assign the Terrain object in the Inspector

    public void GenerateMap(LocationMapData locationMapData)
    {
        int width = terrain.terrainData.heightmapResolution;
        int height = terrain.terrainData.heightmapResolution;
        Debug.Log("In ProceduralMapGenerator");
        Debug.Log(locationMapData.Grad3);
        Debug.Log(locationMapData.P);
        Debug.Log(locationMapData.Perm);
        Debug.Log(locationMapData.GradP);

        float[,] noiseMap = new float[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float noiseValue = SimplexNoise.Generate(x, y, locationMapData);
                noiseMap[x, y] = noiseValue;
            }
        }

        terrain.terrainData.size = new Vector3(width, 600, height); // adjust terrain size to match noiseMap
        ApplyHeightMapToTerrain(noiseMap, terrain);
    }

    public void ApplyHeightMapToTerrain(float[,] noiseMap, Terrain terrain)
    {
        TerrainData terrainData = terrain.terrainData;
        terrainData.SetHeights(0, 0, noiseMap);
    }
}
