using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static LevelManager;
using static GalaxyGenerator;

public class LevelManager : MonoBehaviour
{
    [SerializeField] public GalaxyData currentTheme { get; private set; }
    [SerializeField] public LevelParams levelParams { get; private set; }
    [SerializeField] public GalaxyParams[] currentProceduralUniverse { get; private set; } //An array of galaxy params in order that correspond to which universe you are in
    [SerializeField] public string[] currentGalaxy { get; private set; } //An array of scene names in order that correspond to which galaxy you are in
    [SerializeField] public LevelParams[] currentProceduralGalaxy { get; private set; } //An array of level params in order that correspond to which galaxy you are in
    [SerializeField] public int galaxyIndex { get; private set; }
    [SerializeField] public int levelIndex { get; private set; } = 0; //The index of the current level
    [SerializeField] public bool procedural { get; private set; } = false; //Whether you are currently in a procedural universe or not
    [SerializeField] public bool isReset { get; private set; } = false; //Whether you are currently playing a resetted galaxy or not
    [SerializeField] public bool isPointCard { get; private set; } = false; //Whether point card is enabled
    [SerializeField] public UniverseParams currentUniverse { get; private set; }
    public float[] _DifficultyStarMult => difficultyStarMult;
    [Header("Parameters")]
    [SerializeField] float[] difficultyStarMult = new float[10];
    [SerializeField] LevelGenStaticReferences levelStaticValue;
    [SerializeField] GalaxyData[] galaxyDatas;
    [SerializeField] TextMeshProUGUI scoreText; //Text that displays score
    [Space()]
    [SerializeField] TextMeshProUGUI streakText; //Text that displays under par streak
    [SerializeField] TextMeshProUGUI hioStreakText; //Text that hole in one streak
    [Header("Serialized Variables")]
    [SerializeField] List<string> completedLevels; //A list of the scene names of completed levels
    Animator levelMenuAnimator; //The canvas for level UI
    bool chosenOption; //True when the player has clicked a button on the canvas UI

    [Serializable]
    public struct LevelParams
    {
        public int seed; // Seed for level planets and stuff
        public int levelPar;
        public int difficulty; // Range 0-10 determining size of level and number of special objects
        public ColorSchemeStaticReference.PlanetType[] themes; // Themes/color schemes for level - First scheme in list is primary
        public LevelGenerator.SpecialType[] specialObjects;
        public bool hasCollectible;

        public LevelParams(int seed, int difficulty, int par, ColorSchemeStaticReference.PlanetType[] themes, LevelGenerator.SpecialType[] specialObjects, bool hasCollectible)
        {
            this.seed = seed;
            this.levelPar = par;
            this.difficulty = difficulty;
            this.themes = themes;
            this.specialObjects = specialObjects;
            this.hasCollectible = hasCollectible;
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
            hash = (hash * 397) ^ hasCollectible.GetHashCode();
            return hash;
        }

