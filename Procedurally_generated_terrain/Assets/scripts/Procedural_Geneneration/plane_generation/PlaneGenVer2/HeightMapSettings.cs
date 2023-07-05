using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "HeightMapSettings", menuName = "Custom/HeightMapSettings")]
public class HeightMapSettings : ScriptableObject
{
    [Tooltip("seed of the map")]
    public int seed;
    public float xMove;
    public float yMove;
    public LayerSettings[] layerSettings;
    
    public HeightMapSettings Copy(float newXMove, float newYMove)
    {
        HeightMapSettings copy = Instantiate(this);

        copy.xMove = newXMove;
        copy.yMove = newYMove;

        copy.layerSettings = new LayerSettings[layerSettings.Length];
        for (int i = 0; i < layerSettings.Length; i++)
        {
            LayerSettings originalLayer = layerSettings[i];
            LayerSettings copyLayer = new LayerSettings
            {
                enable = originalLayer.enable,
                animationCurve = new AnimationCurve(originalLayer.animationCurve.keys),
                scale = originalLayer.scale,
                octaves = originalLayer.octaves,
                persistence = originalLayer.persistence,
                lacunarity = originalLayer.lacunarity,
                amplificationConstant = originalLayer.amplificationConstant,
                heightScalar = originalLayer.heightScalar,
                layerMap = originalLayer.layerMap.Clone() as float[,]
            };

            copy.layerSettings[i] = copyLayer;
        }

        return copy;
    }

}

[System.Serializable]
public class LayerSettings
{
    [Tooltip("enables the layer")]
    public bool enable;
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
    [Tooltip("HeightScalar of noise map")] 
    public float heightScalar;
    
    [SerializeField]
    public float[,] layerMap;

    public void GenerateHeightMapValues(int size, float xMove, float yMove, int seed)
    {
        var map = new float[size, size];
        var mapNormalized = new float[size, size];
        System.Random rng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = rng.Next(-100000, 100000) + xMove;
            float offsetY = rng.Next(-100000, 100000) + yMove;
            octaveOffsets [i] = new Vector2 (offsetX, offsetY);
        }
        float halfHeight = size / 2;
        float halfWidth = size / 2;
        //row z
        for (int i = 0; i < size; i++)
        {
            //column x
            for (int j = 0; j < size; j++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                //octaves
                for (int k = 0; k < octaves; k++)
                {
                    float x = (j - halfWidth + octaveOffsets[k].x) / scale * frequency;
                    float y = (i - halfHeight + octaveOffsets[k].y) / scale * frequency;
                
                    float perlinNumber = Mathf.PerlinNoise(x, y) * 2 - 1;
                    noiseHeight += perlinNumber * amplitude;
                
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }
                //adding the value to map
                map[i, j] = noiseHeight;
            }
        }
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float originalValue = map[i, j];
                //hard coded normal max and min form the general max and min each chunk takes usually 
                mapNormalized[i, j] = Mathf.InverseLerp (-2, 1.8f, map[i, j]);
            }
        }
        //height map that normalized
        layerMap = mapNormalized;
    }
}
