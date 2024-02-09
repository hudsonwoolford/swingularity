using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Reflection;

[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class FullPlanetGeneration : MonoBehaviour
{
    [Serializable]
    public struct ColorSchemePool
    {
        public string name;
        public bool oAPrimary;
        [Tooltip("This is optional")]
        public PlanetColorScheme primaryBiome;
        public PlanetColorScheme[] planetColorSchemes;
    }
    public ComputeShader calculateVertexHeight;
    [SerializeField] public FullPlanetObj planet;
    public Transform ocean;
    public Light innerAtmosphere, outerAtmosphere, frontAtmosphere;
    [HideInInspector]
    public bool colorSchemesOut;
    [SerializeField] TerrainVar[] terrainVarPool;
    [SerializeField] ColorSchemePool[] colorSchemes;
    [HideInInspector]
    [SerializeField] PlanetColorScheme biomeA, biomeB;
    [HideInInspector]
    [SerializeField] bool oABiomeA, oABiomeB;
    [HideInInspector]
    PlanetColorScheme biomeASaved, biomeBSaved;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        biomeASaved = ScriptableObject.CreateInstance(typeof(PlanetColorScheme)) as PlanetColorScheme;
        biomeBSaved = ScriptableObject.CreateInstance(typeof(PlanetColorScheme)) as PlanetColorScheme;
        
    }
    void Update()
    {
        
        if (meshRenderer.sharedMaterial!=planet.planetMaterial)
        {
            meshRenderer.sharedMaterial = planet.planetMaterial;
        }
        if(colorSchemesOut)
        {
            ApplyColorScheme();
        }
        UpdateAtmosphere();
        UpdateOcean();
        ApplyBiomeNoise();
        meshFilter.mesh = OctahedronSphereCreator.Create(planet.terrainVar, calculateVertexHeight);

    }   
    void UpdateOcean()
    {
        Material oceanMat = ocean.GetComponent<MeshRenderer>().sharedMaterial;
        Material mat = planet.planetMaterial;

        ocean.localScale = (planet.oceanVariables._SeaLevel + 0.01f) * Vector3.one * 2;

        oceanMat.color = planet.oceanVariables._OceanColor;
        oceanMat.SetFloat("_Metallic", planet.oceanVariables._OceanMetallic);
        oceanMat.SetFloat("_Glossiness", planet.oceanVariables._OceanSmoothness);

        mat.SetFloat("_SeaLevel", planet.oceanVariables._SeaLevel);
        mat.SetFloat("_OceanBlendEnd", planet.oceanVariables._OceanBlendEnd);
        mat.SetFloat("_OceanBlendIntensity", planet.oceanVariables._OceanBlendIntensity);
        mat.SetColor("_ShorelineColor", planet.oceanVariables._ShorelineColor);
        mat.SetColor("_SeabedColor", planet.oceanVariables._SeabedColor);
    }
    void UpdateAtmosphere()
    {
        innerAtmosphere.color = planet.atmosphereVariables._InnerBandColor;
        innerAtmosphere.range = planet.atmosphereVariables._InnerBandRange;
        innerAtmosphere.intensity = planet.atmosphereVariables._InnerBandIntensity;

        outerAtmosphere.color = planet.atmosphereVariables._OuterBandColor;
        outerAtmosphere.range = planet.atmosphereVariables._OuterBandRange;
        outerAtmosphere.intensity = planet.atmosphereVariables._OuterBandIntensity;

        frontAtmosphere.color = planet.atmosphereVariables._FrontColor;
        frontAtmosphere.range = planet.atmosphereVariables._FrontRange;
        frontAtmosphere.intensity = planet.atmosphereVariables._FrontIntensity;
    }
    public void ToggleOcean()
    {
        ocean.gameObject.SetActive(!ocean.gameObject.activeInHierarchy);
    }
    public void ToggleAtmosphere()
    {
        frontAtmosphere.gameObject.SetActive(!frontAtmosphere.gameObject.activeInHierarchy);
        outerAtmosphere.gameObject.SetActive(!outerAtmosphere.gameObject.activeInHierarchy);
        innerAtmosphere.gameObject.SetActive(!innerAtmosphere.gameObject.activeInHierarchy);
    }
    void ApplyBiomeNoise()
    { 
        Material mat = planet.planetMaterial;
        BiomeNoiseSettings bns = planet.terrainVar.biomeNoiseSettings;
        BiomeWarpNoiseSettings bwns = planet.terrainVar.biomeWarpNoiseSettings;

        //Normal noise
        mat.SetInt("_Octaves", bns._Octaves);
        mat.SetVector("_Offset", bns._Offset);
        mat.SetFloat("_Scale", bns._Scale);
        mat.SetFloat("_Persistence", bns._Persistence);
        mat.SetFloat("_Lacunarity", bns._Lacunarity);
        mat.SetFloat("_Multiplier", bns._Multiplier);
        mat.SetFloat("_Gain", bns._Gain);

        //Warp noise
        mat.SetInt("_WarpOctaves", bwns._WarpOctaves);
        mat.SetVector("_WarpOffset", bwns._WarpOffset);
        mat.SetFloat("_WarpScale", bwns._WarpScale);
        mat.SetFloat("_WarpPersistence", bwns._WarpPersistence);
        mat.SetFloat("_WarpLacunarity", bwns._WarpLacunarity);
        mat.SetFloat("_WarpMultiplier", bwns._WarpMultiplier);
        mat.SetFloat("_WarpGain", bwns._WarpGain);

    }
    public void RandomizeOffset()
    {
        UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
        float[] offset = { UnityEngine.Random.value * System.DateTime.Now.Millisecond, UnityEngine.Random.value * System.DateTime.Now.Millisecond, UnityEngine.Random.value * System.DateTime.Now.Millisecond };
        planet.terrainVar.generalSettings.seed = Mathf.RoundToInt(offset[1]);
        planet.terrainVar.warpedNoiseSettings.offset = offset;
        planet.terrainVar.continentNoiseSettings.offset = offset;
        planet.terrainVar.ridgeNoiseSettings.offset = offset;
        planet.terrainVar.detailNoiseSettings.offset = offset;
        planet.terrainVar.shapeNoiseSettings.offset = offset;
        planet.terrainVar.biomeNoiseSettings._Offset = new Vector3(offset[0], offset[1], offset[2]);
        planet.terrainVar.biomeWarpNoiseSettings._WarpOffset = new Vector3(offset[0], offset[1], offset[2]);
    }
    public void RandomizeShading()
    {
        Color[] colors = new Color[8];
        UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
        for (int i = 0; i < 8; i++)
        {
            Color randomColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
            colors[i] = randomColor;
        }

        Material mat = planet.planetMaterial;
        mat.SetColor("_FlatColorA", colors[0]);
        mat.SetColor("_FlatColorAVariant", colors[0]);
        mat.SetColor("_SteepColorA", colors[1]);
        mat.SetColor("_HighElevationColorA", colors[2]);
        mat.SetColor("_HighElevationSteepColorA", colors[3]);
        mat.SetColor("_FlatColorB", colors[4]);
        mat.SetColor("_FlatColorBVarient", colors[4]);
        mat.SetColor("_SteepColorB", colors[5]);
        mat.SetColor("_HighElevationColorB", colors[6]);
        mat.SetColor("_HighElevationSteepColorB", colors[7]);
        
    }
    public void RandomizeSmoothAndMetallic()
    {
        Material mat = planet.planetMaterial;
        float[] metAndSmooth = new float[4];
        UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
        for (int i = 0; i < 4; i++)
        {
            metAndSmooth[i] = UnityEngine.Random.value;

        }
        mat.SetFloat("_MetallicA", metAndSmooth[0]);
        mat.SetFloat("_MetallicB", metAndSmooth[1]);
        mat.SetFloat("_SmoothnessA", metAndSmooth[2]);
        mat.SetFloat("_SmoothnessB", metAndSmooth[3]);
    }
    public void RandomizeTerrainScheme()
    {
        if(terrainVarPool.Length == 0)
        {
            Debug.LogWarning("No terrain schemes in variable pool");
            return;
        }
        UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
        int index = UnityEngine.Random.Range(0, terrainVarPool.Length);
        planet.terrainVar = terrainVarPool[index];
    }
    public void RandomizeColorScheme()
    {
        UnityEngine.Random.InitState(System.DateTime.Now.Millisecond); //Change the random values returned
        int index = UnityEngine.Random.Range(0, colorSchemes.Length); //Index of the pool
        int index2= UnityEngine.Random.Range(0, colorSchemes[index].planetColorSchemes.Length); //Index of the first biome
        if(colorSchemes[index].primaryBiome!=null) //If a primary biome is used
        {
            //Randomizes which biome is primary
            ref PlanetColorScheme primaryBiomeBiome = ref biomeA;
            int choiceBiome = UnityEngine.Random.Range(0, 2);
            switch (choiceBiome)
            {
                case 0:
                    biomeA = colorSchemes[index].planetColorSchemes[index2];
                    biomeB = colorSchemes[index].primaryBiome;
                    primaryBiomeBiome = ref biomeB;
                    break;
                case 1:
                    biomeB = colorSchemes[index].planetColorSchemes[index2];
                    biomeA = colorSchemes[index].primaryBiome;
                    primaryBiomeBiome = ref biomeA;
                    break;
                default:
                    break;
            }
            if(colorSchemes[index].oAPrimary)
            {
                oABiomeA = primaryBiomeBiome == biomeA ? true : false;
                oABiomeB = primaryBiomeBiome == biomeB ? true : false;
            }
            else
            {
                oABiomeA = primaryBiomeBiome != biomeA ? true : false;
                oABiomeB = primaryBiomeBiome != biomeB ? true : false;
            }
            ApplyColorScheme();
            return;
        }
        UnityEngine.Random.InitState(System.DateTime.Now.Millisecond - 500); //Redo random values returned
        int index3 = UnityEngine.Random.Range(0, colorSchemes[index].planetColorSchemes.Length); //Index of the second biome
        while(index3==index2) //Make sure they are not the same as the first biome
        {
            UnityEngine.Random.InitState(Mathf.RoundToInt(System.DateTime.Now.Millisecond * UnityEngine.Random.value));
            index3 = UnityEngine.Random.Range(0, colorSchemes[index].planetColorSchemes.Length);
        }
        //Randomizes which biome is which
        int choice = UnityEngine.Random.Range(0, 2);
        switch (choice)
        {
            case 0:
                biomeA = colorSchemes[index].planetColorSchemes[index2];
                biomeB = colorSchemes[index].planetColorSchemes[index3];
                break;
            case 1:
                biomeB = colorSchemes[index].planetColorSchemes[index2];
                biomeA = colorSchemes[index].planetColorSchemes[index3];
                break;
            default:
                break;
        }
        ApplyColorScheme();
    }
    #region Color Schemes
    public void ApplyColorScheme()
    {
        Material mat = planet.planetMaterial;

        PlanetColorScheme biomeAScheme = biomeA;
        PlanetColorScheme biomeBScheme = biomeB;

        if (biomeA != null)
        {
            // Biome A color scheme
            mat.SetColor("_FlatColorA", biomeAScheme._FlatColor);
            mat.SetColor("_FlatColorAVariant", biomeAScheme._FlatColorVariant);
            mat.SetColor("_SteepColorA", biomeAScheme._SteepColor);
            mat.SetColor("_HighElevationColorA", biomeAScheme._HighElevationColor);
            mat.SetColor("_HighElevationSteepColorA", biomeAScheme._HighElevationSteepColor);
            mat.SetFloat("_MetallicA", biomeAScheme._Metallic);
            mat.SetFloat("_SmoothnessA", biomeAScheme._Smoothness);
        }
        if(biomeB != null)
        {
            // Biome B color scheme
            mat.SetColor("_FlatColorB", biomeBScheme._FlatColor);
            mat.SetColor("_FlatColorBVariant", biomeBScheme._FlatColorVariant);
            mat.SetColor("_SteepColorB", biomeBScheme._SteepColor);
            mat.SetColor("_HighElevationColorB", biomeBScheme._HighElevationColor);
            mat.SetColor("_HighElevationSteepColorB", biomeBScheme._HighElevationSteepColor);
            mat.SetFloat("_MetallicB", biomeBScheme._Metallic);
            mat.SetFloat("_SmoothnessB", biomeBScheme._Smoothness);
        }
        if(oABiomeA)
        {
            planet.oceanVariables = biomeA.oceanVariables;
            planet.atmosphereVariables = biomeA.atmosphereVariables;
        }
        else if(oABiomeB)
        {
            planet.oceanVariables = biomeB.oceanVariables;
            planet.atmosphereVariables = biomeB.atmosphereVariables;
        }
    }
    public void SaveColorScheme()
    {
        Material mat = planet.planetMaterial;
        biomeASaved._FlatColor = mat.GetColor("_FlatColorA");
        biomeASaved._FlatColorVariant = mat.GetColor("_FlatColorAVariant");
        biomeASaved._SteepColor = mat.GetColor("_SteepColorA");
        biomeASaved._HighElevationColor = mat.GetColor("_HighElevationColorA");
        biomeASaved._HighElevationSteepColor = mat.GetColor("_HighElevationSteepColorA");
        biomeASaved._Metallic = mat.GetFloat("_MetallicA");
        biomeASaved._Smoothness = mat.GetFloat("_SmoothnessA");

        biomeBSaved._FlatColor = mat.GetColor("_FlatColorB");
        biomeBSaved._FlatColorVariant = mat.GetColor("_FlatColorBVariant");
        biomeBSaved._SteepColor = mat.GetColor("_SteepColorB");
        biomeBSaved._HighElevationColor = mat.GetColor("_HighElevationColorB");
        biomeBSaved._HighElevationSteepColor = mat.GetColor("_HighElevationSteepColorB");
        biomeBSaved._Metallic = mat.GetFloat("_MetallicB");
        biomeBSaved._Smoothness = mat.GetFloat("_SmoothnessB");
        
        biomeASaved.oceanVariables = planet.oceanVariables;
        biomeASaved.atmosphereVariables = planet.atmosphereVariables;
        biomeBSaved.oceanVariables = planet.oceanVariables;
    }
    public void RestoreSavedColorScheme(bool restoreA, bool restoreB)
    {
        Material mat = planet.planetMaterial;

        if(biomeASaved != null && restoreA)
        {
            mat.SetColor("_FlatColorA", biomeASaved._FlatColor);
            mat.SetColor("_FlatColorAVariant", biomeASaved._FlatColorVariant);
            mat.SetColor("_SteepColorA", biomeASaved._SteepColor);
            mat.SetColor("_HighElevationColorA", biomeASaved._HighElevationColor);
            mat.SetColor("_HighElevationSteepColorA", biomeASaved._HighElevationSteepColor);
            mat.SetFloat("_MetallicA", biomeASaved._Metallic);
            mat.SetFloat("_SmoothnessA", biomeASaved._Smoothness);
        }
        if(biomeBSaved!=null && restoreB)
        {
            mat.SetColor("_FlatColorB", biomeBSaved._FlatColor);
            mat.SetColor("_FlatColorBVariant", biomeBSaved._FlatColorVariant);
            mat.SetColor("_SteepColorB", biomeBSaved._SteepColor);
            mat.SetColor("_HighElevationColorB", biomeBSaved._HighElevationColor);
            mat.SetColor("_HighElevationSteepColorB", biomeBSaved._HighElevationSteepColor);
            mat.SetFloat("_MetallicB", biomeBSaved._Metallic);
            mat.SetFloat("_SmoothnessB", biomeBSaved._Smoothness);
        }
        planet.oceanVariables = biomeASaved.oceanVariables;
        planet.atmosphereVariables = biomeASaved.atmosphereVariables;
    }
    #endregion
    #region Editor Tools
    [MenuItem("ProceduralTools/Create Planet Prefab")]
    static void CreatePrefab()
    {
        GameObject selectedObj = Selection.activeGameObject;

        if (selectedObj == null || selectedObj.GetComponent<FullPlanetGeneration>() == null)
        {
            Debug.LogWarning("Please select a valid game object");
            return;
        }

        GameObject newObj = Instantiate(selectedObj, selectedObj.transform.position, selectedObj.transform.rotation);
        GameObject newOceanObj = newObj.transform.GetChild(0).gameObject;

        DestroyImmediate(newObj.GetComponent<FullPlanetGeneration>());
        newObj.SetActive(true);

        MeshFilter meshFilter = selectedObj.GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = selectedObj.GetComponent<MeshRenderer>();
        MeshRenderer oceanRenderer = newOceanObj.GetComponent<MeshRenderer>();

        Mesh mesh = meshFilter.sharedMesh;
        Material material = meshRenderer.sharedMaterial;
        Material oceanMaterial = oceanRenderer.sharedMaterial;

        Material copyMat = new Material(material.shader);
        Material copyOceanMat = new Material(oceanMaterial.shader);
        Mesh copyMesh = mesh;

        copyMat.CopyMatchingPropertiesFromMaterial(material);
        copyOceanMat.CopyMatchingPropertiesFromMaterial(oceanMaterial);

        MeshFilter newMeshFilter = newObj.GetComponent<MeshFilter>();
        MeshRenderer newMeshRenderer = newObj.GetComponent<MeshRenderer>();

        newMeshFilter.mesh = copyMesh;
        newMeshRenderer.material = copyOceanMat;
        oceanRenderer.material = copyOceanMat;

        string prefabPath = EditorUtility.SaveFilePanel("Save Prefab", "Assets/", "BakedPlanet", "prefab");

        if (prefabPath.Length == 0)
        {
            DestroyImmediate(newObj);
            return;
        }

        prefabPath = prefabPath.Substring(prefabPath.IndexOf("Assets/"));
        PrefabUtility.SaveAsPrefabAsset(newObj, prefabPath);

        AssetDatabase.AddObjectToAsset(copyMat, prefabPath);
        AssetDatabase.AddObjectToAsset(copyOceanMat, prefabPath);
        AssetDatabase.AddObjectToAsset(copyMesh, prefabPath);
        AssetDatabase.SaveAssets();

        DestroyImmediate(newObj);
    }
    [MenuItem("ProceduralTools/Wipe Planet Asset")]
    static void WipePlanetAssets()
    {
        GameObject selectedObj = Selection.activeGameObject;

        FullPlanetGeneration fpg;
        if(!selectedObj.TryGetComponent<FullPlanetGeneration>(out fpg))
        {
            Debug.LogWarning("Please choose a valid gameobject");
            return;
        }

        string planetObjPath = EditorUtility.OpenFilePanel("Select Planet", "Assets/Procedural Planets/DonePlanets", "asset");
        if(planetObjPath.Length==0)
        {
            print("File Selection Cancelled");
            return;
        }

        planetObjPath = planetObjPath.Substring(planetObjPath.IndexOf("Assets/"));

        FullPlanetObj fullPlanetObj = AssetDatabase.LoadAssetAtPath<FullPlanetObj>(planetObjPath);

        if(fullPlanetObj == fpg.planet) 
        {
            Debug.LogWarning("Sorry, please unselect the PlanetObj from the planet generation script");
            return;
        }

        Material planetMat = fullPlanetObj.planetMaterial;
        TerrainVar terrainVar = fullPlanetObj.terrainVar;

        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(planetMat));
        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(terrainVar));
        AssetDatabase.DeleteAsset(planetObjPath);


    }

    public class PlanetNameWindow : EditorWindow
    {
        string planetName = "CHANGETHIS";
        private void OnGUI()
        {
            planetName = GUILayout.TextField(planetName);
            if (GUILayout.Button("CreatePlanet"))
            {
                CreatePlanet(planetName);
                Close();
            }
        }
        [MenuItem("ProceduralTools/Create New Planet")]
        static void ShowWindow()
        {
            PlanetNameWindow window = ScriptableObject.CreateInstance<PlanetNameWindow>();
            window.name = "Create New Planet";
            window.maxSize = new Vector2(250, 100);
            window.ShowUtility();
        }
        void CreatePlanet(string planetName)
        {
            GameObject selectedObj = Selection.activeGameObject;

            FullPlanetGeneration fpg;
            if (selectedObj == null || !selectedObj.TryGetComponent<FullPlanetGeneration>(out fpg))
            {
                Debug.LogWarning("Please select a valid game object");
                return;
            }

            FullPlanetObj fullPlanetObj = ScriptableObject.CreateInstance<FullPlanetObj>();
            TerrainVar terrainVar = ScriptableObject.CreateInstance<TerrainVar>();
            Material planetMat = new Material(Shader.Find("Custom/FinalTerrestrial"));

            fullPlanetObj.terrainVar = terrainVar;
            fullPlanetObj.planetMaterial = planetMat;

            AssetDatabase.CreateAsset(fullPlanetObj, "Assets/Procedural Planets/DonePlanets/" + planetName + ".asset");
            AssetDatabase.CreateAsset(planetMat, "Assets/Procedural Planets/DonePlanets/Terrain&Materials/" + planetName + "Mat.mat");
            AssetDatabase.CreateAsset(terrainVar, "Assets/Procedural Planets/DonePlanets/Terrain&Materials/" + planetName + "TerrainVar.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            fpg.planet = fullPlanetObj;
        }
    }
}
#endregion

