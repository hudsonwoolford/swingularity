using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using System;
using Unity.VisualScripting;
using System.Linq;
using static ProceduralLevelGeneration;
using System.Reflection;

public class LevelGenerator : MonoBehaviour
{
    #region Variables
    #region Type Definitions
    [Serializable]
    public struct SpecialObject
    {
        public SpecialType type;
        public int amount;
        public GenerateSpecialPlanetObject replaceGeneration;
        public GenerateSpecialObject addGeneration;
        public SpecialObject(SpecialType type, int amount, GenerateSpecialPlanetObject replaceGeneration, GenerateSpecialObject addGeneration)
        {
            this.type = type;
            this.amount = amount;
            this.replaceGeneration = replaceGeneration;
            this.addGeneration = addGeneration;
        }
    }
    [Serializable]
    public enum SpecialType
    {
        Moon,
        Star,
        NeutronStar,
        DarkMatter,
        BlackHole,
        Quasar
    }
    [Serializable]
    public struct Planet
    {
        public GameObject planet;
        public ColorSchemeStaticReference.PlanetType planetType;
        public PlanetColorScheme biomeA;
        public PlanetColorScheme biomeB;
        public GameObject forceField;
        //Constructor
        public Planet(GameObject planet, ColorSchemeStaticReference.PlanetType planetType, PlanetColorScheme biomeA, PlanetColorScheme biomeB, GameObject forceField)
        {
            this.planet = planet;
            this.planetType = planetType;
            this.biomeA = biomeA;
            this.biomeB = biomeB;
            this.forceField = forceField;
        }
    }
    public delegate void GenerateSpecialPlanetObject(ref Planet planet, float size, Vector3 position, Vector3 seed, ColorSchemeStaticReference.PlanetType planetType, FullPlanetGeneration generator, int index);
    public delegate void GenerateSpecialObject(Planet[] planets, Vector3 seed, int index);
    public delegate void LevelInit(GalaxyData theme); //Delegate called in initiation of procedural level, this coccurs before the level is actually generated
    public static LevelInit levelInit;
    public delegate void LevelGenerated();
    public static LevelGenerated levelGenerated; //Delegate called in the final generation of the level, this occurs after the level is actually generated

    #endregion //Includes Delegates
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
    public static LevelGenerator Instance;
    public float generationProgress { get; private set; } = 0;
    [Header("Level Parameters")]
    public int seed; //The seed for the level
    //Level gen params
    float minPlanets, maxPlanets;
    [SerializeField] int resolution;

    [SerializeField] Vector2 regionSize;
    [SerializeField] ColorSchemeStaticReference.PlanetType[] possiblePlanetTypes; //Filters which planet types are chosen
    [SerializeField] SpecialObject[] specialObjects;
    [SerializeField] bool generateCollectible;

    [Header("Constant Variables")]
    [SerializeField] LevelGenStaticReferences levelStatic;
    [Header("Special Prefabs")]
    [SerializeField] GameObject planetGeneratorPrefab;
    [SerializeField] GameObject forcefieldPrefab, starPrefab, neutronStarPrefab, blackHolePrefab, quasarPrefab, darkMatterPrefab, audioSrcPrefab, astronaut, starCollectiblePrefab; //Needed prefabs

    //Private variables
    Transform specialObjectsParent;
    RenderTexture frontTexture, backTexture;
    Texture2D planetTexture;
    Transform environment, allPlanetParent;
    Camera mainCam;
    Vector2 camSize;
    bool xOry;

    #endregion
    private void Awake()
    {
        generationProgress = 0;
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        generationProgress = 0;
        LevelManager.notifyProcedural?.Invoke(true);
        SetLevelParams();
        mainCam = Camera.main; //Cache camera
        resolution = SettingsManager.userSettings.planetRes;
        frontTexture = new RenderTexture(resolution, resolution, 0);
        backTexture = new RenderTexture(resolution, resolution, 0);
        planetTexture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);

        SetSpecialDelegates();
        //Randomising or keeping seed
        UnityEngine.Random.InitState(seed);

        int typeIndex = UnityEngine.Random.Range(0, possiblePlanetTypes.Length);
        bool init = false;
        for (int i = 0; i < levelStatic.possibleGalaxySchemes.Length; i++)
        {
            if(levelStatic.possibleGalaxySchemes[i].type == possiblePlanetTypes[typeIndex])
            {
                init = true;
                levelInit?.Invoke(levelStatic.possibleGalaxySchemes[i]); //Initiation delegate
                break;
            }
        }
        if(!init) {levelInit?.Invoke(levelStatic.possibleGalaxySchemes[0]);}  //Initiation failsafe

        //Sets the level generation settings
        float[] levelGenSettings = new float[10]{
            minPlanets,
            maxPlanets,
            levelStatic.minSize,
            levelStatic.maxSize,
            levelStatic.sizeBias,
            levelStatic.sizeScale,
            (regionSize.x - levelStatic.maxSize/2),
            (regionSize.y - levelStatic.maxSize/2),
            levelStatic.samplesBeforeRejection,
            levelStatic.offsetScale
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
        StartCoroutine(Generatelevel(level));
    }
    #region Variable Setting

