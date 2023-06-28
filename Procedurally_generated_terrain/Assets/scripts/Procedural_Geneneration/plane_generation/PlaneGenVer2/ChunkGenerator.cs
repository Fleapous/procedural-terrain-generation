using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGenerator : MonoBehaviour
{
    private Terrain terrain;
    private Transform location;
    private void Start()
    {
        var parent = transform.parent;
        terrain = parent.GetComponent<Terrain>();
        location = parent.GetComponent<Transform>();
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
        var position1 = location.position;
        var scale = terrain.terrainData.size;
        
        Vector2 position = new Vector2(position1.x, position1.z);
        Vector2 gridPosition = new Vector2(position1.x / scale.x, position1.z / scale.z);

        for (int xOffset = -1; xOffset <= 1; xOffset++)
        {
            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                Vector2 neighborPos = gridPosition + new Vector2(xOffset, yOffset);
                Vector2 worldPos = position + new Vector2(xOffset * scale.x, yOffset * scale.z);
                
                
                
                // Debug.Log(neighborPos + " " + worldPos);
                // Debug.Log("next Chunk");
                
                
            }
        }
        
    }
    
    // // Given a location, check if a chunk exists at that location
    // public bool ChunkExistsAtLocation(Vector2Int location)
    // {
    //     // Calculate the position of the center of the chunk
    //     Vector3 chunkPosition = new Vector3(location.x * chunkSize, 0f, location.y * chunkSize);
    //
    //     // Calculate the size of the chunk bounds
    //     Vector3 chunkBoundsSize = new Vector3(chunkSize, chunkSize, chunkSize);
    //
    //     // Perform an overlap check with a BoxCollider at the chunk position
    //     Collider[] colliders = Physics.OverlapBox(chunkPosition, chunkBoundsSize / 2f);
    //
    //     // Check if any colliders were found
    //     return colliders.Length > 0;
    // }
}
