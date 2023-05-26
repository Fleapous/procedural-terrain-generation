using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class noise_generation : MonoBehaviour
{
    public float[,] GenerateNoise(int mapDepth, int mapWith, float scale)
    {
        float[,] noiseMap = new float[mapDepth, mapWith];

        for (int zIndex = 0; zIndex < mapDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < mapWith; xIndex++)
            {
                float sampleX = xIndex / scale;
                float sampleZ = zIndex / scale;

                float noise = Mathf.PerlinNoise(sampleX, sampleZ);

                noiseMap[zIndex, xIndex] = noise;
            }
        }
        return noiseMap;
    }
    
}
