using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using System.Threading;
using UnityEngine.Serialization;

public class HeightmapVisiulizerAsync : MonoBehaviour
{
    [SerializeField] private int seed;
    [SerializeField] private float scale;
    [SerializeField] public float xMove;
    [SerializeField] public float yMove;
    [SerializeField] private int octaves;
    [SerializeField] private float persistance;
    [SerializeField] private float lacunarity;
    [SerializeField] private bool showHeight = true;
    [SerializeField] private bool showGray;
    [SerializeField] private AnimationCurve curve1;
    [SerializeField] private AnimationCurve curve2;
    [SerializeField] private float heightScalar = 1;
    [SerializeField] private Textures textures;
    [SerializeField] private bool ShowSeeds;
    [SerializeField] private float SeedHeight;
    [SerializeField] private float SeedRad;
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

    private struct Seed
    {
        public Vector2 pos;
        public Color color;
    }
        
    public List<Vector2> neighbouringChunkSeedPos;
    private HeightmapGenerator _heightmapGenerator;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private static Dictionary<Vector3, Seed> seedCollection = new Dictionary<Vector3, Seed>();
    private static object _lock = new object();
    
    
    public async void HeightVizWrapperFunction()
    {
        _heightmapGenerator = GetComponent<HeightmapGenerator>();
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
        Textures texture = textures;
        
        
        Vector3 chunkPos = GetComponent<Transform>().position;
        int size = _meshFilter.sharedMesh.vertices.Length;
        Vector3[] vertices = _meshFilter.mesh.vertices;
        int n = (int)Mathf.Sqrt(size);
        Texture2D newTexture = new Texture2D(n, n);
        Color32[] color32s = new Color32[n * n];
        float[,] mapMain = new float[n, n];
        float[,] weightMap = new float[n, n];
    
        //2d noise map generation
        Task<float[,]> taskMapMain = Task.Run(() => _heightmapGenerator.MapGenerator(n, n, scale, octaves,
            persistance, lacunarity, xMove * 1 / 100, yMove * 1 / 100, seed));
        mapMain = await taskMapMain;
        
        //curve function map
        Task<float[,]> taskWeightMap = Task.Run(() => _heightmapGenerator.MapGenerator(n, n, 900, octaves,
            persistance, lacunarity, xMove * 1 / 100, yMove * 1 / 100, seed));
        weightMap = await taskWeightMap;
        
        Dictionary<Vector3, Seed> nearSeeds = new Dictionary<Vector3, Seed>();
        if(ShowSeeds)
            nearSeeds = GetNearSeeds(chunkPos, 240, texture, SeedHeight, SeedRad, ShowSeeds);
        else
        {
            Task<Dictionary<Vector3, Seed>> taskSeed = Task.Run(() => GetNearSeeds(chunkPos, 240, texture, SeedHeight, SeedRad, ShowSeeds));
            nearSeeds = await taskSeed;
        }
        
        //make it a texture
        Vector3[] newMeshHeight = new Vector3[n * n];
        Task<Vector3[]> taskTexture = Task.Run((() => MakeTexture(chunkPos, vertices, mapMain, n, n, color32s, nearSeeds, texture, weightMap)));
        newMeshHeight = await taskTexture;

        _meshFilter.mesh.vertices = newMeshHeight;
        _meshFilter.mesh.RecalculateNormals();
        _meshFilter.mesh.RecalculateBounds();
        if (newTexture)
        {
            newTexture.SetPixels32(color32s);
            newTexture.Apply();
            _meshRenderer.material.mainTexture = newTexture;
        }
        
    }

