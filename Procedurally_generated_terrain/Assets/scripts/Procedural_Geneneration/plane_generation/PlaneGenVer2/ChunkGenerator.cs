using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGenerator : MonoBehaviour
{
    public GameObject terrainChunk;
    
    private Terrain terrain;
    private Transform locationTerrain;
    private Transform locationRayCaster;
    private RayCaster rayCaster;
    private HeightMapSettings heightMapSettingsOg;
    private TerrainData terrainDataOg;
    private void Start()
    {
        var parent = transform.parent;
        terrain = parent.GetComponent<Terrain>();
        locationTerrain = parent.GetComponent<Transform>();
        locationRayCaster = GetComponentInChildren<Transform>();
        rayCaster = GetComponentInChildren<RayCaster>();
        heightMapSettingsOg = GetComponentInParent<HeightMap>().heightMapSettings;
        terrainDataOg = GetComponentInParent<Terrain>().terrainData;
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("Player"))
        {
            // Debug.Log("TriggerEnter ID: " + GetInstanceID());
            GenerateChunks();
        }
    }
    
    public void GenerateChunks()
    {
        var position1 = locationTerrain.position;
        var scale = terrain.terrainData.size;
        
        Vector2 position = new Vector2(position1.x, position1.z);
        Vector2 gridPosition = new Vector2(Mathf.RoundToInt(position1.x / scale.x), Mathf.RoundToInt(position1.z / scale.z));
        int k = 0;
        for (int xOffset = -1; xOffset <= 1; xOffset++)
        {
            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                if (xOffset == 0 && yOffset == 0)
                {
                    k++;
                    continue;
                }
                Vector2 neighborPos = gridPosition + new Vector2(xOffset, yOffset);
                Vector2 worldPos = position + new Vector2(xOffset * scale.x, yOffset * scale.z);
                if (rayCaster.PositionAndRayCast(k, new Vector3(locationRayCaster.transform.position.x + scale.x/2, -100, locationRayCaster.transform.position.z  + scale.z/2), scale.x))  
                {
                    // Debug.Log("there is a terrain on: " + neighborPos + " " + worldPos);
                }
                else
                {
                    InstantiateNewChunk(scale, neighborPos, worldPos, position1);
                }
                k++;
            }
        }
    }

    private void InstantiateNewChunk(Vector3 scale, Vector2 neighborPos, Vector2 worldPos, Vector3 position1)
    {
        Vector2 offsetVector = neighborPos * (terrainDataOg.heightmapResolution - 1);
        HeightMapSettings heightMapSettings = ScriptableObject.CreateInstance<HeightMapSettings>();
        heightMapSettings = heightMapSettingsOg.Copy(offsetVector.x, offsetVector.y);
        TerrainData terrainData = CopyTerrainData(terrainDataOg);

        GameObject chunk = Instantiate(terrainChunk, new Vector3(worldPos.x, position1.y, worldPos.y), Quaternion.identity);
        HeightMap script = chunk.GetComponent<HeightMap>();
        script.heightMapSettings = heightMapSettings;

        Terrain tmp = chunk.GetComponent<Terrain>();
        tmp.terrainData = terrainData;

        TerrainCollider terrainCollider = chunk.GetComponent<TerrainCollider>();
        terrainCollider.terrainData = terrainData;
    }

    private TerrainData CopyTerrainData(TerrainData original)
    {
        TerrainData terrainData = new TerrainData();

        // Copy desired properties from terrainDataOg to terrainData
        var propertiesToCopy = new[]
        {
            "thickness", "splatPrototypes",
            "treePrototypes", "treeInstances", "alphamapResolution",
            "heightmapResolution","size"
        };

        foreach (var property in propertiesToCopy)
        {
            Debug.Log(property);
            var sourceProperty = terrainDataOg.GetType().GetProperty(property);
            var targetProperty = terrainData.GetType().GetProperty(property);
            var value = sourceProperty.GetValue(terrainDataOg);
            targetProperty.SetValue(terrainData, value);
        }

        // Recreate detail prototypes to match the new detail resolution
        terrainData.SetDetailResolution(terrainDataOg.detailResolution, terrainDataOg.detailResolutionPerPatch);
        terrainData.RefreshPrototypes();

        // Reset the heightmap of the new terrain data
        float[,] newHeightMap = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
        terrainData.SetHeights(0, 0, newHeightMap);

        return terrainData;
    }
}
