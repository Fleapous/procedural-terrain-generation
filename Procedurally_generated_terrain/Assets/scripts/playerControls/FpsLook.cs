using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsLook : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private Transform playerTransform;

    private float xRotation = 0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotate the player around the Y-axis (horizontal rotation)
        playerTransform.Rotate(Vector3.up * mouseX);

        // Rotate the camera around the X-axis (vertical rotation)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Apply the rotation to the camera
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
