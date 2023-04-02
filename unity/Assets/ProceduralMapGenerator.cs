using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

public class ProceduralMapGenerator : MonoBehaviour
{
    public Terrain terrain; // assign the Terrain object in the Inspector

    public void GenerateMap(LocationMapData locationMapData)
    {
        int width = terrain.terrainData.heightmapResolution;
        int height = terrain.terrainData.heightmapResolution;
        
        Debug.Log("Height: " + height);
        Debug.Log("Width: " + width);
        Debug.Log(locationMapData.Grad3);
        Debug.Log(locationMapData.P);
        Debug.Log(locationMapData.Perm);
        Debug.Log(locationMapData.GradP);

        float[,] noiseMap = new float[width, height];

        // Create an array of threads
        Thread[] threads = new Thread[Environment.ProcessorCount];

        // Divide the work into equal parts based on the number of available threads
        int numThreads = threads.Length;
        int partHeight = height / numThreads;

        // Start each thread with its own part of the heightmap
        for (int i = 0; i < numThreads; i++)
        {
            int startY = i * partHeight;
            int endY = (i + 1) * partHeight;
            if (i == numThreads - 1) endY = height;

            threads[i] = new Thread(() =>
            {
                for (int y = startY; y < endY; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        float noiseValue = SimplexNoise.Generate(x, y, locationMapData);
                        noiseMap[x, y] = noiseValue;
                    }
                }
            });

            threads[i].Start();
        }

        // Wait for all threads to finish before proceeding
        for (int i = 0; i < numThreads; i++)
        {
            threads[i].Join();
        }

        terrain.terrainData.size = new Vector3(width, 600, height); // adjust terrain size to match noiseMap
        ApplyHeightMapToTerrain(SmoothMap(noiseMap), terrain);
    }

    public void ApplyHeightMapToTerrain(float[,] noiseMap, Terrain terrain)
    {
        TerrainData terrainData = terrain.terrainData;
        terrainData.SetHeights(0, 0, noiseMap);
    }

    private float[,] SmoothMap(float[,] map)
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);

        float[,] smoothedMap = new float[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float avgValue = 0;
                int nValues = 0;

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int nx = x + dx;
                        int ny = y + dy;

                        if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                        {
                            avgValue += map[nx, ny];
                            nValues++;
                        }
                    }
                }

                smoothedMap[x, y] = avgValue / nValues;
            }
        }

        return smoothedMap;
    }
}
