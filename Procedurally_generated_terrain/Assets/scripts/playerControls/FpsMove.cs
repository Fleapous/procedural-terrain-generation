using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsMove : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float forceMagnitude;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ApplyForceInLookDirection();
        }
    }

    private void ApplyForceInLookDirection()
    {
        Vector3 lookDirection = transform.forward;
        Vector3 force = lookDirection * forceMagnitude;
        rb.AddForce(force, ForceMode.Impulse);
    }
}
