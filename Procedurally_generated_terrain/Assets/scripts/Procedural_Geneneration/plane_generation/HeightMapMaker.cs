using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;



public class HeightMapMaker : MonoBehaviour
{
    public HeightmapGenerator heightmapGenerator;
    [System.Serializable]
    public class LayerSettings
    {
        public bool isVisible;
        public bool useAsMask;
        public float scale;
        public  int octaves;
        public float persistance;
        public float lacunarity;
        public float xDrift;
        public float yDrift;
        public float min;
        public float heightScale;
        public float level;
    }
    [SerializeField]
    public LayerSettings layerSettings;
    [SerializeField]
    public LayerSettings layerSettingsMountains;

    private void OnValidate()
    {
        
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        int n = 241;
        float[,] mapLayer1 = new float[n, n];
        float[,] mapLayer2 = new float[n, n];
        mapLayer1 = heightmapGenerator.MapGenerator(n, n,
            layerSettings.scale,
            layerSettings.octaves,
            layerSettings.persistance, layerSettings.lacunarity,
            layerSettings.xDrift, layerSettings.yDrift, 42);
        mapLayer2 = heightmapGenerator.MapGenerator(n, n,
            layerSettingsMountains.scale,
            layerSettingsMountains.octaves,
            layerSettingsMountains.persistance, layerSettingsMountains.lacunarity,
            layerSettingsMountains.xDrift, layerSettingsMountains.yDrift, 42);
        
        Layer[] layers = new Layer[2];
        layers[0] = new Layer(layerSettings, mapLayer1);
        layers[1] = new Layer(layerSettingsMountains, mapLayer2);
        
        LoadLayers(layers, meshFilter, n);
    }

    public void LoadLayers(Layer[] layers, MeshFilter meshFilter, int size)
    {
        Vector3[] vertices = meshFilter.mesh.vertices;
        Color[] colors = new Color[size * size];
        int l = 0;
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                layers[0].layerMap[i, j] = Mathf.Max(0, layers[0].layerMap[i, j] - layers[0].layerSettings.min);
                float value = layers[0].layerMap[i, j] * layers[0].layerSettings.heightScale;
                float valueColor = 0f;
                float mask;
                if (layers[0].layerSettings.useAsMask)
                {

                    mask = value;
                }
                else
                {
                    mask = 0;
                }
                for (int k = 1; k < layers.Length; k++)
                {
                    if (layers[k].layerSettings.isVisible)
                    {
                        
                        layers[k].layerMap[i, j] = Mathf.Max(0, layers[k].layerMap[i, j] - layers[k].layerSettings.min);

                        value += layers[k].layerMap[i, j] * layers[k].layerSettings.heightScale * mask;
                        // valueColor += layers[k].layerMap[i, j];
                    }
                }
                vertices[l].y = value;
                colors[l] = Color.magenta;
                l++;
            }
        }

        meshFilter.mesh.vertices = vertices;
        meshFilter.mesh.RecalculateNormals();
        meshFilter.mesh.RecalculateBounds();
        var tmp = new Texture2D(size, size);
        tmp.SetPixels(colors);
        tmp.Apply();
        var renderer_ = GetComponent<Renderer>(); // Get the Renderer component
        renderer_.material.mainTexture = tmp;
    }

    public class Layer
    {
        public LayerSettings layerSettings;
        public float[,] layerMap;

        public Layer(LayerSettings layer, float[,] map)
        {
            layerMap = map;
            layerSettings = layer;
        }
    }
}


