using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Texture Container", menuName = "Custom/Texture Container")]
public class TextureContainer : ScriptableObject
{
    public TextureLayer[] textures;

    public float[] SetTextureValues(float height, float angle)
    {
        float[] textureValues = new float[textures.Length];
        for (int i = 0; i < textureValues.Length; i++)
        {
            textureValues[i] = textures[i].CalculateAlpha(height, angle);
        }

        return textureValues;
    }
}
[System.Serializable]
public class TextureLayer
{
    public bool enable;
    public Vector2 heightRange;
    public Vector2 angleRange;

    public float CalculateAlpha(float height, float angle)
    {
        if (!enable)
            return 0;

        float alphaValue = 0;
        if (height >= heightRange.x && height <= heightRange.y)
        {
            if (angle >= angleRange.x && angle <= angleRange.y)
            {
                alphaValue = angle;
            }
        }

        return alphaValue;
    }
}