    void SetLevelParams(){ //Calculates region size and special objects
        LevelManager.LevelParams levelParams = GameManager.LevelManager.levelParams;
        UnityEngine.Random.InitState(levelParams.seed);
        seed = levelParams.seed;

        levelStatic = LevelManager.levelStatic;

        float specialObjMultiplier = levelStatic.specialObjectMultipliers[levelParams.difficulty - 1] * 1.75f;

        possiblePlanetTypes = levelParams.themes;

        int numSpecialObj = Mathf.RoundToInt((UnityEngine.Random.value/4 + 0.8825f) * specialObjMultiplier);
        int[] numPerObj = new int[levelParams.specialObjects.Length];   

        for (int i = 0; i < numSpecialObj; i++){
            UnityEngine.Random.InitState(levelParams.seed + i*UnityEngine.Random.Range(-100, 100));
            int index = UnityEngine.Random.Range(0,levelParams.specialObjects.Length);
            numPerObj[index]++;
            
        }
        specialObjects = new SpecialObject[levelParams.specialObjects.Length];
        LevelGenStaticReferences.SpecialObjectMultWrapper[] inherentMult = levelStatic.inherentSpecialMultipliers;
        for (int i = 0; i < levelParams.specialObjects.Length; i++)
        {

            Vector2 inherentMultiplier = Array.Find(inherentMult, t => t.specialType == levelParams.specialObjects[i]).minMaxMultiplier;
            SpecialObject newSpecialObject = new SpecialObject(levelParams.specialObjects[i], Mathf.RoundToInt(numPerObj[i] * UnityEngine.Random.Range(inherentMultiplier.x, inherentMultiplier.y)), GenerateStar, GenerateQuasar);
            specialObjects[i] = newSpecialObject;
        }

        int randIndex = UnityEngine.Random.Range(0, levelStatic.regionSizes[levelParams.difficulty - 1].nestedRegionSizes.Count);
        regionSize = levelStatic.regionSizes[levelParams.difficulty - 1].nestedRegionSizes[randIndex] * levelStatic.regionSizes[levelParams.difficulty - 1].baseMultiplier; //Random region size from difficulty
        if(regionSize.magnitude < 2.5f) { regionSize = Vector2.one * 10; }

        generateCollectible = levelParams.hasCollectible;
    }

    #endregion
    #region Level Calculation
   
