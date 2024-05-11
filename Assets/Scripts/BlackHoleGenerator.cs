using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BlackHoleGenerator : MonoBehaviour
{
    [Serializable]
    public struct SpaceHoleColorScheme
    {
        public ColorSchemeStaticReference.PlanetType planetType;
        public Color darkColor, lightColor;
        [ColorUsage(true, true)]
        public Color fresnel;
        //[Range(0f, 1f)]
        //public float swirlIntensity;
    }
    [SerializeField] bool whiteHole = false;
    [SerializeField] Transform correspondent;
    [SerializeField] SpaceHoleColorScheme[] schemePool;
    [SerializeField] int schemeIndex;
    [SerializeField] float systemRotSpeed = 8;

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
        if(schemePool.Length == 0) { return; }
        NullChecks();
        Coloring();
    }
    public void InitRandom(bool white, Transform corresponding, int seed, ColorSchemeStaticReference.PlanetType planetType)
    {
        NullChecks();
        whiteHole = white;
        correspondent = corresponding;
        UnityEngine.Random.InitState(seed);

        List<int> typeSchemeIndices = new List<int>();
        for (int i = 0; i < schemePool.Length; i++)
        {
            if (schemePool[i].planetType == planetType)
            {
                typeSchemeIndices.Add(i);
            }
        }
        if(typeSchemeIndices.Count == 0) { return; }
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
        distortionQuad.rotation = Quaternion.Euler(UnityEngine.Random.Range(30, 55) * mult1, UnityEngine.Random.Range(30, 55) * mult2, 0);
    }
    void Coloring()
    {
        cMass.mass = whiteHole ? Mathf.Abs(cMass.mass) * -1 : Mathf.Abs(cMass.mass); 
        //Clamping index
        schemeIndex = Mathf.Clamp(schemeIndex, 0, schemePool.Length-1);

        //Sphere fresnel
        Color mainColor = whiteHole ? Color.white : Color.black;
        sphereBlock.SetColor("_MainColor", mainColor);
        sphereBlock.SetColor("_FresnelColor", (schemePool[schemeIndex].fresnel));
        float fresStrength = whiteHole ? 1.25f : 2.5f;
        sphereBlock.SetFloat("_FresnelStrength", fresStrength);
        sphereRenderer.SetPropertyBlock(sphereBlock);

        //White hole vs black hole twirl direction
        int mult = whiteHole ? 1 : -1;
        distortMat.SetFloat("_CameraOrthoSize", Camera.main.orthographicSize);
        distortBlock.SetFloat("_Speed", Mathf.Abs(distortMat.GetFloat("_Speed")) * mult);
        distortRenderer.SetPropertyBlock(distortBlock);
        ParticleSystem.RotationOverLifetimeModule rotModule = swirlSystem.rotationOverLifetime;
        rotModule.z =  new ParticleSystem.MinMaxCurve(systemRotSpeed * mult);

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
        if(cMass == null) { cMass = GetComponent<CelestialMass>(); }
    }
    private void OnTriggerEnter(Collider collision)
    {
        if(correspondent == null) { return; }
        if (collision.gameObject.tag == "Player") //Player or simulated player
        {
            collision.attachedRigidbody.velocity *= -1;
            TrailRenderer trailRenderer = collision.gameObject.GetComponent<TrailRenderer>();

            collision.transform.position = correspondent.position + collision.attachedRigidbody.velocity.normalized * 1.5f;
            trailRenderer.Clear();
        }
        if(collision.gameObject.layer == 7){
            collision.attachedRigidbody.velocity *= -1;
            collision.attachedRigidbody.position = correspondent.position + collision.attachedRigidbody.velocity.normalized * 1.5f;
        }
    }
}
