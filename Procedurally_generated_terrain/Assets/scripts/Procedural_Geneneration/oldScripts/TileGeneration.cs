﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGeneration : MonoBehaviour
{
    [SerializeField] private noise_generation noiseGeneration;
    [SerializeField] private MeshRenderer tileRenderer;
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshCollider meshCollider;
    [SerializeField] private float mapScale;

    private void OnValidate()
    {
        GenerateTile();
    }

    private void GenerateTile()
    {
        // calculate tile depth and width based on the mesh vertices
        Vector3[] meshVertices = this.meshFilter.sharedMesh.vertices;
        int tileDepth = (int)Mathf.Sqrt(meshVertices.Length);
        int tileWith = tileDepth;
        
        // calculate the offsets based on the tile position
        float[,] heightMap = this.noiseGeneration.GenerateNoise(tileDepth, tileWith, this.mapScale);
        
        // generate a heightMap using noise
        Texture2D tileTexture = BuildTexture(heightMap);
        this.tileRenderer.material.mainTexture = tileTexture;
    }

    private Texture2D BuildTexture(float[,] heightMap)
    {
        int tileDepth = heightMap.GetLength(0);
        int tilewith = heightMap.GetLength(1);

        Color[] colorMap = new Color[tileDepth * tilewith];
        for (int zIndex = 0; zIndex < tileDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < tilewith; xIndex++)
            {
                // transform the 2D map index is an Array index
                int colorIndex = zIndex * tilewith + xIndex;
                float height = heightMap[zIndex, xIndex];
                // assign as color a shade of grey proportional to the height value
                colorMap[colorIndex] = Color.Lerp(Color.black, Color.white, height);
            }
        }
        // create a new texture and set its pixel colors
        Texture2D tileTexture = new Texture2D(tilewith, tileDepth);
        tileTexture.wrapMode = TextureWrapMode.Clamp;
        tileTexture.SetPixels(colorMap);
        tileTexture.Apply();

        return tileTexture;
    }
}