    /// <summary>
    /// Calculates the start pos and end pos for the camera. Takes an average of planet positions.
    /// </summary>
    (Vector2, Vector2) CalculateStartEndPos(Planet[] planets, Transform flag, Transform ball){
        Vector3 avg = Vector3.zero;
        foreach (Planet planet in planets){
            avg += planet.planet.transform.position;             
        }
        avg /= planets.Length;
        Vector2 planetsCenter = avg;
        Vector2 startPos = planetsCenter;
        Vector2 endPos = startPos;


        endPos = new Vector2(flag.position.x, planetsCenter.y);
        startPos = new Vector2(ball.position.x, planetsCenter.y);
        return (startPos, endPos);
    }
    /// <summary>
    /// Calculates what planet indices to replace with special object
    /// </summary>
    /// <returns>An index and special object</returns>
    (int, SpecialObject)[] CalculateReplaceIndices(ProceduralLevelGeneration.Level level, int[] ignoreIndices)
    {
        List<(int, SpecialObject)> replaceObjects = new List<(int, SpecialObject)>(specialObjects.Length); //Takes all the special object that replace a planet
        int numSpecial = 0; //number of objects
        int moonIndex = -1;
        for (int i = 0; i < specialObjects.Length; i++) //Iterates through current objects set in inspector
        {
            //Filters on whether they are replace objects or not
            if (specialObjects[i].type == (SpecialType.Star) || specialObjects[i].type == (SpecialType.NeutronStar))
            {
                numSpecial += specialObjects[i].amount;
                replaceObjects.Add((specialObjects[i].amount, specialObjects[i]));
            }
            else if (specialObjects[i].type == (SpecialType.Moon))
            {
                numSpecial += specialObjects[i].amount;
                moonIndex = replaceObjects.Count;
                replaceObjects.Add((specialObjects[i].amount, specialObjects[i]));
            }
            else if (specialObjects[i].type == SpecialType.BlackHole)
            {
                numSpecial += specialObjects[i].amount * 2;
                replaceObjects.Add((specialObjects[i].amount, specialObjects[i]));
            }
            else if (specialObjects[i].type == SpecialType.DarkMatter)
            {
                numSpecial += specialObjects[i].amount;
                replaceObjects.Add((specialObjects[i].amount, specialObjects[i]));
            }
        }
        numSpecial = Mathf.Clamp(numSpecial, 0, level.numPlanets - 2); //Allow space for 2 normal planets

        List<(int, SpecialObject)> replacers = new List<(int, SpecialObject)>(); //Preparing return array
        List<int> availableIndices = new List<int>(); //Prevents overlapping indices
        for (int i = 0; i < level.numPlanets; i++)
        {
            if (i != ignoreIndices[0] && i != ignoreIndices[1])
            {
                availableIndices.Add(i);
            }
        }

        //Moon priority to bigger planets
        if (moonIndex >= 0 && specialObjects[moonIndex].amount > 0)
        {
            int moonAmount = 0;
            List<int> removeIndices = new List<int>();
            for (int u = 0; u < availableIndices.Count; u++)
            {
                if (level.planetSizes[availableIndices[u]] > levelStatic.moonCutoffSize)
                {
                    replacers.Add((availableIndices[u], replaceObjects[moonIndex].Item2));
                    replaceObjects[moonIndex] = (replaceObjects[moonIndex].Item1 - 1, replaceObjects[moonIndex].Item2);
                    removeIndices.Add(availableIndices[u]);
                    moonAmount++;
                    if (replaceObjects[moonIndex].Item1 <= 0)
                    {
                        break;
                    }
                }
            }
            numSpecial -= moonAmount;
            replaceObjects.Remove(replaceObjects[moonIndex]);
            for (int i = 0; i < removeIndices.Count; i++)
            {
                availableIndices.Remove(removeIndices[i]);
            }
        }



        //Setting the return array
        for (int i = 0; i < numSpecial; i++)
        {
            if (replaceObjects.Count == 0) { break; } //no more objects to choose from

            //Choosing random special object
            int randomSpecial = UnityEngine.Random.Range(0, replaceObjects.Count);
            SpecialObject chosenObject = replaceObjects[randomSpecial].Item2;

            //if enough space is left for black hole to spawn
            if (chosenObject.type == SpecialType.BlackHole && numSpecial - i < 2)
            {
                replaceObjects.Remove(replaceObjects[randomSpecial]);
                if (replaceObjects.Count == 0) { break; } //no more objects to choose from
                randomSpecial = UnityEngine.Random.Range(0, replaceObjects.Count);
                chosenObject = replaceObjects[randomSpecial].Item2;
            }

            //Determining replacement index and adding object
            int index = availableIndices[UnityEngine.Random.Range(0, availableIndices.Count)];
            availableIndices.Remove(index);
            replacers.Add((index, chosenObject));

            //Subtracting from amount on that object
            replaceObjects[randomSpecial] = (replaceObjects[randomSpecial].Item1 - 1, replaceObjects[randomSpecial].Item2);
            if (replaceObjects[randomSpecial].Item1 <= 0)
            {
                replaceObjects.Remove(replaceObjects[randomSpecial]);
            }

            //Doubling with black holes
            if (chosenObject.type == SpecialType.BlackHole)
            {
                index = availableIndices[UnityEngine.Random.Range(0, availableIndices.Count)];
                availableIndices.Remove(index);
                replacers.Add((index, chosenObject));
                i++;
            }
        }
        //Goop planets are not limited to planets - 2
        availableIndices = ignoreIndices.ToList();
        for (int i = 0; i < replaceObjects.Count; i++)
        {
            if (replaceObjects[i].Item2.type == SpecialType.DarkMatter)
            {
                for (int u = 0; u < replaceObjects[i].Item1 && u < 2; u++)
                {
                    int index = availableIndices[UnityEngine.Random.Range(0, availableIndices.Count)];
                    availableIndices.Remove(index);
                    replacers.Add((index, replaceObjects[i].Item2));
                }
            }
        }

        return replacers.ToArray();
    }
    /// <summary>
    /// Calculates which objects in special objects array are adding to the level instead of replacing planets
    /// </summary>
    /// <returns></returns>
    SpecialObject[] CalculateAddObjects()
    {
        List<SpecialObject> list = new List<SpecialObject>();
        for (int i = 0; i < specialObjects.Length; i++)
        {
            if (specialObjects[i].type == SpecialType.Quasar)
            {
                list.Add(specialObjects[i]);
            }
        }
        return list.ToArray();
    }
    #endregion
    #region Level Generation
    /// <summary>
    /// Generates the planets off of the level parameters
    /// </summary>
    IEnumerator Generatelevel(ProceduralLevelGeneration.Level level){
        generationProgress = 0;
        Time.timeScale = 0; 
        FullPlanetGeneration generator = Instantiate(planetGeneratorPrefab).GetComponent<FullPlanetGeneration>();
        Planet[] planetList = new Planet[level.numPlanets];
        Instantiate(audioSrcPrefab);
        environment = GameObject.FindGameObjectWithTag("Environment").transform;
        specialObjectsParent = new GameObject("Special Objects").transform;
        allPlanetParent = new GameObject("Planets"){
            tag = "PlanetParent"
        }.transform;
        Vector3 levelSize = new Vector3(level.levelSize.x, level.levelSize.y, 0f);
        
        int[] ignoreIndices = FarthestIndices(level);
        //Replacement special objects
        (int, SpecialObject)[] replacers = CalculateReplaceIndices(level, ignoreIndices);
        Array.Sort(replacers, (x, y) => CompareEnums(x, y));
        int[] replaceIndices = Array.ConvertAll(replacers, r => r.Item1);

        //add objects, used later in the coroutine
        SpecialObject[] addObjects = CalculateAddObjects();

        int totalObjects = level.numPlanets + addObjects.Length + 1; //used for progress
        for (int i = 0; i < replacers.Length; i++)
        {
            generationProgress += 1f/totalObjects;
            replacers[i].Item2.replaceGeneration(ref planetList[replaceIndices[i]], level.planetSizes[replaceIndices[i]], level.planetPoints[replaceIndices[i]], level.planetSeeds[replaceIndices[i]], level.planetTypes[replaceIndices[i]], generator, i);
            yield return new WaitForSecondsRealtime(0.1f);
        }
        for (int i = 0; i < level.numPlanets; i++){
            if(replaceIndices.Contains(i)){continue;}
            generationProgress += 1f / totalObjects;
            planetList[i] = GeneratePlanet(generator, level.planetSizes[i], i, level.planetPoints[i], level.planetSeeds[i], level.planetTypes[i]);

            yield return new WaitForSecondsRealtime(0.2f);
        }
        Destroy(generator.gameObject);

        List<Planet> normPlanets = new List<Planet>();
        for (int i = 0; i < planetList.Length; i++)
        {
            if (planetList[i].planetType != ColorSchemeStaticReference.PlanetType.special)
            {
                normPlanets.Add(planetList[i]);
            }
        }
        //Additional special objects
        for (int i = 0; i < addObjects.Length; i++)
        {
            generationProgress += 1f / totalObjects;
            for (int u = 0; u < addObjects[i].amount; u++)
            {
                addObjects[i].addGeneration(normPlanets.ToArray(), level.planetSeeds[0], u + i * 1000);
                yield return new WaitForSecondsRealtime(0.1f);
            }
        }
        //Ball and flag positions
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Transform flag = GameObject.FindGameObjectWithTag("Hole").transform;

        SetBallFlagPositionFarthest(flag, player.transform, planetList, ignoreIndices);
        Physics.SyncTransforms();
        player.GetComponent<ShootBall>().Init(environment, player.GetComponent<Rigidbody>().position);

        GenerateBlackHoles(player.transform.position);


        //Camera start and end positions
        (Vector2, Vector2) startEndPos = CalculateStartEndPos(planetList, flag, player.transform);
        Vector2 startPos = startEndPos.Item1;
        Vector2 endPos = startEndPos.Item2;

        mainCam.GetComponent<CameraFollow>().InitPositions(startPos, endPos, 1);
        float maxZoom = Mathf.Min(CameraFollow.innerShape.bounds.extents.x / (mainCam.aspect), CameraFollow.innerShape.bounds.extents.y) - 5;
        mainCam.GetComponent<CameraFollow>().InitPositions(startPos, endPos, maxZoom);

        if(generateCollectible)
        {
            GenerateCollectible((startPos + endPos) / 2);
        }
        generationProgress = 1;
        Time.timeScale = 1;
        levelGenerated?.Invoke();
    }

