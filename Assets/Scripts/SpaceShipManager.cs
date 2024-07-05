using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceShipManager : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] AnimationClip[] possibleFlybys;
    [SerializeField] Animator animControl;
    [SerializeField] Vector2 minMaxAnimInterval;
    [Header("Colors and Thrust")]
    [SerializeField][ColorUsageAttribute(true,true)] Color glowCol;
    [SerializeField] Color thrustCol;
    [SerializeField] Material glowMat;
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
    float animTime, currentAnimInterval;
    #if UNITY_EDITOR
    Material startGlowMat;
    #endif
    // Start is called before the first frame update
    void Start()
    {
        currentAnimInterval = UnityEngine.Random.Range(minMaxAnimInterval.x, minMaxAnimInterval.y);
        animTime=currentAnimInterval-0.5f;
        #if UNITY_EDITOR
        startGlowMat = new Material(glowMat);
        #endif
        SetMatColor();
        SetParticleColor();
        posLastUpdate = transform.position;
    }
    void FixedUpdate()
    {
        AnimationHandling();

        velocity = transform.position - posLastUpdate;
        SetParticleLifetime();
        posLastUpdate = transform.position;
    }
    void AnimationHandling()
    {
        animTime += Time.deltaTime;
        if(animTime>=currentAnimInterval)
        {
            UnityEngine.Random.InitState((int)(Time.time + System.DateTime.Now.Millisecond));
            int animIndex = UnityEngine.Random.Range(0, possibleFlybys.Length);
            currentAnimInterval = UnityEngine.Random.Range(minMaxAnimInterval.x, minMaxAnimInterval.y) + possibleFlybys[animIndex].length;
            animTime=0;
            animControl.Play(possibleFlybys[animIndex].name);
        }
    }
    void SetParticleColor() //Sets particle colors
    {
        ParticleSystem.MainModule main = orbThrust.thrustSystem.main;
        main.startColor = thrustCol;

        main = mainThrust.thrustSystem.main;
        main.startColor = thrustCol;

        main = sparkThrust.thrustSystem.main;
        main.startColor = thrustCol;
    }
    void SetParticleLifetime() //Sets the lifetime of the particles according to velocity interpolation
    {
        float velMag = Mathf.Clamp(velocity.magnitude, minMaxThrustVelocity.x, minMaxThrustVelocity.y);
        velMag -= minMaxThrustVelocity.x;
        float velInterp = velMag / (minMaxThrustVelocity.y - minMaxThrustVelocity.x);

        ParticleSystem.MainModule main = orbThrust.thrustSystem.main;
        main.startLifetime = Mathf.Lerp(orbThrust.minMaxLifetime.x, orbThrust.minMaxLifetime.y, velInterp);

        main = mainThrust.thrustSystem.main;
        main.startLifetime = Mathf.Lerp(mainThrust.minMaxLifetime.x, mainThrust.minMaxLifetime.y, velInterp);

        main = sparkThrust.thrustSystem.main;
        main.startLifetime = Mathf.Lerp(sparkThrust.minMaxLifetime.x, sparkThrust.minMaxLifetime.y, velInterp);
    }
    void SetMatColor() //Sets the glow material color
    {
        glowMat.SetColor("_EmissionColor", glowCol);
    }
     #if UNITY_EDITOR
     void OnApplicationQuit() {
         glowMat.CopyPropertiesFromMaterial(startGlowMat);
     }
    #endif
}
