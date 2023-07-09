using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Texture Container", menuName = "Custom/Texture Container")]
public class TextureContainer : ScriptableObject
{
    public TextureLayer[] textures;

    public float[] SetTextureValues(float height)
    {
        float[] textureValues = new float[textures.Length];
        for (int i = 0; i < textureValues.Length; i++)
        {
            if (height >= textures[i].range.x && height <= textures[i].range.y)
            {
                textureValues[i] = 1;
            }
            else
            {
                textureValues[i] = 0;
            }
        }

        return textureValues;
    }
}
[System.Serializable]
public class TextureLayer
{
    // public Texture2D texture2D;
    public Vector2 range;
}
