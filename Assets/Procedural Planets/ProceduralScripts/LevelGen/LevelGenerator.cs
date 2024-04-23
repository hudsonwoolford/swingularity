using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using System;
using Unity.VisualScripting;

public class LevelGenerator : MonoBehaviour
{
        /*Level Gen settings
    0: minimum number of planets
    1: maximum number of planets
    2: minimum radius for each poisson point
    3: maximum radius for each poisson point
    4: bias towards or against planet size
    5: IGNORE
    6: x dimension of size of level
    7: y dimension of size of level
    8: number of poisson disk samples before rejecting the point as a spawner
    9: offset scale for determining random values of planet
    */
    [SerializeField] bool randomSeed = false; //Whether seed is chosen randomly in start
    [SerializeField] bool canSpawnOffPlanet = false; //Whether the hole can spawn off a planet 
    //[SerializeField] bool sameColourPoolTwice = false;
    [SerializeField] Material invisOcean; //Invisble ocean material used for imaging
    public int seed; //The seed for the level

    //Level gen params
    [SerializeField]
    float minPlanets, maxPlanets, minSize, maxSize, sizeBias, sizeScale, samplesBeforeRejection, offsetScale;
    [SerializeField]
    Vector2 regionExtension; //how much region is extended
    [SerializeField]
    GameObject planetGeneratorPrefab, forcefieldPrefab, starPrefab, audioSrcPrefab; //Needed prefabs
    [SerializeField]
    float planetScale; //Scale of planets multiplier
    [SerializeField]
    ColorSchemeStaticReference.PlanetType[] possiblePlanetTypes; //Filters which planet types are chosen

    Vector2 regionSize;

    [Serializable]
    public struct Planet{
        public GameObject planet;
        public ColorSchemeStaticReference.PlanetType planetType;
        public PlanetColorScheme biomeA;
        public PlanetColorScheme biomeB;
        public GameObject forceField;
        //Constructor
        public Planet(GameObject planet, ColorSchemeStaticReference.PlanetType planetType, PlanetColorScheme biomeA, PlanetColorScheme biomeB, GameObject forceField){
            this.planet = planet;
            this.planetType = planetType;
            this.biomeA = biomeA;
            this.biomeB = biomeB;
            this.forceField = forceField;
        }
    }

    public delegate void LevelInit(); //Delegate called in initiation of procedural level, this coccurs before the level is actually generated
    public static LevelInit levelInit;

