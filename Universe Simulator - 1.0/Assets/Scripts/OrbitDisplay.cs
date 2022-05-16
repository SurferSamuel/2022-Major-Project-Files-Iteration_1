using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class OrbitDisplay : MonoBehaviour
{
    // time step
    float dt = 86400f;

    // position scale
    float posScale = 1e+8f;

    // gravational constant
    float G = 6.67384e-11f;

    public bool isEnabled;
    public int numSteps;
    public float lineWidth;

    public bool relativeToBody;
    public GameObject centralBody;
    int centralBodyIndex;

    // Create new class to hold all virtual planets
    class VirtualBody
    {
        public string bodyName;
        public float mass;
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 acceleration;
        public Color pathColour;

        public VirtualBody(CelestialBody body)
        {
            bodyName = body.bodyName;
            mass = body.mass;
            position = body.position;
            velocity = body.velocity;
            acceleration = body.acceleration;
            pathColour = body.pathColour;
        }
    }

    void OnValidate()
    {        
        // Update variables from planets controller
        dt = gameObject.GetComponent<PlanetsController>().dt;
        posScale = gameObject.GetComponent<PlanetsController>().posScale;
        G = gameObject.GetComponent<PlanetsController>().G;

        // Update central body index
        centralBodyIndex = centralBody.transform.GetSiblingIndex();

        // Show orbits
        UpdateOrbitDisplay();
    }

    public void UpdateOrbitDisplay()
    {
        // Show orbits in editor only
        if (!Application.isPlaying && isEnabled)
        {
            DrawOrbits();
        }
        else
        {
            HideOrbits();
        }
    }

    void DrawOrbits()
    {
        // Create vars to hold virtual bodies and path points
        var virtualBodies = new VirtualBody[transform.childCount];
        var pathPoints = new Vector3[transform.childCount][];

        // Initialise virtual bodies (we don't want to move the actual bodies)
        // NOTE: initial vel and acc at t = 0 is already calculated
        for (int i = 0; i < virtualBodies.Length; i++)
        {
            virtualBodies[i] = new VirtualBody(transform.GetChild(i).GetComponent<CelestialBody>());
            pathPoints[i] = new Vector3[numSteps];

            // Set first point at initial pos
            pathPoints[i][0] = virtualBodies[i].position / posScale;
        }

        // Set central body inital pos (zero if relative to body is false)
        Vector3 centralBodyInitialPosition = (relativeToBody) ? virtualBodies[centralBodyIndex].position : Vector3.zero;
    
        // Simulate to get path points
        for (int step = 1; step < numSteps; step++)
        {
            // Get central body pos (zero if relative to body is false)
            Vector3 centralBodyPosition = (relativeToBody) ? virtualBodies[centralBodyIndex].position : Vector3.zero;

            var new_pos = new Vector3[virtualBodies.Length];

            // Update pos using Velocity Verlet 1 time step ahead 
            for (int i = 0; i < virtualBodies.Length; i++)
            {
                new_pos[i] = virtualBodies[i].position + virtualBodies[i].velocity * dt + 0.5f * virtualBodies[i].acceleration * dt * dt;
                virtualBodies[i].position = new_pos[i];

                // Update pos if relative to body
                if (relativeToBody)
                {
                    new_pos[i] = (i == centralBodyIndex) ? centralBodyInitialPosition : (new_pos[i] + (centralBodyInitialPosition - centralBodyPosition));
                }

                // Save position in path points
                pathPoints[i][step] = new_pos[i] / posScale;
            }

            var old_acc = new Vector3[virtualBodies.Length];

            // Update vel and acc for next time step
            for (int i = 0; i < virtualBodies.Length; i++)
            {
                // Save old acc before calculating new acc
                old_acc[i] = virtualBodies[i].acceleration;

                // Calculate new acc at 1 time step ahead
                virtualBodies[i].acceleration = CalculateAcceleration(virtualBodies, i);

                // Update vel using Velocity Verlet 1 time step ahead
                virtualBodies[i].velocity += 0.5f * (old_acc[i] + virtualBodies[i].acceleration) * dt;
            }
        }

        // Draw path lines using points
        for (int bodyIndex = 0; bodyIndex < transform.childCount; bodyIndex++)
        {
            var lineRenderer = transform.GetChild(bodyIndex).GetComponent<LineRenderer>();
            lineRenderer.positionCount = pathPoints[bodyIndex].Length;
            lineRenderer.SetPositions(pathPoints[bodyIndex]);
            lineRenderer.startColor = virtualBodies[bodyIndex].pathColour;
            lineRenderer.endColor = virtualBodies[bodyIndex].pathColour;
            lineRenderer.widthMultiplier = lineWidth;
        }
    }

    void HideOrbits()
    {
        // Hide path lines
        for (int bodyIndex = 0; bodyIndex < transform.childCount; bodyIndex++)
        {
            var lineRenderer = transform.GetChild(bodyIndex).GetComponent<LineRenderer>();
            lineRenderer.positionCount = 0;
        }
    }

    Vector3 CalculateAcceleration(VirtualBody[] bodies, int i)
    {
        // Calculate new acceleration
        Vector3 new_acc = Vector3.zero;
        VirtualBody ownBody = bodies[i];
        foreach (VirtualBody otherBody in bodies)
        {
            if (otherBody != ownBody)
            {
                // Use Netwon's Law of Gravation
                Vector3 positionVector = (otherBody.position - ownBody.position);
                new_acc += ((G * otherBody.mass) / positionVector.sqrMagnitude) * positionVector.normalized;
            }
        }

        return new_acc;
    }
}
