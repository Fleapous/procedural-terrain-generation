using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayCaster : MonoBehaviour
{
    public bool rayTrack;
    public GameObject debugSphere;
    public float sphereheight;
    
    public bool PositionAndRayCast(int position, Vector3 origin, float size)
{
    switch (position)
    {
        case 0:
            return CastRayAndDebug(origin, 225f, size, true);
        case 1:
            return CastRayAndDebug(origin, 180f, size, false);
        case 2:
            return CastRayAndDebug(origin, 135f, size, true);
        case 3:
            return CastRayAndDebug(origin, 270f, size, false);
        case 5:
            return CastRayAndDebug(origin, 90f, size, false);
        case 6:
            return CastRayAndDebug(origin, 315f, size, true);
        case 7:
            return CastRayAndDebug(origin, 0f, size, false);
        case 8:
            return CastRayAndDebug(origin, 45f, size, true);
        default:
            return false;
    }
}
    private bool CastRayAndDebug(Vector3 origin, float direction, float size, bool isSide)
    {
        size += 200;
        if (isSide)
            size = Mathf.Sqrt(2) * size;
        bool isChunk = Physics.Raycast(origin, Quaternion.AngleAxis(direction, Vector3.up) * Vector3.forward, out var hit, size);
        if(isChunk)
            Debug.Log(hit.collider.gameObject.tag + "location: " + (origin + Quaternion.AngleAxis(direction, Vector3.up) * Vector3.forward.normalized * size));
        if(rayTrack)
            CreateSphereAtMaxDistance(origin, Quaternion.AngleAxis(direction, Vector3.up) * Vector3.forward, size, isChunk);
        return isChunk;
    }
    private void CreateSphereAtMaxDistance(Vector3 origin, Vector3 direction, float distance, bool isGreen)
    {
        Vector3 endPoint = origin + direction.normalized * distance;
        GameObject sphere = Instantiate(debugSphere, new Vector3(endPoint.x, sphereheight, endPoint.z), Quaternion.identity);
        
        Renderer sphereRenderer = sphere.GetComponent<Renderer>();

        // Set the color based on the 'isGreen' parameter
        if (isGreen)
        {
            sphereRenderer.material.color = Color.green;
        }
        else
        {
            sphereRenderer.material.color = Color.red;
        }
    }

}
