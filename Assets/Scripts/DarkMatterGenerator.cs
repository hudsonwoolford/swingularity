using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode()]
public class DarkMatterGenerator : MonoBehaviour
{
    [SerializeField] PlanetDataHolder planetData;
    [Header("Local Variables")]
    [SerializeField] ParticleSystem darkParticles;
    [SerializeField] ParticleSystem glowParticles;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ApplyingVariables();
    }
    void ApplyingVariables()
    {
        Gradient grad = new Gradient();
        grad.SetKeys(new GradientColorKey[] { new GradientColorKey(planetData.forceFieldColor * 0.25f, 0.0f), new GradientColorKey(planetData.forceFieldColor * 1f, 1.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1) });

        ParticleSystem.MainModule module = darkParticles.main;
        module.startColor = grad;

    }
}
