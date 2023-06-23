using System;
using UnityEngine;

public class HeightMap : MonoBehaviour
{
    [Tooltip("Executes script in OnValidate")]
    [SerializeField] private bool debug;
    [Tooltip("settings for heightMap")]
    [SerializeField] public HeightMapSettings heightMapSettings;

    private void OnValidate()
    {
        // Ensure heightMapSettings is not null
        if (heightMapSettings == null)
            heightMapSettings = new HeightMapSettings();

        // Initialize layerSettings array with the specified numberOfLayers
        if (heightMapSettings.layerSettings == null || heightMapSettings.layerSettings.Length != heightMapSettings.numberOfLayers)
            heightMapSettings.layerSettings = new LayerSettings[heightMapSettings.numberOfLayers];
        
        if (debug)
        {
            // Debug or execute your logic here
            Debug.Log("OnValidate executed.");
        }
    }

    private void Start()
    {
        foreach (var t in heightMapSettings.layerSettings)
        {
            Debug.Log(t.octaves);
        }
    }
}





