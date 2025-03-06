using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragAndLaunch : MonoBehaviour
{
    public Rigidbody sphereRigidbody;
    public float forceMultiplier = 10f;
    public Renderer sphereRenderer; // Reference to the sphere's Renderer

    private Vector3 mousePressDownPos;
    private Vector3 mouseReleasePos;

    private Color originalColor; // Store original color
    void Start()
    {
        // Store the sphere's original color
        if (sphereRenderer != null)
        {
            originalColor = sphereRenderer.material.color;
        }
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button pressed
        {
            mousePressDownPos = Input.mousePosition;
            // Change sphere color to red
            if (sphereRenderer != null)
            {
                sphereRenderer.material.color = Color.red;
            }
        }

        if (Input.GetMouseButtonUp(0)) // Left mouse button released
        {
            mouseReleasePos = Input.mousePosition;
            LaunchSphere();

            // Restore original color
            if (sphereRenderer != null)
            {
                sphereRenderer.material.color = originalColor;
            }
        }
    }

    void LaunchSphere()
    {
        Vector3 direction = mouseReleasePos - mousePressDownPos; // Drag direction
        direction.z = direction.y; // Map vertical screen movement to Z-axis movement
        direction.y = 0; // Keep movement on XZ plane

        Vector3 launchForce = direction.normalized * forceMultiplier;
        sphereRigidbody.AddForce(launchForce, ForceMode.Impulse);
    }
}