    void Start()
    {
        levelInit?.Invoke(); //Initiation Delegate
        Camera mainCam = Camera.main; //Cache camera
        //Randomising or keeping seed
        if(randomSeed)
        {
            seed = Mathf.RoundToInt(UnityEngine.Random.value * System.DateTime.Now.Millisecond + System.DateTime.Now.Millisecond * System.DateTime.Now.Second + UnityEngine.Random.Range(-1f, 1f) * Time.time);
        }
        UnityEngine.Random.InitState(seed);

        //Region size calculation
        Vector2 camSize = CamSize(mainCam);                             //1. Calculates camera size
        bool xOry = UnityEngine.Random.Range(0, 2) == 1 ? true : false; //2. Chooses which axis to extend into, X or Y.
        float extensionAmount = UnityEngine.Random.value;               //3. Randomizes extension amount. 
        regionSize = CalculateBounds(camSize, extensionAmount, xOry);   //4. Calculating bounds based off previous variables

        //Sets the level generation settings
        float[] levelGenSettings = new float[10]{
            minPlanets,
            maxPlanets,
            minSize,
            maxSize,
            sizeBias,
            sizeScale,
            (regionSize.x - maxSize) / 1.25f,
            (regionSize.y - maxSize) / 1.25f,
            samplesBeforeRejection,
            offsetScale
        };

        //Calculates level based off settings
        ProceduralLevelGeneration.Level level = ProceduralLevelGeneration.CalculateLevel(seed, possiblePlanetTypes, levelGenSettings);
        //Recalculates levels with (seed+1) if number of planets is less than zero
        for (int i = 0; level.numPlanets == 0 && i < 50; i++)
        {
            level = ProceduralLevelGeneration.CalculateLevel(seed, possiblePlanetTypes, levelGenSettings);
            Debug.LogWarning(seed + " generated no planets. Regenerating planets, iteration: " + i + "/50 | Current Planet Count = " + level.numPlanets);
            seed++;
        }
        
        //Generates level based of calculation
        Planet[] planets = Generatelevel(level);

        //Camera start and end positions
        (Vector2, Vector2) startEndPos = CalculateStartEndPos(planets, camSize, extensionAmount, xOry);
        Vector2 startPos = startEndPos.Item1;
        Vector2 endPos = startEndPos.Item2;
        float maxZoom = Mathf.Max((regionSize.x) / (2 * mainCam.aspect), regionSize.y / 2) + 0.5f;

        mainCam.GetComponent<CameraFollow>().InitPositions(startPos, endPos, maxZoom);
    }
    #region Level Calculation
    /// <summary>
    /// Calculates the start pos and end pos for the camera. Takes an average of planet positions.
    /// </summary>
    (Vector2, Vector2) CalculateStartEndPos(Planet[] planets, Vector2 camSize, float extensionAmount, bool x){
        Vector3 avg = Vector3.zero;
        foreach (Planet planet in planets){
            avg += planet.planet.transform.position;             
        }
        avg /= planets.Length;
        Vector2 planetsCenter = avg;
        Vector2 startPos = planetsCenter;
        Vector2 endPos = startPos;
        if (x){endPos += new Vector2(extensionAmount * regionExtension.x, 0); startPos -= new Vector2(extensionAmount * regionExtension.x, 0); }
        else{endPos += new Vector2(0, extensionAmount * regionExtension.y); startPos -=  new Vector2(0, extensionAmount * regionExtension.y); }
        return (startPos, endPos);
    }
    /// <summary>
    /// Calculates the region size
    /// </summary>
    Vector2 CalculateBounds(Vector3 camSize, float extensionAmount, bool x){
        //Extend in the x direction maybe?
        
        Vector2 regionSize = new Vector3 (camSize.x + maxSize, camSize.y + maxSize, camSize.z);
        if(x){ regionSize += new Vector2(extensionAmount * regionExtension.x, 0); }
        else{ regionSize += new Vector2(0, extensionAmount * regionExtension.y);}

        return regionSize;
    }
    #endregion
    #region Level Generation
    /// <summary>
    /// Generates the planets off of the level parameters
    /// </summary>
    Planet[] Generatelevel(ProceduralLevelGeneration.Level level){
        FullPlanetGeneration generator = Instantiate(planetGeneratorPrefab).GetComponent<FullPlanetGeneration>();
        Planet[] planetList = new Planet[level.numPlanets];
        Instantiate(audioSrcPrefab);
        Transform env = GameObject.FindGameObjectWithTag("Environment").transform;
        Transform allPlanetParent = new GameObject("Planets").transform;
        Vector3 levelSize = new Vector3(level.levelSize.x, level.levelSize.y, 0f);
        for (int i = 0; i < level.numPlanets; i++){
            if (level.planetTypes[i] == ColorSchemeStaticReference.PlanetType.star)
            {
                planetList[i].planet = GenerateStar(level.planetSizes[i], i, level.planetPoints[i], level.planetSeeds[i], level.planetTypes[i]);
                planetList[i].planet.transform.parent =env;
                planetList[i].planetType = level.planetTypes[i];
                planetList[i].forceField = null;
                continue;
            }
            planetList[i] = GeneratePlanet(generator, level.planetSizes[i], i, level.planetPoints[i], level.planetSeeds[i], level.planetTypes[i], planetList);
            planetList[i].planet.SetActive(true);
            planetList[i].planet.transform.parent = allPlanetParent;
            GameObject forcefield = GenerateForcefield(planetList[i]);
            planetList[i].forceField = forcefield;
            planetList[i].planetType = level.planetTypes[i];
            forcefield.transform.parent = env;
            //print(forcefield.transform.localScale.x);
        }
        Destroy(generator.gameObject);
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Transform flag = GameObject.FindGameObjectWithTag("Hole").transform;

        List<Planet> normPlanets = new List<Planet>();
        for (int i = 0; i < planetList.Length; i++)
        {
            if (planetList[i].planetType != (ColorSchemeStaticReference.PlanetType.star | ColorSchemeStaticReference.PlanetType.blackhole))
            {
                normPlanets.Add(planetList[i]);
            }
        }
        SetBallFlagPosition(flag, player.transform, normPlanets.ToArray());
        player.GetComponent<ShootBall>().Init(env, player.GetComponent<Rigidbody>().position);
        return planetList;
    }
    /// <summary>
    /// Generates a planet using FullPlanetGeneration
    /// </summary>
    Planet GeneratePlanet(FullPlanetGeneration generator, float size, int index, Vector3 position, Vector3 seed, ColorSchemeStaticReference.PlanetType planetType, Planet[] planetList)
    {
        //(PlanetColorScheme, PlanetColorScheme)[] previousBiomes = new (PlanetColorScheme, PlanetColorScheme)[planetList.Length];
        //for (int i = 0; i < planetList.Length; i++)
        //{
        //    previousBiomes[i].Item1 = planetList[i].biomeA;
        //    previousBiomes[i].Item2 = planetList[i].biomeB;
        //}
        (PlanetColorScheme, PlanetColorScheme) biomes = generator.RandomizeColorScheme(seed, planetType, index);
        generator.ClearAllTrees();
        generator.UpdateAndApplyAll();

        GameObject newPlanet = ConvertToImage(generator.gameObject, "ImagingScene" + seed.x);
        newPlanet.transform.position = new Vector3(position.x + maxSize / 2, position.y + maxSize / 2, position.z);
        newPlanet.transform.localScale = Vector3.one * size * planetScale * 0.2f;
        return new Planet(newPlanet, planetType, biomes.Item1, biomes.Item2, gameObject);
    }
    /// <summary>
    /// Generates a forcefield for a planet
    /// </summary>
    GameObject GenerateForcefield(Planet planet)
    {
        GameObject forcefield = this.gameObject;
        if (planet.planetType != ColorSchemeStaticReference.PlanetType.blackhole || planet.planetType != ColorSchemeStaticReference.PlanetType.star)
        {
            forcefield = Instantiate(forcefieldPrefab);

            ForceField ff = forcefield.GetComponent<ForceField>();
            ff.enabled = true;
            ff.Init(planet.planet.transform);
        }
        return forcefield;
    }
    GameObject GenerateStar(float size, int index, Vector3 position, Vector3 seed, ColorSchemeStaticReference.PlanetType planetType)
    {
        Transform star = Instantiate(starPrefab, new Vector3(position.x + maxSize / 2, position.y + maxSize / 2, position.z), Quaternion.identity).transform;
        star.transform.localScale = Vector3.one * size * planetScale * 0.66f;
        star.GetComponent<SunGenerator>().RandomSun(Mathf.RoundToInt(seed.z));
        return star.gameObject;
    }
    /// <summary>
    /// Sets the position for the flag and ball. Random position for the flag, farthest position from flag for the ball.
    /// </summary>
    void SetBallFlagPosition(Transform flag, Transform ball, Planet[] candidates)
    {
        if(candidates.Length == 0) { return; }
        int candidateIndex = UnityEngine.Random.Range(0, Mathf.Max(0,candidates.Length-1));

        int rot = UnityEngine.Random.Range(0, 360);
        flag.eulerAngles = new Vector3(flag.eulerAngles.z, flag.eulerAngles.y, rot);
        bool planetOrRandom = true;
        if(canSpawnOffPlanet){
            planetOrRandom = UnityEngine.Random.Range(0, 2) == 1 ? true : false;
        }
        if(planetOrRandom){
            flag.position = candidates[candidateIndex].forceField.transform.position + flag.up * (candidates[candidateIndex].forceField.transform.localScale.x + 1f);
        }
        else{
            bool intersecting = true;
            for(int u = 0; u < 15 && intersecting; u++){
                bool currentlyIntersecting = true;
                Vector3 position = new Vector3(UnityEngine.Random.value * regionSize.x, UnityEngine.Random.value * regionSize.y, candidates[candidateIndex].forceField.transform.position.z);
                for (int v = 0; v < candidates.Length; v++){
                    if((candidates[v].forceField.transform.position - position).magnitude < candidates[candidateIndex].forceField.transform.localScale.x + 1f){
                        intersecting = true;
                    }
                }
                if (!currentlyIntersecting){
                    intersecting = false;
                    flag.position = position;
                }
            }
        }
        
        // flag.localScale *= Mathf.Clamp(candidates[candidateIndex].forceField.transform.localScale.x *1.5f, 0, 5);
        //farthest planet
        float distTracker = 0;
        int farthestIndex = 0;
        int i = 0;
        foreach (Planet p in candidates)
        {
            float dist = (flag.position - p.planet.transform.position).magnitude;
            if(dist > distTracker)
            {
                farthestIndex = i;
                distTracker = dist;
            }
            i++;
        }
        float flagAng = Mathf.Atan2((candidates[farthestIndex].planet.transform.position - flag.position).normalized.y, (candidates[farthestIndex].planet.transform.position - flag.position).normalized.x);
        flagAng += Mathf.PI * 0.4f * UnityEngine.Random.value;
        ball.GetComponent<Rigidbody>().position = candidates[farthestIndex].forceField.transform.position + new Vector3(Mathf.Cos(flagAng), Mathf.Sin(flagAng)) * (candidates[farthestIndex].forceField.transform.localScale.x + (ball.localScale.x * ball.GetComponent<SphereCollider>().radius));
    }
    #endregion
    #region Utility
    /// <summary>
    /// Takes an image of the camera render texture and creates a planet object with sprites
    /// </summary>
    GameObject ConvertToImage(GameObject selectedObj, string sceneName)
    {
        MeshRenderer oceanRend = selectedObj.transform.Find("Ocean").GetComponent<MeshRenderer>();
        bool activeOcean = oceanRend.gameObject.activeInHierarchy;

        Material oceanMat = oceanRend.material;
        Material cullMat = invisOcean;
        oceanRend.sharedMaterial = cullMat;

        Camera cam = new GameObject().AddComponent<Camera>();
        cam.transform.position = Camera.main.transform.position;
        cam.orthographic = true;
        cam.orthographicSize = 1.5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0, 0, 0, 0);
        cam.cullingMask = LayerMask.GetMask("Planet Front Capture");



