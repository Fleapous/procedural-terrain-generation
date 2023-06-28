using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGenerator : MonoBehaviour
{
    private Terrain terrain;
    private Transform locationTerrain;
    private Transform locationRayCaster;
    private RayCaster rayCaster;
    private void Start()
    {
        var parent = transform.parent;
        terrain = parent.GetComponent<Terrain>();
        locationTerrain = parent.GetComponent<Transform>();
        locationRayCaster = GetComponentInChildren<Transform>();
        rayCaster = GetComponentInChildren<RayCaster>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
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
                Vector2 neighborPos = gridPosition + new Vector2(xOffset, yOffset);
                Vector2 worldPos = position + new Vector2(xOffset * scale.x, yOffset * scale.z);
                if (rayCaster.PositionAndRayCast(k, new Vector3(locationRayCaster.transform.position.x + scale.x/2, -100, locationRayCaster.transform.position.z  + scale.z/2), scale.x))  
                {
                    Debug.Log("there is a terrain on: " + neighborPos + " " + worldPos);
                }
                
                
                // Debug.Log(neighborPos + " " + worldPos);
                // Debug.Log("next Chunk");

                k++;
            }
        }
    }
}
