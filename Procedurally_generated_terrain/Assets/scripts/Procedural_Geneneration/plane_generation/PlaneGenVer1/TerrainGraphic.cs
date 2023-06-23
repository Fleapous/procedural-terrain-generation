using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public class TerrainGraphics
{
    public TerrainGraphic mountain;
    public TerrainGraphic normal;
    public TerrainGraphic water;

    public Color Evaluate(float height)
    {
        Color res = Color.magenta;
        res = mountain.Evaluate(height);
        if (res != Color.magenta)
            return res;
        res = normal.Evaluate(height);
        if (res != Color.magenta)
            return res;
        res = water.Evaluate(height);
        if (res != Color.magenta)
            return res;
        return res;
    }
}
[System.Serializable]
public class TerrainGraphic
{
    public Color terrainColor;
    public float heightMin;
    public float heightMax;

    public Color Evaluate(float height)
    {
        if (height >= heightMin && height < heightMax)
            return terrainColor;
        return Color.magenta;
    }
}
