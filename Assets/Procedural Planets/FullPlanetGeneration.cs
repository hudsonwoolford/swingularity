using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class FullPlanetGeneration : MonoBehaviour
{
    public ComputeShader calculateVertexHeight; //compute shader for planet generation
    public ComputeShader calculateBiomeVertices;//compute shader for tree vertices
    public Material pBlockMat;
    [SerializeField] public FullPlanetObj planet; //planet scriptable object
    public Transform ocean; //ocean object transform
    public Light innerAtmosphere, outerAtmosphere, frontAtmosphere; //atmosphere lights
    [HideInInspector]
    public bool colorSchemesOut; //if colorschemes are folded out/on
    [SerializeField] TerrainVar[] terrainVarPool; //terrain variables pool
    [HideInInspector]
    [SerializeField] PlanetColorScheme biomeA, biomeB, biomeP; //biome A and biome B for color schemes + polar biome
    [HideInInspector]
    [SerializeField] bool oBiomeA, oBiomeB, aBiomeA, aBiomeB; //booleans determining which biome/colorscheme has control over the atmosphere or the ocean or trees
    [HideInInspector]
    PlanetColorScheme biomeASaved, biomeBSaved; //cache for biome schemes
    MeshRenderer meshRenderer; //the mesh renderer attatched to this object
    MeshFilter meshFilter; //the mesh filter attached to this object
    List<GameObject> treesA = new List<GameObject>(); //list of instantiated trees
    List<GameObject> treesB = new List<GameObject>();
    List<GameObject> treesP = new List<GameObject>();
    private void Awake()
    {
        //initialisation
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        biomeASaved = ScriptableObject.CreateInstance(typeof(PlanetColorScheme)) as PlanetColorScheme;
        biomeBSaved = ScriptableObject.CreateInstance(typeof(PlanetColorScheme)) as PlanetColorScheme;
        
    }
    void Update()
    {
        if (meshRenderer.sharedMaterial != planet.planetMaterial) //updating material depending on planet
        {
            meshRenderer.sharedMaterial = planet.planetMaterial;
        }
        if (colorSchemesOut) //if color schemes are on it will always apply them
        {
            ApplyColorScheme();
        }
        UpdateAtmosphere(); //updating the atmosphere from the atmosphere variables
        UpdateOcean(); //updating the ocean from the ocean variables
        UpdateTrees();
        ApplyBiomeNoise(); //transferring biome noise from terrain var to the material
        meshFilter.mesh = OctahedronSphereCreator.Create(planet.terrainVar, calculateVertexHeight); //creating the planet
    }
    void UpdateOcean()
    {
        if(!planet.terrainVar.generalSettings.ocean)
        {
            ocean.gameObject.SetActive(false);
        }
        else if(!ocean.gameObject.activeInHierarchy)
        {
            ocean.gameObject.SetActive(true);
        }
        Material oceanMat = ocean.GetComponent<MeshRenderer>().sharedMaterial; //caching ocean material
        Material mat = planet.planetMaterial; //caching planet material

        ocean.localScale = (planet.terrainVar.generalSettings._SeaLevel + 0.01f) * Vector3.one * 2; //ocean scale depending on sea level

        //Setting variables inside the ocean material and planet material
        oceanMat.color = planet.oceanVariables._OceanColor; //color
        oceanMat.SetFloat("_Metallic", planet.oceanVariables._OceanMetallic); //metallic
        oceanMat.SetFloat("_Glossiness", planet.oceanVariables._OceanSmoothness); //smoothness

        int oceanToggle = planet.terrainVar.generalSettings.ocean ? 1 : 0;
        mat.SetFloat("_Ocean", oceanToggle); //ocean is enabled or not
        mat.SetFloat("_SeaLevel", planet.terrainVar.generalSettings._SeaLevel); //sea level
        mat.SetFloat("_OceanBlendEnd", planet.oceanVariables._OceanBlendEnd); //ocean blending between shoreline and seabed
        mat.SetFloat("_OceanBlendIntensity", planet.oceanVariables._OceanBlendIntensity); //ocean blending intensity between shoreline and seabed
        mat.SetFloat("_BeachBlendEnd", planet.oceanVariables._BeachBlendEnd); //ocean blending between shoreline and seabed
        mat.SetFloat("_BeachBlendIntensity", planet.oceanVariables._BeachBlendIntensity); //ocean blending intensity between shoreline and seabed
        mat.SetColor("_SeabedColor", planet.oceanVariables._SeabedColor); //seabed color
    }
    void UpdateAtmosphere()
    {
        Material mat = planet.planetMaterial; //caching planet material
        //inner atmosphere variable setting
        innerAtmosphere.color = planet.atmosphereVariables._InnerBandColor;
        innerAtmosphere.range = planet.atmosphereVariables._InnerBandRange;
        innerAtmosphere.intensity = planet.atmosphereVariables._InnerBandIntensity;
        //outer atmosphere variable setting
        outerAtmosphere.color = planet.atmosphereVariables._OuterBandColor;
        outerAtmosphere.range = planet.atmosphereVariables._OuterBandRange;
        outerAtmosphere.intensity = planet.atmosphereVariables._OuterBandIntensity;
        //front atmosphere variable setting
        frontAtmosphere.color = planet.atmosphereVariables._FrontColor;
        frontAtmosphere.range = planet.atmosphereVariables._FrontRange;
        frontAtmosphere.intensity = planet.atmosphereVariables._FrontIntensity;
        float fresnelToggle = planet.fresnelVariables._UseFresnel ? 1 : 0;

        mat.SetFloat("_UseFresnel", fresnelToggle);
        mat.SetColor("_FresnelColor", planet.fresnelVariables._FresnelColor);
        mat.SetFloat("_FresnelStrengthNear", planet.fresnelVariables._FresnelStrengthNear);
        mat.SetFloat("_FresnelStrengthFar", planet.fresnelVariables._FresnelStrengthFar);
        mat.SetFloat("_FresnelPower", planet.fresnelVariables._FresnelPower);
    }
    void UpdateTrees(){
        if(!biomeA.treeVariables._UseTrees && !biomeB.treeVariables._UseTrees) { ClearTrees(treesA); ClearTrees(treesB); return; } //if no biomes have trees return fucntion
        if(biomeA.treeVariables._TreeObjects == null && biomeB.treeVariables._TreeObjects == null) { return; }
        TreeObject[] treeObjects = biomeA.treeVariables._TreeObjects;
        if(biomeB.treeVariables._UseTrees && biomeB.treeVariables._TreeObjects != null){
            treeObjects = biomeA.treeVariables._TreeObjects.Concat(biomeB.treeVariables._TreeObjects).ToArray();
        }

        foreach (var obj in treeObjects)
        {
            if (obj == null) { return; } //if the prefab for trees are null return function
        }

        #region TreeShader
        BiomeNoiseSettings bns = planet.terrainVar.biomeNoiseSettings; //cache simple biome noise settings
        BiomeWarpNoiseSettings bwns = planet.terrainVar.biomeWarpNoiseSettings; //cache warped biome noise settings
        #endregion
        #region ComputeShader
        ComputeShader com = calculateBiomeVertices; //cache shader
        //General var
        com.SetFloat("_SeaLevel", planet.terrainVar.generalSettings._SeaLevel); 
        com.SetVector("_PlanetCenter", ocean.transform.position);

        //Normal noise transfer
        com.SetInt("_Octaves", bns._Octaves);
        com.SetVector("_Offset", bns._Offset);
        com.SetFloat("_Scale", bns._Scale);
        com.SetFloat("_Persistence", bns._Persistence);
        com.SetFloat("_Lacunarity", bns._Lacunarity);
        com.SetFloat("_Multiplier", bns._Multiplier);
        com.SetFloat("_Gain", bns._Gain);

        //Warp noise transfer
        com.SetInt("_WarpOctaves", bwns._WarpOctaves);
        com.SetVector("_WarpOffset", bwns._WarpOffset);
        com.SetFloat("_WarpScale", bwns._WarpScale);
        com.SetFloat("_WarpPersistence", bwns._WarpPersistence);
        com.SetFloat("_WarpLacunarity", bwns._WarpLacunarity);
        com.SetFloat("_WarpMultiplier", bwns._WarpMultiplier);
        com.SetFloat("_WarpGain", bwns._WarpGain);

        //Set global variables
        com.SetFloat("_BiomeA", planet.planetMaterial.GetFloat("_BiomeA"));
        com.SetFloat("_BiomeB", planet.planetMaterial.GetFloat("_BiomeB"));
        com.SetFloat("_BiomeAEnd", planet.planetMaterial.GetFloat("_BiomeAEnd"));
        com.SetFloat("_BiomeBStart", planet.planetMaterial.GetFloat("_BiomeBStart"));
        com.SetFloat("_SwapBiome", planet.planetMaterial.GetFloat("_SwapBiome"));
        com.SetFloat("_PolarHeight", planet.planetMaterial.GetFloat("_PolarHeight"));
        com.SetFloat("_PolarBlendIntensity", planet.planetMaterial.GetFloat("_PolarBlendIntensity"));
        com.SetFloat("_Polar", planet.planetMaterial.GetFloat("_Polar"));

        com.SetFloat("_BiomeAVegetationLine", planet.planetMaterial.GetFloat("_VegetationLineA"));
        com.SetFloat("_BiomeBVegetationLine", planet.planetMaterial.GetFloat("_VegetationLineB"));
        com.SetFloat("_BiomePVegetationLine", planet.planetMaterial.GetFloat("_VegetationLineP"));

        Vector3[] verts = meshFilter.sharedMesh.vertices; //all vertices on planet
        com.SetFloat("_NumVertices", verts.Length);

        ComputeBuffer vertBuffer = new ComputeBuffer(verts.Length, sizeof(float) * 3); //vertex buffer
        ComputeBuffer aBuffer    = new ComputeBuffer(verts.Length, sizeof(float) * 3, ComputeBufferType.Append); //biome a vertex buffer
        ComputeBuffer bBuffer    = new ComputeBuffer(verts.Length, sizeof(float) * 3, ComputeBufferType.Append); //biome b vertex buffer
        ComputeBuffer pBuffer    = new ComputeBuffer(verts.Length, sizeof(float) * 3, ComputeBufferType.Append); //biome b vertex buffer
        aBuffer.SetCounterValue(0); //resetting counter values
        bBuffer.SetCounterValue(0); //resetting counter values
        pBuffer.SetCounterValue(0); //resetting counter values

        vertBuffer.SetData(verts); //setting data in vertex buffer

        //setting compute shader buffers
        calculateBiomeVertices.SetBuffer(0, "planetVertices", vertBuffer);
        calculateBiomeVertices.SetBuffer(0, "biomeAVertices", aBuffer);    
        calculateBiomeVertices.SetBuffer(0, "biomeBVertices", bBuffer);
        calculateBiomeVertices.SetBuffer(0, "biomePVertices", pBuffer);

        PlanetaryGenerator.RunComputeShader(calculateBiomeVertices, vertBuffer.count); //run the shader

        // Create an indirect arguments buffer for ComputeBuffer.CopyCount
        ComputeBuffer indirectArgumentsBuffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
        // Get the count of elements appended to the append buffers
        int[] args = new int[] { 0, 1, 0, 0 };

        ComputeBuffer.CopyCount(aBuffer, indirectArgumentsBuffer, 0); //length of biome a buffer copied
        ComputeBuffer.CopyCount(bBuffer, indirectArgumentsBuffer, sizeof(int)); //length of biome b buffer copied
        ComputeBuffer.CopyCount(pBuffer, indirectArgumentsBuffer, sizeof(int)*2); //length of biome b buffer copied
        ComputeBuffer.CopyCount(vertBuffer, indirectArgumentsBuffer, sizeof(int)*3); //length of vertex buffer copied (for debugging)
        indirectArgumentsBuffer.GetData(args); //biome a length --> args[0], biome b length --> args[1], vertex length --> args[2]
        //counts
        int numAVertices = args[0];
        int numBVertices = args[1];
        int numPVertices = args[2];
        int numVertices = args[3];

        //biome vertices arrays
        Vector3[] biomeAVerts = new Vector3[numAVertices];
        Vector3[] biomeBVerts = new Vector3[numBVertices];
        Vector3[] biomePVerts = new Vector3[numPVertices];

        //write data from buffers to array
        aBuffer.GetData(biomeAVerts); 
        bBuffer.GetData(biomeBVerts);
        pBuffer.GetData(biomePVerts);

        //release buffers
        aBuffer.Release();
        bBuffer.Release();
        pBuffer.Release();
        vertBuffer.Release();
        indirectArgumentsBuffer.Release();
        #endregion

        if (biomeA.treeVariables._UseTrees) CreateAndDestroyTrees(ref treesA, ref biomeA, biomeAVerts);
        if (biomeB.treeVariables._UseTrees) CreateAndDestroyTrees(ref treesB, ref biomeB, biomeBVerts);
        if (biomeP != null && biomeP.treeVariables._UseTrees) CreateAndDestroyTrees(ref treesP, ref biomeP, biomePVerts);
      
    }
    void CreateAndDestroyTrees(ref List<GameObject> trees, ref PlanetColorScheme biome, Vector3[] vertPool){
        if (biome.treeVariables._TreeObjects == null || biome.treeVariables._TreeObjects.Length == 0) { return; }
        foreach (var obj in biome.treeVariables._TreeObjects)
        {
            if (obj == null) { return; } //if the prefab for trees are null return function
        }

        while (trees.Count > biome.treeVariables._NumTrees) //loop destroys tree one at a time
        {
            GameObject treeToDestroy = trees[trees.Count - 1];
            trees.Remove(treeToDestroy);
            DestroyImmediate(treeToDestroy);
        }
        while(trees.Count < biome.treeVariables._NumTrees) //loop add trees one at a time
        {
            int objIndex = Random.Range(0, biome.treeVariables._TreeObjects.Length); //random prefab from tree
            TreeObject selectedTreeObj = biome.treeVariables._TreeObjects[objIndex];
            GameObject tree = PrefabUtility.InstantiatePrefab(selectedTreeObj.treePrefab) as GameObject;

            Random.InitState(biome.treeVariables._NumTrees + trees.Count); //setting random seed

            int chosenVert = UnityEngine.Random.Range(0, vertPool.Length); //random vertex from pool
            Vector3 vertexElevationDir = vertPool[chosenVert ] - ocean.position; //vertex normal
            tree.transform.position = vertPool[chosenVert]; //set tree position to vertex
            tree.transform.rotation = Quaternion.FromToRotation(Vector3.up, vertexElevationDir); //tree normal = vertex normal
            tree.transform.GetChild(0).Rotate(0,0,Random.Range(0, 359)); //tree normal = vertex normal
            ; //tree normal = vertex normal
            tree.transform.parent = transform; //parent tree to gameObject

            MeshRenderer treeMeshRenderer = tree.GetComponentInChildren<MeshRenderer>(); // Renderer on tree object
            Material[] rendererMaterials = treeMeshRenderer.sharedMaterials; // All materials on tree array

            // Apply the MaterialPropertyBlock to each material in the renderer's materials array
            for (int i = 0; i < rendererMaterials.Length; i++)
            {
                MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

                if (selectedTreeObj.dynamicMaterial[i])
                {
                    // Set color property on the MaterialPropertyBlock
                    propertyBlock.SetColor("_Color", biome.treeVariables._TreeLeafColors[Random.Range(0, 3)]);
                }
                else
                {
                    propertyBlock.SetColor("_Color", biome.treeVariables._TreeBarkColor);
                }

                // Assign the MaterialPropertyBlock to the material instance
                treeMeshRenderer.SetPropertyBlock(propertyBlock, i);
            }


            tree.SetActive(true); //make sure tree is active

            trees.Add(tree); //add tree to list
        }
    }
    public void ClearAllTrees()
    {
        ClearTrees(treesA);
        ClearTrees(treesB);
        ClearTrees(treesP);
    }
    void ClearTrees(List<GameObject> trees)
    {
        //destroys every tree object in list
        foreach(GameObject tree in trees)
        {
            DestroyImmediate(tree);
        }
        trees.Clear(); //clears list
    }
    public void ToggleOcean() //turns ocean on or off
    {
        ocean.gameObject.SetActive(!ocean.gameObject.activeInHierarchy);
    }
    public void ToggleAtmosphere() //turns atmosphere on or off
    {
        frontAtmosphere.gameObject.SetActive(!frontAtmosphere.gameObject.activeInHierarchy);
        outerAtmosphere.gameObject.SetActive(!outerAtmosphere.gameObject.activeInHierarchy);
        innerAtmosphere.gameObject.SetActive(!innerAtmosphere.gameObject.activeInHierarchy);
    }
    void ApplyBiomeNoise()
    { 
        //caching material and biome noise setting
        Material mat = planet.planetMaterial;
        BiomeNoiseSettings bns = planet.terrainVar.biomeNoiseSettings;
        BiomeWarpNoiseSettings bwns = planet.terrainVar.biomeWarpNoiseSettings;
        BiomeDetailNoiseSettings bdns = planet.terrainVar.biomeDetailNoiseSettings;
        
        //Transfering from terrain to the material
        //Normal noise transfer
        mat.SetInt("_Octaves", bns._Octaves);
        mat.SetVector("_Offset", bns._Offset);
        mat.SetFloat("_Scale", bns._Scale);
        mat.SetFloat("_Persistence", bns._Persistence);
        mat.SetFloat("_Lacunarity", bns._Lacunarity);
        mat.SetFloat("_Multiplier", bns._Multiplier);
        mat.SetFloat("_Gain", bns._Gain);

        //Warp noise transfer
        mat.SetInt("_WarpOctaves", bwns._WarpOctaves);
        mat.SetVector("_WarpOffset", bwns._WarpOffset);
        mat.SetFloat("_WarpScale", bwns._WarpScale);
        mat.SetFloat("_WarpPersistence", bwns._WarpPersistence);
        mat.SetFloat("_WarpLacunarity", bwns._WarpLacunarity);
        mat.SetFloat("_WarpMultiplier", bwns._WarpMultiplier);
        mat.SetFloat("_WarpGain", bwns._WarpGain);

        //Detail noise transfer
        mat.SetInt("_ColorOctaves", bdns._Octaves);
        mat.SetVector("_ColorOffset", bdns._Offset);
        mat.SetFloat("_ColorScale", bdns._Scale);
        mat.SetFloat("_ColorPersistence", bdns._Persistence);
        mat.SetFloat("_ColorLacunarity", bdns._Lacunarity);
        mat.SetFloat("_ColorMultiplier", bdns._Multiplier);
        mat.SetFloat("_ColorGain", bdns._Gain);

        mat.SetFloat("_PolarHeight", planet.terrainVar.polarBiomeSettings._PolarHeight);
        mat.SetFloat("_PolarBlendIntensity", planet.terrainVar.polarBiomeSettings._PolarBlendIntensity);


    }
    public void RandomizeOffset()
    {
        //random offset
        float[] offset = { UnityEngine.Random.value * System.DateTime.Now.Millisecond, UnityEngine.Random.value * System.DateTime.Now.Millisecond, UnityEngine.Random.value * System.DateTime.Now.Millisecond };
        //setting the offsets to randomly generated offset
        planet.terrainVar.generalSettings.seed = Mathf.RoundToInt(offset[1]);
        planet.terrainVar.warpedNoiseSettings.offset = offset;
        planet.terrainVar.continentNoiseSettings.offset = offset;
        planet.terrainVar.ridgeNoiseSettings.offset = offset;
        planet.terrainVar.detailNoiseSettings.offset = offset;
        planet.terrainVar.shapeNoiseSettings.offset = offset;
        planet.terrainVar.biomeNoiseSettings._Offset = new Vector3(offset[0], offset[1], offset[2]);
        planet.terrainVar.biomeWarpNoiseSettings._WarpOffset = new Vector3(offset[0], offset[1], offset[2]);
        meshFilter.mesh = OctahedronSphereCreator.Create(planet.terrainVar, calculateVertexHeight); //creating the planet
        ClearAllTrees();
    }
    public void RandomizeShading()
    {
        Color[] colors = new Color[8];
        //random colors for each value
        for (int i = 0; i < 8; i++)
        {
            Color randomColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
            colors[i] = randomColor;
        }
        //transferring random colors to material
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
        float[] metAndSmooth = new float[4];
        //random floats for each value
        for (int i = 0; i < 4; i++)
        {
            metAndSmooth[i] = UnityEngine.Random.value;
        }
        //transferring random floats to material
        Material mat = planet.planetMaterial;
        mat.SetFloat("_MetallicA", metAndSmooth[0]);
        mat.SetFloat("_MetallicB", metAndSmooth[1]);
        mat.SetFloat("_SmoothnessA", metAndSmooth[2]);
        mat.SetFloat("_SmoothnessB", metAndSmooth[3]);
    }
    public void RandomizeTerrainScheme()
    {
        if(terrainVarPool.Length == 0) //making sure that there are terrain schemes
        {
            Debug.LogWarning("No terrain schemes in variable pool");
            return;
        }
        //randomized number for the terrain
        int index = UnityEngine.Random.Range(0, terrainVarPool.Length);
        planet.terrainVar = terrainVarPool[index];
        RandomizeOffset();
    }
    public void RandomizeColorScheme()
    {
        ColorSchemeStaticReference.ColorSchemePool[] colorSchemes = planet.terrainVar.colorSchemePools.colorSchemePools; //array for color scheme pool
        UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
        if (colorSchemes.Length == 0){
            Debug.LogWarning("Terrain variable [" + planet.terrainVar.name + "] does not contain any colour pools");
            return; 
        }

        int index = UnityEngine.Random.Range(0, colorSchemes.Length); //Index of the pool
        if (colorSchemes[index].planetColorSchemes.Length == 0)
        {
            Debug.LogWarning("Color Scheme Pool: [" + colorSchemes[index].name + "] does not contain secondary biomes");
            return;
        }
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
            if(colorSchemes[index].oPrimary) //if ocean variables come from the primary
            {
                oBiomeA = primaryBiomeBiome == biomeA ? true : false; //setting bool for which biome is determined by for ocean
                oBiomeB = primaryBiomeBiome == biomeB ? true : false; //setting bool for which biome is determined by for ocean
            }
            else //if ocean variables doesn't come from the primary
            {
                oBiomeA = primaryBiomeBiome != biomeA ? true : false; //setting bool fo which biome is determined by for ocean
                oBiomeB = primaryBiomeBiome != biomeB ? true : false; //setting bool for which biome is determined by for ocean
            }
            if(colorSchemes[index].aPrimary) //if atmosphere variables come from the primary
            {
                aBiomeA = primaryBiomeBiome == biomeA ? true : false;
                aBiomeB = primaryBiomeBiome == biomeB ? true : false;
            }
            else //if atmosphere variables doesn't come from the primary
            {
                aBiomeA = primaryBiomeBiome != biomeA ? true : false; //setting bool for which biome is determined by for atmosphere
                aBiomeB = primaryBiomeBiome != biomeB ? true : false; //setting bool for which biome is determined by for atmosphere
            }
            RandomPolarBiome(colorSchemes[index]);
            return;
        }
        int index3 = UnityEngine.Random.Range(0, colorSchemes[index].planetColorSchemes.Length); //Index of the second biome
        while(index3==index2) //Make sure they are not the same as the first biome
        {
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
        RandomPolarBiome(colorSchemes[index]);
    }
    void RandomPolarBiome(ColorSchemeStaticReference.ColorSchemePool colorScheme)
    {
        if (colorScheme.polarColorSchemes == null) return;
        //Polar Biome
        if (colorScheme.polarColorSchemes.Length > 0)
        {
            int index4 = UnityEngine.Random.Range(0, colorScheme.polarColorSchemes.Length);
            biomeP = colorScheme.polarColorSchemes[index4];
        }
        else
        {
            biomeP = null;
        }
    }
    #region Color Schemes
    public void ApplyColorScheme()
    {
        Material mat = planet.planetMaterial;

        PlanetColorScheme biomeAScheme = biomeA;
        PlanetColorScheme biomeBScheme = biomeB;
        PlanetColorScheme biomePScheme = biomeP;

        if (biomeA != null)
        {
            // Biome A color scheme
            mat.SetColor("_ShorelineColorA", biomeAScheme.oceanVariables._ShorelineColor); //shoreline color
            mat.SetColor("_FlatColorA", biomeAScheme._FlatColor);
            mat.SetColor("_FlatColorAVariant", biomeAScheme._FlatColorVariant);
            mat.SetColor("_SteepColorA", biomeAScheme._SteepColor);
            mat.SetColor("_HighElevationColorA", biomeAScheme._HighElevationColor);
            mat.SetColor("_HighElevationSteepColorA", biomeAScheme._HighElevationSteepColor);
            mat.SetFloat("_MetallicA", biomeAScheme._Metallic);
            mat.SetFloat("_SmoothnessA", biomeAScheme._Smoothness);
            mat.SetFloat("_VegetationLineA", biomeAScheme._VegetationLine);
            mat.SetFloat("_VegetationLineHarshnessA", biomeAScheme._VegetationLineHarshness);
            mat.SetFloat("_SteepMultiplierA", biomeAScheme._SteepMultiplier);
        }
        if(biomeB != null)
        {
            // Biome B color scheme
            mat.SetColor("_ShorelineColorB", biomeBScheme.oceanVariables._ShorelineColor); //shoreline color
            mat.SetColor("_FlatColorB", biomeBScheme._FlatColor);
            mat.SetColor("_FlatColorBVariant", biomeBScheme._FlatColorVariant);
            mat.SetColor("_SteepColorB", biomeBScheme._SteepColor);
            mat.SetColor("_HighElevationColorB", biomeBScheme._HighElevationColor);
            mat.SetColor("_HighElevationSteepColorB", biomeBScheme._HighElevationSteepColor);
            mat.SetFloat("_MetallicB", biomeBScheme._Metallic);
            mat.SetFloat("_SmoothnessB", biomeBScheme._Smoothness);
            mat.SetFloat("_VegetationLineB", biomeBScheme._VegetationLine);
            mat.SetFloat("_VegetationLineHarshnessB", biomeBScheme._VegetationLineHarshness);
            mat.SetFloat("_SteepMultiplierB", biomeBScheme._SteepMultiplier);
        }
        if (biomeP != null)
        {
            mat.SetFloat("_Polar", 1);
            // Biome P color scheme
            mat.SetColor("_ShorelineColorP", biomePScheme.oceanVariables._ShorelineColor); //shoreline color
            mat.SetColor("_FlatColorP", biomePScheme._FlatColor);
            mat.SetColor("_FlatColorPVariant", biomePScheme._FlatColorVariant);
            mat.SetColor("_SteepColorP", biomePScheme._SteepColor);
            mat.SetColor("_HighElevationColorP", biomePScheme._HighElevationColor);
            mat.SetColor("_HighElevationSteepColorP", biomePScheme._HighElevationSteepColor);
            mat.SetFloat("_MetallicP", biomePScheme._Metallic);
            mat.SetFloat("_SmoothnessP", biomePScheme._Smoothness);
            mat.SetFloat("_VegetationLineP", biomePScheme._VegetationLine);
            mat.SetFloat("_VegetationLineHarshnessP", biomePScheme._VegetationLineHarshness);
            mat.SetFloat("_SteepMultiplierP", biomePScheme._SteepMultiplier);
        }
        else
        {
            mat.SetFloat("_Polar", 0);
        }

        planet.oceanVariables = oBiomeA ? biomeA.oceanVariables : planet.oceanVariables;
        planet.oceanVariables = oBiomeB ? biomeB.oceanVariables : planet.oceanVariables;
        planet.atmosphereVariables = aBiomeA ? biomeA.atmosphereVariables : planet.atmosphereVariables;
        planet.atmosphereVariables = aBiomeB ? biomeB.atmosphereVariables : planet.atmosphereVariables;
        planet.fresnelVariables = aBiomeA ? biomeA.fresnelVariables : planet.fresnelVariables;
        planet.fresnelVariables = aBiomeB ? biomeB.fresnelVariables : planet.fresnelVariables;
        bool restoreA = biomeA == null ? true : false;
        bool restoreB = biomeB == null ? true : false;
        RestoreSavedColorScheme(restoreA, restoreB);
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
        biomeASaved._VegetationLine = mat.GetFloat("_VegetationLineA");
        biomeASaved._VegetationLineHarshness = mat.GetFloat("_VegetationLineHarshnessA");
        biomeASaved._SteepMultiplier = mat.GetFloat("_SteepMultiplierA");

        biomeBSaved._FlatColor = mat.GetColor("_FlatColorB");
        biomeBSaved._FlatColorVariant = mat.GetColor("_FlatColorBVariant");
        biomeBSaved._SteepColor = mat.GetColor("_SteepColorB");
        biomeBSaved._HighElevationColor = mat.GetColor("_HighElevationColorB");
        biomeBSaved._HighElevationSteepColor = mat.GetColor("_HighElevationSteepColorB");
        biomeBSaved._Metallic = mat.GetFloat("_MetallicB");
        biomeBSaved._Smoothness = mat.GetFloat("_SmoothnessB");
        biomeBSaved._VegetationLine = mat.GetFloat("_VegetationLineB");
        biomeBSaved._VegetationLineHarshness = mat.GetFloat("_VegetationLineHarshnessB");
        biomeBSaved._SteepMultiplier = mat.GetFloat("_SteepMultiplierB");

        biomeASaved.oceanVariables = planet.oceanVariables;
        biomeASaved.atmosphereVariables = planet.atmosphereVariables;
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
            mat.SetFloat("_VegetationLineA", biomeASaved._VegetationLine);
            mat.SetFloat("_VegetationLineHarshnessA", biomeASaved._VegetationLineHarshness);
            mat.SetFloat("_SteepMultiplierA", biomeASaved._SteepMultiplier);
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
            mat.SetFloat("_VegetationLineB", biomeBSaved._VegetationLine);
            mat.SetFloat("_VegetationLineHarshnessB", biomeBSaved._VegetationLineHarshness);
            mat.SetFloat("_SteepMultiplierB", biomeBSaved._SteepMultiplier);

        }
        if (restoreA && restoreB)
        {
            planet.atmosphereVariables = biomeASaved.atmosphereVariables;
            planet.oceanVariables = biomeASaved.oceanVariables;
        }
    }
    #endregion
    #region Editor Tools
    [MenuItem("ProceduralTools/Create Planet Prefab")]
    static void CreatePrefab()
    {
        GameObject selectedObj = Selection.activeGameObject; //store selection

        if (selectedObj == null || selectedObj.GetComponent<FullPlanetGeneration>() == null) //selection must have fullplanetgeneration
        {
            Debug.LogWarning("Please select a valid game object");
            return;
        }

        GameObject newObj = Instantiate(selectedObj, selectedObj.transform.position, selectedObj.transform.rotation); //create the obj
        GameObject newOceanObj = newObj.transform.GetChild(0).gameObject; //select the ocean object from the object

        DestroyImmediate(newObj.GetComponent<FullPlanetGeneration>()); //destroy the fullplanetgeneration on the object
        newObj.SetActive(true); //make sure the object is active

        MeshFilter meshFilter = selectedObj.GetComponent<MeshFilter>();         //store mesh filter
        MeshRenderer meshRenderer = selectedObj.GetComponent<MeshRenderer>();   //store mesh renderer
        MeshRenderer oceanRenderer = newOceanObj.GetComponent<MeshRenderer>();  //store ocean mesh renderer

        Mesh mesh = meshFilter.sharedMesh;                                      //store mesh
        Material material = meshRenderer.sharedMaterial;                        //store planet material
        Material oceanMaterial = oceanRenderer.sharedMaterial;                  //store ocean material

        Material copyMat = new Material(material.shader);                       //create unique planet material
        Material copyOceanMat = new Material(oceanMaterial.shader);             //create unique ocean material
        Mesh copyMesh = mesh;                                                   //store copy of the mesh

        copyMat.CopyMatchingPropertiesFromMaterial(material); //copy planet material properties
        copyOceanMat.CopyMatchingPropertiesFromMaterial(oceanMaterial); //copy ocean material properties

        MeshFilter newMeshFilter = newObj.GetComponent<MeshFilter>();           //store the new mesh filter
        MeshRenderer newMeshRenderer = newObj.GetComponent<MeshRenderer>();     //store the new mesh renderer

        newMeshFilter.mesh = copyMesh;              //setting the mesh in the new object
        newMeshRenderer.material = copyOceanMat;    //setting the planet material in the new object
        oceanRenderer.material = copyOceanMat;      //setting the ocean material in the new object

        string prefabPath = EditorUtility.SaveFilePanel("Save Prefab", "Assets/", "BakedPlanet", "prefab"); //opens a file panel for the user to choose the file path

        if (prefabPath.Length == 0) //in case the user cancels the file path
        {
            DestroyImmediate(newObj);
            return;
        }

        prefabPath = prefabPath.Substring(prefabPath.IndexOf("Assets/")); //editing the path so its compatible with unity
        PrefabUtility.SaveAsPrefabAsset(newObj, prefabPath); //saves the prefab

        AssetDatabase.AddObjectToAsset(copyMat, prefabPath);        //saves the planet material
        AssetDatabase.AddObjectToAsset(copyOceanMat, prefabPath);   //saves the ocean  material
        AssetDatabase.AddObjectToAsset(copyMesh, prefabPath);       //saves the mesh
        AssetDatabase.SaveAssets();

        DestroyImmediate(newObj); //destroys the prefab that was instantiated in the scene
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

