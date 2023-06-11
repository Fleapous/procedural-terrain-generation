using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField] private float boostFactor;
    [SerializeField] private float sensitivity;
    public bool flightMode = false;
    public bool boost = false;
    [SerializeField] private Rigidbody rb;
    private void Awake()
    {
        // rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (flightMode && boost)
        {
            rb.AddForce(transform.forward * boostFactor * Time.deltaTime);    
        }
        
    }

    // Start is called before the first frame update
    public void ActionJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Debug.Log("Jump!");
            flightMode = !flightMode;
        }
            
    }

    public void ActionBoost(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            boost = true;
        }

        if (context.canceled)
        {
            boost = false;
        }
    }

    public void ActionLook(InputAction.CallbackContext context)
    {
        Vector2 cords = context.ReadValue<Vector2>();
        Debug.Log(context.ReadValue<Vector2>());
        transform.localRotation = Quaternion.Euler(-cords.y, cords.x, 0);
    }
}
