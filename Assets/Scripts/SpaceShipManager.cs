using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceShipManager : MonoBehaviour
{
    [Header("Main")]
    [SerializeField][ColorUsageAttribute(true,true)] Color glowCol;
    [SerializeField] Color thrustCol;
    [SerializeField] Vector2 minMaxThrustVelocity;
    [System.Serializable]
    struct ThrustParticles
    {
        public ParticleSystem thrustSystem;
        public Vector2 minMaxLifetime;
    }
    [Header("Particles")]

    [SerializeField] ThrustParticles orbThrust;
    [SerializeField] ThrustParticles mainThrust;
    [SerializeField] ThrustParticles sparkThrust;
    Vector3 velocity;
    Vector3 posLastUpdate;
    // Start is called before the first frame update
    void Start()
    {
        posLastUpdate = transform.position;
    }
    void FixedUpdate()
    {
        velocity = transform.position - posLastUpdate;
        float velMag = Mathf.Clamp(velocity.magnitude, minMaxThrustVelocity.x, minMaxThrustVelocity.y);
        velMag -= minMaxThrustVelocity.x;
        float velInterp = velMag / (minMaxThrustVelocity.y - minMaxThrustVelocity.x);

        ParticleSystem.MainModule main = orbThrust.thrustSystem.main;
        main.startLifetime = Mathf.Lerp(orbThrust.minMaxLifetime.x, orbThrust.minMaxLifetime.y, velInterp);
        main.startColor = thrustCol;

        main = mainThrust.thrustSystem.main;
        main.startLifetime = Mathf.Lerp(mainThrust.minMaxLifetime.x, mainThrust.minMaxLifetime.y, velInterp);
        main.startColor = thrustCol;

        main = sparkThrust.thrustSystem.main;
        main.startLifetime = Mathf.Lerp(sparkThrust.minMaxLifetime.x, sparkThrust.minMaxLifetime.y, velInterp);
        main.startColor = thrustCol;


        posLastUpdate = transform.position;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
