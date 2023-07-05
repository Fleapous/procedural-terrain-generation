using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;

public class HeightMap : MonoBehaviour
{
    [Tooltip("Executes script in OnValidate")]
    [SerializeField] private bool debug;
    [Tooltip("settings for heightMap")]
    [SerializeField] public HeightMapSettings heightMapSettings;
    private Terrain terrain;
    private void OnValidate()
    {
        // Ensure heightMapSettings is not null
        if (heightMapSettings == null)
            heightMapSettings = new HeightMapSettings();

        // // Initialize layerSettings array with the specified numberOfLayers
        // if (heightMapSettings.layerSettings == null || heightMapSettings.layerSettings.Length != heightMapSettings.layerSettings.)
        //     heightMapSettings.layerSettings = new LayerSettings[heightMapSettings.numberOfLayers];
        
        if (debug)
        {
            GenerateHeightMap();
        }
    }

    private void Start()
    {
        if (!debug)
        {
            Debug.Log($"instance: {GetInstanceID()}, heighmap settings: {heightMapSettings.xMove} {heightMapSettings.yMove} {heightMapSettings.layerSettings.Length}");
            // Debug.Log(heightMapSettings.xMove + " " + heightMapSettings.yMove);
            GenerateHeightMap();
        }
    }

    public async void GenerateHeightMap()
    {
        terrain = GetComponent<Terrain>();
        int numberOfLayers = heightMapSettings.layerSettings.Length;
        int resolution = terrain.terrainData.heightmapResolution;
        float[,] finalMap = new float[resolution, resolution];
        Task[] tasks = new Task[numberOfLayers];
        
        for (int i = 0; i < numberOfLayers; i++)
        {
            int index = i;
            tasks[i] = Task.Run(() => heightMapSettings.layerSettings[index].GenerateHeightMapValues(
                resolution,
                heightMapSettings.xMove,
                heightMapSettings.yMove, 
                heightMapSettings.seed));
        }
        await Task.WhenAll(tasks);
        
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                finalMap[i, j] = CalculateHeight(i, j);
            }
        }
        
        terrain.terrainData.SetHeights(0,0,finalMap);
    }
    public float CalculateHeight(int x, int y)
    {
        float value = 0;
        foreach (var layer in heightMapSettings.layerSettings)
        {
            float amplifiedNum = (float)Math.Pow(layer.layerMap[x,y], layer.amplificationConstant);  // Amplify the input value exponentially with the specified power
            float normalizedNum = (amplifiedNum - 0) / (1 - 0);  // Normalize the amplified number to the range [0, 1]
            if(layer.enable)
                value += normalizedNum * (layer.heightScalar * 1f/10f);
        }
        return value;
    }
}





