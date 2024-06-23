using System;
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
    [SerializeField] public bool procedural { get; private set; } = false; //Whether you are currently in a procedural level or not
    [Header("Parameters")]
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
    [Serializable]
    public struct LevelParams{
        public int seed; //Seed for level planets and stuff
        public int levelPar;
        public int difficulty; //Range 0-10 determining size of level and number of special objects
        public ColorSchemeStaticReference.PlanetType[] themes; //Themes/color schemes for level - First scheme in list is primary
        public LevelGenerator.SpecialType[] specialObjects;
        public LevelParams(int seed, int difficulty, int par, ColorSchemeStaticReference.PlanetType[] themes, LevelGenerator.SpecialType[] specialObjects)
        {
            this.seed = seed;
            this.levelPar = par;
            this.difficulty = difficulty;
            this.themes = themes;
            this.specialObjects = specialObjects;
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public static bool operator ==(LevelParams a, LevelParams b)
        {
            return (a.seed == b.seed && a.levelPar == b.levelPar && a.difficulty == b.difficulty && a.themes == b.themes && a.specialObjects == b.specialObjects);
        }
        public static bool operator !=(LevelParams a, LevelParams b)
        {
            return (a.seed != b.seed || a.levelPar != b.levelPar || a.difficulty != b.difficulty || a.themes != b.themes || a.specialObjects != b.specialObjects);
        }
    }

    private void Awake()
    {
        completedLevels = new List<string>();
        levelCanvas = GetComponent<Canvas>();
        levelCanvas.enabled = false;
        chosenOption = false;
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
        int currentIndex = Array.IndexOf(currentGalaxy, name);
        string nextName = currentIndex + 1 <= currentGalaxy.Length - 1 ? currentGalaxy[currentIndex + 1] : "Main Menu";

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
        chosenOption = false;
        loadNextOp.allowSceneActivation = true;
        levelCanvas.enabled = false;
    }
    IEnumerator ProceduralNextLevelUI() //Handles the UI that displays when you've completed the level, asynchronously loads the following level
    {
        levelCanvas.enabled = true;
        scoreText.enabled = true;
        streakText.enabled = false;
        hioStreakText.enabled = false;

        int levelIndex = 0;
        for (int i = 0; i < currentProceduralGalaxy.Length; i++)
        {
            if (levelParams == currentProceduralGalaxy[i]) //ADD == OPERAND FUNCTIONALITY PLEASE, THIS IS PROBABLY SUFFICIENT BUT COULD CAUSE ISSUES
            {
                levelIndex = i; break;
            }
        }
        print(levelIndex);
        string name = levelIndex+1 < 4 ? SceneManager.GetActiveScene().name : "Main Menu";
        levelParams = currentProceduralGalaxy[Mathf.Clamp(levelIndex + 1, 0, 4)];
        
        AsyncOperation loadNextOp = SceneManager.LoadSceneAsync(name);
        loadNextOp.allowSceneActivation = false;
        while (!chosenOption)
        {
            if (SceneManager.GetActiveScene().name != name)
            {
                levelCanvas.enabled = false;
                chosenOption = false;
                yield break;
            }
            yield return new WaitForFixedUpdate();
        }
        chosenOption = false;
        loadNextOp.allowSceneActivation = true;
        levelCanvas.enabled = false;
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
    void SetProceduralGalaxyLevels(string[] galaxy, LevelParams[] galaxyLevels, GalaxyData theme) //Sets the current galaxy levels, we also know it cannot be procedural
    {
        currentTheme = theme;
        procedural = true;
        currentGalaxy = galaxy;
        currentProceduralGalaxy = galaxyLevels;
    }
    void LevelInitiation(GalaxyData theme) //Sets procedural to true
    {
        currentTheme = theme;
    }
    void ProceduralInitiation()
    {
        procedural = true;
    }
    void OnScore(string score, int scoreRelToPar, int totalShots, Color scoreColor) //Updates score text to the score invoked in the event by LevelScoring
    {
        if(scoreText == null) { return; }
        scoreText.text = score;
        if(procedural)
        {
            int scene = SceneManager.GetActiveScene().buildIndex;
            AsyncOperation loadNextOp = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
            loadNextOp.allowSceneActivation = true;
        }
    }

    #endregion
    private void OnEnable()
    {
        LevelScoring.onScore += OnScore;                    //Invoked by level scoring after it calculated the score for the level | (string score, int scoreRelToPar)
        LevelGenerator.levelInit += LevelInitiation;        //Invoked by level generator in start when after generating the level
        LevelGenerator.proceduralInit += ProceduralInitiation;//Invoked by level generator in start when after generating the level
        ProceduralGalaxyGeneration.initProceduralGalaxy += SetProceduralGalaxyLevels;//Invoked by procedural galaxy generator in awake after creating the solar systems | (string[] levels, LevelParams[] levelParams, GalaxyData theme)
        GalaxyLevels.initGalaxy += SetGalaxyLevels;         //Invoked by galaxy levels in start | (string[] levels, GalaxyTheme currentTheme)
        ShootBall.onHole += CompleteCurrentLevel;           //Invoked by shoot ball on collision with the flag
        LevelSettingsManager.onLevelSettingChange += SetLevelParams;//Invoked by level setting params when the player changes a value to do with the level params | (LevelParams params)
    }
    private void OnDisable()
    {
        LevelScoring.onScore -= OnScore;
        LevelGenerator.levelInit -= LevelInitiation;
        GalaxyLevels.initGalaxy -= SetGalaxyLevels;
        ShootBall.onHole -= CompleteCurrentLevel;
        ProceduralGalaxyGeneration.initProceduralGalaxy -= SetProceduralGalaxyLevels;
        LevelSettingsManager.onLevelSettingChange -= SetLevelParams;
    }
}