        public static bool operator ==(LevelParams a, LevelParams b)
        {
            return a.seed == b.seed
                && a.levelPar == b.levelPar
                && a.difficulty == b.difficulty
                && (a.themes == b.themes || (a.themes != null && b.themes != null && a.themes.SequenceEqual(b.themes)))
                && (a.specialObjects == b.specialObjects || (a.specialObjects != null && b.specialObjects != null && a.specialObjects.SequenceEqual(b.specialObjects)))
                && a.hasCollectible == b.hasCollectible;
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
        [Range(1, 10)]
        public int relativeDifficulty; //relative difficulty of galaxy (1-10)
        public int galaxyDataIndex;
        public ColorSchemeStaticReference.PlanetType[] possibleTypes;
        public LevelGenerator.SpecialType[] possibleSpecial;
        public bool[] collectedStars;
        public int requiredStars;
        public string bestScore;

        public GalaxyParams(string name, int seed, int relativeDifficulty, int galaxyDataIndex, ColorSchemeStaticReference.PlanetType[] possibleTypes, LevelGenerator.SpecialType[] possibleSpecial, bool[] collectedStars, int requiredStars, string bestScore, bool hasCollectedCollectible)
        {
            this.name = name;
            this.seed = seed;
            this.galaxyDataIndex = galaxyDataIndex;
            this.relativeDifficulty = relativeDifficulty;
            this.possibleTypes = possibleTypes;
            this.possibleSpecial = possibleSpecial;
            this.collectedStars = collectedStars;
            this.requiredStars = requiredStars;
            this.bestScore = bestScore;
        }
        public bool EqualNoStar(GalaxyParams a, GalaxyParams b)
        {
            return a.name == b.name
            && a.seed == b.seed
            && a.relativeDifficulty == b.relativeDifficulty
            && (a.possibleTypes == b.possibleTypes || (a.possibleTypes != null && b.possibleTypes != null && a.possibleTypes.SequenceEqual(b.possibleTypes)))
            && (a.possibleSpecial == b.possibleSpecial || (a.possibleSpecial != null && b.possibleSpecial != null && a.possibleSpecial.SequenceEqual(b.possibleSpecial)));
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
            hash = (hash * 397) ^ galaxyDataIndex.GetHashCode();
            hash = (hash * 397) ^ possibleTypes.GetHashCode();
            hash = (hash * 397) ^ possibleSpecial.GetHashCode();
            hash = (hash * 397) ^ collectedStars.GetHashCode();
            hash = (hash * 397) ^ requiredStars.GetHashCode();
            hash = (hash * 397) ^ bestScore.GetHashCode();
            return hash;
        }

        public static bool operator ==(GalaxyParams a, GalaxyParams b)
        {
            return a.name == b.name
                && a.seed == b.seed
                && a.relativeDifficulty == b.relativeDifficulty
                && (a.possibleTypes == b.possibleTypes || (a.possibleTypes != null && b.possibleTypes != null && a.possibleTypes.SequenceEqual(b.possibleTypes)))
                && (a.possibleSpecial == b.possibleSpecial || (a.possibleSpecial != null && b.possibleSpecial != null && a.possibleSpecial.SequenceEqual(b.possibleSpecial)))
                && (a.collectedStars == b.collectedStars || (a.collectedStars != null && b.collectedStars != null && a.collectedStars.SequenceEqual(b.collectedStars)))
                && a.requiredStars == b.requiredStars
                && a.bestScore == b.bestScore;
        }

        public static bool operator !=(GalaxyParams a, GalaxyParams b)
        {
            return !(a == b);
        }
    }
    [Serializable]
    public struct UniverseParams
    {
        public enum UniverseType{
            procedural,
            home,
            challenge,
            parallel,
            recursive,
            shrodinger,
            shattered,
            nebula,
        }
        public string name; //name of the universe save, set in new game
        public int seed; //master seed, determines everything else
        [Range(0, 10)]
        public int relativeDifficulty; //relative difficulty of universe (1-10)
        public string size; //the size of the universe (small, medium, large)
        public int completedGalaxies; //number of galaxies that have been completed
        public int numGalaxies; //number of galaxies in the universe
        public int numStars;
        public UniverseType universeType;
        public UniverseParams(string name, int seed, int relativeDifficulty, string size, int completedGalaxies, int numGalaxies,int numStars, UniverseType universeType)
        {
            this.name = name;
            this.seed = seed;
            this.relativeDifficulty = relativeDifficulty;
            this.size = size;
            this.completedGalaxies = completedGalaxies;
            this.numGalaxies = numGalaxies;
            this.numStars = numStars;
            this.universeType = universeType;
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
            hash = (hash * 397) ^ numStars.GetHashCode();
            hash = (hash * 397) ^ universeType.GetHashCode();
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
                && a.numStars == b.numStars
                && a.universeType == b.universeType;
        }

        public static bool operator !=(UniverseParams a, UniverseParams b)
        {
            return !(a == b);
        }
    }

    //events
    public delegate void NotifyProcedural(bool procedural);
    public static NotifyProcedural notifyProcedural;
    public delegate void SetUniverse(UniverseParams universeParams);
    public static SetUniverse setUniverse;
    public delegate void SetGalaxies(GalaxyParams[] galaxyParams);
    public static SetGalaxies setGalaxies;

    public static bool ballInput { get; private set; }
    public static bool levelComplete { get; private set; }
    public static LevelGenStaticReferences levelStatic { get; private set; } //global level static references
    public static GalaxyData[] allGalaxyData { get; private set; } //all possible galaxy data

