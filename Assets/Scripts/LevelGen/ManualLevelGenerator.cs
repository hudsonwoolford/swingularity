using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using static LevelGenerator;

public class ManualLevelGenerator : MonoBehaviour
{
    public float generationProgress { get; private set; } = 0;  
    public static ManualLevelGenerator Instance;
    public int aestheticSeed;
    [SerializeField] Transform mockPlanetsParent;
    [SerializeField] GameObject planetGeneratorPrefab, forcefieldPrefab, darkMatterPrefab;
    [SerializeField] LevelGenStaticReferences levelStatic;
    [SerializeField] int resolution = 1024;
    [SerializeField] bool spawnForcefields = true;
    [SerializeField] Transform allPlanetParent;
    MockPlanet[] mockPlanets = new MockPlanet[0];
    Transform environment;
    RenderTexture frontTexture, backTexture;
    Texture2D planetTexture;
    GameObject player;
    bool procedural = false;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        player = GameObject.FindGameObjectWithTag("Player");
        environment = GameObject.FindGameObjectWithTag("Environment").transform;
    }
    // Start is called before the first frame update
    void Start()
    {
        if (allPlanetParent == null)
        {
            allPlanetParent = new GameObject("Planets").transform;
        }
        allPlanetParent.gameObject.tag = "PlanetParent";

        levelInit?.Invoke(GameManager.LevelManager.currentTheme);  //Initiation failsafe
        resolution = SettingsManager.userSettings.planetRes;
        frontTexture = new RenderTexture(resolution, resolution, 0);
        backTexture = new RenderTexture(resolution, resolution, 0);
        planetTexture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        mockPlanets = mockPlanetsParent.GetComponentsInChildren<MockPlanet>();
        List<int> moons = new List<int>();
        List<int> darkMatter = new List<int>();
        for (int i = 0; i < mockPlanets.Length; i++)
        {
            if (mockPlanets[i] is MockMoon)
            {
                moons.Add(i);
            }
            if(mockPlanets[i].darkMatter){
                darkMatter.Add(i);
            }
        }

        ProceduralLevelGeneration.Level level = ManualLevelGeneration.CreateLevel(aestheticSeed, mockPlanets, levelStatic);
        StartCoroutine(Generatelevel(level, moons.ToArray(), darkMatter.ToArray()));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /// <summary>
    /// Generates the planets off of the level parameters
    /// </summary>
    IEnumerator Generatelevel(ProceduralLevelGeneration.Level level, int[] moonIndices, int[] darkMatterIndices)
    {
        //Parallel flipping - NEEDS FIXING
        if(SaveLoadManager.currentUniverseSave.universeParams.universeType == LevelManager.UniverseParams.UniverseType.parallel){
            GameObject flag = GameObject.FindGameObjectWithTag("Hole");
            Vector3 flagPos = flag.transform.position;
            Vector3 playerPos = player.transform.position;
            player.transform.position = flagPos;
            flag.transform.position = playerPos;

        }
        generationProgress = 0;
        Time.timeScale = 0;
        FullPlanetGeneration generator = Instantiate(planetGeneratorPrefab).GetComponent<FullPlanetGeneration>();
        Planet[] planetList = new Planet[level.numPlanets];
        for (int i = 0; i < level.numPlanets; i++)
        {
            if(moonIndices.Contains(i) || darkMatterIndices.Contains(i))
            {
                planetList[i] = new Planet();
                continue;
            }
            generationProgress += 1 / (float)level.numPlanets;

            if(procedural){
                planetList[i] = GeneratePlanet(generator, level.planetSizes[i], i, level.planetPoints[i], level.planetSeeds[i], level.planetTypes[i]);
            }
            else{
                planetList[i] = GenerateManualPlanet(generator, level.planetSizes[i], level.planetPoints[i], level.planetSeeds[i], level.planetTypes[i]);
            }
            

            yield return new WaitForSecondsRealtime(0.1f);
        }
        //Moons
        for (int i = 0; i < moonIndices.Length; i++)
        {
            generationProgress += 1 / (float)level.numPlanets;

            MockMoon mockMoon = (MockMoon)mockPlanets[moonIndices[i]];
            CelestialMass correspondant = null;
            //Get celestial mass correspondant
            for (int x = 0; x < planetList.Length; x++)
            {
                if(x == moonIndices[i]) { continue; }
                if (mockPlanets[x].transform == mockMoon.orbitPlanet)
                {
                    correspondant = planetList[x].forceField.GetComponent<CelestialMass>();
                }
            }
            planetList[moonIndices[i]] = GenerateMoon(correspondant, level.planetSizes[moonIndices[i]], level.planetPoints[moonIndices[i]], level.planetSeeds[moonIndices[i]], level.planetTypes[moonIndices[i]], generator, moonIndices[i], mockMoon.eccentricity, mockMoon.speed, mockMoon.rotation, mockMoon.apoapsis, mockMoon.showLr);
        }
        
        for (int i = 0; i < darkMatterIndices.Length; i++)
        {
            planetList[darkMatterIndices[i]] = GenerateDarkMatter(ref planetList[darkMatterIndices[i]],  level.planetSizes[darkMatterIndices[i]], level.planetPoints[darkMatterIndices[i]],  level.planetSeeds[darkMatterIndices[i]],level.planetTypes[darkMatterIndices[i]], generator);
        }
        Destroy(generator.gameObject);

        ////Camera start and end positions
        //(Vector2, Vector2, Vector2) startEndPos = CalculateStartEndPos(planetList);
        //Vector2 startPos = startEndPos.Item1;
        //Vector2 endPos = startEndPos.Item2;
        //Vector2 regionSize = startEndPos.Item3;
        //float maxZoom = Mathf.Max((regionSize.x) / (2 * Camera.main.aspect), regionSize.y / 2) + 5;

        //Camera.main.GetComponent<CameraFollow>().InitPositions(startPos, endPos, maxZoom);


        generationProgress = 1;
        Time.timeScale = 1;
        levelGenerated?.Invoke();
        player?.GetComponent<ShootBall>().Init(environment, player.GetComponent<Rigidbody>().position);
        mockPlanetsParent.gameObject.SetActive(false);
    }
    (Vector2, Vector2, Vector2) CalculateStartEndPos(Planet[] planets)
    {
        Vector3 avg = Vector3.zero;
        float maxX = 0, maxY = 0;
        foreach (Planet planet in planets)
        {
            avg += planet.planet.transform.position;
        }
        avg /= planets.Length;
        Vector2 planetsCenter = avg;
        Vector2 startPos = planetsCenter;
        Vector2 endPos = startPos;
        foreach (Planet planet in planets)
        {
            maxX = Mathf.Max(planet.planet.transform.position.x - planetsCenter.x, maxX);
            maxY = Mathf.Max(planet.planet.transform.position.y - planetsCenter.y, maxY);
        }

        endPos += new Vector2(maxX, maxY); startPos -= new Vector2(maxX, maxY);
        return (startPos, endPos, new Vector2(maxX, maxY) * 2);
    }
    #region Generation Functions
    Planet GenerateManualPlanet(FullPlanetGeneration generator, float size, Vector3 position, Vector3 seed, ColorSchemeStaticReference.PlanetType planetType)
    {
        UnityEngine.Random.InitState((int)seed.z * 100);
        (PlanetColorScheme, PlanetColorScheme) biomes = generator.RandomizeColorScheme(seed, planetType, 1, false);
        generator.ClearAllTrees();
        generator.UpdateAndApplyAll();

        if (generator.biomeP == null)
        {
            generator.transform.rotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(-180, 180));
        }
        else
        {
            generator.transform.rotation = Quaternion.identity;
        }


        GameObject newPlanet = ConvertToImage(generator.gameObject, position, size);
        frontTexture.Release(); //Memory
        backTexture.Release();  //Memory
        newPlanet.SetActive(true);

        newPlanet.transform.position = new Vector3(position.x, position.y, 0);
        newPlanet.transform.localScale = Vector3.one * size * levelStatic.planetScale;
        GameObject forceField = null;
        if(spawnForcefields)
        {
            forceField = GenerateForcefield(newPlanet.transform);
            forceField.transform.parent = environment;
        }

        newPlanet.transform.parent = allPlanetParent;


        return new Planet(newPlanet, planetType, biomes.Item1, biomes.Item2, forceField);
    }
    Planet GenerateDarkMatter(ref Planet planet, float size, Vector3 position, Vector3 seed, ColorSchemeStaticReference.PlanetType planetType, FullPlanetGeneration generator)
    {
        UnityEngine.Random.InitState((int)seed.z);
        planet = GenerateManualPlanet(generator, size, position, seed, planetType);

        DarkMatterGenerator darkMatter = Instantiate(darkMatterPrefab, new Vector3(position.x + levelStatic.maxSize / 2, position.y + levelStatic.maxSize / 2, position.z), Quaternion.identity).GetComponent<DarkMatterGenerator>();
        darkMatter.Init(planet.planet.GetComponent<PlanetDataHolder>(), planet.forceField);
        return planet;
    }
    Planet GeneratePlanet(FullPlanetGeneration generator, float size, int index, Vector3 position, Vector3 seed, ColorSchemeStaticReference.PlanetType planetType)
    {
        UnityEngine.Random.InitState((int)seed.z * 100);
        (PlanetColorScheme, PlanetColorScheme) biomes = generator.RandomizeColorScheme(seed, planetType, index + 1, false);
        generator.ClearAllTrees();
        generator.UpdateAndApplyAll();

        if (generator.biomeP == null)
        {
            generator.transform.rotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(-180, 180));
        }
        else
        {
            generator.transform.rotation = Quaternion.identity;
        }


        GameObject newPlanet = ConvertToImage(generator.gameObject, position, size);
        frontTexture.Release(); //Memory
        backTexture.Release();  //Memory
        newPlanet.SetActive(true);

        newPlanet.transform.position = new Vector3(position.x, position.y, 0);
        newPlanet.transform.localScale = Vector3.one * size * levelStatic.planetScale;
        GameObject forceField = null;
        if(spawnForcefields)
        {
            forceField = GenerateForcefield(newPlanet.transform);
            forceField.transform.parent = environment;
        }

        newPlanet.transform.parent = allPlanetParent;


        return new Planet(newPlanet, planetType, biomes.Item1, biomes.Item2, forceField);
    }
    /// <summary>
    /// Generates a forcefield for a planet
    /// </summary>
    GameObject GenerateForcefield(Transform planet)
    {
        GameObject forcefield = Instantiate(forcefieldPrefab); 

        ForceField ff = forcefield.GetComponent<ForceField>();
        ff.enabled = true;
        ff.Init(planet);
        return forcefield;
    }
    Planet GenerateMoon(CelestialMass correspondant, float size, Vector3 position, Vector3 seed, ColorSchemeStaticReference.PlanetType planetType, FullPlanetGeneration generator, int index, float eccentricity, float speed, float rotation, float apoapsis, bool showLr)
    {
        if(correspondant == null) { Debug.LogWarning("Correspondant of moon is null, this could be due to it not being set in inspector."); return new Planet(); }
        UnityEngine.Random.InitState((int)(seed.y + index));

        //Generate orbiter planet
        Planet smallMoon = CreateMoonMesh(generator, size, position, seed + new Vector3(levelStatic.offsetScale * 2, levelStatic.offsetScale * 2, levelStatic.offsetScale * 2) + Vector3.one, planetType);
        smallMoon.planet.transform.parent = allPlanetParent;
        smallMoon.forceField.transform.parent = environment;

        MoonManager moonScript = smallMoon.planet.AddComponent<MoonManager>();

        float semiMajorAxis = apoapsis / (1 - eccentricity);
        smallMoon.forceField.GetComponent<ForceField>().AutoMass();
        smallMoon.forceField.GetComponent<Collider>().material = levelStatic.superBouncyMoon;
        moonScript.Init(semiMajorAxis, ArtificialGravity.G, eccentricity, speed, smallMoon.forceField.GetComponent<CelestialMass>(), correspondant, levelStatic.moonLine, rotation, (int)seed.x);
        if(!showLr){
            moonScript.DisableLr();
        }
        moonScript.GetComponent<MeshRenderer>().enabled = true;
        return smallMoon;
    }
    Planet CreateMoonMesh(FullPlanetGeneration generator, float size, Vector3 position, Vector3 seed, ColorSchemeStaticReference.PlanetType planetType)
    {

        (PlanetColorScheme, PlanetColorScheme) biomes = generator.RandomizeColorScheme(seed, planetType, 1495, true);
        generator.UpdateAndApplyAll();
        generator.ClearAllTrees();
        GameObject genObj = Instantiate(generator.gameObject);
        MeshRenderer genRenderer = genObj.GetComponent<MeshRenderer>();
        Material customMoonMat = new Material(genRenderer.material);
        genRenderer.material = customMoonMat;

        Destroy(genObj.GetComponent<FullPlanetGeneration>());
        if (genObj.GetComponentInChildren<Camera>() != null)
        {
            Destroy(genObj.GetComponentInChildren<Camera>().gameObject); //camera on moon generator
        }
        genObj.transform.position = new Vector3(position.x + levelStatic.maxSize / 2, position.y + levelStatic.maxSize / 2, position.z);
        genObj.transform.localScale = Vector3.one * size * 0.375f;
        GameObject forceField = GenerateForcefield(genObj.transform);
        return new Planet(genObj, ColorSchemeStaticReference.PlanetType.special, biomes.Item1, biomes.Item2, forceField);
    }
    #endregion
    #region Utility
    /// <summary>
    /// Takes an image of the camera render texture and creates a planet object with sprites
    /// </summary>
    GameObject ConvertToImage(GameObject selectedObj, Vector3 position, float size)
    {
        MeshRenderer oceanRend = selectedObj.transform.Find("Ocean").GetComponent<MeshRenderer>();
        bool activeOcean = oceanRend.gameObject.activeInHierarchy;

        selectedObj.transform.position = new Vector3(position.x, position.y, 0);

        Material oceanMat = oceanRend.material;
        Material cullMat = levelStatic.invisOcean;
        oceanRend.sharedMaterial = cullMat;

        Camera cam = new GameObject().AddComponent<Camera>();
        cam.transform.parent = selectedObj.transform;
        cam.transform.localPosition = new Vector3(0, 0, -10);
        cam.orthographic = true;
        cam.orthographicSize = 1.5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0, 0, 0, 0);
        cam.cullingMask = LayerMask.GetMask("Planet Front Capture");



        //Front Texture
        cam.targetTexture = frontTexture;
        cam.Render();

        Sprite frontSprite = ConvertRenderTextureToSprite(frontTexture);

        //Ocean disabled
        oceanRend.gameObject.SetActive(false);

        //Back Texture
        cam.cullingMask = LayerMask.GetMask("Planet Back Capture", "Planet Front Capture");

        cam.targetTexture = backTexture;
        cam.Render();

        Sprite backSprite = ConvertRenderTextureToSprite(backTexture);
        backSprite.name = "dsad";

        //Creation of prefab object

        //Parent planet
        GameObject planetParent = new GameObject("Planet Parent");
        planetParent.transform.position = Vector3.zero;
        planetParent.transform.localScale = Vector3.one * 0.294f;
        SortingGroup sr = planetParent.AddComponent<SortingGroup>();
        sr.sortingLayerName = "Planet Sprites";
        PlanetDataHolder pd = planetParent.AddComponent<PlanetDataHolder>();
        PlanetDataHolder cd = selectedObj.GetComponent<PlanetDataHolder>();
        pd.biomeA = cd.biomeA;
        pd.biomeB = cd.biomeB;
        pd.oBiomeA = cd.oBiomeA;
        pd.oBiomeB = cd.oBiomeB;
        pd.aBiomeA = cd.aBiomeA;
        pd.aBiomeB = cd.aBiomeB;

        //Front sprite
        GameObject frontObject = new GameObject("frontObject");
        frontObject.transform.parent = planetParent.transform;
        frontObject.transform.localScale = Vector3.one * (1024 / resolution);
        SpriteRenderer frontSR = frontObject.AddComponent<SpriteRenderer>();
        frontSR.sprite = frontSprite;
        frontSR.sortingOrder = 1;

        //Back sprite
        GameObject backObject = new GameObject("backObject");
        backObject.transform.parent = planetParent.transform;
        backObject.transform.position += new Vector3(0, 0, 2);
        backObject.transform.localScale = Vector3.one * (1024 / resolution);
        SpriteRenderer backSR = backObject.AddComponent<SpriteRenderer>();
        backSR.sprite = backSprite;

        //ocean
        GameObject ocean = Instantiate(oceanRend.gameObject, planetParent.transform);
        ocean.GetComponent<MeshRenderer>().material = oceanMat;
        ocean.transform.position += new Vector3(0, 0, 2);
        ocean.transform.localScale = Vector3.one * 6.67f;
        ocean.SetActive(activeOcean);

        //Atmospheres
        GameObject[] atmospheres = { selectedObj.transform.GetChild(1).gameObject, selectedObj.transform.GetChild(2).gameObject, selectedObj.transform.GetChild(3).gameObject };
        for (int i = 0; i < 3; i++)
        {
            GameObject instaAtmosphere = Instantiate(atmospheres[i], planetParent.transform);
            instaAtmosphere.AddComponent<AtmosphereHandler>();
            float depth = i == 2 ? 0 : 2;
            instaAtmosphere.transform.localScale = Vector3.one;
            instaAtmosphere.transform.position += new Vector3(0, 0, depth);
            instaAtmosphere.layer = LayerMask.NameToLayer("Default");
            instaAtmosphere.SetActive(true);
        }

        oceanRend.sharedMaterial = oceanMat;
        oceanRend.gameObject.SetActive(true);
        ocean.gameObject.layer = LayerMask.NameToLayer("Water");

        Destroy(cam.gameObject);
        planetParent.SetActive(false);

        return planetParent;
    }
    /// <summary>
    /// Converts a RenderTexture to a Sprite
    /// </summary>
    /// <param name="renderTexture"></param>
    /// <returns></returns>
    Sprite ConvertRenderTextureToSprite(RenderTexture renderTexture)
    {

        RenderTexture prevActive = RenderTexture.active;
        RenderTexture.active = renderTexture;
        planetTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
        planetTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        planetTexture.Apply();

        Sprite sprite = Sprite.Create(planetTexture, new Rect(0, 0, planetTexture.width, planetTexture.height), Vector2.one * 0.5f);
        RenderTexture.active = prevActive;
        return sprite;
    }
    #endregion

    void OnEnable(){
        GalaxyGenerator.initProceduralSystem += IsProcedural; 
    }
    void OnDisable(){
        GalaxyGenerator.initProceduralSystem -= IsProcedural; 
    }
    void IsProcedural(string[] levels, LevelManager.LevelParams[] levelParams, LevelManager.GalaxyParams galaxyParams, GalaxyData theme){
        procedural = true;
    }
}
public static class ManualLevelGeneration
{
    public static ProceduralLevelGeneration.Level CreateLevel(int aestheticSeed, MockPlanet[] mockPlanets, LevelGenStaticReferences levelStatic)
    {
        UnityEngine.Random.InitState(aestheticSeed);

        ColorSchemeStaticReference.PlanetType[] types = new ColorSchemeStaticReference.PlanetType[mockPlanets.Length];
        Vector3[] seeds  = new Vector3[mockPlanets.Length];
        Vector3[] points = new Vector3[mockPlanets.Length];
        float[]   sizes  = new float[mockPlanets.Length];


        for (int i = 0; i < mockPlanets.Length; i++)
        {
            points[i] = mockPlanets[i].transform.position;
            sizes[i]  = mockPlanets[i].transform.localScale.x;
            types[i]  = mockPlanets[i].planetType;

            Vector3 rand = mockPlanets[i].seed;
            seeds[i] = rand;
            if (!mockPlanets[i].specificSeed)
            {
                seeds[i] = new Vector3(UnityEngine.Random.value * levelStatic.offsetScale + i * levelStatic.offsetScale, UnityEngine.Random.value * levelStatic.offsetScale + 2 * i * levelStatic.offsetScale, UnityEngine.Random.value * levelStatic.offsetScale + 3 * i * levelStatic.offsetScale);
            }
        }
        return new ProceduralLevelGeneration.Level(mockPlanets.Length, sizes, points, seeds, types, Vector2.zero);
    }
}