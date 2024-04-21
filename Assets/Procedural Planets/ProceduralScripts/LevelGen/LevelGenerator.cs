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
    [SerializeField] bool randomSeed = false;
    //[SerializeField] bool sameColourPoolTwice = false;
    [SerializeField] Material invisOcean;
    public int seed;
    //Level gen params
    [SerializeField]
    float minPlanets, maxPlanets, minSize, maxSize, sizeBias, sizeScale, samplesBeforeRejection, offsetScale;
    [SerializeField]
    Vector2 regionExtension;
    [SerializeField]
    GameObject planetGeneratorPrefab, forcefieldPrefab, audioSrcPrefab;
    [SerializeField]
    float planetScale;
    [SerializeField]
    ColorSchemeStaticReference.PlanetType[] possiblePlanetTypes;

    Vector2 regionSize;

    [Serializable]
    public struct Planet{
        public GameObject planet;
        public ColorSchemeStaticReference.PlanetType planetType;
        public PlanetColorScheme biomeA;
        public PlanetColorScheme biomeB;
        public GameObject forceField;
        public Planet(GameObject planet, ColorSchemeStaticReference.PlanetType planetType, PlanetColorScheme biomeA, PlanetColorScheme biomeB, GameObject forceField){
            this.planet = planet;
            this.planetType = planetType;
            this.biomeA = biomeA;
            this.biomeB = biomeB;
            this.forceField = forceField;
        }
    }

    public delegate void LevelInit();
    public static LevelInit levelInit;

    void Start()
    {
        levelInit?.Invoke();
        Camera mainCam = Camera.main;
        if(randomSeed)
        {
            seed = Mathf.RoundToInt(UnityEngine.Random.value * System.DateTime.Now.Millisecond + System.DateTime.Now.Millisecond * System.DateTime.Now.Second + UnityEngine.Random.Range(-1f, 1f) * Time.time);
        }
        UnityEngine.Random.InitState(seed);
        Vector2 camSize = CamSize(mainCam);
        bool xOry = UnityEngine.Random.Range(0, 2) == 1 ? true : false;
        float extensionAmount = UnityEngine.Random.value;
        regionSize = CalculateBounds(camSize, extensionAmount, xOry);
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
        ProceduralLevelGeneration.Level level = ProceduralLevelGeneration.CalculateLevel(seed, possiblePlanetTypes, levelGenSettings);
        for (int i = 0; level.numPlanets == 0 && i < 50; i++)
        {
            level = ProceduralLevelGeneration.CalculateLevel(seed, possiblePlanetTypes, levelGenSettings);
            Debug.LogWarning(seed + " generated no planets. Regenerating planets, iteration: " + i + "/50 | Current Planet Count = " + level.numPlanets);
            seed++;
        }
        Planet[] planets = Generatelevel(level);
        (Vector2, Vector2) startEndPos = CalculateStartEndPos(planets, camSize, extensionAmount, xOry);
        Vector2 startPos = startEndPos.Item1;
        Vector2 endPos = startEndPos.Item2;
        float maxZoom = Mathf.Max((regionSize.x) / (2 * mainCam.aspect), regionSize.y / 2) + 0.5f;

        mainCam.GetComponent<CameraFollow>().InitPositions(startPos, endPos, maxZoom);
    }

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

    Vector2 CalculateBounds(Vector3 camSize, float extensionAmount, bool x){
        //Extend in the x direction maybe?
        
        Vector2 regionSize = new Vector3 (camSize.x + maxSize, camSize.y + maxSize, camSize.z);
        if(x){ regionSize += new Vector2(extensionAmount * regionExtension.x, 0); }
        else{ regionSize += new Vector2(0, extensionAmount * regionExtension.y);}

        return regionSize;
    }
    Planet[] Generatelevel(ProceduralLevelGeneration.Level level){
        FullPlanetGeneration generator = Instantiate(planetGeneratorPrefab).GetComponent<FullPlanetGeneration>();
        Planet[] planetList = new Planet[level.numPlanets];
        Instantiate(audioSrcPrefab);
        Transform env = GameObject.FindGameObjectWithTag("Environment").transform;
        Transform allPlanetParent = new GameObject("Planets").transform;
        for (int i = 0; i < level.numPlanets; i++){
            Vector3 levelSize = new Vector3(level.levelSize.x, level.levelSize.y, 0f);
            planetList[i] = GeneratePlanet(generator, level.planetSizes[i], i, level.planetPoints[i], level.planetSeeds[i], levelSize, level.planetTypes[i], planetList);
            planetList[i].planet.SetActive(true);
            planetList[i].planet.transform.parent = allPlanetParent;
            GameObject forcefield = GenerateForcefield(planetList[i]);
            planetList[i].forceField = forcefield;
            forcefield.transform.parent = env;
            //print(forcefield.transform.localScale.x);
        }
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Transform flag = GameObject.FindGameObjectWithTag("Hole").transform;
        SetBallFlagPosition(flag, player.transform, planetList);
        player.GetComponent<ShootBall>().Init(env, player.GetComponent<Rigidbody>().position);
        return planetList;
    }

    void SetBallFlagPosition(Transform flag, Transform ball, Planet[] candidates)
    {
        int candidateIndex = UnityEngine.Random.Range(0, Mathf.Max(0,candidates.Length-1));
        int rot = UnityEngine.Random.Range(0, 360);
        flag.eulerAngles = new Vector3(flag.eulerAngles.z, flag.eulerAngles.y, rot);
        bool planetOrRandom = UnityEngine.Random.Range(0, 1) == 1 ? true : false;
        if(planetOrRandom){
            flag.position = candidates[candidateIndex].forceField.transform.position + flag.up * (candidates[candidateIndex].forceField.transform.localScale.x + 1f);
        }
        else{
            bool intersecting = true;
            for(int u = 0; u < 100 && intersecting; u++){
                Vector3 position = new Vector3(UnityEngine.Random.value * regionSize.x, UnityEngine.Random.value * regionSize.y, candidates[candidateIndex].forceField.transform.position.z);
                for (int v = 0; v < candidates.Length; v++){
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
    GameObject GenerateForcefield(Planet planet){
        GameObject forcefield = this.gameObject;
        if (planet.planetType != ColorSchemeStaticReference.PlanetType.blackhole || planet.planetType != ColorSchemeStaticReference.PlanetType.star){  
            forcefield = Instantiate(forcefieldPrefab);

            ForceField ff = forcefield.GetComponent<ForceField>();
            ff.enabled = true;
            ff.Init(planet.planet.transform);
        }
        return forcefield;
    }

    Planet GeneratePlanet(FullPlanetGeneration generator, float size, int index, Vector3 position, Vector3 seed, Vector3 levelSize, ColorSchemeStaticReference.PlanetType planetType, Planet[] previousPlanets)
    {
        (PlanetColorScheme, PlanetColorScheme)[] previousBiomes = new (PlanetColorScheme, PlanetColorScheme)[previousPlanets.Length];
        for (int i = 0; i < previousPlanets.Length; i++){
            previousBiomes[i].Item1 = previousPlanets[i].biomeA;
            previousBiomes[i].Item2 = previousPlanets[i].biomeB;
        }
        (PlanetColorScheme, PlanetColorScheme) biomes = generator.RandomizeColorScheme(seed, planetType, index);
        generator.ClearAllTrees();
        generator.UpdateAndApplyAll();

        GameObject newPlanet = ConvertToImage(generator.gameObject, "ImagingScene" + seed.x);
        newPlanet.transform.position = new Vector3 (position.x + maxSize/2, position.y + maxSize/2, position.z);
        newPlanet.transform.localScale = Vector3.one * size * planetScale * 0.2f;
        return new Planet(newPlanet, planetType, biomes.Item1, biomes.Item2, gameObject);
    }

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


        Scene currentScene = SceneManager.GetActiveScene();
        Scene emptyScene = SceneManager.CreateScene(sceneName);
        SceneManager.MoveGameObjectToScene(cam.gameObject, emptyScene);
        SceneManager.MoveGameObjectToScene(selectedObj, emptyScene);
        SceneManager.SetActiveScene(currentScene);

        //Front Texture
        RenderTexture frontTexture = new RenderTexture(1000, 1000, 0);
        cam.targetTexture = frontTexture;
        cam.Render();

        Sprite frontSprite = ConvertRenderTextureToSprite(frontTexture);

        //Ocean disabled
        oceanRend.gameObject.SetActive(false);

        //Back Texture
        cam.cullingMask = LayerMask.GetMask("Planet Back Capture", "Planet Front Capture");

        RenderTexture backTexture = new RenderTexture(1000, 1000, 0);
        cam.targetTexture = backTexture;
        cam.Render();

        Sprite backSprite = ConvertRenderTextureToSprite(backTexture);
        backSprite.name = "dsad";

        //Creation of prefab object
        
        //Parent planet
        GameObject planetParent = new GameObject("Planet Parent");
        planetParent.transform.position = Vector3.zero;
        planetParent.transform.localScale = Vector3.one * 0.3f;
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
        ocean.transform.localScale = Vector3.one * 6.9f;
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
        SceneManager.UnloadSceneAsync(emptyScene);
        planetParent.SetActive(false);
        return planetParent;
    }
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
    public static Vector2 CamSize(Camera cam){
        float height = cam.GetComponent<CameraFollow>().defaultCameraSize * 2; //camera height in units
        float width = height * cam.aspect;       //camera width in units
        Vector3 camSize = new Vector3(width, height);
        return camSize;
    }

}
