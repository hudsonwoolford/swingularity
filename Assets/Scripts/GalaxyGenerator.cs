using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GalaxyGenerator : MonoBehaviour
{
    [System.Serializable]
    struct GalaxyScheme
    {
        public string name;
        public ColorSchemeStaticReference.PlanetType type;
        public Color primarySpiral, secondarySpiral;
        public Color coreGlow, coreQuasar;
        public Color primaryTransparent, secondaryTransparent;
    }
    [SerializeField] GalaxyScheme[] schemes;
    [SerializeField] int schemeIndex;
    [SerializeField] Vector3 galaxyRotation;
    [SerializeField] bool update = false;
    [Header("Particles")]
    [SerializeField] ParticleSystem outerSpiral;
    [SerializeField] ParticleSystem innerSpiral;
    [SerializeField] ParticleSystem nebulaSpiral1;
    [SerializeField] ParticleSystem nebulaSpiral2;
    [SerializeField] ParticleSystem coreGlow;
    [SerializeField] ParticleSystem coreFlicker;
    [SerializeField] ParticleSystem coreQuasar;
    [SerializeField] ParticleSystem coreTwinkle;

    // Start is called before the first frame update
    void Start()
    {
        InitParticleSystems();
    }
    void UpdateParticleSystemsEditor() //Called in both play and editor
    {
        outerSpiral.transform.eulerAngles = galaxyRotation; //Galaxy rotation
    }
    public string SetScheme(ColorSchemeStaticReference.PlanetType type)
    {
        for (int i = 0; i < schemes.Length; i++)
        {
            if (schemes[i].type == type)
            {
                schemeIndex = i;
                InitParticleSystems();
                return schemes[i].name;
            }
        }
        return "Invalid Type";
    }
    void InitParticleSystems() //Called in start to initialise colours
    {
        schemeIndex = Mathf.Clamp(schemeIndex, 0, schemes.Length - 1);
        //Gradients
        ParticleSystem.MinMaxGradient primaryGrad = new ParticleSystem.MinMaxGradient(schemes[schemeIndex].primarySpiral);
        ParticleSystem.MinMaxGradient secondaryGrad = new ParticleSystem.MinMaxGradient(schemes[schemeIndex].secondarySpiral);
        ParticleSystem.MinMaxGradient coreGrad = new ParticleSystem.MinMaxGradient(schemes[schemeIndex].coreGlow);
        ParticleSystem.MinMaxGradient coreQuasarGrad = new ParticleSystem.MinMaxGradient(schemes[schemeIndex].coreQuasar);
        ParticleSystem.MinMaxGradient pTransparentGrad = new ParticleSystem.MinMaxGradient(schemes[schemeIndex].primaryTransparent);
        ParticleSystem.MinMaxGradient sTransparentGrad = new ParticleSystem.MinMaxGradient(schemes[schemeIndex].secondaryTransparent);
        //Outer spiral
        ParticleSystem.MainModule main = outerSpiral.main;
        main.startColor = primaryGrad;
        //Inner spiral
        main = innerSpiral.main;
        main.startColor = secondaryGrad;
        //Nebula 1 spiral
        main = nebulaSpiral1.main;
        main.startColor = pTransparentGrad;
        //Nebula 2 spiral
        main = nebulaSpiral2.main;
        main.startColor = sTransparentGrad;
        //Core glow
        main = coreGlow.main;
        main.startColor = coreGrad;
        //Core flicker
        main = coreFlicker.main;
        main.startColor = primaryGrad;
        //Core quasar
        main = coreQuasar.main;
        main.startColor = coreQuasarGrad;
        //Core Twinkle
        main = coreTwinkle.main;
        main.startColor = primaryGrad;
        ResetParticles();
    }
    void ResetParticles() //Clears and plays all particle systems
    {
        outerSpiral.Clear();
        innerSpiral.Clear();
        nebulaSpiral1.Clear();
        nebulaSpiral2.Clear();
        coreFlicker.Clear();
        coreGlow.Clear();
        coreQuasar.Clear();
        coreTwinkle.Clear();
        outerSpiral.Play();
        innerSpiral.Play();
        nebulaSpiral1.Play();
        nebulaSpiral2.Play();
        coreFlicker.Play();
        coreGlow.Play();
        coreQuasar.Play();
        coreTwinkle.Play();
    }
    // Update is called once per frame
    void Update()
    {
        if(update) { InitParticleSystems(); update = false; }
        UpdateParticleSystemsEditor();
        //if(Application.isPlaying) { UpdateParticleSystems(); }
    }
}
