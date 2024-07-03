using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.SceneManagement;
using static Galaxy;
using static LevelGenerator;
using static LevelManager;

[ExecuteInEditMode]
public class GalaxyGenerator : MonoBehaviour
{
    [SerializeField] Galaxy.GalaxyScheme[] schemes;
    [SerializeField] int schemeIndex;
    [SerializeField] Vector3 galaxyRotation;
    [SerializeField] bool update = false;
    [Header("Procedural")]
    [SerializeField] bool proceduralGalaxy;
    [SerializeField] LevelGenerator.SpecialType[] specialTypes;
    [SerializeField] LevelManager.GalaxyParams galaxyParams;
    [Header("Particles")]
    [SerializeField] ParticleSystem outerSpiral;
    [SerializeField] ParticleSystem innerSpiral;
    [SerializeField] ParticleSystem nebulaSpiral1;
    [SerializeField] ParticleSystem nebulaSpiral2;
    [SerializeField] ParticleSystem coreGlow;
    [SerializeField] ParticleSystem coreFlicker;
    [SerializeField] ParticleSystem coreQuasar;
    [SerializeField] ParticleSystem coreTwinkle;
    [SerializeField] ParticleSystem swirlSystem;

    public delegate void InitProceduralSystem(string[] galaxyLevels, LevelManager.LevelParams[] galaxyLevelParams, GalaxyData theme);
    public static InitProceduralSystem initProceduralSystem;
    public delegate void InitSystem(string[] galaxyLevels, GalaxyData theme);
    public static InitSystem initSystem;

    // Start is called before the first frame update
    void Start()
    {
        InitParticleSystems();
        
    }
    void SetNormalGalaxy() //Sets the galaxy in level manager through event
    {
        initSystem?.Invoke(schemes[schemeIndex].levelScenes, schemes[schemeIndex].galaxyData);
    }
    void SetProceduralGalaxy()
    {
        LevelManager.LevelParams[] galaxyLevelParams = new LevelManager.LevelParams[5];
        string[] galaxyLevels = new string[5];
        for (int i = 0; i < 5; i++) //Creating levels as level params
        {
            UnityEngine.Random.InitState(i * 120523 + galaxyParams.seed);
            int levelSeed = UnityEngine.Random.Range(-2147483647, 2147483647);
            int levelDifficulty = Mathf.Clamp(galaxyParams.relativeDifficulty + UnityEngine.Random.Range(-1, 1), 1, 10);

            LevelManager.LevelParams systemParams = new LevelManager.LevelParams(levelSeed, levelDifficulty, galaxyParams.levelStatic.difficultyPars[levelDifficulty-1], galaxyParams.possibleTypes, specialTypes, galaxyParams.levelStatic);

            galaxyLevelParams[i] = systemParams;
            galaxyLevels[i] = "LevelGen";
        }
        initProceduralSystem?.Invoke(galaxyLevels, galaxyLevelParams, schemes[schemeIndex].galaxyData);
        LevelSettingsManager.onLevelSettingChange?.Invoke(galaxyLevelParams[0]);
        GameManager.LevelSettingsManager.UIMatchParams();
    }//Sets the procedural galaxy in level manager through event
    void LoadGalaxyScene() //Called by animation event (press animation)
    {
        string targetScene = schemes[schemeIndex].levelScenes[0];
        if (proceduralGalaxy) { SetProceduralGalaxy(); targetScene = "LevelGen"; }
        else { SetNormalGalaxy(); }
        if (!string.IsNullOrEmpty(targetScene))
        {
            SceneManager.LoadScene(targetScene);
        }
        else
        {
            Debug.LogWarning("LevelScene from '" + schemes[schemeIndex].name + "' galaxy struct references an invalid scene: '" + targetScene + "'.");
        }
    }
    void UpdateParticleSystemsEditor() //Called in both play and editor
    {
        outerSpiral.transform.eulerAngles = galaxyRotation; //Galaxy rotation
    }
    public void SetProceduralScheme(LevelManager.GalaxyParams galaxyParams)
    {
        proceduralGalaxy = true;
        for (int i = 0; i < schemes.Length; i++)
        {
            for (int x = 0; x < galaxyParams.possibleTypes.Length; x++)
            {
                if (schemes[i].type == galaxyParams.possibleTypes[x])
                {
                    schemeIndex = i;
                    InitParticleSystems();
                }
            }
        }
        this.galaxyParams = galaxyParams;
    }
    public Galaxy.GalaxyScheme SetScheme(ColorSchemeStaticReference.PlanetType type)
    {
        proceduralGalaxy = false;
        for (int i = 0; i < schemes.Length; i++)
        {
            if (schemes[i].type == type)
            {
                schemeIndex = i;
                InitParticleSystems();
                return schemes[schemeIndex];
            }
        }
        return schemes[schemeIndex];
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

        ParticleSystem.MainModule module = swirlSystem.main;
        Gradient grad = new Gradient();
        grad.SetKeys(new GradientColorKey[] { new GradientColorKey(schemes[schemeIndex].lightColor, 0.0f), new GradientColorKey(schemes[schemeIndex].darkColor, 1.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) });
        module.startColor = grad;
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
        swirlSystem.Clear();
        swirlSystem.Play();
    }
    // Update is called once per frame
    void Update()
    {
        if(update) { InitParticleSystems(); update = false; }
        UpdateParticleSystemsEditor();
    }
}
public static class Galaxy
{
    [System.Serializable]
    public struct GalaxyScheme
    {
        public string name;
        public string[] levelScenes;
        public GalaxyData galaxyData;
        public ColorSchemeStaticReference.PlanetType type;
        public Color primarySpiral, secondarySpiral;
        public Color coreGlow, coreQuasar;
        public Color primaryTransparent, secondaryTransparent; //Obsolete
        [ColorUsage(true, true)]
        public Color lightColor, darkColor;
    }

}

