using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyParticleController : MonoBehaviour
{   
    [SerializeField]
    ParticleSystem starParticles;
    [Range(0f, 0.35f)]
    [SerializeField] float bulgeAmplitude = 0.5f;
    [Range(0f, 7.5f)]
    [SerializeField] float bulgeWidth = 0.5f;
    [Range(0f, 0.2f)]
    [SerializeField] float waveAmplitude = 0.1f;
    [Range(1f, 2.75f)]
    [SerializeField] float waveFrequency = 0.1f;
    [Range(0f, 0.7f)]
    [SerializeField] float twistZ = 0.25f;
    [Range(0f, 0.6f)]
    [SerializeField] float twistX = 0.25f;
    [Range(0f, 0.6f)]
    [SerializeField] float twistY = 0.25f;
    [Range(-1f, 1.5f)]
    [SerializeField] float scatterX = 0.25f;
    [Range(-1f, 1.5f)]
    [SerializeField] float scatterY = 0.25f;
    [Range(0f, 0.3f)]
    [SerializeField] float scatterZ = 0.25f;
    [SerializeField] int numIterations = 40;
    [SerializeField] int numSamples = 40;
    [SerializeField] float size = 2f;
    float scalar = 0.25f;
    [Range(0.1f, 0.5f)]
    [SerializeField] float speed = 0.25f;
    [SerializeField] bool stationary = false;

    [SerializeField] public Color col1, col2, col3;

    [SerializeField] int galaxySeed;

    Dictionary<int, (int, float)> particleIndexing =
    new Dictionary<int, (int, float)>();

    private ParticleSystem.Particle[] particles;

    private struct Polar2{
        public float r;
        public float theta;
    }

    void Awake(){

    }

    void Start()
    {
        Init(galaxySeed);
    }

    public void Init(int inputSeed){
        if (starParticles == null)
        {
            starParticles = GetComponent<ParticleSystem>();
        }
        if(galaxySeed == inputSeed){return;}
        galaxySeed = inputSeed;

        particleIndexing.Clear();
        starParticles.Clear();

        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
        starParticles.Emit(emitParams, numSamples * numIterations);

        particles = new ParticleSystem.Particle[starParticles.particleCount];
        int numParticles = starParticles.GetParticles(particles);

        for (int i = 0; i < numSamples * numIterations; i++){
            int seed = (int)particles[i].randomSeed;
            
            Vector3 pos = particles[i].position;

            Polar2 polarPos = Vector2Polar(pos);
            particleIndexing.Add(seed, (i, polarPos.theta));

            UnityEngine.Random.InitState(seed);


        }
        
        starParticles.SetParticles(particles);
    }

    public void RandomizeVars(int seed){
        UnityEngine.Random.InitState(seed);
        bulgeAmplitude = UnityEngine.Random.Range(0f, 0.35f);
        bulgeWidth = UnityEngine.Random.Range(2.5f, 7.5f);
        waveAmplitude = UnityEngine.Random.Range(0f, 0.2f);
        waveFrequency = UnityEngine.Random.Range(1f, 2.75f);
        twistZ = UnityEngine.Random.Range(0f, 0.7f);
        scatterY = UnityEngine.Random.Range(-1f, 1.5f);
        twistX = UnityEngine.Random.Range(0f, 0.6f);
        UnityEngine.Random.InitState(seed+1);
        twistY = UnityEngine.Random.Range(0f, 0.6f);
        scatterX = UnityEngine.Random.Range(-1f, 1.5f);
        scatterZ = UnityEngine.Random.Range(0f, 0.3f);
        speed = UnityEngine.Random.Range(0.1f, 0.5f);
    }

    void Update()
    {
        if (starParticles == null)
        {
            starParticles = GetComponent<ParticleSystem>();
        }
        RandomizeVars(galaxySeed);


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
            float theta = indexedParticle.Item2;
            if (!stationary){
                theta -= Time.time * speed;;
            }

            scalar = 1/CalculateMaxDistance();
            Vector2 newPos = ScatteredEllipse(w, h, theta, r) * scalar * size;
            
            float newZ = (Gaussian(w+h, bulgeAmplitude * 2f, 0, bulgeWidth * 2f)) * UnityEngine.Random.Range(-1f, 1f) + UnityEngine.Random.Range(-scatterZ, scatterZ);

            newZ += ModifiedSin(newPos.x, waveAmplitude, waveFrequency, Time.time * speed);
            Vector3 pos = new Vector3(newPos.x, newPos.y, newZ);
            particles[i].position = pos;
            
            Color particleCol = Color.Lerp(col1, col2, UnityEngine.Random.value);
            Color col = Color.Lerp(particleCol, col3, UnityEngine.Random.value);

            particles[i].startColor = col;


        }

        starParticles.SetParticles(particles);
    }

    float CalculateMaxDistance()
    {
        float maxW = (numIterations - 1) * twistX + scatterX;
        float maxH = (numIterations - 1) * twistY + scatterY;
        float maxR = (numIterations - 1) * twistZ;
        return new Vector2 (maxW, maxH).magnitude;
    }

    //theta and r are the polar coords
    //w and h are the dimensions
    Vector2 ScatteredEllipse(float w, float h, float theta, float r){
        Vector2 ellipse;
        //parametric ellipse form
        ellipse.x = Rotate2DX(new Vector2(w * Mathf.Cos(theta), h * Mathf.Sin(theta)), r);
        ellipse.y = Rotate2DY(new Vector2(w * Mathf.Cos(theta), h * Mathf.Sin(theta)), r);
        return ellipse;
    }

    float Gaussian(float x, float amplitude, float mean, float stdDev)
    {
        return amplitude * Mathf.Exp(-(Mathf.Pow(x - mean, 2) / (2 * Mathf.Pow(stdDev, 2))));
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
