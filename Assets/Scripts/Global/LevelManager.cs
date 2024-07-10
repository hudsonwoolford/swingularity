using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [SerializeField] public GalaxyData currentTheme { get; private set; }
    [SerializeField] public LevelParams levelParams { get; private set; }
    [SerializeField] public GalaxyParams[] currentProceduralUniverse { get; private set; } //An array of galaxy params in order that correspond to which universe you are in
    [SerializeField] public int levelIndex { get; private set; } = 0; //The index of the current level
    [SerializeField] public bool procedural { get; private set; } = false; //Whether you are currently in a procedural universe or not
    [SerializeField] public UniverseParams currentUniverse { get; private set; }
    [Header("Parameters")]
    [SerializeField] GalaxyData backupTheme;
    [SerializeField] TextMeshProUGUI scoreText; //Text that displays score
    [Space()]
    [SerializeField] TextMeshProUGUI streakText; //Text that displays under par streak
    [SerializeField] TextMeshProUGUI hioStreakText; //Text that hole in one streak
    [Header("Serialized Variables")]
    [SerializeField] List<string> completedLevels; //A list of the scene names of completed levels
    [SerializeField] string[] currentGalaxy; //An array of scene names in order that correspond to which galaxy you are in
    [SerializeField] LevelParams[] currentProceduralGalaxy; //An array of level params in order that correspond to which galaxy you are in
    Canvas levelCanvas; //The canvas for level UI
    bool chosenOption; //True when the player has clicked a button on the canvas UI
    int currentGalaxyIndex;

    [Serializable]
    public struct LevelParams
    {
        public int seed; // Seed for level planets and stuff
        public int levelPar;
        public int difficulty; // Range 0-10 determining size of level and number of special objects
        public ColorSchemeStaticReference.PlanetType[] themes; // Themes/color schemes for level - First scheme in list is primary
        public LevelGenerator.SpecialType[] specialObjects;
        public LevelGenStaticReferences levelStatic; //static references for level generation

        public LevelParams(int seed, int difficulty, int par, ColorSchemeStaticReference.PlanetType[] themes, LevelGenerator.SpecialType[] specialObjects, LevelGenStaticReferences levelStatic)
        {
            this.seed = seed;
            this.levelPar = par;
            this.difficulty = difficulty;
            this.themes = themes;
            this.specialObjects = specialObjects;
            this.levelStatic = levelStatic;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is LevelParams))
            {
                return false;
            }
            var other = (LevelParams)obj;
            return this == other;
        }

        public override int GetHashCode()
        {
            int hash = seed.GetHashCode();
            hash = (hash * 397) ^ levelPar.GetHashCode();
            hash = (hash * 397) ^ difficulty.GetHashCode();
            hash = (hash * 397) ^ (themes != null ? themes.GetHashCode() : 0);
            hash = (hash * 397) ^ (specialObjects != null ? specialObjects.GetHashCode() : 0);
            hash = (hash * 397) ^ (levelStatic != null ? levelStatic.GetHashCode() : 0);
            return hash;
        }

        public static bool operator ==(LevelParams a, LevelParams b)
        {
            return a.seed == b.seed
                && a.levelPar == b.levelPar
                && a.difficulty == b.difficulty
                && (a.themes == b.themes || (a.themes != null && b.themes != null && a.themes.SequenceEqual(b.themes)))
                && (a.specialObjects == b.specialObjects || (a.specialObjects != null && b.specialObjects != null && a.specialObjects.SequenceEqual(b.specialObjects)))
                && a.levelStatic == b.levelStatic;
        }

        public static bool operator !=(LevelParams a, LevelParams b)
        {
            return !(a == b);
        }
    }
    [Serializable]
    public struct GalaxyParams
    {
        public string name; //namne of galaxy, appears in ui
        public int seed; //galxy seed, determines levels
        [Range(0, 10)]
        public int relativeDifficulty; //relative difficulty of galaxy (1-10)
        public GalaxyData galaxyData;
        public ColorSchemeStaticReference.PlanetType[] possibleTypes;
        public LevelGenerator.SpecialType[] possibleSpecial;
        public LevelGenStaticReferences levelStatic; //static references for level generation
        public bool[] collectedStars;
        public int numStars;

        public GalaxyParams(string name, int seed, int relativeDifficulty, GalaxyData galaxyData, ColorSchemeStaticReference.PlanetType[] possibleTypes, LevelGenerator.SpecialType[] possibleSpecial, LevelGenStaticReferences levelStatic, bool[] collectedStars, int numStars)
        {
            this.name = name;
            this.seed = seed;
            this.galaxyData = galaxyData;
            this.relativeDifficulty = relativeDifficulty;
            this.possibleTypes = possibleTypes;
            this.possibleSpecial = possibleSpecial;
            this.levelStatic = levelStatic;
            this.collectedStars = collectedStars;
            this.numStars = numStars;
        }
        public bool EqualNoStar(GalaxyParams a, GalaxyParams b)
        {
            return a.name == b.name
            && a.seed == b.seed
            && a.relativeDifficulty == b.relativeDifficulty
            && (a.possibleTypes == b.possibleTypes || (a.possibleTypes != null && b.possibleTypes != null && a.possibleTypes.SequenceEqual(b.possibleTypes)))
            && (a.possibleSpecial == b.possibleSpecial || (a.possibleSpecial != null && b.possibleSpecial != null && a.possibleSpecial.SequenceEqual(b.possibleSpecial)))
            && a.levelStatic == b.levelStatic;
        }
        public override bool Equals(object obj)
        {
            if (!(obj is GalaxyParams))
            {
                return false;
            }
            var other = (GalaxyParams)obj;
            return this == other;
        }

        public override int GetHashCode()
        {
            int hash = seed.GetHashCode();
            hash = (hash * 397) ^ name.GetHashCode();
            hash = (hash * 397) ^ seed.GetHashCode();
            hash = (hash * 397) ^ relativeDifficulty.GetHashCode();
            hash = (hash * 397) ^ galaxyData.GetHashCode();
            hash = (hash * 397) ^ possibleTypes.GetHashCode();
            hash = (hash * 397) ^ possibleSpecial.GetHashCode();
            hash = (hash * 397) ^ levelStatic.GetHashCode();
            hash = (hash * 397) ^ collectedStars.GetHashCode();
            return hash;
        }

        public static bool operator ==(GalaxyParams a, GalaxyParams b)
        {
            return a.name == b.name
                && a.seed == b.seed
                && a.relativeDifficulty == b.relativeDifficulty
                && (a.possibleTypes == b.possibleTypes || (a.possibleTypes != null && b.possibleTypes != null && a.possibleTypes.SequenceEqual(b.possibleTypes)))
                && (a.possibleSpecial == b.possibleSpecial || (a.possibleSpecial != null && b.possibleSpecial != null && a.possibleSpecial.SequenceEqual(b.possibleSpecial)))
                && a.levelStatic == b.levelStatic
                && (a.collectedStars == b.collectedStars || (a.collectedStars != null && b.collectedStars != null && a.collectedStars.SequenceEqual(b.collectedStars)));
        }

        public static bool operator !=(GalaxyParams a, GalaxyParams b)
        {
            return !(a == b);
        }
    }
    [Serializable]
    public struct UniverseParams
    {
        public string name; //name of the universe save, set in new game
        public int seed; //master seed, determines everything else
        [Range(0, 10)]
        public int relativeDifficulty; //relative difficulty of universe (1-10)
        public string size; //the size of the universe (small, medium, large)
        public int completedGalaxies; //number of galaxies that have been completed
        public int numGalaxies; //number of galaxies in the universe
        public int numStars;
        public LevelGenStaticReferences levelStatic; //static references for level generation
        public UniverseParams(string name, int seed, int relativeDifficulty, string size, int completedGalaxies, int numGalaxies, LevelGenStaticReferences levelStatic, int numStars)
        {
            this.name = name;
            this.seed = seed;
            this.relativeDifficulty = relativeDifficulty;
            this.size = size;
            this.completedGalaxies = completedGalaxies;
            this.numGalaxies = numGalaxies;
            this.levelStatic = levelStatic;
            this.numStars = numStars;
        }
        public override bool Equals(object obj)
        {
            if (!(obj is UniverseParams))
            {
                return false;
            }
            var other = (UniverseParams)obj;
            return this == other;
        }

        public override int GetHashCode()
        {
            int hash = seed.GetHashCode();
            hash = (hash * 397) ^ name.GetHashCode();
            hash = (hash * 397) ^ seed.GetHashCode();
            hash = (hash * 397) ^ relativeDifficulty.GetHashCode();
            hash = (hash * 397) ^ size.GetHashCode();
            hash = (hash * 397) ^ completedGalaxies.GetHashCode();
            hash = (hash * 397) ^ numGalaxies.GetHashCode();
            hash = (hash * 397) ^ levelStatic.GetHashCode();
            hash = (hash * 397) ^ numStars.GetHashCode();
            return hash;
        }

        public static bool operator ==(UniverseParams a, UniverseParams b)
        {
            return a.name == b.name
                && a.seed == b.seed
                && a.relativeDifficulty == b.relativeDifficulty
                && a.size == b.size
                && a.completedGalaxies == b.completedGalaxies
                && a.numGalaxies == b.numGalaxies
                && a.levelStatic == b.levelStatic
                && a.numStars == b.numStars;
        }

        public static bool operator !=(UniverseParams a, UniverseParams b)
        {
            return !(a == b);
        }
    }

    //events
    public delegate void NotifyProcedural(bool procedural);
    public static NotifyProcedural notifyProcedural;
    public delegate void SetProceduralUniverse(UniverseParams universeParams);
    public static SetProceduralUniverse setProceduralUniverse;
    public delegate void SetProceduralGalaxies(GalaxyParams[] galaxyParams);
    public static SetProceduralGalaxies setProceduralGalaxies;

    private void Awake()
    {
        completedLevels = new List<string>();
        levelCanvas = GetComponent<Canvas>();
        levelCanvas.enabled = false;
        chosenOption = false;
        currentTheme = backupTheme;
        currentUniverse = new UniverseParams();
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }
    public bool LevelCompleted(string level) //Check if a level has been completed, intended to be used external scripts
    {
        return completedLevels.Contains(level);
    }
    public void NextLevelUIButton() //Called by the "Next" button
    {
        chosenOption = true;
    }
    #region Next Level UI
    IEnumerator NextLevelUI() //Handles the UI that displays when you've completed the level, asynchronously loads the following level
    {
        levelCanvas.enabled = true;
        scoreText.enabled = true;
        streakText.enabled = false;
        hioStreakText.enabled = false;

        string name = SceneManager.GetActiveScene().name;
        levelIndex = Array.IndexOf(currentGalaxy, name);
        string nextName = levelIndex + 1 <= currentGalaxy.Length - 1 ? currentGalaxy[levelIndex + 1] : "Galaxies";

        AsyncOperation loadNextOp = SceneManager.LoadSceneAsync(nextName);
        loadNextOp.allowSceneActivation = false;
        while (!chosenOption)
        {
            if(SceneManager.GetActiveScene().name != name) 
            {
                levelCanvas.enabled = false;
                chosenOption = false;
                yield break; 
            }
            yield return new WaitForFixedUpdate();
        }
        levelIndex++;
        chosenOption = false;
        loadNextOp.allowSceneActivation = true;
        levelCanvas.enabled = false;
    }
    IEnumerator ProceduralNextLevelUI() //Handles the UI that displays when you've completed the level, asynchronously loads the following level
    {
        if (currentProceduralGalaxy == null || currentProceduralGalaxy.Length == 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            yield break;
        }

        // Ensure levelIndex is within a valid range
        if (levelIndex < 0 || levelIndex >= currentProceduralGalaxy.Length)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            yield break;
        }
        //enable things
        levelCanvas.enabled = true;
        scoreText.enabled = true;
        streakText.enabled = false;
        hioStreakText.enabled = false;
        //determine current level index
        DetermineCurrentLevelIndex();
        //complete universe
        if(levelIndex>=4) 
        {
            UniverseParams cU = currentUniverse;
            cU.completedGalaxies++;
            setProceduralUniverse?.Invoke(cU);
        }
        //determine next level string
        int currentBuildIndex = SceneManager.GetActiveScene().buildIndex;
        string name = levelIndex < 4 ? SceneManager.GetActiveScene().name/*"Procedural Galaxy"*/ : "Galaxies";
        levelParams = currentProceduralGalaxy[Mathf.Clamp(levelIndex + 1, 0, currentProceduralGalaxy.Length-1)];
        
        AsyncOperation loadNextOp = SceneManager.LoadSceneAsync(name);
        loadNextOp.allowSceneActivation = false;
        while (!chosenOption)
        {
            if (SceneManager.GetActiveScene().buildIndex != currentBuildIndex)
            {
                levelCanvas.enabled = false;
                chosenOption = false;
                yield break;
            }
            yield return new WaitForFixedUpdate();
        }
        levelIndex = levelIndex < 4 ? levelIndex + 1 : 0;
        chosenOption = false;
        loadNextOp.allowSceneActivation = true;
        levelCanvas.enabled = false;
    }
    void DetermineCurrentLevelIndex()
    {
        for (int i = 0; i < currentProceduralGalaxy.Length; i++)
        {
            if (levelParams == currentProceduralGalaxy[i]) //OPERATOR DOESNT WORK IDK WHY SHOULD BE FIXED BUT THIS FOR NOW
            {
                levelIndex = i; break;
            }
        }
    }
    #endregion
    #region EventFunctions
    public void SetLevelParams(LevelParams lp)
    {
        levelParams = lp;
    }
    void CompleteCurrentLevel() //Adds the current level to completed level array
    {
        if (procedural) { StartCoroutine(ProceduralNextLevelUI()); return; }

        string name = SceneManager.GetActiveScene().name;
        if(!completedLevels.Contains(name)) { 
            completedLevels.Add(name);
        }

        StartCoroutine(NextLevelUI());
    }
    void SetGalaxyLevels(string[] galaxy, GalaxyData theme) //Sets the current galaxy levels, we also know it cannot be procedural
    {
        currentTheme = theme;
        procedural = false;
        currentGalaxy = galaxy;
    }
    void SetProceduralGalaxyLevels(string[] galaxy, LevelParams[] galaxyLevels, GalaxyParams galaxyParams, GalaxyData theme) //Sets the current galaxy levels, we also know it cannot be procedural
    {
        currentTheme = theme;
        procedural = true;
        currentGalaxy = galaxy;
        currentProceduralGalaxy = galaxyLevels;
        levelIndex = 0;

        for (int i = 0; i < currentProceduralGalaxy.Length; i++)
        {
            if (currentProceduralUniverse[i] == galaxyParams) { currentGalaxyIndex = i; break; }
        }
    }
    void SetProceduralGalaxiesEvent(GalaxyParams[] galaxyParams) //Sets the current universe
    {
        procedural = true;
        currentProceduralUniverse = galaxyParams;
    }
    void SetProceduralUniverseEvent(UniverseParams universeParams) //Sets the current universe
    {
        procedural = true;
        currentUniverse = universeParams;
    }
    void ScoreGalaxy(int scoreSum, int shotsSum, int parSum)
    {
        bool[] starScore = new bool[]
        {
            true,
            scoreSum <= 0,
            scoreSum < 0,
            shotsSum == 5,
            shotsSum == 5
        };
        int starTotal = 0;
        for (int i = 0; i < currentProceduralUniverse[currentGalaxyIndex].collectedStars.Length; i++)
        {
            currentProceduralUniverse[currentGalaxyIndex].collectedStars[i] |= starScore[i]; 
            starTotal++;
        }
        UniverseParams universeParams = currentUniverse;
        universeParams.numStars += starTotal;
        setProceduralUniverse?.Invoke(universeParams);
    }
    void LevelInitiation(GalaxyData theme) //Just sets theme, called by procedural level generation in start before generation
    {
        currentTheme = theme;
    }
    void IsProcedural(bool procedural) //Sets procedural to bool
    {
        this.procedural = procedural;
    }
    void OnScore(string score, int scoreRelToPar, int totalShots, Color scoreColor) //Updates score text to the score invoked in the event by LevelScoring
    {
        if(scoreText == null) { return; }
        scoreText.text = score;
        // if(procedural)
        // {
        //     int scene = SceneManager.GetActiveScene().buildIndex;
        //     AsyncOperation loadNextOp = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
        //     loadNextOp.allowSceneActivation = true;
        // }
    }

    #endregion
    private void OnEnable()
    {
        PointCardManager.onGalaxyScore += ScoreGalaxy;
        LevelManager.setProceduralUniverse += SetProceduralUniverseEvent; //Invoked by anything (usually create new game) to set the universe | (UniverseParams universeParams)
        LevelManager.setProceduralGalaxies += SetProceduralGalaxiesEvent; //Invoked by anything (galaxy menu manager) to set the galaxies | (GalaxyParams[] galaxyParams)
        LevelScoring.onScore += OnScore;                    //Invoked by level scoring after it calculated the score for the level | (string score, int scoreRelToPar)
        LevelGenerator.levelInit += LevelInitiation;        //Invoked by level generator in start when after generating the level
        LevelManager.notifyProcedural += IsProcedural;//Invoked by level generator in start when after generating the level
        GalaxyGenerator.initProceduralSystem += SetProceduralGalaxyLevels;//Invoked by procedural galaxy generator in awake after creating the solar systems | (string[] levels, LevelParams[] levelParams, GalaxyData theme)
        GalaxyGenerator.initSystem += SetGalaxyLevels;         //Invoked by galaxy levels in start | (string[] levels, GalaxyTheme currentTheme)
        ShootBall.onHole += CompleteCurrentLevel;           //Invoked by shoot ball on collision with the flag
        LevelSettingsManager.onLevelSettingChange += SetLevelParams;//Invoked by level setting params when the player changes a value to do with the level params | (LevelParams params)
    }
    private void OnDisable()
    {
        PointCardManager.onGalaxyScore -= ScoreGalaxy;
        LevelManager.setProceduralGalaxies -= SetProceduralGalaxiesEvent; //Invoked by anything (galaxy menu manager) to set the galaxies | (GalaxyParams[] galaxyParams)
        LevelManager.setProceduralUniverse -= SetProceduralUniverseEvent; //Invoked by anything (usually create new game) to set the universe | (UniverseParams universeParams)
        LevelScoring.onScore -= OnScore;
        LevelGenerator.levelInit -= LevelInitiation;
        LevelManager.notifyProcedural -= IsProcedural;
        GalaxyGenerator.initSystem -= SetGalaxyLevels;
        ShootBall.onHole -= CompleteCurrentLevel;
        GalaxyGenerator.initProceduralSystem -= SetProceduralGalaxyLevels;
        LevelSettingsManager.onLevelSettingChange -= SetLevelParams;
    }
}
