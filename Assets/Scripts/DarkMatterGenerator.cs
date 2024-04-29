using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode()]
public class DarkMatterGenerator : MonoBehaviour
{
    [SerializeField] PlanetDataHolder planetData;
    [Header("Local Variables")]
    [SerializeField] float planetNormalScale = 0.61f;
    [Space()]
    [SerializeField] PhysicMaterial goop;
    [Space()]
    [SerializeField] ParticleSystem darkParticles;
    [SerializeField] ParticleSystem glowParticles;
    SphereCollider ffCollider;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        NullChecks();
        if(planetData == null){return;}

        ApplyingVariables();
    }
    public void Init(PlanetDataHolder pData)
    {
        planetData = pData;
        NullChecks();

        darkParticles.Clear();
        glowParticles.Clear();
        ApplyingVariables();
        darkParticles.Play();
        glowParticles.Play();
    }
    void ApplyingVariables()
    {
        transform.position = planetData.transform.position;
        transform.localScale = planetData.transform.localScale * (1/planetNormalScale);
        if(ffCollider != null && ffCollider.material != goop) { ffCollider.material = goop; }

        PlanetColorScheme primaryOceanBiome = planetData.oBiomeB ? planetData.biomeB : planetData.biomeA;
        PlanetColorScheme primaryAtmosphereBiome = planetData.aBiomeB ? planetData.biomeB : planetData.biomeA;
        Gradient grad = new Gradient();
        grad.SetKeys(new GradientColorKey[] { new GradientColorKey(primaryAtmosphereBiome.atmosphereVariables._OuterBandColor * 0.5f, 0.0f), new GradientColorKey(primaryOceanBiome.oceanVariables._OceanColor * 0.5f, 0.6f) }, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1) });

        ParticleSystem.MainModule module = darkParticles.main;
        module.startColor = grad;

        module = glowParticles.main;
        grad.SetKeys(new GradientColorKey[] { new GradientColorKey(planetData.forceFieldColor, 0.0f), new GradientColorKey(planetData.forceFieldColor, 1.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1) });
        module.startColor = grad;
    }
    void NullChecks()
    {
        if (planetData == null) { return; }
        if (ffCollider == null && planetData.forceField != null) { ffCollider = planetData.forceField.gameObject.GetComponent<SphereCollider>(); }
    }
}
