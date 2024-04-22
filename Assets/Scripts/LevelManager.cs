using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] TextMeshProUGUI scoreText; //Text that displays score
    [Space()]
    [SerializeField] TextMeshProUGUI streakText; //Text that displays under par streak
    [SerializeField] TextMeshProUGUI hioStreakText; //Text that hole in one streak
    [Header("Serialized Variables")]
    [SerializeField] List<string> completedLevels; //A list of the scene names of completed levels
    [SerializeField] string[] currentGalaxy; //An array of scene names in order that correspond to which galaxy you are in
    [SerializeField] bool procedural = false; //Whether you are currently in a procedural level or not
    Canvas levelCanvas; //The canvas for level UI
    bool chosenOption; //True when the player has clicked a button on the canvas UI

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
    #endregion
    #region EventFunctions
    void CompleteCurrentLevel() //Adds the current level to completed level array
    {
        if (procedural) { return; }

        string name = SceneManager.GetActiveScene().name;
        if(!completedLevels.Contains(name)) { 
            completedLevels.Add(name);
        }

        StartCoroutine(NextLevelUI());
    }
    void SetGalaxyLevels(string[] galaxy) //Sets the current galaxy levels, we also know it cannot be procedural
    {
        procedural = false;
        currentGalaxy = galaxy;
    }
    void ProceduralInitiation() //Sets procedural to true
    {
        procedural = true;
    }
    void OnScore(string score, int scoreRelToPar, int totalShots) //Updates score text to the score invoked in the event by LevelScoring
    {
        if(scoreText == null) { return; }
        scoreText.text = score;
        if(procedural)
        {
            StartCoroutine(LoadProcedural());
        }
    }
    IEnumerator LoadProcedural()
    {
        int scene = SceneManager.GetActiveScene().buildIndex;
        AsyncOperation loadNextOp = SceneManager.LoadSceneAsync(scene);
        loadNextOp.allowSceneActivation = true;
        while(!loadNextOp.isDone)
        {
            print(loadNextOp.progress);
            yield return new WaitForSeconds(0.01f);
        }
    }
    #endregion
    private void OnEnable()
    {
        LevelScoring.onScore += OnScore;                    //Invoked by level scoring after it calculated the score for the level | (string score, int scoreRelToPar)
        LevelGenerator.levelInit += ProceduralInitiation;   //Invoked by level generator in start when after generating the level
        GalaxyLevels.initGalaxy += SetGalaxyLevels;         //Invoked by galaxy levels in start | (string[] levels)
        ShootBall.onHole += CompleteCurrentLevel;           //Invoked by shoot ball on collision with the flag
    }
    private void OnDisable()
    {
        LevelScoring.onScore -= OnScore;
        LevelGenerator.levelInit -= ProceduralInitiation;
        GalaxyLevels.initGalaxy -= SetGalaxyLevels;
        ShootBall.onHole -= CompleteCurrentLevel;
    }
}
