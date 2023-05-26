using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class PlaneGenerator : MonoBehaviour
{
    [SerializeField] private int size;
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;

    private void Start()
    {
        
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        
        CreateShape();
        UpdateShapes();
    }

    void CreateShape()
    {
        vertices = new Vector3[]
        {
            new Vector3(0, 0, 0),
            new Vector3(0, 0, 1),
            new Vector3(1, 0, 0),
            new Vector3(1, 0, 1)
        };

        triangles = new int[]
        {
            0, 1, 2,
            1, 3, 2
        };
    }

    void UpdateShapes()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
    }
}
