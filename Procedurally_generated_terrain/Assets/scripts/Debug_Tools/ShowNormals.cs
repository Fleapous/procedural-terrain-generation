using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowNormals : MonoBehaviour
{
    [SerializeField] private float normalLength = 0.1f; // Length of the normal line
    [SerializeField] private Color normalColor = Color.red; // Color of the normal line

    private void OnDrawGizmosSelected()
    {
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;

        if (mesh == null)
            return;

        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;

        if (vertices.Length != normals.Length)
            return;

        Gizmos.color = normalColor;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = transform.TransformPoint(vertices[i]);
            Vector3 normal = transform.TransformDirection(normals[i]);

            Gizmos.DrawLine(vertex, vertex + normal * normalLength);
        }
    }
}
