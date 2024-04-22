using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode()]
public class SunGenerator : MonoBehaviour
{
    [ColorUsage(true,true)]
    [SerializeField] Color sunColor = Color.white, sunDarkColor = Color.white, atmosphereColor = Color.white;
    [SerializeField] float bigNoiseOffset;
    [SerializeField] GameObject collisionParticles;
    [SerializeField] AudioClip collisionClip;
    AudioSource source;
    Material sunMaterial;
    ParticleSystem system;
    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
        sunMaterial = transform.GetChild(0).GetComponent<Material>();
        system = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!sunMaterial) { sunMaterial = transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial; }
        if (system == null) { system = GetComponent<ParticleSystem>(); } 
        if(source == null) { source = GetComponent<AudioSource>();}
        sunMaterial.SetColor("_Color1", sunDarkColor);
        sunMaterial.SetColor("_Color2", sunColor);
        sunMaterial.SetFloat("_BigNoise", bigNoiseOffset);

        ParticleSystem.ColorOverLifetimeModule colorModule = system.colorOverLifetime;
        Gradient grad = new Gradient();
        grad.SetKeys(new GradientColorKey[] { new GradientColorKey(atmosphereColor, 0.0f), new GradientColorKey(atmosphereColor, 1.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(0.0f, 0f), new GradientAlphaKey(1.0f, 0.15f), new GradientAlphaKey(1.0f, 0.85f), new GradientAlphaKey(0.0f, 1f) });
        colorModule.color = grad;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 7)
        {
            return;
        }
        source.PlayOneShot(collisionClip, 0.6f);
        GameObject insta = Instantiate(collisionParticles, other.transform.position, Quaternion.Euler(new Vector3(-Mathf.Atan2(other.transform.position.y - transform.position.y, other.transform.position.x - transform.position.x) * Mathf.Rad2Deg, 90, -90)));
        SetParticleColours(insta.GetComponentsInChildren<ParticleSystem>());
        Destroy(insta, 4);
    }
    void SetParticleColours(ParticleSystem[] particleSystems)
    {
        Gradient normalGrad = new Gradient();
        normalGrad.SetKeys(new GradientColorKey[] { new GradientColorKey(sunColor, 0.33f), new GradientColorKey(sunDarkColor, 0.66f) }, new GradientAlphaKey[] { new GradientAlphaKey(0.0f, 0f), new GradientAlphaKey(1.0f, 0.5f), new GradientAlphaKey(0.0f, 1f) });
        Gradient intenseGrad = new Gradient();
        intenseGrad.SetKeys(new GradientColorKey[] { new GradientColorKey(sunColor * 1.2f, 0.33f), new GradientColorKey(sunDarkColor * 1.2f, 0.66f) }, new GradientAlphaKey[] { new GradientAlphaKey(0.0f, 0f), new GradientAlphaKey(1.0f, 0.5f), new GradientAlphaKey(0.0f, 1f) });
        ParticleSystem.ColorOverLifetimeModule[] modules = System.Array.ConvertAll<ParticleSystem, ParticleSystem.ColorOverLifetimeModule>(particleSystems, p => p.colorOverLifetime);
        modules[0].color = normalGrad;
        modules[1].color = intenseGrad;
        modules[2].color = intenseGrad;
    }
}
