using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PlanetsController : MonoBehaviour
{
    // simulation speed factor
    [Range(0.002f, 100.0000f)]
    public float speedFactor = 1f;
    
    // time step
    public float dt = 86400f;

    // gravational constant
    public float G = 6.67384e-11f;

    // position scale
    public float posScale = 1e+8f;

    // planet radius scale (1 = to scale)
    public float radScale = 1f;

    // planet trail renderer
    public bool showTrail = true;
    [Range(0.01f, 100.00f)]
    public float trailWidth = 1f;

    CelestialBody[] bodies;

    void Update()
    {
        // Update sim speed
        Time.fixedDeltaTime = 0.02f / speedFactor;

        // Get planets
        bodies = GetComponentsInChildren<CelestialBody>();

        // Update trail renderer
        foreach (TrailRenderer trailRenderer in GetComponentsInChildren<TrailRenderer>())
        {
            trailRenderer.enabled = showTrail;
            trailRenderer.widthMultiplier = trailWidth;
        }

        if (!Application.isPlaying)
        {
            for (int i = 0; i < bodies.Length; i++)
            {
                // Update position
                bodies[i].position = bodies[i].gameObject.transform.position * posScale;

                // Set initial velocities for all planets (ignore sun)
                if (bodies[i].bodyName != "Sun")
                {
                    bodies[i].velocity = bodies[i].CalculateInitialVelocity(G);
                }

                // Calculate acceleration at t = 0
                bodies[i].acceleration = bodies[i].CalculateAcceleration(bodies, dt, G);

                // Update orbit paths (only if planet has moved)
                if (bodies[i].transform.hasChanged)
                {
                    transform.GetComponent<OrbitDisplay>().UpdateOrbitDisplay();
                    bodies[i].transform.hasChanged = false;
                }

                // Update axial tilt
                bodies[i].transform.rotation = Quaternion.Euler(0, 0, -bodies[i].axialTilt);
            }
        }
    }

    void OnValidate()
    {
        // Get planets
        bodies = GetComponentsInChildren<CelestialBody>();

        // Update planets size
        for (int i = 0; i < bodies.Length; i++)
        {
            float new_scale = 2f * bodies[i].radius * radScale / posScale;
            bodies[i].gameObject.transform.localScale = new Vector3(new_scale, new_scale, new_scale);
        }
    }

    void FixedUpdate()
    {
        // Get planets
        bodies = GetComponentsInChildren<CelestialBody>();

        // Update pos using Velocity Verlet 1 time step ahead 
        for (int i = 0; i < bodies.Length; i++)
        {
            bodies[i].position += bodies[i].velocity * dt + 0.5f * bodies[i].acceleration * dt * dt;
        }

        var old_acc = new Vector3[bodies.Length];

        for (int i = 0; i < bodies.Length; i++)
        {
            // Save old acc before calculating new acc
            old_acc[i] = bodies[i].acceleration;

            // Calculate new acc at 1 time step ahead
            bodies[i].acceleration = bodies[i].CalculateAcceleration(bodies, dt, G);

            // Update vel using Velocity Verlet 1 time step ahead
            bodies[i].velocity += 0.5f * (old_acc[i] + bodies[i].acceleration) * dt;

            // Now move planets to new pos
            bodies[i].transform.position = bodies[i].position / posScale;

            // Update planet rotation
            var axialTiltRadians = Mathf.Abs(bodies[i].axialTilt) * (Mathf.PI / 180f);
            var rotationAnglePerDt = 360f / (bodies[i].rotationPeriod / dt);
            bodies[i].transform.Rotate(rotationAnglePerDt * Mathf.Sin(axialTiltRadians), rotationAnglePerDt * Mathf.Cos(axialTiltRadians), 0f, Space.World);
        }
    }
}
