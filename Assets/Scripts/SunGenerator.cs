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
    SphereCollider col;
    AudioSource source;
    MeshRenderer meshRenderer, heatMeshRenderer;
    ParticleSystem system;
    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<SphereCollider>();
        source = GetComponent<AudioSource>();
        meshRenderer = GetComponent<MeshRenderer>();
        heatMeshRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
        system = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        starSchemeIndex = Mathf.Clamp(starSchemeIndex, 0, schemePool.Length - 1);
        if (meshRenderer == null) { meshRenderer = GetComponent<MeshRenderer>(); }
        if (heatMeshRenderer == null) { heatMeshRenderer = transform.GetChild(0).GetComponent<MeshRenderer>(); }
        if (system == null) { system = GetComponentInChildren<ParticleSystem>(); } 
        if (source == null) { source = GetComponent<AudioSource>();}
        if (col == null) { col = GetComponent<SphereCollider>(); }
        MaterialPropertyBlock sunBlock = new MaterialPropertyBlock();

        sunBlock.SetColor("_Color1", schemePool[starSchemeIndex].sunDarkColor);
        sunBlock.SetColor("_Color2", schemePool[starSchemeIndex].sunColor);
        sunBlock.SetFloat("_BigNoise", bigNoiseOffset);

        meshRenderer.SetPropertyBlock(sunBlock);
        MaterialPropertyBlock heatBlock = new MaterialPropertyBlock();

        heatBlock.SetFloat("_Scale", transform.localScale.x * 0.45f);

        heatMeshRenderer.SetPropertyBlock(heatBlock);


        ParticleSystem.ColorOverLifetimeModule colorModule = system.colorOverLifetime;
        Gradient grad = new Gradient();
        grad.SetKeys(new GradientColorKey[] { new GradientColorKey(schemePool[starSchemeIndex].atmosphereColor, 0.0f), new GradientColorKey(schemePool[starSchemeIndex].atmosphereColor, 1.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(0.0f, 0f), new GradientAlphaKey(1.0f, 0.15f), new GradientAlphaKey(1.0f, 0.85f), new GradientAlphaKey(0.0f, 1f) });
        colorModule.color = grad;
    }
    [ContextMenu("Random Sun")]
    void RandomSun()
    {
        if(schemePool.Length == 0) { return; }
        UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
        starSchemeIndex = UnityEngine.Random.Range(0, schemePool.Length);
        bigNoiseOffset = UnityEngine.Random.Range(-10000f, 10000f);
        system.Clear();
        system.Play();
    }
    private void OnCollisionEnter(Collision c)
    {
        if(c.gameObject.layer == 7) { return; }
        source.PlayOneShot(collisionClip, 0.6f);
        GameObject insta = Instantiate(collisionParticles, c.contacts[0].point + new Vector3(0,0,-5), Quaternion.Euler(new Vector3(-Mathf.Atan2(c.contacts[0].point.y - transform.position.y, c.contacts[0].point.x - transform.position.x) * Mathf.Rad2Deg, 90, -90)));
        SetParticleColours(insta.GetComponentsInChildren<ParticleSystem>());
        Destroy(insta, 4);
    }
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