        //Front Texture
        RenderTexture frontTexture = new RenderTexture(1024, 1024, 0);
        cam.targetTexture = frontTexture;
        cam.Render();

        Sprite frontSprite = ConvertRenderTextureToSprite(frontTexture);

        //Ocean disabled
        oceanRend.gameObject.SetActive(false);

        //Back Texture
        cam.cullingMask = LayerMask.GetMask("Planet Back Capture", "Planet Front Capture");

        RenderTexture backTexture = new RenderTexture(1024, 1024, 0);
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
        pd.biomeA =  cd.biomeA;
        pd.biomeB =  cd.biomeB;
        pd.oBiomeA = cd.oBiomeA;
        pd.oBiomeB = cd.oBiomeB;
        pd.aBiomeA = cd.aBiomeA;
        pd.aBiomeB = cd.aBiomeB;

        //Front sprite
        GameObject frontObject = new GameObject("frontObject");
        frontObject.transform.parent = planetParent.transform;
        frontObject.transform.localScale = Vector3.one;
        SpriteRenderer frontSR = frontObject.AddComponent<SpriteRenderer>();
        frontSR.sprite = frontSprite;
        frontSR.sortingOrder = 1;

        //Back sprite
        GameObject backObject = new GameObject("backObject");
        backObject.transform.parent = planetParent.transform;
        backObject.transform.position += new Vector3(0, 0, 2);
        backObject.transform.localScale = Vector3.one;
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
        }

        oceanRend.sharedMaterial = oceanMat;
        oceanRend.gameObject.SetActive(true);

        Destroy(cam.gameObject);
        planetParent.SetActive(false);
        return planetParent;
    }
    /// <summary>
    /// Converts a RenderTexture to a Sprite
    /// </summary>
    /// <param name="renderTexture"></param>
    /// <returns></returns>
    public static Sprite ConvertRenderTextureToSprite(RenderTexture renderTexture)
    {
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);

        RenderTexture prevActive = RenderTexture.active;
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
        RenderTexture.active = prevActive;
        return sprite;
    }
    /// <summary>
    /// Calculates the Camera Size
    /// </summary>
    public static Vector2 CamSize(Camera cam){
        float height = cam.GetComponent<CameraFollow>().defaultCameraSize * 2; //camera height in units
        float width = height * cam.aspect;       //camera width in units
        Vector3 camSize = new Vector3(width, height);
        return camSize;
    }
    #endregion
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 1, 0.1f);
        Gizmos.DrawCube(regionSize / 2, regionSize);
    }
    private void OnEnable()
    {
    }
    private void OnDisable()
    {
    }
}
