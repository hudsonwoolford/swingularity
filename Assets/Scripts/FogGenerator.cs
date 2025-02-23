using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogGenerator : NebulaGenerator
{
    [SerializeField] ParticleSystem darkFog;
    [SerializeField] float gridDensity = 1f;
    [SerializeField] float noiseScale = 0.1f; // Adjust to control noise detail
    [SerializeField] float threshold = 0.5f; // Only spawn if noise value is above this
    [SerializeField] int layers = 1;
    [Header("Coloring")]
    [Range(0f, 1f)]
    [SerializeField] float minAlpha;
    [Range(0f, 1f)]
    [SerializeField] float maxAlpha = 1;
    [SerializeField] Material particleMaterial;
    [SerializeField] Material darkParticleMaterial;
    Transform ball, hole;
    Vector4[] objectPos = new Vector4[2];
    private void Start()
    {
        ball = GameObject.FindGameObjectWithTag("Player").transform;
        hole = GameObject.FindGameObjectWithTag("Hole").transform;
    }
    private void Update()
    {
        objectPos[0] = ball.position;
        objectPos[1] = hole.position;
        particleMaterial.SetVectorArray("_ObjectPos", objectPos);
        darkParticleMaterial.SetVectorArray("_ObjectPos", objectPos);
    }
    protected override void CalculateNebulaSize()
    {
        //nothing needed
    }
    protected override void ApplyParticleColors()
    {
        base.ApplyParticleColors();
        ParticleSystem.MainModule main1 = nebula1.main;
        ParticleSystem.MainModule main2 = nebula2.main;

        Gradient gradient1 = new Gradient(); Gradient gradient2 = new Gradient();

        gradient1.SetKeys(new GradientColorKey[] { new GradientColorKey(main1.startColor.colorMin, 0) }, new GradientAlphaKey[] { new GradientAlphaKey(maxAlpha, 0), new GradientAlphaKey(minAlpha, 1) });
        gradient2.SetKeys(new GradientColorKey[] { new GradientColorKey(main1.startColor.colorMax, 0) }, new GradientAlphaKey[] { new GradientAlphaKey(maxAlpha, 0), new GradientAlphaKey(minAlpha, 1) });
        main1.startColor = new ParticleSystem.MinMaxGradient(gradient1, gradient2);                                                                                                       
                                                                                                                                                                                          
        gradient1.SetKeys(new GradientColorKey[] { new GradientColorKey(main2.startColor.colorMin, 0) }, new GradientAlphaKey[] { new GradientAlphaKey(maxAlpha, 0), new GradientAlphaKey(minAlpha, 1) });
        gradient2.SetKeys(new GradientColorKey[] { new GradientColorKey(main2.startColor.colorMax, 0) }, new GradientAlphaKey[] { new GradientAlphaKey(maxAlpha, 0), new GradientAlphaKey(minAlpha, 1) });
        main2.startColor = new ParticleSystem.MinMaxGradient(gradient1, gradient2);
        GenerateFogParticles();
    }
    void GenerateFogParticles()
    {
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
        Vector2[] fogPoints = FogPoints();

        ParticleSystem.MainModule main = nebula1.main;
        main.maxParticles = Mathf.CeilToInt(fogPoints.Length * (1-threshold)); 
        main = nebula2.main;
        main.maxParticles = Mathf.CeilToInt(fogPoints.Length * (1-threshold));

        foreach (var p in fogPoints)
        {
            float noiseVal = Mathf.PerlinNoise(p.x * noiseScale, p.y * noiseScale);

            if (noiseVal > threshold)
            {
                emitParams.position = new Vector3(p.x, p.y, 0);

                UnityEngine.Random.InitState(System.DateTime.Now.Millisecond + Mathf.CeilToInt(p.x * p.y));
                int choice = UnityEngine.Random.Range(0, 3);

                darkFog.Emit(emitParams, 1);

                if((noiseVal - threshold) / (1 - threshold) < 0.5f)
                {
                    nebula1.Emit(emitParams, layers);
                }
                else
                {
                    nebula2.Emit(emitParams, layers);
                }
            }
        }
    }
    Vector2[] FogPoints()
    {
        PolygonCollider2D levelShape = CameraFollow.innerShape;
        List<Vector2> fogPoints = new List<Vector2>();

        for (float x = levelShape.bounds.min.x; x < levelShape.bounds.max.x; x += 1 / gridDensity)
        {
            for (float y = levelShape.bounds.min.y; y < levelShape.bounds.max.y; y += 1 / gridDensity)
            {
                if (levelShape.OverlapPoint(new Vector2(x, y)))
                {
                    fogPoints.Add(new Vector2(x, y));
                }
            }
        }

        return fogPoints.ToArray();
    }
}