    /// <summary>
    /// Generates a planet using FullPlanetGeneration
    /// </summary>
    Planet GeneratePlanet(FullPlanetGeneration generator, float size, int index, Vector3 position, Vector3 seed, ColorSchemeStaticReference.PlanetType planetType)
    {
        UnityEngine.Random.InitState((int)seed.z + index * 100);
        //(PlanetColorScheme, PlanetColorScheme)[] previousBiomes = new (PlanetColorScheme, PlanetColorScheme)[planetList.Length];
        //for (int i = 0; i < planetList.Length; i++)
        //{
        //    previousBiomes[i].Item1 = planetList[i].biomeA;
        //    previousBiomes[i].Item2 = planetList[i].biomeB;
        //}
        (PlanetColorScheme, PlanetColorScheme) biomes = generator.RandomizeColorScheme(seed, planetType, index + 1, false);
        generator.ClearAllTrees();
        generator.UpdateAndApplyAll();
        if(generator.biomeP == null)
        {
            generator.transform.rotation = Quaternion.Euler(0,0,UnityEngine.Random.Range(-180, 180));
        }
        else
        {
            generator.transform.rotation = Quaternion.identity;
        }


        GameObject newPlanet = ConvertToImage(generator.gameObject, position, size);
        frontTexture.Release(); //Memory
        backTexture.Release();  //Memory
        newPlanet.SetActive(true);

        newPlanet.transform.position = new Vector3(position.x + levelStatic.maxSize / 2, position.y + levelStatic.maxSize / 2, position.z);
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
        GameObject forcefield = this.gameObject;
        forcefield = Instantiate(forcefieldPrefab);

        ForceField ff = forcefield.GetComponent<ForceField>();
        ff.enabled = true;
        ff.Init(planet);
        return forcefield;
    }
    #region Special Generation
    void GenerateCollectible(Vector3 midPoint)
    {
        StarCollectible sc = Instantiate(starCollectiblePrefab, environment).GetComponent<StarCollectible>();
        sc.Init(midPoint, seed);
    }
    List<Planet> cachedBlackHoles = new List<Planet>();
    void CacheBlackHole(ref Planet planet, float size, Vector3 position, Vector3 seed, ColorSchemeStaticReference.PlanetType planetType, FullPlanetGeneration generator, int index)
    {
        Transform blackHole = Instantiate(blackHolePrefab, new Vector3(position.x + levelStatic.maxSize / 2, position.y + levelStatic.maxSize / 2, position.z), Quaternion.identity).transform;
        blackHole.localScale = Vector3.one * size * levelStatic.planetScale * 0.5f;
        blackHole.parent = specialObjectsParent;

        planet.planet = blackHole.gameObject;
        planet.planetType = planetType;
        cachedBlackHoles.Add(planet);
        planet.planetType = ColorSchemeStaticReference.PlanetType.special;

        planet.forceField = null;
    }
    void GenerateBlackHoles(Vector3 ballPos)
    {
        for (int i = 0; i < cachedBlackHoles.Count; i+=2)
        {
            bool firstBlack = (cachedBlackHoles[i].planet.transform.position - ballPos).magnitude < (cachedBlackHoles[i+1].planet.transform.position - ballPos).magnitude ? false : true;
            cachedBlackHoles[i].planet.GetComponent<BlackHoleGenerator>().InitRandom(firstBlack, cachedBlackHoles[i + 1].planet.transform, seed + i, cachedBlackHoles[i].planetType);
            cachedBlackHoles[i+1].planet.GetComponent<BlackHoleGenerator>().InitRandom(!firstBlack, cachedBlackHoles[i].planet.transform, seed + i, cachedBlackHoles[i].planetType);
        }
        cachedBlackHoles.Clear();
    }
    void GenerateQuasar(Planet[] planets, Vector3 seed, int index)
    {
        UnityEngine.Random.InitState((int)seed.y + index);
        Physics.SyncTransforms();

        //shuffle planet array
        int planetLength = planets.Length;
        while (planetLength > 1)
        {
            planetLength--;
            int k = UnityEngine.Random.Range(0, planetLength + 1);
            Planet pSwap = planets[k];
            planets[k] = planets[planetLength];
            planets[planetLength] = pSwap;
        }

        int index1 = 0;
        int index2 = 0;
        bool foundPlanet = false;
        for (int i = 0; i < planets.Length; i++)
        {
            for (int u = 0; u < planets.Length; u++)
            {
                if(u == i) { continue; }

                Vector3 dirTemp = planets[u].planet.transform.position - planets[i].planet.transform.position;
                float distanceTemp = dirTemp.magnitude;
                if (distanceTemp < 6) { continue; }
                dirTemp = dirTemp.normalized;

                RaycastHit[] allHits = Physics.BoxCastAll(planets[i].planet.transform.position, Vector3.one * 3, dirTemp, Quaternion.LookRotation(dirTemp), distanceTemp);
                if(allHits.Length <= 2)
                {
                    foundPlanet = true;
                    index1 = i;
                    index2 = u;
                    break;
                }
            }
            if(foundPlanet)
            {
                break;
            }
        }
        if(!foundPlanet) { Debug.Log("No quasar eligible planets."); return; }

        Vector3 dir = planets[index2].planet.transform.position - planets[index1].planet.transform.position;
        float distance = dir.magnitude - (planets[index2].forceField.transform.localScale.x + planets[index1].forceField.transform.localScale.x);
        dir = dir.normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
        Vector3 midPoint = ((planets[index1].planet.transform.position + planets[index1].forceField.transform.localScale.x * dir) + (planets[index2].planet.transform.position - planets[index2].forceField.transform.localScale.x * dir)) / 2;

        QuasarGenerator quasar = Instantiate(quasarPrefab, specialObjectsParent).GetComponent<QuasarGenerator>();
        quasar.transform.parent = specialObjectsParent;
        quasar.InitRandom((int)seed.x + index, planets[index1].planetType, midPoint, angle, distance/4 + distance/64, planets[index1].forceField.GetComponent<ForceField>(), planets[index2].forceField.GetComponent<ForceField>());
    }
    void GenerateDarkMatter(ref Planet planet, float size, Vector3 position, Vector3 seed, ColorSchemeStaticReference.PlanetType planetType, FullPlanetGeneration generator, int index)
    {
        UnityEngine.Random.InitState((int)seed.z + index);
        planet = GeneratePlanet(generator, size, index, position, seed, planetType);

        DarkMatterGenerator darkMatter = Instantiate(darkMatterPrefab, new Vector3(position.x + levelStatic.maxSize / 2, position.y + levelStatic.maxSize / 2, position.z), Quaternion.identity, specialObjectsParent).GetComponent<DarkMatterGenerator>();
        darkMatter.Init(planet.planet.GetComponent<PlanetDataHolder>(), planet.forceField);
    }
    void GenerateStar(ref Planet planet, float size, Vector3 position, Vector3 seed, ColorSchemeStaticReference.PlanetType planetType, FullPlanetGeneration generator, int index)
    {
        Transform star = Instantiate(starPrefab, new Vector3(position.x + levelStatic.maxSize / 2, position.y + levelStatic.maxSize / 2, position.z), Quaternion.identity).transform;
        star.transform.localScale = Vector3.one * size * levelStatic.planetScale * 0.66f;
        star.parent = environment;
        star.GetComponent<SunGenerator>().RandomSun(Mathf.RoundToInt(seed.z + index * 1000), planetType);
        planet.planet = star.gameObject;
        planet.planetType = ColorSchemeStaticReference.PlanetType.special;
        planet.forceField = null;
    }
    void GenerateNeutronStar(ref Planet planet, float size, Vector3 position, Vector3 seed, ColorSchemeStaticReference.PlanetType planetType, FullPlanetGeneration generator, int index)
    {
        Transform star = Instantiate(neutronStarPrefab, new Vector3(position.x + levelStatic.maxSize / 2, position.y +levelStatic. maxSize / 2, position.z), Quaternion.identity).transform;
        star.transform.localScale = Vector3.one * size * levelStatic.planetScale * 0.25f;
        star.parent = environment;
        star.GetComponent<NeutronStarGenerator>().RandomSun(Mathf.RoundToInt(seed.z + index * 1000), planetType);
        planet.planet = star.gameObject;
        planet.planetType = ColorSchemeStaticReference.PlanetType.special;
        planet.forceField = null;
    }
    void GenerateMoon(ref Planet planet, float size, Vector3 position, Vector3 seed, ColorSchemeStaticReference.PlanetType planetType, FullPlanetGeneration generator, int index)
    {   
        UnityEngine.Random.InitState((int)(seed.y + index));
        float eccentricity = UnityEngine.Random.Range(0f, 0.2f);
        float smallMoonSize = size * levelStatic.planetScale / 1.75f;
        float apoapsis = size * levelStatic.planetScale - smallMoonSize + 1.2f;

        
        float bigPlanetDecreasedSize = apoapsis * levelStatic.moonDstScale - smallMoonSize;
        //Generate orbitee planet
        Planet bigPlanet = GeneratePlanet(generator, bigPlanetDecreasedSize, index, position, seed, planetType);
        bigPlanet.planet.transform.parent = GameObject.FindGameObjectWithTag("PlanetParent").transform;
        bigPlanet.forceField.transform.parent = GameObject.FindGameObjectWithTag("Environment").transform;

        //Generate orbiter planet
        Planet smallMoon = CreateMoonMesh(generator, smallMoonSize, index, position, seed + (index + 1) * new Vector3 (levelStatic.offsetScale * 2, levelStatic.offsetScale * 2, levelStatic.offsetScale * 2) + Vector3.one, planetType);
        smallMoon.planet.transform.parent = GameObject.FindGameObjectWithTag("PlanetParent").transform;
        smallMoon.forceField.transform.parent = GameObject.FindGameObjectWithTag("Environment").transform;

        MoonManager moonScript = smallMoon.planet.AddComponent<MoonManager>();
        
        float semiMajorAxis = apoapsis/(1 - eccentricity);
        float speed = UnityEngine.Random.Range(0.5f, 1.2f);
        float rotation = UnityEngine.Random.Range(0,360);
        smallMoon.forceField.GetComponent<ForceField>().AutoMass();
        smallMoon.forceField.GetComponent<Collider>().material = levelStatic.superBouncyMoon;
        bigPlanet.forceField.GetComponent<ForceField>().AutoMass();
        moonScript.Init(semiMajorAxis, ArtificialGravity.G, eccentricity, speed, smallMoon.forceField.GetComponent<CelestialMass>(), bigPlanet.forceField.GetComponent<CelestialMass>(), levelStatic.moonLine, rotation, (int)seed.x);

        bigPlanet.planetType = ColorSchemeStaticReference.PlanetType.special;
        planet = bigPlanet;

    }
    Planet CreateMoonMesh(FullPlanetGeneration generator, float size, int index, Vector3 position, Vector3 seed, ColorSchemeStaticReference.PlanetType planetType){
        
        (PlanetColorScheme, PlanetColorScheme) biomes = generator.RandomizeColorScheme(seed, planetType, index + 1495, true);
        generator.UpdateAndApplyAll();
        generator.ClearAllTrees();
        GameObject genObj = Instantiate(generator.gameObject);
        MeshRenderer genRenderer = genObj.GetComponent<MeshRenderer>();
        Material customMoonMat = new Material(genRenderer.material);
        genRenderer.material = customMoonMat;

        Destroy(genObj.GetComponent<FullPlanetGeneration>());
        if(genObj.GetComponentInChildren<Camera>() != null)
        {
            Destroy(genObj.GetComponentInChildren<Camera>().gameObject); //camera on moon generator
        }
        genObj.transform.position = new Vector3(position.x + levelStatic.maxSize / 2, position.y + levelStatic.maxSize / 2, position.z);
        genObj.transform.localScale = Vector3.one * size * levelStatic.planetScale * 0.6f;
        GameObject forceField = GenerateForcefield(genObj.transform);
        return new Planet(genObj, ColorSchemeStaticReference.PlanetType.special, biomes.Item1, biomes.Item2, forceField);
    }

