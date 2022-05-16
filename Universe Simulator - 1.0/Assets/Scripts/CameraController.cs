using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    public InputActionAsset inputProvider;
    public CinemachineFreeLook freeLookCamera;
    public CinemachineInputProvider cinemachineInputProviderScript;
    public GameObject cameraTarget;

    public float zoomSpeed = 0.05f;
    public float zoomScale = 0.05f;
    public float minZoom = 3f;
    public float maxZoom = 50f;

    private float zoom;
    public float currentRadius = 1f;
    private float newRadius;

    void Awake()
    {
        // Update zoom with mouse scroll
        inputProvider.FindActionMap("Freelook Camera").FindAction("Mouse Zoom").performed += context => UpdateZoom(context.ReadValue<float>());
        inputProvider.FindActionMap("Freelook Camera").FindAction("Mouse Zoom").canceled += context => UpdateZoom(0f);

        // Only move camera when left mouse button is down
        inputProvider.FindActionMap("Freelook Camera").FindAction("Mouse Left Button").performed += context => cinemachineInputProviderScript.enabled = true;
        inputProvider.FindActionMap("Freelook Camera").FindAction("Mouse Left Button").canceled += context => cinemachineInputProviderScript.enabled = false;

    }

    void FixedUpdate()
    {
        // Update camera target
        freeLookCamera.m_LookAt = cameraTarget.transform;
        freeLookCamera.m_Follow = cameraTarget.transform;

        // Scale zoom speed with radius (ie. faster further away // slower closer)
        zoomSpeed = currentRadius * zoomScale;

        // Scale min zoom to target size
        minZoom = 1.1f * cameraTarget.transform.localScale.x;
    }

    void OnEnable()
    {
        inputProvider.FindAction("Mouse Left Button").Enable();
        inputProvider.FindAction("Mouse Zoom").Enable();
    }

    void OnDisable()
    {
        inputProvider.FindAction("Mouse Left Button").Disable();
        inputProvider.FindAction("Mouse Zoom").Disable();
    }

    void UpdateZoom(float newZoom)
    {
        // Only update zoom if new value is different from current value
        if (newZoom != zoom)
        {
            zoom = newZoom;
            AdjustRadiusValue();
            UpdateCameraZoom();
        }
    }

    void AdjustRadiusValue()
    {
        // If zoom is set to 0, no need to update radius
        if (zoom == 0f)
        {
            return;
        }

        // If zoom is increased (ie. zoom in), shrink radius 
        if (zoom > 0f)
        {
            newRadius = currentRadius - zoomSpeed;
        }

        // If zoom is decreased (ie. zoom out), increase radius
        if (zoom < 0f)
        {
            newRadius = currentRadius + zoomSpeed;
        }
    }

    void UpdateCameraZoom()
    {
        // Only need to update if new radius is different to current
        if (newRadius != currentRadius)
        {
            // Update current radius
            currentRadius = newRadius;

            // Clamp radius between max and min (prevent zoom outside of range)
            currentRadius = Mathf.Clamp(currentRadius, minZoom, maxZoom);

            // Update camera radius from target
            freeLookCamera.m_Orbits[0].m_Height = currentRadius;
            freeLookCamera.m_Orbits[1].m_Radius = currentRadius;
            freeLookCamera.m_Orbits[2].m_Height = -currentRadius;
        }
    }
}
