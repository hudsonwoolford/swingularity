using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class QuasarGenerator : MonoBehaviour
{
    [Serializable]
    public struct QuasarColorScheme
    {
        public ColorSchemeStaticReference.PlanetType planetType;
        public Color darkColor, lightColor;
        [ColorUsage(true, true)]
        public Color fresnel;
        public Color sparkColor;
    }
    [SerializeField] QuasarColorScheme[] schemePool;
    [SerializeField] int schemeIndex;
    [SerializeField] float systemRotSpeed = 8;
    [Header("Local Variables")]

    MeshRenderer sphereRenderer, distortRenderer;
    ParticleSystem swirlSystem;
    Transform distortionQuad;
    MaterialPropertyBlock sphereBlock, distortBlock;
    Material distortMat;
    CelestialMass cMass;
    // Start is called before the first frame update
    void Start()
    {
        NullChecks();
    }

    // Update is called once per frame
    void Update()
    {
        if (schemePool.Length == 0) { return; }
        NullChecks();
        Coloring();
    }
    public void InitRandom(int seed, ColorSchemeStaticReference.PlanetType planetType)
    {
        NullChecks();
        UnityEngine.Random.InitState(seed);

        List<int> typeSchemeIndices = new List<int>();
        for (int i = 0; i < schemePool.Length; i++)
        {
            if (schemePool[i].planetType == planetType)
            {
                typeSchemeIndices.Add(i);
            }
        }
        if (typeSchemeIndices.Count == 0) { return; }
        schemeIndex = typeSchemeIndices[UnityEngine.Random.Range(0, typeSchemeIndices.Count)];

        //Applying
        RandomRotation();
        Coloring();

        //Reset system
        swirlSystem.Clear();
        swirlSystem.Play();
    }
    [ContextMenu("Random Color")]
    void RandomHole()
    {
        schemeIndex = UnityEngine.Random.Range(0, schemePool.Length);

        //Applying
        RandomRotation();
        Coloring();

        //Reset system
        swirlSystem.Clear();
        swirlSystem.Play();
    }
    void RandomRotation()
    {
        int mult1 = UnityEngine.Random.Range(0, 1) == 0 ? -1 : 1;
        int mult2 = UnityEngine.Random.Range(0, 1) == 0 ? -1 : 1;
        distortionQuad.rotation = Quaternion.Euler(UnityEngine.Random.Range(40, 70) * mult1, UnityEngine.Random.Range(40, 70) * mult2, 0);
    }
    void Coloring()
    {
        //Clamping index
        schemeIndex = Mathf.Clamp(schemeIndex, 0, schemePool.Length - 1);

        //Sphere fresnel
        sphereBlock.SetColor("_MainColor", Color.white);
        sphereBlock.SetColor("_FresnelColor", (schemePool[schemeIndex].fresnel));
        sphereBlock.SetFloat("_FresnelStrength", 1.25f);
        sphereRenderer.SetPropertyBlock(sphereBlock);

        //White hole vs black hole twirl direction
        distortBlock.SetFloat("_Speed", Mathf.Abs(distortMat.GetFloat("_Speed")));
        distortRenderer.SetPropertyBlock(distortBlock);
        ParticleSystem.RotationOverLifetimeModule rotModule = swirlSystem.rotationOverLifetime;
        rotModule.z = new ParticleSystem.MinMaxCurve(systemRotSpeed);

        //Particle system colors
        ParticleSystem.MainModule module = swirlSystem.main;
        Gradient grad = new Gradient();
        grad.SetKeys(new GradientColorKey[] { new GradientColorKey(schemePool[schemeIndex].lightColor, 0.0f), new GradientColorKey(schemePool[schemeIndex].darkColor, 1.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) });
        module.startColor = grad;
    }
    void NullChecks()
    {
        if (sphereRenderer == null) { sphereRenderer = GetComponent<MeshRenderer>(); }
        if (distortRenderer == null) { distortRenderer = transform.GetChild(0).GetComponent<MeshRenderer>(); }
        if (distortMat == null) { distortMat = distortRenderer.sharedMaterial; }
        if (swirlSystem == null) { swirlSystem = GetComponentInChildren<ParticleSystem>(); }
        if (sphereBlock == null) { sphereBlock = new MaterialPropertyBlock(); }
        if (distortBlock == null) { distortBlock = new MaterialPropertyBlock(); }
        if (distortionQuad == null) { distortionQuad = transform.GetChild(0); }
        if (cMass == null) { cMass = GetComponent<CelestialMass>(); }
    }
    private void OnTriggerEnter(Collider collision)
    {
    }
}
