using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using System;
using Unity.VisualScripting;
using System.Linq;
using static ProceduralLevelGeneration;

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
        Star,
        NeutronStar,
        BlackHole,
        Quasar,
        DarkMatter,
        Moon
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
    [Header("Level Parameters")]
    public int seed; //The seed for the level
    [SerializeField] bool randomSeed = false; //Whether seed is chosen randomly in start
    //Level gen params
    [SerializeField] float minPlanets, maxPlanets;
    [SerializeField] Vector2 minRegionExtension, maxRegionExtension; //how much region is extended
    [SerializeField] int resolution;

    [SerializeField] ColorSchemeStaticReference.PlanetType[] possiblePlanetTypes; //Filters which planet types are chosen
    [SerializeField] SpecialObject[] specialObjects;

    [Header("Constant Variables")]
    [SerializeField] LevelGenStaticReferences levelStatic;
    Transform specialObjectsParent;
    [Header("Special Prefabs")]
    [SerializeField] GameObject planetGeneratorPrefab;
    [SerializeField] GameObject forcefieldPrefab, starPrefab, neutronStarPrefab, blackHolePrefab, quasarPrefab, darkMatterPrefab, audioSrcPrefab; //Needed prefabs

    //Private variables
    RenderTexture frontTexture, backTexture;
    Texture2D planetTexture;
    Transform environment, allPlanetParent;
    Camera mainCam;
    Vector2 regionSize;
    Vector2 camSize;
    bool xOry;
    float extensionAmount;

    #endregion
    void Start()
    {
        mainCam = Camera.main; //Cache camera
        frontTexture = new RenderTexture(resolution, resolution, 0);
        backTexture = new RenderTexture(resolution, resolution, 0);
        planetTexture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);

        SetSpecialDelegates();
        //Randomising or keeping seed
        if(randomSeed)
        {
            seed = Mathf.RoundToInt(UnityEngine.Random.value * System.DateTime.Now.Millisecond + System.DateTime.Now.Millisecond * System.DateTime.Now.Second * UnityEngine.Random.Range(-1000f, 1000f) + UnityEngine.Random.value * Time.time);
        }
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

        //Region size calculation
        camSize = CamSize(mainCam);                             //1. Calculates camera size
        xOry = UnityEngine.Random.Range(0, 2) == 1 ? true : false; //2. Chooses which axis to extend into, X or Y.
        extensionAmount = UnityEngine.Random.value;               //3. Randomizes extension amount. 
        regionSize = CalculateBounds(camSize, extensionAmount, xOry);   //4. Calculating bounds based off previous variables

        //Sets the level generation settings
        float[] levelGenSettings = new float[10]{
            minPlanets,
            maxPlanets,
            levelStatic.minSize,
            levelStatic.maxSize,
            LevelGenStaticReferences.sizeBias,
            LevelGenStaticReferences.sizeScale,
            (regionSize.x - levelStatic.maxSize) / 1.25f,
            (regionSize.y - levelStatic.maxSize) / 1.25f,
            LevelGenStaticReferences.samplesBeforeRejection,
            LevelGenStaticReferences.offsetScale
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

    public void SetLevelParams(LevelManager.LevelParams levelParams){
        UnityEngine.Random.InitState(levelParams.seed);

        float specialObjMultiplier = levelStatic.specialObjectMultipliers[levelParams.difficulty];


        possiblePlanetTypes = levelParams.themes;

        specialObjects = new SpecialObject[levelParams.specialObjects.Length];

        for(int i = 0; i < levelParams.specialObjects.Length; i++){
            int num = Mathf.RoundToInt(UnityEngine.Random.value * specialObjMultiplier);
            SpecialObject newSpecialObject = new SpecialObject(levelParams.specialObjects[i], num, GenerateStar, GenerateQuasar);
            specialObjects[i] = newSpecialObject;
        }
    }

    #endregion
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
        float xExtension = Mathf.Clamp(extensionAmount * maxRegionExtension.x, minRegionExtension.x, maxRegionExtension.x);
        float yExtension = Mathf.Clamp(extensionAmount * maxRegionExtension.y, minRegionExtension.y, maxRegionExtension.y);
        if (x){endPos += new Vector2(xExtension, 0); startPos -= new Vector2(xExtension, 0); }
        else{endPos += new Vector2(0, yExtension); startPos -=  new Vector2(0, yExtension); }
        return (startPos, endPos);
    }
    /// <summary>
    /// Calculates the region size
    /// </summary>
    Vector2 CalculateBounds(Vector3 camSize, float extensionAmount, bool x){
        //Extend in the x direction maybe?
        
        float xExtension = Mathf.Clamp(extensionAmount * maxRegionExtension.x, minRegionExtension.x, maxRegionExtension.x);
        float yExtension = Mathf.Clamp(extensionAmount * maxRegionExtension.y, minRegionExtension.y, maxRegionExtension.y);

        Vector2 regionSize = new Vector3 (camSize.x + levelStatic.maxSize, camSize.y + levelStatic.maxSize, camSize.z);
        if(x){ regionSize += new Vector2(xExtension, 0); }
        else{ regionSize += new Vector2(0, yExtension);}

        return regionSize;
    }
    #endregion
    #region Level Generation
    /// <summary>
    /// Calculates what planet indices to replace with special object
    /// </summary>
    /// <returns>An index and special object</returns>
    (int, SpecialObject)[] CalculateReplaceIndices(ProceduralLevelGeneration.Level level)
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
            availableIndices.Add(i);
        }

        //Moon priority to bigger planets
        if(moonIndex >= 0)
        {
            int moonAmount = 0;
            List<int> removeIndices = new List<int>();
            for (int u = 0; u < level.numPlanets - 2; u++)
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
            if(chosenObject.type == SpecialType.BlackHole && numSpecial - i < 2)
            {
                replaceObjects.Remove(replaceObjects[randomSpecial]);
                if(replaceObjects.Count == 0) { break; } //no more objects to choose from
                randomSpecial = UnityEngine.Random.Range(0, replaceObjects.Count);
                chosenObject = replaceObjects[randomSpecial].Item2;
            }

            //Determining replacement index and adding object
            int index = availableIndices[UnityEngine.Random.Range(0, availableIndices.Count)];
            availableIndices.Remove(index);
            replacers.Add((index, chosenObject));

            //Subtracting from amount on that object
            replaceObjects[randomSpecial] = (replaceObjects[randomSpecial].Item1-1, replaceObjects[randomSpecial].Item2);
            if (replaceObjects[randomSpecial].Item1 <=0)
            {
                replaceObjects.Remove(replaceObjects[randomSpecial]);
            }

            //Doubling with black holes
            if(chosenObject.type == SpecialType.BlackHole)
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
                    int index = availableIndices[UnityEngine.Random.Range(0, availableIndices.Count)];
                    availableIndices.Remove(index);
                    replacers.Add((index, replaceObjects[i].Item2));
                }
            }
        }

        return replacers.ToArray();
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
    /// <summary>
    /// Generates the planets off of the level parameters
    /// </summary>
    IEnumerator Generatelevel(ProceduralLevelGeneration.Level level){
        Time.timeScale = 0; 
        FullPlanetGeneration generator = Instantiate(planetGeneratorPrefab).GetComponent<FullPlanetGeneration>();
        Planet[] planetList = new Planet[level.numPlanets];
        Instantiate(audioSrcPrefab);
        environment = GameObject.FindGameObjectWithTag("Environment").transform;
        allPlanetParent = new GameObject("Planets"){
            tag = "PlanetParent"
        }.transform;
        Vector3 levelSize = new Vector3(level.levelSize.x, level.levelSize.y, 0f);

        //Replacement special objects
        (int, SpecialObject)[] replacers = CalculateReplaceIndices(level);
        int[] replaceIndices = Array.ConvertAll(replacers, r => r.Item1);
        Array.Sort(replacers, (x, y) => CompareEnums(x, y));
        for (int i = 0; i < replacers.Length; i++)
        {
            replacers[i].Item2.replaceGeneration(ref planetList[replaceIndices[i]], level.planetSizes[replaceIndices[i]], level.planetPoints[replaceIndices[i]], level.planetSeeds[replaceIndices[i]], level.planetTypes[replaceIndices[i]], generator, i);
        }
        for (int i = 0; i < level.numPlanets; i++){
            if(replaceIndices.Contains(i)){continue;}
            planetList[i] = GeneratePlanet(generator, level.planetSizes[i], i, level.planetPoints[i], level.planetSeeds[i], level.planetTypes[i]);
            
            yield return new WaitForSecondsRealtime(0.1f);
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
        SpecialObject[] addObjects = CalculateAddObjects();
        for (int i = 0; i < addObjects.Length; i++)
        {
            for (int u = 0; u < addObjects[i].amount; u++)
            {
                addObjects[i].addGeneration(normPlanets.ToArray(), level.planetSeeds[0], u + i * 1000);
                yield return new WaitForSecondsRealtime(0.1f);
            }
        }

        //Ball and flag positions
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Transform flag = GameObject.FindGameObjectWithTag("Hole").transform;

        SetBallFlagPosition(flag, player.transform, normPlanets.ToArray());
        Physics.SyncTransforms();
        player.GetComponent<ShootBall>().Init(environment, player.GetComponent<Rigidbody>().position);

        GenerateBlackHoles(player.transform.position);


        //Camera start and end positions
        (Vector2, Vector2) startEndPos = CalculateStartEndPos(planetList, camSize, extensionAmount, xOry);
        Vector2 startPos = startEndPos.Item1;
        Vector2 endPos = startEndPos.Item2;
        float maxZoom = Mathf.Max((regionSize.x) / (2 * mainCam.aspect), regionSize.y / 2) + 0.5f;

        mainCam.GetComponent<CameraFollow>().InitPositions(startPos, endPos, maxZoom);

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
    List<Planet> cachedBlackHoles = new List<Planet>();
    void CacheBlackHole(ref Planet planet, float size, Vector3 position, Vector3 seed, ColorSchemeStaticReference.PlanetType planetType, FullPlanetGeneration generator, int index)
    {
        Transform blackHole = Instantiate(blackHolePrefab, new Vector3(position.x + levelStatic.maxSize / 2, position.y + levelStatic.maxSize / 2, position.z), Quaternion.identity).transform;
        blackHole.localScale = Vector3.one * size * levelStatic.planetScale * 0.5f;
        blackHole.parent = environment;

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

                RaycastHit[] allHits = Physics.BoxCastAll(planets[i].planet.transform.position, Vector3.one, dirTemp, Quaternion.LookRotation(dirTemp), distanceTemp);
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
        if(!foundPlanet) { print("No Quasar Eligible Planets"); return; }

        Vector3 dir = planets[index2].planet.transform.position - planets[index1].planet.transform.position;
        float distance = dir.magnitude - (planets[index2].forceField.transform.localScale.x + planets[index1].forceField.transform.localScale.x);
        dir = dir.normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
        Vector3 midPoint = ((planets[index1].planet.transform.position + planets[index1].forceField.transform.localScale.x * dir) + (planets[index2].planet.transform.position - planets[index2].forceField.transform.localScale.x * dir)) / 2;

        QuasarGenerator quasar = Instantiate(quasarPrefab, specialObjectsParent).GetComponent<QuasarGenerator>();
        quasar.transform.parent = GameObject.FindGameObjectWithTag("Environment").transform;
        quasar.InitRandom((int)seed.x + index, planets[index1].planetType, midPoint, angle, distance/4 + distance/64, planets[index1].forceField.GetComponent<ForceField>(), planets[index2].forceField.GetComponent<ForceField>());
    }
    void GenerateDarkMatter(ref Planet planet, float size, Vector3 position, Vector3 seed, ColorSchemeStaticReference.PlanetType planetType, FullPlanetGeneration generator, int index)
    {
        UnityEngine.Random.InitState((int)seed.z + index);
        planet = GeneratePlanet(generator, size, index, position, seed, planetType);

        DarkMatterGenerator darkMatter = Instantiate(darkMatterPrefab, new Vector3(position.x + levelStatic.maxSize / 2, position.y + levelStatic.maxSize / 2, position.z), Quaternion.identity, specialObjectsParent).GetComponent<DarkMatterGenerator>();
        darkMatter.Init(planet.planet.GetComponent<PlanetDataHolder>());
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
        Planet smallMoon = CreateMoonMesh(generator, smallMoonSize, index, position, seed + (index + 1) * new Vector3 (LevelGenStaticReferences.offsetScale * 2, LevelGenStaticReferences.offsetScale * 2, LevelGenStaticReferences.offsetScale * 2) + Vector3.one, planetType);
        smallMoon.planet.transform.parent = GameObject.FindGameObjectWithTag("PlanetParent").transform;
        smallMoon.forceField.transform.parent = GameObject.FindGameObjectWithTag("Environment").transform;

        MoonManager moonScript = smallMoon.planet.AddComponent<MoonManager>();
        
        float semiMajorAxis = apoapsis/(1 - eccentricity);
        float speed = UnityEngine.Random.Range(0.5f, 1.2f);
        float rotation = UnityEngine.Random.Range(0,360);
        smallMoon.forceField.GetComponent<ForceField>().AutoMass();
        smallMoon.forceField.GetComponent<Collider>().material = levelStatic.superBouncyMoon;
        bigPlanet.forceField.GetComponent<ForceField>().AutoMass();
        moonScript.Init(semiMajorAxis, ArtificialGravity.G, eccentricity, speed, smallMoon.forceField.GetComponent<CelestialMass>(), bigPlanet.forceField.GetComponent<CelestialMass>(), levelStatic.moonLine, rotation);

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
    /// Sets the position for the flag and ball. Random position for the flag, farthest position from flag for the ball.
    /// </summary>
    void SetBallFlagPosition(Transform flag, Transform ball, Planet[] candidates)
    {
        Physics.SyncTransforms();
        if(candidates.Length == 0) { return; }
        int candidateIndex = UnityEngine.Random.Range(0, Mathf.Max(0,candidates.Length-1));

        int rot = UnityEngine.Random.Range(0, 360);
        Vector3 predictedFlagPos = Vector3.zero;
        for (int u = 0; u < 12; u++)
        {
            flag.eulerAngles = new Vector3(0, 0, rot);
            predictedFlagPos = candidates[candidateIndex].forceField.transform.position + flag.up * (candidates[candidateIndex].forceField.transform.localScale.x + 1.2f); //maybe scale with radius
            if (!Physics.CheckSphere(predictedFlagPos, 1.15f))
            {
                break;
            }
            rot += 360 / 24;
        }
        flag.position = predictedFlagPos;
        
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
        Vector3 initBallPos = candidates[farthestIndex].forceField.transform.position + new Vector3(Mathf.Cos(flagAng), Mathf.Sin(flagAng)) * (candidates[farthestIndex].forceField.transform.localScale.x);
        Vector3 closestPoint = candidates[farthestIndex].forceField.GetComponent<Collider>().ClosestPoint(initBallPos);
        ball.GetComponent<Rigidbody>().position =  closestPoint + (closestPoint - candidates[farthestIndex].planet.transform.position).normalized * (0.22f);
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
