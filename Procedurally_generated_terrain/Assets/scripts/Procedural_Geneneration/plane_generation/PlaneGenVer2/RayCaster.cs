using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayCaster : MonoBehaviour
{
    public bool RayCast(Ray direction, float distance)
    {
        return Physics.Raycast(direction, distance);
    }
}
