using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode()]
public class SunGenerator : MonoBehaviour
{
    [Serializable]
    protected struct StarColorScheme {
        public string name;
        public ColorSchemeStaticReference.PlanetType planetType;
        [ColorUsage(true, true)]
        public Color sunColor, sunDarkColor, atmosphereColor;
    }
    [SerializeField] protected StarColorScheme[] schemePool = new StarColorScheme[0];
    [SerializeField] protected int starSchemeIndex;
    [SerializeField] protected float bigNoiseOffset;
    [SerializeField] protected float lightRangeMult = 1, lightIntensityMult = 1;
    [SerializeField] protected GameObject collisionParticles;
    [SerializeField] protected AudioClip collisionClip;
    [SerializeField] protected bool autoMass = true;
    protected Light lightSource;
    protected SphereCollider col;
    protected CelestialMass cMass;
    protected AudioSource source;
    protected MeshRenderer meshRenderer;
    protected ParticleSystem system;
    void Awake(){
        if(schemePool.Length > 0)
        {
            starSchemeIndex = Mathf.Clamp(starSchemeIndex, 0, schemePool.Length - 1); 
        }
    }
    // Start is called before the first frame update
    protected virtual void Start()
    {
        NullChecks();
        if (autoMass) { cMass.mass = Mathf.Pow(transform.localScale.x, 2f) * 150; }
    }

    // Update is called once per frame
    protected virtual void FixedUpdate()
    {

        //Null checks because this script works in edit mode
        NullChecks();

        //No index out of range errors
        starSchemeIndex = Mathf.Clamp(starSchemeIndex, 0, schemePool.Length - 1); 
        
        //Lights
        lightSource.range = transform.localScale.x * lightRangeMult;
        lightSource.intensity = transform.localScale.x * lightIntensityMult;

        //Material property block for the sun sphere
        MaterialPropertyBlock sunBlock = new MaterialPropertyBlock();
        sunBlock.SetColor("_Color1", schemePool[starSchemeIndex].sunDarkColor);
        sunBlock.SetColor("_Color2", schemePool[starSchemeIndex].sunColor);
        sunBlock.SetFloat("_BigNoise", bigNoiseOffset);
        sunBlock.SetVector("_HeatOffset", new Vector3(bigNoiseOffset, bigNoiseOffset, bigNoiseOffset));
        meshRenderer.SetPropertyBlock(sunBlock);

        //Setting atmosphere particle colours
        Gradient grad = new Gradient();
        grad.SetKeys(new GradientColorKey[] { new GradientColorKey(schemePool[starSchemeIndex].atmosphereColor, 0.0f), new GradientColorKey(schemePool[starSchemeIndex].atmosphereColor, 1.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(0.0f, 0f), new GradientAlphaKey(1.0f, 0.15f), new GradientAlphaKey(1.0f, 0.85f), new GradientAlphaKey(0.0f, 1f) });
        ParticleSystem.ColorOverLifetimeModule colorModule = system.colorOverLifetime;
        colorModule.color = grad;
    }
    protected virtual void OnCollisionEnter(Collision c)
    {
        if(c.gameObject.layer == 7) { return; } //Simulation layer
        source.PlayOneShot(collisionClip, 0.6f);

        //Instantiates collision particles
        GameObject insta = Instantiate(collisionParticles, c.contacts[0].point + new Vector3(0,0,-5), Quaternion.Euler(new Vector3(-Mathf.Atan2(c.contacts[0].point.y - transform.position.y, c.contacts[0].point.x - transform.position.x) * Mathf.Rad2Deg, 90, -90)));
        SetParticleColours(insta.GetComponentsInChildren<ParticleSystem>());
        Destroy(insta, 1);
    }
    /// <summary>
    /// Randomly chooses fromt sun colour scheme array
    /// </summary>
    [ContextMenu("Random Sun")] //Can be acessed in the Unity Editor by a button. 3 dots next to the script and it's in the dropdown menu.
    void RandomSun()
    {
        //Prevent errors
        if(schemePool.Length == 0) { return; }

        //Choose a random sun colour scheme and big noise offset
        UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
        starSchemeIndex = UnityEngine.Random.Range(0, schemePool.Length);
        bigNoiseOffset = UnityEngine.Random.Range(-10000f, 10000f);

        //Reset atmosphere particles
        system.Clear();
        system.Play();
    }
    /// <summary>
    /// Randomly chooses fromt sun colour scheme array based on seed
    /// </summary>
    public void RandomSun(int seed, ColorSchemeStaticReference.PlanetType type)
    {
        //Prevent errors
        if (schemePool.Length == 0) { return; }
        NullChecks();

        //Choose a random sun colour scheme and big noise offset
        List<int> typeSchemeIndices = new List<int>();
        for (int i = 0; i < schemePool.Length; i++)
        {
            if (schemePool[i].planetType == type)
            {
                typeSchemeIndices.Add(i);
            }
        }
        if(typeSchemeIndices.Count == 0) { return; }
        UnityEngine.Random.InitState(seed);


        starSchemeIndex = typeSchemeIndices[UnityEngine.Random.Range(0, typeSchemeIndices.Count)];
        lightSource.color = schemePool[starSchemeIndex].sunColor;

        bigNoiseOffset = UnityEngine.Random.Range(-10000f, 10000f);

        //Reset atmosphere particles
        system.Clear();
        system.Play();
    }
    /// <summary>
    /// Checks if variables are null and sets them correctly
    /// </summary>
    protected virtual void NullChecks()
    {
        if (meshRenderer == null) { meshRenderer = GetComponent<MeshRenderer>(); }
        if (system == null) { system = GetComponentInChildren<ParticleSystem>(); }
        if (source == null) { source = GetComponent<AudioSource>(); }
        if (col == null) { col = GetComponent<SphereCollider>(); }
        if (cMass == null) { cMass = GetComponent<CelestialMass>(); }
        if (lightSource == null) { lightSource = GetComponent<Light>(); }
    }
    /// <summary>
    /// Sets the particle colours based off of Sun Colours
    /// </summary>
    protected virtual void SetParticleColours(ParticleSystem[] particleSystems)
    {
        Gradient normalGrad = new Gradient();
        normalGrad.SetKeys(new GradientColorKey[] { new GradientColorKey(schemePool[starSchemeIndex].sunColor, 0.33f), new GradientColorKey(schemePool[starSchemeIndex].sunDarkColor, 0.66f) }, new GradientAlphaKey[] { new GradientAlphaKey(0.0f, 0f), new GradientAlphaKey(1.0f, 0.5f), new GradientAlphaKey(0.0f, 1f) });
        Gradient intenseGrad = new Gradient();
        intenseGrad.SetKeys(new GradientColorKey[] { new GradientColorKey(schemePool[starSchemeIndex].sunColor * 1.2f, 0.33f), new GradientColorKey(schemePool[starSchemeIndex].sunDarkColor * 1.2f, 0.66f) }, new GradientAlphaKey[] { new GradientAlphaKey(0.0f, 0f), new GradientAlphaKey(1.0f, 0.5f), new GradientAlphaKey(0.0f, 1f) });
        ParticleSystem.ColorOverLifetimeModule[] modules = System.Array.ConvertAll<ParticleSystem, ParticleSystem.ColorOverLifetimeModule>(particleSystems, p => p.colorOverLifetime);
        modules[0].color = normalGrad;
        modules[1].color = intenseGrad;
        modules[2].color = intenseGrad;
    }
}
#if UNITY_EDITOR

#endif