    private Vector3[] MakeTexture(Vector3 chunkPosition, Vector3[] newHeight,
        float[,] map, int height, int width, Color32[] colors, Dictionary<Vector3, Seed> nearSeeds, Textures terrainTexture, float[,] weightMap)
    {
        int k = 0;
        lock (_lock)
        {
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    float vertex = map[i, j];
                    float height1 = curve1.Evaluate(vertex) * heightScalar;
                    float height2 = curve2.Evaluate(vertex) * heightScalar;
                    if (showHeight)
                    {
                        newHeight[k].y = CalculateHeight(height1, height2, weightMap[i, j]);
                        if (showGray)
                            colors[k] = Color.Lerp(Color.black, Color.white, weightMap[i, j]);
                        else
                            colors[k] = FindClosestSeed(nearSeeds, chunkPosition, new Vector3(j, 0f, i));
                        k++;
                    }
                    else
                    {
                        newHeight[k].y = 0;
                        if (showGray)
                            colors[k] = Color.Lerp(Color.black, Color.white, vertex);
                        else
                            colors[k] = FindClosestSeed(nearSeeds, chunkPosition, new Vector3(j, 0f, i));
                        k++;
                    }
                }
            }
        }


        return newHeight;
    }
    
    private Color SeedColor(Textures textures, int heightNormal)
    {
        if (textures.texture1Range.x <= heightNormal && heightNormal <= textures.texture1Range.y)
            return textures.texture1;
        if (textures.texture2Range.x < heightNormal && heightNormal < textures.texture2Range.y)
            return textures.texture2;
        if (textures.texture3Range.x <= heightNormal && heightNormal <= textures.texture3Range.y)
            return textures.texture3;
        return Color.white;
    }
    //linear interpolation
    private float CalculateHeight(float height1, float height2, float weight)
    {
        weight *= 2;
        return (1 - weight) * height2 + weight * height1;
    }
    private Dictionary<Vector3, Seed> GetNearSeeds(Vector3 chunkPos, int chunkSize, Textures bioms, float height, float radius, bool showSeeds)
    {
        lock (_lock)
        {
            //creat the random object
            // int seed = Environment.TickCount * Thread.CurrentThread.ManagedThreadId;
            // System.Random random = new System.Random(seed);
            
            //init the dict
            Dictionary<Vector3, Seed> closeSeedsDict = new Dictionary<Vector3, Seed>();

            //main alg for finding/generating the nearby chunk seeds
            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                for (int xOffset = -1; xOffset <= 1; xOffset++)
                {
                    //current chunk position
                    Vector3 viewedChunk = new Vector3(xOffset * chunkSize + chunkPos.x, 0f, yOffset * chunkSize + chunkPos.z);
                    //check if dict has the chunk already
                    if (seedCollection.ContainsKey(viewedChunk))
                        closeSeedsDict.Add(viewedChunk, seedCollection[viewedChunk]);
                    else //create the chunk and seed
                    {
                        int seed = Environment.TickCount * Thread.CurrentThread.ManagedThreadId;
                        System.Random random = new System.Random(seed);
                        Seed newSeed;
                        newSeed.pos = new Vector2(random.Next(0, 241) + viewedChunk.x, random.Next(0, 241) + viewedChunk.z);
                        // newSeed.pos = new Vector2(Random.Range(0, 241) + viewedChunk.x, Random.Range(0, 241) + viewedChunk.y);
                        
                        // newSeed.color = new Color32((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256), 255);
                        newSeed.color = SeedColor(bioms, random.Next(0, 101));
                        seedCollection.Add(viewedChunk, newSeed);
                        closeSeedsDict.Add(viewedChunk, newSeed);
                        
                        if(showSeeds)
                            generateSphere(newSeed.pos, height, radius);
                    }
                }
            }
            return closeSeedsDict;
        }
    }
    private Color FindClosestSeed(Dictionary<Vector3, Seed> seeds, Vector3 chunkPos, Vector3 vertexPosRelative)
    {
        Vector3 vertexPosGlobal = chunkPos + vertexPosRelative;
        vertexPosGlobal.y = 0f;
        float minDist = float.MaxValue;
        Color closestSeedCol = Color.red;
        foreach (var seedIt in seeds)
        {
            Vector3 seedPos = new Vector3(seedIt.Value.pos.x, 0f, seedIt.Value.pos.y);
            float dist = Vector3.Distance(vertexPosGlobal, seedPos);
            if (dist < minDist)
            {
                minDist = dist;
                closestSeedCol = seedIt.Value.color;
            }
        }
        return closestSeedCol;
    }
    //old method not good :(
    private Seed GetClosestSeed(Vector3 chunkPos, Vector3 vertexPosRelative, float chunkSize, int chunkInViewDist, Textures bioms)
    {
        int seed = Environment.TickCount * Thread.CurrentThread.ManagedThreadId;
        System.Random random = new System.Random(seed);
        
        Seed closestSeed;
        closestSeed.color = Color.magenta;
        closestSeed.pos = new Vector2(float.MaxValue, float.MaxValue);
        
        Vector3 vertexPosGlobal = chunkPos + vertexPosRelative;
        vertexPosGlobal.y = 0f;
        float closestDist = float.MaxValue;
        for (int yOffset = -chunkInViewDist; yOffset <= chunkInViewDist; yOffset++)
        {
            for (int xOffset = -chunkInViewDist; xOffset <= chunkInViewDist; xOffset++)
            {
                Vector2 viewedChunk = new Vector2(xOffset * chunkSize + chunkPos.x, yOffset * chunkSize + chunkPos.z);
                //check if seed exists in position
                if (seedCollection.ContainsKey(viewedChunk))
                {
                    Vector3 seedPos = new Vector3(seedCollection[viewedChunk].pos.x, 0f, seedCollection[viewedChunk].pos.y);
                    float dist = Vector3.Distance(vertexPosGlobal, seedPos);

                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestSeed = seedCollection[viewedChunk];
                    }
                }
                else
                {
                    Seed newSeed;
                    newSeed.pos = new Vector2(random.Next(0, 241) + viewedChunk.x, random.Next(0, 241) + viewedChunk.y);
                    // newSeed.color = new Color32((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256), 255);
                    newSeed.color = SeedColor(bioms, random.Next(0, 101));
                    
                    seedCollection.Add(viewedChunk, newSeed);

                    float dist = Vector3.Distance(vertexPosGlobal, new Vector3(newSeed.pos.x, 0f, newSeed.pos.y));
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestSeed = seedCollection[viewedChunk];
                    }
                }
            }
        }

        return closestSeed;
    }
    private void generateSphere(Vector2 pos, float height, float radius)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = new Vector3(pos.x, height, pos.y);
        sphere.transform.localScale = Vector3.one * radius;
        var renderer = sphere.GetComponent<Renderer>();
        var material = new Material(Shader.Find("Standard"));
        material.color = Color.red;
        renderer.material = material;
    }
}