    #endregion
    /// <summary>
    /// Sets the position for the flag and ball. Farthest position possible for flag and ball.
    /// </summary>
    void SetBallFlagPositionFarthest(Transform flag, Transform ball, Planet[] candidates, int[] farthestPlanets)
    {
        Physics.SyncTransforms();
        Vector2 ballToFlagPlanet = candidates[farthestPlanets[0]].forceField.transform.position - candidates[farthestPlanets[1]].forceField.transform.position;
        //Flag position
        UnityEngine.Random.InitState(seed);
        float flagRad = flag.GetComponent<SphereCollider>().radius * flag.localScale.x;
        float flagRot = Mathf.Atan2(-ballToFlagPlanet.y, -ballToFlagPlanet.x) + UnityEngine.Random.Range(-Mathf.PI/2, Mathf.PI/2);
        Vector3 initFlagPos = Vector3.zero;
        for (int u = 0; u < 6; u++)
        {
            Vector3 angleVector = new Vector3(Mathf.Cos(flagRot), Mathf.Sin(flagRot), 0);
            initFlagPos = candidates[farthestPlanets[0]].forceField.transform.position + angleVector * (candidates[farthestPlanets[0]].forceField.transform.localScale.x + flagRad + 0.05f); //maybe scale with radius
            if (!Physics.CheckSphere(initFlagPos, flagRad))
            {
                break;
            }
            Debug.LogFormat("Flag position intercepted collider. Rotation #{0} | Current rotation: {1} radians. Check scene view to see iterated ball positions.", u, flagRot);
            ExtensionMethods.DrawSquare(initFlagPos, Vector2.one, Color.red, 100);
            flagRot += 2 * Mathf.PI / 7;
        }
        flag.position = initFlagPos;
        //Ball position
        UnityEngine.Random.InitState(seed + 9);
        float ballRad = ball.GetComponent<SphereCollider>().radius * ball.localScale.x;
        float ballRot = Mathf.Atan2(ballToFlagPlanet.y, ballToFlagPlanet.x) + UnityEngine.Random.Range(-Mathf.PI / 2, Mathf.PI / 2);
        Vector3 initBallPos = candidates[farthestPlanets[1]].forceField.transform.position + new Vector3(Mathf.Cos(ballRot), Mathf.Sin(ballRot)) * (candidates[farthestPlanets[1]].forceField.transform.localScale.x + ballRad);
        for (int u = 0; u < 6; u++)
        {
            Vector3 angleVector = new Vector3(Mathf.Cos(ballRot), Mathf.Sin(ballRot), 0);
            initBallPos = candidates[farthestPlanets[1]].forceField.transform.position + angleVector * (candidates[farthestPlanets[1]].forceField.transform.localScale.x + ballRad);
            if (!Physics.CheckSphere(initBallPos, ballRad - 0.05f))
            {
                break;
            }
            Debug.LogFormat("Ball position intercepted collider. Rotation #{0} | Current rotation: {1} radians. Check scene view to see iterated ball positions.", u, ballRot);
            ExtensionMethods.DrawSquare(initBallPos, Vector2.one, Color.red, 100);
            ballRot += 2 * Mathf.PI / 7;
        }
        ball.GetComponent<Rigidbody>().position = initBallPos;
    }
    
