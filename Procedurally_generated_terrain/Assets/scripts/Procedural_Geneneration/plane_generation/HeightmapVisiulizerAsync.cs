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
    [SerializeField] public float xMove;
    [SerializeField] public float yMove;
    [SerializeField] private int octaves;
    [SerializeField] private float persistance;
    [SerializeField] private float lacunarity;
    [SerializeField] private bool showHeight = true;
    [SerializeField] private bool showGray;
    [System.Serializable]
    public class CurveSettings
    {
        [Tooltip("Curve Function for Default terrain generation")]
        public AnimationCurve curve1;
        public float curve1Scale;

        [Tooltip("Curve Function for Mountain generation")]
        public AnimationCurve curve2;
        public float curve2Scale;
        [Range(0, 1)] public float curve2Offset;
        
        [Tooltip("Curve Function for bodies of water generation")]
        public AnimationCurve curve3;
        public float curve3Scale;
        [Range(0, 1)] public float curve3Offset;

        [Tooltip("Amplifies the train")] public float amplificationNumber;
        
        [Tooltip("adds scalar to all heights")]
        public float heightScalar;
    }

    [SerializeField]
    private CurveSettings curveSettings;
    
    // [SerializeField] private float heightScalar = 1;
    [SerializeField] private Textures textures;
    [System.Serializable]
    public class VoronoiSeedSettings
    {
        [Tooltip("displays seeds as spheres on the map")]
        public bool showSeeds;
        [Tooltip("height of the spheres")]
        public float seedHeight;
        [Tooltip("radius of the spheres")]
        public float seedRad;
    }

    [SerializeField]
    private VoronoiSeedSettings voronoiSeedSettings;

    [SerializeField] private bool debug;
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
    //storing prev weights
    private float prevweight2 = 0;
    private float prevweight3 = 0;
    
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
        // Texture2D newTexture = new Texture2D(n, n);
        Color32[] color32s = new Color32[n * n];
        float[,] mapMain = new float[n, n];
        float[,] curve2WeightMap = new float[n, n];
        float[,] curve3WeightMap = new float[n, n];
        float amplificationNumber = curveSettings.amplificationNumber;
        
        //modes for curve functions
        curveSettings.curve1.preWrapMode = WrapMode.Default;
        curveSettings.curve1.postWrapMode = WrapMode.Default;
        curveSettings.curve2.preWrapMode = WrapMode.Default;
        curveSettings.curve2.postWrapMode = WrapMode.Default;
        curveSettings.curve3.preWrapMode = WrapMode.Default;
        curveSettings.curve3.postWrapMode = WrapMode.Default;
        
        //init serialized fields
        float settingsHeightScalar = curveSettings.heightScalar;
        float settingsCurve1Scale = curveSettings.curve1Scale;
        float settingsCurve2Scale = curveSettings.curve2Scale;
        float settingsCurve2Offset = curveSettings.curve2Offset;
        float settingsCurve3Scale = curveSettings.curve3Scale;
        float settingsCurve3Offset = curveSettings.curve3Offset;
        
        //2d noise map 
        Task<float[,]> taskMapMain = Task.Run(() => _heightmapGenerator.MapGenerator(n, n, settingsCurve1Scale, octaves,
            persistance, lacunarity, xMove * 1 / 100, yMove * 1 / 100, seed));
 
        //curve2 weight map
        Task<float[,]> taskCurve2WeightMap = Task.Run(() => _heightmapGenerator.MapGenerator(n, n, settingsCurve2Scale, octaves,
            persistance, lacunarity, xMove * 1 / 100, yMove * 1 / 100, 98));
        
        //curve3 weight map
        Task<float[,]> taskCurve3WeightMap = Task.Run(() => _heightmapGenerator.MapGenerator(n, n, settingsCurve3Scale, octaves,
            persistance, lacunarity, xMove * 1 / 100, yMove * 1 / 100, 100));

        mapMain = await taskMapMain;
        curve2WeightMap = await taskCurve2WeightMap;
        curve3WeightMap = await taskCurve3WeightMap;
        //biome 
        Dictionary<Vector3, Seed> nearSeeds = new Dictionary<Vector3, Seed>();
        if(voronoiSeedSettings.showSeeds)
            nearSeeds = GetNearSeeds(chunkPos, 240, texture, voronoiSeedSettings.seedHeight, voronoiSeedSettings.seedRad, voronoiSeedSettings.showSeeds);
        else
        {
            Task<Dictionary<Vector3, Seed>> taskSeed = Task.Run(() => GetNearSeeds(chunkPos, 240, texture, voronoiSeedSettings.seedHeight, voronoiSeedSettings.seedRad, voronoiSeedSettings.showSeeds));
            nearSeeds = await taskSeed;
        }
        
        //make it a texture
        Vector3[] newMeshHeight = new Vector3[n * n];
        Task<Vector3[]> taskTexture = Task.Run(() => MakeTexture(chunkPos, vertices, mapMain, n, n, color32s,
            nearSeeds, texture,
            curve2WeightMap, settingsCurve2Offset,
            curve3WeightMap, settingsCurve3Offset
            ,settingsHeightScalar, amplificationNumber));
        newMeshHeight = await taskTexture;

        _meshFilter.mesh.vertices = newMeshHeight;
        _meshFilter.mesh.RecalculateNormals();
        _meshFilter.mesh.RecalculateBounds();
        var tmp = new Texture2D(n, n);
        tmp.SetPixels32(color32s);
        tmp.Apply();
        _meshRenderer.material.mainTexture = tmp;
    }

    private Vector3[] MakeTexture(Vector3 chunkPosition, Vector3[] newHeight,
        float[,] map, int height, int width, Color32[] colors, Dictionary<Vector3, Seed> nearSeeds, Textures terrainTexture,
        float[,] curve2WeightMap, float curve2Offset,
        float[,] curve3WeightMap, float curve3Offset,
        float settingsHeightScalar, float amplificationNumber)
    {
        int k = 0;
        lock (_lock)
        {
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    float vertex = map[i, j];
                    float height1 = curveSettings.curve1.Evaluate(vertex);
                    float height2 = curveSettings.curve2.Evaluate(vertex);
                    float height3 = curveSettings.curve3.Evaluate(vertex);
                    if (showHeight)
                    {
                        newHeight[k].y = CalculateHeight(
                            height1, map[i, j],
                            height2, curve2WeightMap[i, j],
                            height3, curve3WeightMap[i, j], amplificationNumber) * settingsHeightScalar;
                        if (showGray)
                        {
                            // colors[k] = Color.Lerp(Color.blue, Color.magenta, curve3WeightMap[i, j]);
                            if(curve3WeightMap[i, j] > curve3Offset && curve2WeightMap[i, j] > curve2Offset)
                                colors[k] = Color.magenta;
                            else if (curve3WeightMap[i, j] > curve3Offset)
                                colors[k] = Color.blue;
                            else if (curve2WeightMap[i, j] > curve2Offset)
                                colors[k] = Color.red;
                            else
                                colors[k] = Color.Lerp(Color.black, Color.white, map[i, j]);
                        }
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
    
    private float CalculateHeight(
        float height1, float curve1Weight,
        float height2, float curve2Weight,
        float height3, float curve3Weight, float amplificationScale)
    {
        
        float weight2 = AmplifyAndNormalize(curve2Weight, amplificationScale);
        float weight3 = AmplifyAndNormalize(curve3Weight, amplificationScale);
        float weight1 = curve1Weight;
        
        // Normalize weights
        float sumWeights = weight1 + weight2 + weight3;
        weight1 /= sumWeights;
        weight2 /= sumWeights;
        weight3 /= sumWeights;

        // Interpolate heights using exponential weights
        float finalHeight = height1 * weight1 + height2 * weight2 + height3 * weight3;
        return finalHeight;
    }
    
    private float AmplifyAndNormalize(float value, float power)
    {
        float amplifiedNum = (float)Math.Pow(value, power);  // Amplify the input value exponentially with the specified power
        float normalizedNum = (amplifiedNum - 0) / (1 - 0);  // Normalize the amplified number to the range [0, 1]
        return normalizedNum;
    }
    private Dictionary<Vector3, Seed> GetNearSeeds(Vector3 chunkPos, int chunkSize, Textures bioms, float height, float radius, bool showSeeds)
    {
        lock (_lock)
        {
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
            // float dist = MinkowskiDistance(vertexPosGlobal, seedPos, 3);
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

    private float MinkowskiDistance(Vector3 a, Vector3 b, int p)
    {
        float xCords = Mathf.Pow((Mathf.Abs(a.x - b.x)), p);
        float yCords = Mathf.Pow((Mathf.Abs(a.z - b.z)), p);

        return Mathf.Pow((xCords + yCords), 1f / p);
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