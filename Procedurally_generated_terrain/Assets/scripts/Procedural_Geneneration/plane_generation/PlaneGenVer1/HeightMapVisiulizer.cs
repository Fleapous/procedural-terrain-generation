using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;


public class HeightMapVisiulizer : MonoBehaviour
{
    [SerializeField] private int seed;
    [SerializeField] private float scale;
    [SerializeField] public float xMove;
    [SerializeField] public float yMove;
    [SerializeField] private int octaves;
    [SerializeField] private float persistance;
    [SerializeField] private float lacunarity;
    [SerializeField] private bool showHeight = true;
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private float heightScalar = 1;
    [SerializeField] private Textures textures;
    [SerializeField] private bool debugNoise;
    [SerializeField] private bool ShowBioms;
    [System.Serializable]
        public class Textures
        {
            public Color texture1;
            public Color texture2;
            public Color texture3;
            public Vector2 texture1Range;
            public Vector2 texture2Range;
            public Vector2 texture3Range;
        }
        
    public List<Vector2> neighbouringChunkSeedPos;
    private HeightmapGenerator _heightmapGenerator;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    private void Start()
    {
        // if(!debugNoise)
        //     HeightVizWrapperFunction();
    }
    private void OnValidate()
    {
        if (debugNoise)
            HeightVizWrapperFunction();
    }
    public async void HeightVizWrapperFunction()
    {
        _heightmapGenerator = GetComponent<HeightmapGenerator>();
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
        Textures texture = textures;
        //get the size of the mesh
        int size = _meshFilter.sharedMesh.vertices.Length;
        int n = (int)Mathf.Sqrt(size);
        float[,] map = new float[n, n];

        //create the HeightMap
        // map = _heightmapGenerator.MapGenerator(n, n, scale, octaves,
        //     persistance, lacunarity, xMove * 1 / 100, yMove * 1 / 100, seed);
        
        Task<float[,]> task = Task.Run(() => _heightmapGenerator.MapGenerator(n, n, scale, octaves,
            persistance, lacunarity, xMove * 1 / 100, yMove * 1 / 100, seed));

        // Do other work while the MapGenerator method is running...

        // Wait for the MapGenerator method to complete and get its return value
        map = await task;

        //make it a texture
        Texture2D mapTexture = MakeTexture(map, n, n, texture);
        _meshRenderer.material.mainTexture = mapTexture;
    }

    private Texture2D MakeTexture(float[,] map, int height, int width, Textures textures)
    {
        Transform chunkPos = GetComponent<Transform>();
        Vector3[] newHeight = _meshFilter.mesh.vertices;
        Texture2D texture = new Texture2D(width, height);
        int k = 0;
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                float value = map[i, j];
                float heightNormal = curve.Evaluate(value);
                
                if (showHeight)
                {
                    newHeight[k].y = heightNormal * heightScalar;
                    texture.SetPixel(j, i, Color.Lerp(Color.black, Color.white, map[i, j]));
                    k++;
                }else
                {
                    newHeight[k].y = 0;
                    
                    k++;
                }
            }
        }
        _meshFilter.mesh.vertices = newHeight;
        _meshFilter.mesh.RecalculateNormals();
        _meshFilter.mesh.RecalculateBounds();
        
        texture.Apply();
        return texture;
    }

    private static void Texturing(Textures textures, float heightNormal, Texture2D texture, int j, int i)
    {
        if (textures.texture1Range.x <= heightNormal && heightNormal <= textures.texture1Range.y)
            texture.SetPixel(j, i, textures.texture1);
        else if (textures.texture2Range.x < heightNormal && heightNormal < textures.texture2Range.y)
            texture.SetPixel(j, i, textures.texture2);
        else if (textures.texture3Range.x <= heightNormal && heightNormal <= textures.texture3Range.y)
            texture.SetPixel(j, i, textures.texture3);
    }

    private void BiomGeneration(Texture2D texture, int i, int j, Vector3 chunkPosition, int size)
    {
        float closestSeed = float.MaxValue;
        Vector2 chunkPosV2;
        chunkPosV2.x = chunkPosition.x;
        chunkPosV2.y = chunkPosition.z;
        // Calculate the local position of the vertex
        Vector2 localVertexPosition = new Vector2(i * size, j * size);
        // Calculate the global position of the vertex
        Vector2 globalVertexPosition = localVertexPosition + chunkPosV2;

        foreach (var seedPos in neighbouringChunkSeedPos)
        {
            float distanceToSeed = Vector2.Distance(globalVertexPosition, seedPos);
            if (distanceToSeed < closestSeed)
                closestSeed = distanceToSeed;
        }
        texture.SetPixel(j, i, Color.cyan);
    }
}

