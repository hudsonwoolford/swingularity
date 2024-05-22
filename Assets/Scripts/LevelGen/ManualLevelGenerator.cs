using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using static LevelGenerator;
using static UnityEditor.Experimental.GraphView.GraphView;

public class ManualLevelGenerator : MonoBehaviour
{
    [SerializeField] int aestheticSeed;
    [SerializeField] Transform mockPlanetsParent;
    [SerializeField] GameObject planetGeneratorPrefab, forcefieldPrefab;
    [SerializeField] LevelGenStaticReferences levelStatic;
    [SerializeField] int resolution = 1024;
    MockPlanet[] mockPlanets = new MockPlanet[0];
    Transform environment, allPlanetParent;
    RenderTexture frontTexture, backTexture;
    Texture2D planetTexture;
    GameObject player;
    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        environment = GameObject.FindGameObjectWithTag("Environment").transform;
    }
    // Start is called before the first frame update
    void Start()
    {
        levelInit?.Invoke(levelStatic.possibleGalaxySchemes[0]);  //Initiation failsafe
        frontTexture = new RenderTexture(resolution, resolution, 0);
        backTexture = new RenderTexture(resolution, resolution, 0);
        planetTexture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        mockPlanets = mockPlanetsParent.GetComponentsInChildren<MockPlanet>();
        ProceduralLevelGeneration.Level level = ManualLevelGeneration.CreateLevel(aestheticSeed, mockPlanets, levelStatic);
        StartCoroutine(Generatelevel(level));
        mockPlanetsParent.gameObject.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /// <summary>
    /// Generates the planets off of the level parameters
    /// </summary>
    IEnumerator Generatelevel(ProceduralLevelGeneration.Level level)
    {
        Time.timeScale = 0;
        FullPlanetGeneration generator = Instantiate(planetGeneratorPrefab).GetComponent<FullPlanetGeneration>();
        Planet[] planetList = new Planet[level.numPlanets];
        allPlanetParent = new GameObject("Planets")
        {
            tag = "PlanetParent"
        }.transform;

        for (int i = 0; i < level.numPlanets; i++)
        {
            planetList[i] = GeneratePlanet(generator, level.planetSizes[i], i, level.planetPoints[i], level.planetSeeds[i], level.planetTypes[i]);

            yield return new WaitForSecondsRealtime(0.1f);
        }
        Destroy(generator.gameObject);

        Time.timeScale = 1;
        levelGenerated?.Invoke();
        player.GetComponent<ShootBall>().Init(environment, player.GetComponent<Rigidbody>().position);

    }
    Planet GeneratePlanet(FullPlanetGeneration generator, float size, int index, Vector3 position, Vector3 seed, ColorSchemeStaticReference.PlanetType planetType)
    {
        UnityEngine.Random.InitState((int)seed.z + index * 100);
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
        newPlanet.transform.localScale = Vector3.one * size * levelStatic.planetScale * 0.2f;
        GameObject forceField = GenerateForcefield(newPlanet.transform);

        newPlanet.transform.parent = allPlanetParent;
        forceField.transform.parent = environment;


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

            float rand = mockPlanets[i].specificSeed ? mockPlanets[i].seed : UnityEngine.Random.value;
            float xOffset = rand * levelStatic.offsetScale + i * levelStatic.offsetScale;
            float yOffset = rand * levelStatic.offsetScale + 2 * i * levelStatic.offsetScale;
            float zOffset = rand * levelStatic.offsetScale + 3 * i * levelStatic.offsetScale;
            seeds[i] = new Vector3(xOffset, yOffset, zOffset);
        }
        return new ProceduralLevelGeneration.Level(mockPlanets.Length, sizes, points, seeds, types, Vector2.zero);
    }
}

