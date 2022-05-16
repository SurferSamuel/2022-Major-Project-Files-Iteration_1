using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialBody : MonoBehaviour
{
    public string bodyName;
    public float mass;
    public float radius;

    // eccentricity = 0 for circular orbit
    public float eccentricity = 0f;

    // orbit inclination angle in respect to the earth-sun plane
    public float orbitInclination = 0f;

    // planet axial tilt angle
    public float axialTilt = 0f;

    // sidereal rotation period in secs
    public float rotationPeriod;

    public CelestialBody orbitedBody = null;

    public Vector3 position;
    public Vector3 velocity;
    public Vector3 acceleration;

    public Color pathColour;

    public Vector3 CalculateInitialVelocity(float G)
    {
        // Calculate initial velocity relative to orbited body
        Vector3 positionVector = orbitedBody.position - this.position;
        Vector3 directionVector = Vector3.Cross(Quaternion.Euler(-orbitInclination, 0, 0) * Vector3.up, positionVector).normalized;
        Vector3 init_vel = Mathf.Sqrt(G * orbitedBody.mass * ((1f + eccentricity) / positionVector.magnitude)) * directionVector;

        // Add initial velocity of orbited body (ignore if orbited body is the sun)
        if (orbitedBody.bodyName != "Sun")
        {
            init_vel += orbitedBody.CalculateInitialVelocity(G);
        }

        return init_vel;
    }

    public Vector3 CalculateAcceleration(CelestialBody[] bodies, float dt, float G)
    {
        // Calculate new acceleration
        Vector3 new_acc = Vector3.zero;
        foreach (CelestialBody otherBody in bodies)
        {
            if (otherBody != this)
            {
                // Use Netwon's Law of Gravation
                Vector3 positionVector = (otherBody.position - this.position);
                new_acc += ((G * otherBody.mass) / positionVector.sqrMagnitude) * positionVector.normalized;
            }
        }

        return new_acc;
    }
}