    int[] FarthestIndices(Level level){
        Physics.SyncTransforms();
        UnityEngine.Random.InitState(seed);
        if (level.planetPoints.Length == 0) { return new int[0]; }
        int[] farthestPlanets = new int[2];
        float farthestDistance = 0;
        for (int x = 0; x < level.planetPoints.Length; x++)
        {
            for (int y = 0; y < level.planetPoints.Length; y++)
            {
                if(y == x) { continue; }
                float distance = Vector2.Distance(level.planetPoints[x], level.planetPoints[y]);
                if  (distance > farthestDistance)
                {
                    farthestDistance = distance;
                    if(UnityEngine.Random.Range(0, 1) == 0){
                        farthestPlanets[0] = x; farthestPlanets[1] = y;
                    }
                    else{
                        farthestPlanets[0] = y; farthestPlanets[1] = x;
                    }
                }
            }
        }
        return farthestPlanets;
    }
    #endregion
    #region Utility
    /// <summary>
    /// Sets the delegates for generation in each special object
    /// </summary>
    void SetSpecialDelegates()
    {
        for (int i = 0; i < specialObjects.Length; i++)
        {
            switch (specialObjects[i].type)
            {
                case SpecialType.Star:
                    specialObjects[i].replaceGeneration = GenerateStar;
                    break;
                case SpecialType.NeutronStar:
                    specialObjects[i].replaceGeneration = GenerateNeutronStar;
                    break;
                case SpecialType.BlackHole:
                    specialObjects[i].replaceGeneration = CacheBlackHole;
                    break;
                case SpecialType.Quasar:
                    specialObjects[i].addGeneration = GenerateQuasar;
                    break;
                case SpecialType.Moon:
                    specialObjects[i].replaceGeneration = GenerateMoon;
                    break;
                case SpecialType.DarkMatter:
                    specialObjects[i].replaceGeneration = GenerateDarkMatter;
                    break;
                default:
                    break;
            }
        }
    }
    private int CompareEnums((int, SpecialObject) x, (int, SpecialObject) y)
    {
        // Specify the enum value you want at the start
        SpecialType desiredEnum = SpecialType.Star;

        // Check if either x or y is the desired enum
        if (x.Item2.type == desiredEnum && y.Item2.type != desiredEnum)
        {
            return -1; // x comes before y
        }
        else if (x.Item2.type != desiredEnum && y.Item2.type == desiredEnum)
        {
            return 1; // y comes before x
        }
        else
        {
            return x.Item2.type.CompareTo(y.Item2.type); // default comparison
        }
    }
    /// <summary>
    /// Takes an image of the camera render texture and creates a planet object with sprites
    /// </summary>
    GameObject ConvertToImage(GameObject selectedObj, Vector3 position, float size)
    {
        MeshRenderer oceanRend = selectedObj.transform.Find("Ocean").GetComponent<MeshRenderer>();
        bool activeOcean = oceanRend.gameObject.activeInHierarchy;
        
        selectedObj.transform.position = new Vector3(position.x + levelStatic.maxSize / 2, position.y + levelStatic.maxSize / 2, position.z);

        Material oceanMat = oceanRend.material;
        Material cullMat = levelStatic.invisOcean;
        oceanRend.sharedMaterial = cullMat;

        Camera cam = new GameObject().AddComponent<Camera>();
        cam.transform.parent = selectedObj.transform;
        cam.transform.localPosition = new Vector3(0,0,-10);
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
        pd.biomeA =  cd.biomeA;
        pd.biomeB =  cd.biomeB;
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
    /// <summary>
    /// Calculates the Camera Size
    /// </summary>
    public static Vector2 CamSize(Camera cam){
        float height = cam.GetComponent<CameraFollow>().cameraMinMax.x * 2; //camera height in units
        float width = height * cam.aspect;       //camera width in units
        Vector3 camSize = new Vector3(width, height);
        return camSize;
    }
    public static (int, SpecialObject)[] TotalCalculateReplacementIndices(ProceduralLevelGeneration.Level level, int seed, LevelGenerator.SpecialObject[] specialObjects, LevelGenStaticReferences levelStatic)
    {
        Physics.SyncTransforms();
        UnityEngine.Random.InitState(seed);
        if (level.planetPoints.Length == 0) { return null; }
        int[] ignoreIndices = new int[2];
        float farthestDistance = 0;
        for (int x = 0; x < level.planetPoints.Length; x++)
        {
            for (int y = 0; y < level.planetPoints.Length; y++)
            {
                if(y == x) { continue; }
                float distance = Vector2.Distance(level.planetPoints[x], level.planetPoints[y]);
                if  (distance > farthestDistance)
                {
                    farthestDistance = distance;
                    if(UnityEngine.Random.Range(0, 1) == 0){
                        ignoreIndices[0] = x; ignoreIndices[1] = y;
                    }
                    else{
                        ignoreIndices[0] = y; ignoreIndices[1] = x;
                    }
                }
            }
        }
        
                List<(int, SpecialObject)> replaceObjects = new List<(int, SpecialObject)>(specialObjects.Length); //Takes all the special object that replace a planet
        int numSpecial = 0; //number of objects
        int moonIndex = -1;
        for (int i = 0; i < specialObjects.Length; i++) //Iterates through current objects set in inspector
        {
            //Filters on whether they are replace objects or not
            if (specialObjects[i].type == (SpecialType.Star) || specialObjects[i].type == (SpecialType.NeutronStar))
            {
                numSpecial += specialObjects[i].amount;
                replaceObjects.Add((specialObjects[i].amount, specialObjects[i]));
            }
            else if (specialObjects[i].type == (SpecialType.Moon))
            {
                numSpecial += specialObjects[i].amount;
                moonIndex = replaceObjects.Count;
                replaceObjects.Add((specialObjects[i].amount, specialObjects[i]));
            }
            else if (specialObjects[i].type == SpecialType.BlackHole)
            {
                numSpecial += specialObjects[i].amount * 2;
                replaceObjects.Add((specialObjects[i].amount, specialObjects[i]));
            }
            else if (specialObjects[i].type == SpecialType.DarkMatter)
            {
                numSpecial += specialObjects[i].amount;
                replaceObjects.Add((specialObjects[i].amount, specialObjects[i]));
            }
        }
        numSpecial = Mathf.Clamp(numSpecial, 0, level.numPlanets - 2); //Allow space for 2 normal planets

        List<(int, SpecialObject)> replacers = new List<(int, SpecialObject)>(); //Preparing return array
        List<int> availableIndices = new List<int>(); //Prevents overlapping indices
        for (int i = 0; i < level.numPlanets; i++)
        {
            if (i != ignoreIndices[0] && i != ignoreIndices[1])
            {
                availableIndices.Add(i);
            }
        }

        //Moon priority to bigger planets
        if (moonIndex >= 0 && specialObjects[moonIndex].amount > 0)
        {
            int moonAmount = 0;
            List<int> removeIndices = new List<int>();
            for (int u = 0; u < availableIndices.Count; u++)
            {
                if (level.planetSizes[availableIndices[u]] > levelStatic.moonCutoffSize)
                {
                    replacers.Add((availableIndices[u], replaceObjects[moonIndex].Item2));
                    replaceObjects[moonIndex] = (replaceObjects[moonIndex].Item1 - 1, replaceObjects[moonIndex].Item2);
                    removeIndices.Add(availableIndices[u]);
                    moonAmount++;
                    if (replaceObjects[moonIndex].Item1 <= 0)
                    {
                        break;
                    }
                }
            }
            numSpecial -= moonAmount;
            replaceObjects.Remove(replaceObjects[moonIndex]);
            for (int i = 0; i < removeIndices.Count; i++)
            {
                availableIndices.Remove(removeIndices[i]);
            }
        }



        //Setting the return array
        for (int i = 0; i < numSpecial; i++)
        {
            if (replaceObjects.Count == 0) { break; } //no more objects to choose from

            //Choosing random special object
            int randomSpecial = UnityEngine.Random.Range(0, replaceObjects.Count);
            SpecialObject chosenObject = replaceObjects[randomSpecial].Item2;

            //if enough space is left for black hole to spawn
            if (chosenObject.type == SpecialType.BlackHole && numSpecial - i < 2)
            {
                replaceObjects.Remove(replaceObjects[randomSpecial]);
                if (replaceObjects.Count == 0) { break; } //no more objects to choose from
                randomSpecial = UnityEngine.Random.Range(0, replaceObjects.Count);
                chosenObject = replaceObjects[randomSpecial].Item2;
            }

            //Determining replacement index and adding object
            int index = availableIndices[UnityEngine.Random.Range(0, availableIndices.Count)];
            availableIndices.Remove(index);
            replacers.Add((index, chosenObject));

            //Subtracting from amount on that object
            replaceObjects[randomSpecial] = (replaceObjects[randomSpecial].Item1 - 1, replaceObjects[randomSpecial].Item2);
            if (replaceObjects[randomSpecial].Item1 <= 0)
            {
                replaceObjects.Remove(replaceObjects[randomSpecial]);
            }

            //Doubling with black holes
            if (chosenObject.type == SpecialType.BlackHole)
            {
                index = availableIndices[UnityEngine.Random.Range(0, availableIndices.Count)];
                availableIndices.Remove(index);
                replacers.Add((index, chosenObject));
                i++;
            }
        }
        //Goop planets are not limited to planets - 2
        for (int i = 0; i < replaceObjects.Count; i++)
        {
            if (replaceObjects[i].Item2.type == SpecialType.DarkMatter)
            {
                for (int u = 0; u < replaceObjects[i].Item1 && u < 2; u++)
                {
                    int index = ignoreIndices[UnityEngine.Random.Range(0, 1)];
                    availableIndices.Remove(index);
                    replacers.Add((index, replaceObjects[i].Item2));
                }
            }
        }

        return replacers.ToArray();
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