    private void Awake()
    {
        levelStatic = levelStaticValue;
        allGalaxyData = galaxyDatas;
        completedLevels = new List<string>();
        levelMenuAnimator = GetComponent<Animator>();
        levelMenuAnimator.Play("DisableMenu");
        chosenOption = false;
        currentTheme = allGalaxyData[0];
        currentUniverse = new UniverseParams();
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        CheatCodeInput();
    }
    void CheatCodeInput() //CHEATS
    {
        if(Input.GetKey(KeyCode.U) && Input.GetKeyDown(KeyCode.A) && SceneManager.GetActiveScene().name == "Galaxies")
        {
            UniverseParams cu = currentUniverse;
            cu.numStars = cu.numGalaxies * 5;
            currentUniverse = cu;
            for (int i = 0; i < currentProceduralUniverse.Length; i++)
            {
                for (int x = 0; x < currentProceduralUniverse[i].collectedStars.Length; x++)
                {
                    currentProceduralUniverse[i].collectedStars[x] = true;
                }
                currentProceduralUniverse[i].bestScore = "0";
            }
            GameManager.SaveLoadManager.Save();
            GameManager.SaveLoadManager.Load(currentUniverse.name + ".txt");
            GameManager.SceneHandling.LoadScene("Galaxies", false);
            GameManager.SaveLoadManager.IncrementSupernovae(currentUniverse.numGalaxies * currentUniverse.relativeDifficulty * 5);
        }
    }
    public void EnableBallInput(bool enable) //usually used by tutorial and dialogue
    {
        //Debug.Log("Ball input " + enable);
        ballInput = enable;
    }
    public void ResetGalaxy()
    {

    }
    public bool LevelCompleted(string level) //Check if a level has been completed, intended to be used external scripts
    {
        return completedLevels.Contains(level);
    }
    public void MenuUIButton() //Called by the "Next" button
    {
        StopAllCoroutines();
        isPointCard = false;
        levelMenuAnimator.Play("DisableMenu");
        GameManager.SceneHandling.LoadScene("Galaxies", false);
        chosenOption = true;
    }
    public void NextLevelUIButton() //Called by the "Next" button
    {
        chosenOption = true;
    }
    #region Next Level UI
    IEnumerator NextLevelUI() //Handles the UI that displays when you've completed the level, asynchronously loads the following level
    {
        isPointCard = true;
        levelMenuAnimator.Play("ShowMenu");
        scoreText.enabled = true;
        streakText.enabled = false;
        hioStreakText.enabled = false;

        chosenOption = false;
        DetermineCurrentLevelIndex(false);
        string currentName = SceneManager.GetActiveScene().name;
        string nextName = levelIndex < currentGalaxy.Length-1 ? currentGalaxy[levelIndex+1] : "Galaxies";
        if (SceneManager.GetActiveScene().name == "Mythical_4")
        {
            nextName = "Credits";
        }
        while (!chosenOption)
        {
            yield return new WaitForFixedUpdate();
        }

        levelIndex++;
        chosenOption = false;
        levelMenuAnimator.Play("DisableMenu");
        
        if(nextName == "Credits")
        {
            GameManager.SceneHandling.LoadScene(nextName, false);
            isPointCard = false;
            yield break;
        }
        GameManager.SceneHandling.LoadScene(nextName, nextName != "Galaxies");
        isPointCard = false;
    }
    IEnumerator ProceduralNextLevelUI() //Handles the UI that displays when you've completed the level, asynchronously loads the following level
    {
        if (currentProceduralGalaxy == null || currentProceduralGalaxy.Length == 0)
        {
            GameManager.SceneHandling.LoadScene("LevelGen", true);

            yield break;
        }

        // Ensure levelIndex is within a valid range
        if (levelIndex < 0 || levelIndex >= currentProceduralGalaxy.Length)
        {
            GameManager.SceneHandling.LoadScene("LevelGen", true);
            yield break;
        }

        isPointCard = true;
        //enable things
        levelMenuAnimator.Play("ShowMenu");
        scoreText.enabled = true;
        streakText.enabled = false;
        hioStreakText.enabled = false;
        //determine current level index
        DetermineCurrentLevelIndex(true);
        //complete universe
        if(levelIndex>=4) 
        {
            UniverseParams cU = currentUniverse;
            cU.completedGalaxies++;
            setUniverse?.Invoke(cU);
        }
        //determine next level string
        string currentName = SceneManager.GetActiveScene().name;
        string sceneName = levelIndex < 4 ? SceneManager.GetActiveScene().name/*"Procedural Galaxy"*/ : "Galaxies";
        levelParams = currentProceduralGalaxy[Mathf.Clamp(levelIndex + 1, 0, currentProceduralGalaxy.Length-1)];
        chosenOption = false;

        while (!chosenOption)
        {

            yield return new WaitForFixedUpdate();
        }

        levelIndex = levelIndex < 4 ? levelIndex + 1 : 0;
        chosenOption = false;
        levelMenuAnimator.Play("DisableMenu");

        GameManager.SceneHandling.LoadScene(sceneName, sceneName != "Galaxies");
        isPointCard = false;
    }
    void DetermineCurrentLevelIndex(bool determineProcedural)
    {
        if (determineProcedural)
        {
            for (int i = 0; i < currentProceduralGalaxy.Length; i++)
            {
                if (levelParams == currentProceduralGalaxy[i])
                {
                    levelIndex = i; break;
                }
            }
        }
        else
        {
            for (int i = 0; i < currentGalaxy.Length; i++)
            {
                if(SceneManager.GetActiveScene().name == currentGalaxy[i])
                {
                    levelIndex = i; break;
                }
            }
        }
    }
    #endregion
    #region EventFunctions
    public void SetLevelParams(LevelParams lp)
    {
      
        levelParams = lp;
    }
    public void RestartGalaxy()
    {
        levelIndex = 0;
        if(!procedural)
        {
            initSystem?.Invoke(currentGalaxy, currentProceduralUniverse[galaxyIndex], allGalaxyData[currentProceduralUniverse[galaxyIndex].galaxyDataIndex]);
        }
        if(procedural)
        {
            initProceduralSystem?.Invoke(currentGalaxy, currentProceduralGalaxy, currentProceduralUniverse[galaxyIndex], allGalaxyData[currentProceduralUniverse[galaxyIndex].galaxyDataIndex]);
            levelParams = currentProceduralGalaxy[0];
        }
        GameManager.SceneHandling.LoadScene(currentGalaxy[0], true);
        if(TutorialManager.instance != null)
        {
            TutorialManager.instance.CompleteTutorial();
        }
        isReset = true;
    }
    void CompleteCurrentLevel() //Adds the current level to completed level array
    {
        ballInput = false;
        levelComplete = true;
        if (procedural) { StartCoroutine(ProceduralNextLevelUI()); return; }

        string name = SceneManager.GetActiveScene().name;
        if(!completedLevels.Contains(name)) { 
            completedLevels.Add(name);
        }

        StartCoroutine(NextLevelUI());
    }
    void SetGalaxyLevels(string[] galaxy, GalaxyParams galaxyParams, GalaxyData theme) //Sets the current galaxy levels, we also know it cannot be procedural
    {
        currentTheme = theme;
        procedural = false;
        currentGalaxy = galaxy;

        levelIndex = 0;
       for (int i = 0; i < currentProceduralUniverse.Length; i++)
        {
            if (currentProceduralUniverse[i] == galaxyParams) { galaxyIndex = i; break; }
        }
        isReset = false;
    }
    void SetProceduralGalaxyLevels(string[] galaxy, LevelParams[] galaxyLevels, GalaxyParams galaxyParams, GalaxyData theme) //Sets the current galaxy levels, we also know it cannot be procedural
    {
        currentTheme = theme;
        procedural = true;
        currentGalaxy = galaxy;
        currentProceduralGalaxy = galaxyLevels;
        levelIndex = 0;

        for (int i = 0; i < currentProceduralUniverse.Length; i++)
        {
            if (currentProceduralUniverse[i] == galaxyParams) { galaxyIndex = i; break; }
        }
        isReset = false;
    }
    void SetGalaxiesEvent(GalaxyParams[] galaxyParams) => currentProceduralUniverse = galaxyParams; //Sets the current universe
    void SetUniverseEvent(UniverseParams universeParams)
    {
        currentUniverse = universeParams; //Sets the current universe
    }
    void LevelInitiation(GalaxyData theme) => currentTheme = theme;//Just sets theme, called by procedural level generation in start before generation
    void IsProcedural(bool p) => procedural = p;  //sets whether procedural
    void ScoreGalaxy(int scoreSum, int shotsSum, int parSum, bool collectedStar, int prevGalaxyScore)
    {
        levelIndex = 0;
        bool[] starScore = new bool[]
        {
            true,
            scoreSum <= 0,
            scoreSum <= -3 || shotsSum == 5,
            collectedStar,
            shotsSum == 5
        };
        int starTotal = 0;
        for (int i = 0; i < currentProceduralUniverse[galaxyIndex].collectedStars.Length; i++)
        {
            if (starScore[i] && !currentProceduralUniverse[galaxyIndex].collectedStars[i]){starTotal++;}
            currentProceduralUniverse[galaxyIndex].collectedStars[i] |= starScore[i]; 
        }
        if(scoreSum > 0){
            currentProceduralUniverse[galaxyIndex].bestScore = "+" + scoreSum.ToString();
        }
        else{
            currentProceduralUniverse[galaxyIndex].bestScore = scoreSum.ToString();  
        }
        UniverseParams universeParams = currentUniverse;
        universeParams.numStars += starTotal;
        GameManager.SaveLoadManager.IncrementSupernovae(Mathf.RoundToInt(starTotal * difficultyStarMult[currentUniverse.relativeDifficulty]));
        setUniverse?.Invoke(universeParams);
        GameManager.SaveLoadManager.Save();
    }
    void OnScore(string score, int scoreRelToPar, int totalShots, Color scoreColor) //Updates score text to the score invoked in the event by LevelScoring
    {
        ballInput = false;
        levelComplete = true;
        if (scoreText == null) { return; }
        scoreText.text = score;
        // if(procedural)
        // {
        //     int scene = SceneManager.GetActiveScene().buildIndex;
        //     AsyncOperation loadNextOp = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
        //     loadNextOp.allowSceneActivation = true;
        // }
    }
    void PauseGame() => ballInput = false;
    void UnpauseGame() => ballInput = true;
    void OnSceneLoad(Scene scene, LoadSceneMode loadSceneMode)
    {
        if(scene.name == "Main Menu")
        {
            galaxyIndex = 0;
        }
        
        chosenOption = true;
        ballInput = true;
        levelComplete = false;

        //check if scene name matches galaxy type DEBUG TOOL, USEFUL FOR CREATING LEVELS
        //if(scene.name.IndexOf('_') > 0)
        //{
        //    string prefix = scene.name.Substring(0, scene.name.IndexOf('_')).ToLower();
        //    if (currentUniverse != null && prefix.Length != 0 && prefix != currentUniverse.universeType.ToString())
        //    {
        //        foreach (var val in Enum.GetValues(typeof(UniverseParams.UniverseType)))
        //        {
        //            if (prefix == val.ToString())
        //            {
        //                UniverseParams currentUni = currentUniverse;
        //                currentUni.universeType = (UniverseParams.UniverseType)val;
        //                currentUniverse = currentUni;
        //            }
        //        }
        //    }
        //}
    }

