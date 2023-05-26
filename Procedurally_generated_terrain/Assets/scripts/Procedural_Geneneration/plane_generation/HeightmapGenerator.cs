using System;
using UnityEngine;
using Random = UnityEngine.Random;


public class HeightmapGenerator : MonoBehaviour
{
    public float[,] MapGenerator(int height, int width, float scale,
        int octaves, float persistance, float lacunarity, float xDrift, float yDrift, int seed)
    {
        System.Random rng = new System.Random (seed);

        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = rng.Next(-100000, 100000) + xDrift;
            float offsetY = rng.Next(-100000, 100000) + yDrift;
            octaveOffsets [i] = new Vector2 (offsetX, offsetY);
        }

        float halfHeight = height / 2;
        float halfWidth = width / 2;
        
        float[,] map = new float[height, width];
        float[,] mapNormalized = new float[height, width];

        float maxFloat = float.MinValue;
        float minFloat = float.MaxValue;
        //row z
        for (int i = 0; i < height; i++)
        {
            //column x
            for (int j = 0; j < width; j++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                //octaves
                for (int k = 0; k < octaves; k++)
                {
                    // float X = (float)i / scale * frequency + octaveOffsets[k].x;
                    // float Y = (float)j / scale * frequency + octaveOffsets[k].y;
                    
                    float x = (j - halfWidth + octaveOffsets[k].x) / scale * frequency;
                    float y = (i - halfHeight + octaveOffsets[k].y) / scale * frequency;
                
                    float perlinNumber = Mathf.PerlinNoise(x, y) * 2 - 1;
                    noiseHeight += perlinNumber * amplitude;
                
                    amplitude *= persistance;
                    frequency *= lacunarity;
                }
                //for normalization
                // if (noiseHeight > maxFloat)
                //     maxFloat = noiseHeight;
                // else if (noiseHeight < minFloat)
                //     minFloat = noiseHeight;
                
                //adding the value to map
                map[i, j] = noiseHeight;
            }
        }

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                float originalValue = map[i, j];
                // mapNormalized[i, j] = (originalValue - minFloat) / (maxFloat - minFloat);
                
                //hard coded normal max and min form the general max and min each chunk takes usually 
                mapNormalized[i, j] = Mathf.InverseLerp (-2, 1.8f, map[i, j]);
            }
        }
        
        // Debug.Log("maxFloat: " + maxFloat + " " + "minFloat: " + minFloat);
        
        //height map that normalized
        return mapNormalized; 
    }
}

