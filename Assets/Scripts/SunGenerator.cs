using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode()]
public class SunGenerator : MonoBehaviour
{
    [Serializable]
    struct StarColorScheme {
        public string name;
        public ColorSchemeStaticReference.PlanetType planetType;
        [ColorUsage(true, true)]
        public Color sunColor, sunDarkColor, atmosphereColor;
    }
    [SerializeField] StarColorScheme[] schemePool;
    [SerializeField] int starSchemeIndex;
    [SerializeField] float bigNoiseOffset;
    [SerializeField] GameObject collisionParticles;
    [SerializeField] AudioClip collisionClip;
    [SerializeField] bool autoMass = true;
    SphereCollider col;
    Rigidbody rb;
    AudioSource source;
    MeshRenderer meshRenderer, heatMeshRenderer;
    ParticleSystem system;
    // Start is called before the first frame update
    void Start()
    {
        NullChecks();
        rb.mass = transform.localScale.x * transform.localScale.x;
    }

    // Update is called once per frame
    void Update()
    {
        if (autoMass) { rb.mass = Mathf.Pow(transform.localScale.x, 2f) * 200; }


        //Null checks because this script works in edit mode
        NullChecks();

        //No index out of range errors
        starSchemeIndex = Mathf.Clamp(starSchemeIndex, 0, schemePool.Length - 1); 

        //Material property block for the sun sphere
        MaterialPropertyBlock sunBlock = new MaterialPropertyBlock();
        sunBlock.SetColor("_Color1", schemePool[starSchemeIndex].sunDarkColor);
        sunBlock.SetColor("_Color2", schemePool[starSchemeIndex].sunColor);
        sunBlock.SetFloat("_BigNoise", bigNoiseOffset);
        meshRenderer.SetPropertyBlock(sunBlock);

        //Material property block for heat distortion
        MaterialPropertyBlock heatBlock = new MaterialPropertyBlock();
        heatBlock.SetFloat("_Scale", transform.localScale.x * 0.45f);
        heatMeshRenderer.SetPropertyBlock(heatBlock);

        //Setting atmosphere particle colours
        Gradient grad = new Gradient();
        grad.SetKeys(new GradientColorKey[] { new GradientColorKey(schemePool[starSchemeIndex].atmosphereColor, 0.0f), new GradientColorKey(schemePool[starSchemeIndex].atmosphereColor, 1.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(0.0f, 0f), new GradientAlphaKey(1.0f, 0.15f), new GradientAlphaKey(1.0f, 0.85f), new GradientAlphaKey(0.0f, 1f) });
        ParticleSystem.ColorOverLifetimeModule colorModule = system.colorOverLifetime;
        colorModule.color = grad;
    }
    private void OnCollisionEnter(Collision c)
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
        bigNoiseOffset = UnityEngine.Random.Range(-10000f, 10000f);

        //Reset atmosphere particles
        system.Clear();
        system.Play();
    }
    /// <summary>
    /// Checks if variables are null and sets them correctly
    /// </summary>
    void NullChecks()
    {
        if (meshRenderer == null) { meshRenderer = GetComponent<MeshRenderer>(); }
        if (heatMeshRenderer == null) { heatMeshRenderer = transform.GetChild(0).GetComponent<MeshRenderer>(); }
        if (system == null) { system = GetComponentInChildren<ParticleSystem>(); }
        if (source == null) { source = GetComponent<AudioSource>(); }
        if (col == null) { col = GetComponent<SphereCollider>(); }
        if (rb == null) { rb = GetComponent<Rigidbody>(); }
    }
    /// <summary>
    /// Sets the particle colours based off of Sun Colours
    /// </summary>
    void SetParticleColours(ParticleSystem[] particleSystems)
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
