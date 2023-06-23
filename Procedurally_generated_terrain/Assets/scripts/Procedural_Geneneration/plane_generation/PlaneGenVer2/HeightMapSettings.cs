using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public class HeightMapSettings
{
    [Range(1, 10)]
    public int numberOfLayers;
    public LayerSettings[] layerSettings;
}

[System.Serializable]
public class LayerSettings
{
    [Tooltip("animation curve for mapping noise map to height")]
    public AnimationCurve animationCurve;
    [Tooltip("scale of the noise map")]
    public float scale;
    [Tooltip("layers of noise for each noise map")]
    public int octaves;
    [Tooltip("strength of the individual noise layers Higher values means each layer will have more influance on the final outcome")]
    public float persistence;
    [Tooltip("variation between layers. A higher lacunarity value increases the gap between layers, resulting in larger-scale structures dominating the noise map")]
    public float lacunarity;
    [Tooltip("while interpolating makes it so that only high noise map values will impact the overall mesh")]
    public float amplificationConstant;

    public Task<float[,]> GenerateHeightMap(int size, float xMove, float yMove)
    {
        var map = new float[size, size];
        
        return Task.FromResult(map);
    }
}
