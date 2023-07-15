using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;

public class TerrainMaker : MonoBehaviour
{
    [Tooltip("Executes script in OnValidate")]
    [SerializeField] private bool debug;
    
    [Tooltip("settings for heightMap")] [SerializeField]
    public HeightMapSettings heightMapSettings;

    [Tooltip("A container for terrain objects and textures")] [SerializeField]
    public TextureContainer textureContainer;

    [HideInInspector] public static bool heightMapSettingsFoldOut;
    [HideInInspector] public static bool textureSettingsFoldOut;
    
    private Terrain terrain;
    private void OnValidate()
    {
        if (debug)
        {
            Initialize();
        }
    }
    private void Start()
    {
        if (!debug)
        {
            Debug.Log($"instance: {GetInstanceID()}, heighmap settings: {heightMapSettings.xMove} {heightMapSettings.yMove} {heightMapSettings.layerSettings.Length}");
            Initialize();
        }
    }

    public void Initialize()
    {
        terrain = GetComponent<Terrain>();
        GenerateHeightMap();
        SetTextures();
    }

    private async void GenerateHeightMap()
    {
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
        // return finalMap;
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

    private void SetTextures()
    {
        var terrainData = terrain.terrainData;
        float[,,] splatmapData = new float[terrainData.alphamapWidth,
                                            terrainData.alphamapHeight,
                                            textureContainer.textures.Length];

        for (int x = 0; x < terrainData.alphamapWidth; x++)
        {
            for (int y = 0; y < terrainData.alphamapHeight; y++)
            {

                float terrainHeight = terrainData.GetHeight(y, x);
                float terrainAngle = terrainData.GetSteepness(y, x);
                float[] textureValues = textureContainer.SetTextureValues(terrainHeight, terrainAngle);
                for (int i = 0; i < textureContainer.textures.Length; i++)
                {
                    splatmapData[x, y, i] = textureValues[i];
                }
            }
        }
        terrainData.SetAlphamaps(0, 0, splatmapData);
    }
}





