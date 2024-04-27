using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using System;
using Unity.VisualScripting;
using System.Linq;
using static ProceduralLevelGeneration;
using UnityEngine.EventSystems;

public class LevelGenerator : MonoBehaviour
{
    #region Variables
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
    float minPlanets, maxPlanets;
    [SerializeField]
    float minSize, maxSize, sizeBias, sizeScale, samplesBeforeRejection, offsetScale;
    [SerializeField]
    Vector2 regionExtension; //how much region is extended
    [SerializeField]
    GameObject planetGeneratorPrefab, forcefieldPrefab, starPrefab, blackHolePrefab, quasarPrefab, audioSrcPrefab; //Needed prefabs
    [SerializeField]
    float planetScale; //Scale of planets multiplier
    [SerializeField] int resolution;

    [SerializeField]
    ColorSchemeStaticReference.PlanetType[] possiblePlanetTypes; //Filters which planet types are chosen
    [SerializeField]
    SpecialObject[] specialObjects;
    Vector2 regionSize;
    public delegate void GenerateSpecialPlanetObject(ref Planet planet, float size, Vector3 position, Vector3 seed, ColorSchemeStaticReference.PlanetType planetType, FullPlanetGeneration generator, int index);
    public delegate void GenerateSpecialObject(Planet[] planets, Vector3 seed, int index);
    [Header("Random References and Stuff")]
    [SerializeField] PhysicMaterial superBouncyMoon;
    [SerializeField] Material moonLine;
    [SerializeField] float moonCutoffSize = 1, moonDstScale = 0.8f;
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
        BlackHole,
        Quasar,
        Moon
    }
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
    RenderTexture frontTexture, backTexture;
    Texture2D planetTexture;



    #endregion
    void Start()
    {

        levelInit?.Invoke(); //Initiation Delegate
        Camera mainCam = Camera.main; //Cache camera
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
    /// Calculates what planet indices to replace with special object
    /// </summary>
    /// <returns>An index and special object</returns>
    (int, SpecialObject)[] CalculateReplaceIndices(ProceduralLevelGeneration.Level level)
    {
        List<(int, SpecialObject)> replaceObjects = new List<(int, SpecialObject)>(); //Takes all the special object that replace a planet
        int numSpecial = 0; //number of objects
        int moonIndex = -1;
        for (int i = 0; i < specialObjects.Length; i++) //Iterates through current objects set in inspector
        {
            //Filters on whether they are replace objects or not
            if (specialObjects[i].type == (SpecialType.Star))
            {
                numSpecial += specialObjects[i].amount;
                replaceObjects.Add((specialObjects[i].amount, specialObjects[i]));
            }
            else if (specialObjects[i].type == (SpecialType.Moon))
            {
                moonIndex = i;
                numSpecial += specialObjects[i].amount;
                replaceObjects.Add((specialObjects[i].amount, specialObjects[i]));
            }
            else if (specialObjects[i].type == SpecialType.BlackHole)
            {
                numSpecial += specialObjects[i].amount * 2;
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
            for (int u = 0; u < availableIndices.Count && u < level.numPlanets - 2; u++)
            {
                if (level.planetSizes[availableIndices[u]] > moonCutoffSize)
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
    /// <summary>
    /// Generates the planets off of the level parameters
    /// </summary>
    Planet[] Generatelevel(ProceduralLevelGeneration.Level level){
        FullPlanetGeneration generator = Instantiate(planetGeneratorPrefab).GetComponent<FullPlanetGeneration>();
        Planet[] planetList = new Planet[level.numPlanets];
        Instantiate(audioSrcPrefab);
        Transform env = GameObject.FindGameObjectWithTag("Environment").transform;
        Transform allPlanetParent = new GameObject("Planets"){
            tag = "PlanetParent"
        }.transform;
        Vector3 levelSize = new Vector3(level.levelSize.x, level.levelSize.y, 0f);

        //Replacement special objects
        (int, SpecialObject)[] replacers = CalculateReplaceIndices(level);
        int[] replaceIndices = Array.ConvertAll(replacers, r => r.Item1);
        for (int i = 0; i < level.numPlanets; i++){

            if(replaceIndices.Contains(i))
            {
                //Generation delegate
                replacers[Array.IndexOf(replaceIndices, i)].Item2.replaceGeneration(ref planetList[i], level.planetSizes[i], level.planetPoints[i], level.planetSeeds[i], level.planetTypes[i], generator, i);
                continue;
            }

            planetList[i] = GeneratePlanet(generator, level.planetSizes[i], i, level.planetPoints[i], level.planetSeeds[i], level.planetTypes[i]);
            planetList[i].planet.transform.parent = allPlanetParent;
            planetList[i].forceField.transform.parent = env;

            //print(forcefield.transform.localScale.x);
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
        print(addObjects.Length);
        for (int i = 0; i < addObjects.Length; i++)
        {
            addObjects[i].addGeneration(normPlanets.ToArray(), level.planetSeeds[0], i);
        }

        //Ball and flag positions
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Transform flag = GameObject.FindGameObjectWithTag("Hole").transform;

        SetBallFlagPosition(flag, player.transform, normPlanets.ToArray());
        player.GetComponent<ShootBall>().Init(env, player.GetComponent<Rigidbody>().position);

        GenerateBlackHoles(player.transform.position);
        return planetList;
    }

    /// <summary>
    /// Generates a planet using FullPlanetGeneration
    /// </summary>
    Planet GeneratePlanet(FullPlanetGeneration generator, float size, int index, Vector3 position, Vector3 seed, ColorSchemeStaticReference.PlanetType planetType)
    {
        //(PlanetColorScheme, PlanetColorScheme)[] previousBiomes = new (PlanetColorScheme, PlanetColorScheme)[planetList.Length];
        //for (int i = 0; i < planetList.Length; i++)
        //{
        //    previousBiomes[i].Item1 = planetList[i].biomeA;
        //    previousBiomes[i].Item2 = planetList[i].biomeB;
        //}
        (PlanetColorScheme, PlanetColorScheme) biomes = generator.RandomizeColorScheme(seed, planetType, index + 1);
        generator.ClearAllTrees();
        generator.UpdateAndApplyAll();

        GameObject newPlanet = ConvertToImage(generator.gameObject);
        frontTexture.Release(); //Memory
        backTexture.Release();  //Memory
        newPlanet.SetActive(true);

        newPlanet.transform.position = new Vector3(position.x + maxSize / 2, position.y + maxSize / 2, position.z);
        newPlanet.transform.localScale = Vector3.one * size * planetScale * 0.2f;
        GameObject forceField = GenerateForcefield(newPlanet.transform);

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
        Transform blackHole = Instantiate(blackHolePrefab, new Vector3(position.x + maxSize / 2, position.y + maxSize / 2, position.z), Quaternion.identity).transform;
        blackHole.localScale = Vector3.one * size * planetScale * 0.5f;
        blackHole.parent = GameObject.FindGameObjectWithTag("Environment").transform;

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
        int index1 = UnityEngine.Random.Range(0, planets.Length);

        List<int> possibleCandidates = new List<int>();
        for (int i = 0; i < planets.Length; i++)
        {
            if(i == index1) { continue; }

            Vector3 dirTemp = planets[i].planet.transform.position - planets[index1].planet.transform.position;
            float distanceTemp = dirTemp.magnitude;
            dirTemp = dirTemp.normalized;

            if (!Physics.BoxCast(planets[index1].forceField.transform.position + dirTemp * planets[index1].forceField.transform.localScale.x, new Vector3(1f, 1f, 1f), dirTemp, Quaternion.LookRotation(dirTemp), distanceTemp - 0.1f))
            {
                possibleCandidates.Add(i);
            }
        }
        if(possibleCandidates.Count == 0) { print("No Quasar Eligible Planets"); return; }
        int index2 = possibleCandidates[UnityEngine.Random.Range(0, possibleCandidates.Count)];

        Vector3 dir = planets[index2].planet.transform.position - planets[index1].planet.transform.position;
        float distance = dir.magnitude - (planets[index2].forceField.transform.localScale.x + planets[index1].forceField.transform.localScale.x);
        dir = dir.normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
        Vector3 midPoint = ((planets[index1].planet.transform.position + planets[index1].forceField.transform.localScale.x * dir) + (planets[index2].planet.transform.position - planets[index2].forceField.transform.localScale.x * dir)) / 2;

        QuasarGenerator quasar = Instantiate(quasarPrefab).GetComponent<QuasarGenerator>();
        quasar.transform.parent = GameObject.FindGameObjectWithTag("Environment").transform;
        quasar.InitRandom((int)seed.x + index, planets[index1].planetType, midPoint, angle, distance/4 + distance/64, planets[index1].forceField.GetComponent<ForceField>(), planets[index2].forceField.GetComponent<ForceField>());
    }
    void GenerateStar(ref Planet planet, float size, Vector3 position, Vector3 seed, ColorSchemeStaticReference.PlanetType planetType, FullPlanetGeneration generator, int index)
    {
        Transform star = Instantiate(starPrefab, new Vector3(position.x + maxSize / 2, position.y + maxSize / 2, position.z), Quaternion.identity).transform;
        star.transform.localScale = Vector3.one * size * planetScale * 0.66f;
        star.parent = GameObject.FindGameObjectWithTag("Environment").transform;
        star.GetComponent<SunGenerator>().RandomSun(Mathf.RoundToInt(seed.z + index * 1000), planetType);
        planet.planet = star.gameObject;
        planet.planetType = ColorSchemeStaticReference.PlanetType.special;
        planet.forceField = null;
    }
    void GenerateMoon(ref Planet planet, float size, Vector3 position, Vector3 seed, ColorSchemeStaticReference.PlanetType planetType, FullPlanetGeneration generator, int index)
    {   
        UnityEngine.Random.InitState((int)(seed.y + index));
        float eccentricity = UnityEngine.Random.Range(0f, 0.2f);
        float smallMoonSize = size * planetScale / 2f;
        float apoapsis = size * planetScale - smallMoonSize + 1.5f;

        
        float bigPlanetDecreasedSize = apoapsis * moonDstScale - smallMoonSize;
        //Generate orbitee planet
        Planet bigPlanet = GeneratePlanet(generator, bigPlanetDecreasedSize, index, position, seed, planetType);
        bigPlanet.planet.transform.parent = GameObject.FindGameObjectWithTag("PlanetParent").transform;
        bigPlanet.forceField.transform.parent = GameObject.FindGameObjectWithTag("Environment").transform;

        //Generate orbiter planet
        Planet smallMoon = GeneratePlanet(generator, smallMoonSize, index, position, seed + (index + 1) * new Vector3 (offsetScale, offsetScale, offsetScale) + Vector3.one, ColorSchemeStaticReference.PlanetType.special);
        smallMoon.planet.transform.parent = GameObject.FindGameObjectWithTag("PlanetParent").transform;
        smallMoon.forceField.transform.parent = GameObject.FindGameObjectWithTag("Environment").transform;

        MoonManager moonScript = smallMoon.planet.AddComponent<MoonManager>();
        
        float semiMajorAxis = apoapsis/(1 - eccentricity);
        float speed = UnityEngine.Random.Range(0.025f, 0.1f);
        float rotation = UnityEngine.Random.Range(0,360);
        smallMoon.forceField.GetComponent<ForceField>().AutoMass();
        smallMoon.forceField.GetComponent<Collider>().material = superBouncyMoon;
        bigPlanet.forceField.GetComponent<ForceField>().AutoMass();
        moonScript.Init(semiMajorAxis, ArtificialGravity.G, eccentricity, speed, smallMoon.forceField.GetComponent<CelestialMass>(), bigPlanet.forceField.GetComponent<CelestialMass>(), moonLine, rotation);

        bigPlanet.planetType = ColorSchemeStaticReference.PlanetType.special;
        planet = bigPlanet;

    }
    #endregion
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
                case SpecialType.BlackHole:
                    specialObjects[i].replaceGeneration = CacheBlackHole;
                    break;
                case SpecialType.Quasar:
                    specialObjects[i].addGeneration = GenerateQuasar;
                    break;
                case SpecialType.Moon:
                    specialObjects[i].replaceGeneration = GenerateMoon;
                    break;
                default:
                    break;
            }
        }
    }
    /// <summary>
    /// Takes an image of the camera render texture and creates a planet object with sprites
    /// </summary>
    GameObject ConvertToImage(GameObject selectedObj)
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
