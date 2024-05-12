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
    [SerializeField] ParticleSystem circleParticles;
    [SerializeField] ParticleSystem smokeParticles;
    [SerializeField] ParticleSystem glowParticles;
    [SerializeField] ParticleSystem trailParticles;
    SphereCollider ffCollider;
    ParticleSystemRenderer circleRenderer;
    Color defaultCircleColor = Color.red;
    // Start is called before the first frame update
    void Start()
    {
        ApplyingVariables();
        glowParticles.Clear();
        circleParticles.Clear();
        smokeParticles.Clear();
        trailParticles.Clear();
        glowParticles.Play();
        circleParticles.Play();
        smokeParticles.Play();
        trailParticles.Play();
        circleRenderer = circleParticles.GetComponent<ParticleSystemRenderer>();
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

        ApplyingVariables();
        glowParticles.Clear();
        circleParticles.Clear();
        smokeParticles.Clear();
        trailParticles.Clear();
        glowParticles.Play();
        circleParticles.Play();
        smokeParticles.Play();
        trailParticles.Play();
    }
    void ApplyingVariables()
    {
        transform.position = planetData.transform.position;
        transform.localScale = planetData.transform.localScale * (1/planetNormalScale);
        if(ffCollider != null && ffCollider.material != goop) { ffCollider.material = goop; }

        PlanetColorScheme primaryOceanBiome = planetData.oBiomeB ? planetData.biomeB : planetData.biomeA;
        PlanetColorScheme primaryAtmosphereBiome = planetData.aBiomeB ? planetData.biomeB : planetData.biomeA;
        Gradient grad = new Gradient();
        //grad.SetKeys(new GradientColorKey[] { new GradientColorKey(Color.black, 0.0f), new GradientColorKey(Color.gray, 0.6f) }, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1) });
        grad.SetKeys(new GradientColorKey[] { new GradientColorKey(planetData.forceFieldColor, 0.0f), new GradientColorKey(planetData.forceFieldColor, 1.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1) });

        ParticleSystem.MainModule module = smokeParticles.main;
        module.startColor = grad;

        module = circleParticles.main;
        module.startColor = grad;

        module = glowParticles.main;
        module.startColor = grad;

        module = trailParticles.main;
        module.startColor = grad;
    }
    void NullChecks()
    {
        if (planetData == null) { return; }
        if (ffCollider == null && planetData.forceField != null) { ffCollider = planetData.forceField.gameObject.GetComponent<SphereCollider>(); }
        if (circleRenderer == null) { circleRenderer = circleParticles.GetComponent<ParticleSystemRenderer>(); }
        if(Application.isPlaying)
        {
            if (defaultCircleColor == Color.red) { defaultCircleColor = circleRenderer.material.GetColor("_Color"); }
        }
    }
    void HackModeColors(bool enable, Material whiteMat)
    {
        Color circleColor = enable ? Color.white : defaultCircleColor;
        circleRenderer.material.SetColor("_Color", circleColor);
    }
    private void OnEnable()
    {
        HackMode.onHackMode += HackModeColors;
    }
    private void OnDisable()
    {
        HackMode.onHackMode -= HackModeColors;
    }
}
