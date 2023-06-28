using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("colision");
        if (collision.gameObject.CompareTag("ChunkTrigger"))
        {
            ChunkGenerator chunkGenerator = collision.gameObject.GetComponent<ChunkGenerator>();
            if (chunkGenerator != null)
            {
                chunkGenerator.GenerateChunks();
            }
        }
    }
}
