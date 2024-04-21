using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode()]
public class SunGenerator : MonoBehaviour
{
    [ColorUsage(true,true)]
    [SerializeField] Color sunColor = Color.white, sunDarkColor = Color.white, atmosphereColor = Color.white;
    [SerializeField] float bigNoiseOffset;
    Material sunMaterial;
    ParticleSystem system;
    // Start is called before the first frame update
    void Start()
    {
        sunMaterial = transform.GetChild(0).GetComponent<Material>();
        system = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!sunMaterial) { sunMaterial = transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial; }
        if (system == null) { system = GetComponent<ParticleSystem>(); } 
        sunMaterial.SetColor("_Color1", sunDarkColor);
        sunMaterial.SetColor("_Color2", sunColor);
        sunMaterial.SetFloat("_BigNoise", bigNoiseOffset);

        ParticleSystem.ColorOverLifetimeModule colorModule = system.colorOverLifetime;
        Gradient grad = new Gradient();
        grad.SetKeys(new GradientColorKey[] { new GradientColorKey(atmosphereColor, 0.0f), new GradientColorKey(atmosphereColor, 1.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(0.0f, 0f), new GradientAlphaKey(1.0f, 0.15f), new GradientAlphaKey(1.0f, 0.85f), new GradientAlphaKey(0.0f, 1f) });
        colorModule.color = grad;
    }
}