    #endregion
    private void OnEnable()
    {
        PauseManager.pauseGame += PauseGame;
        PauseManager.unpauseGame += UnpauseGame;
        SceneManager.sceneLoaded += OnSceneLoad;
        PointCardManager.onGalaxyScore += ScoreGalaxy;
        LevelManager.setUniverse += SetUniverseEvent; //Invoked by anything (usually create new game) to set the universe | (UniverseParams universeParams)
        LevelManager.setGalaxies += SetGalaxiesEvent; //Invoked by anything (galaxy menu manager) to set the galaxies | (GalaxyParams[] galaxyParams)
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
        PauseManager.pauseGame -= PauseGame;
        PauseManager.unpauseGame -= UnpauseGame;
        SceneManager.sceneLoaded -= OnSceneLoad;
        PointCardManager.onGalaxyScore -= ScoreGalaxy;
        LevelManager.setGalaxies -= SetGalaxiesEvent; //Invoked by anything (galaxy menu manager) to set the galaxies | (GalaxyParams[] galaxyParams)
        LevelManager.setUniverse -= SetUniverseEvent; //Invoked by anything (usually create new game) to set the universe | (UniverseParams universeParams)
        LevelScoring.onScore -= OnScore;
        LevelGenerator.levelInit -= LevelInitiation;
        LevelManager.notifyProcedural -= IsProcedural;
        GalaxyGenerator.initSystem -= SetGalaxyLevels;
        ShootBall.onHole -= CompleteCurrentLevel;
        GalaxyGenerator.initProceduralSystem -= SetProceduralGalaxyLevels;
        LevelSettingsManager.onLevelSettingChange -= SetLevelParams;
    }
}
