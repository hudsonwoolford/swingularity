using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyParticleController : MonoBehaviour
{   
    [SerializeField]
    ParticleSystem starParticles;
    [SerializeField] float bulge = 0.5f;
    [SerializeField] float waveAmplitude = 0.1f;
    [SerializeField] float waveFrequency = 0.1f;
    [SerializeField] float twistZ = 0.25f;
    [SerializeField] float twistX = 0.25f;
    [SerializeField] float twistY = 0.25f;
    [SerializeField] float scatterX = 0.25f;
    [SerializeField] float scatterY = 0.25f;
    [SerializeField] float scatterZ = 0.25f;
    [SerializeField] int numIterations = 40;
    [SerializeField] int numSamples = 40;
    [SerializeField] float scalar = 0.25f;
    [SerializeField] float speed = 0.25f;

    Dictionary<int, (int, float)> particleIndexing =
    new Dictionary<int, (int, float)>();

    private ParticleSystem.Particle[] particles;

    private struct Polar2{
        public float r;
        public float theta;
    }

    void Awake(){
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
        starParticles.Emit(emitParams, numSamples * numIterations);
    }

    void Start()
    {
        if (starParticles == null)
        {
            starParticles = GetComponent<ParticleSystem>();
        }
        


        particles = new ParticleSystem.Particle[starParticles.particleCount];
        int numParticles = starParticles.GetParticles(particles);


        for (int i = 0; i < numSamples * numIterations; i++){
            int seed = (int)particles[i].randomSeed;
            
            Vector3 pos = particles[i].position;
            Polar2 polarPos = Vector2Polar(pos);
            particleIndexing.Add(seed, (i, polarPos.theta));
        }

    }

    void Update()
    {
        if (starParticles == null)
        {
            starParticles = GetComponent<ParticleSystem>();
        }

        particles = new ParticleSystem.Particle[starParticles.particleCount];

        int numParticles = starParticles.GetParticles(particles);

        for (int i = 0; i < numParticles; i++){
            int seed = (int)particles[i].randomSeed;

            (int, float) indexedParticle = particleIndexing[seed];
            int index = indexedParticle.Item1;

            UnityEngine.Random.InitState(seed);

            float w = (index % numIterations) * twistX + UnityEngine.Random.Range(0, scatterX);
            float h = (index % numIterations) * twistY + UnityEngine.Random.Range(0, scatterY);
            float r = (index % numIterations) * twistZ;
            float theta = indexedParticle.Item2 - Time.time * speed;

            Vector2 newPos = ScatteredEllipse(w, h, theta, r) * scalar;

            float newZ = ModifiedSin(newPos.x, waveAmplitude, waveFrequency, Time.time * speed) + UnityEngine.Random.Range(0, scatterZ);


            particles[i].position = new Vector3(newPos.x, newPos.y, newZ);

        }

        starParticles.SetParticles(particles);
    }

    //theta and r are the polar coords
    //w and h are the displacements vertically and horizontally
    Vector2 ScatteredEllipse(float w, float h, float theta, float r){
        Vector2 ellipse;
        ellipse.x = Rotate2DX(new Vector2(w * Mathf.Cos(theta), h * Mathf.Sin(theta)), r);
        ellipse.y = Rotate2DY(new Vector2(w * Mathf.Cos(theta), h * Mathf.Sin(theta)), r);
        return ellipse;
    }

    float ModifiedSin(float t, float a, float b, float c){
        return a * Mathf.Sin(b * t + c);
    }

    Polar2 Vector2Polar(Vector2 vector){
        Polar2 polar;
        polar.r = Mathf.Sqrt(vector.x * vector.x + vector.y * vector.y);
        polar.theta = Mathf.Atan2(vector.y, vector.x);
        return polar;
    }

    float Rotate2DX(Vector2 pos, float angle){
        return Mathf.Sqrt(pos.x * pos.x + pos.y * pos.y) * Mathf.Cos(angle + Mathf.Atan2(pos.y, pos.x));
    }
    float Rotate2DY(Vector2 pos, float angle){
        return Mathf.Sqrt(pos.x * pos.x + pos.y * pos.y) * Mathf.Sin(angle + Mathf.Atan2(pos.y, pos.x));
    }
}
