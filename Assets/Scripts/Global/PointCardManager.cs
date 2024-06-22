using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using static LevelManager;
using UnityEngine.Rendering.VirtualTexturing;

public class PointCardManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI[] parTexts; //Text that displays par for a level, 5 in array
    [SerializeField] TextMeshProUGUI[] scoreTexts; //Text that displays play relative score for a level, 5 in array
    [SerializeField] TextMeshProUGUI[] outlineScoreTexts; //Text that provides coloured outlines to the score text, 5 in array
    string[] currentGalaxy; //An array of scene names in order that correspond to which galaxy you are in
    LevelManager.LevelParams[] currentProceduralGalaxy; //An array of scene names in order that correspond to which galaxy you are in

    // Start is called before the first frame update
    void Start()
    {
        ClearPointCard(); //Resetting the scorecard to unkown
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void ClearPointCard() //Iterates through all text and makes them "?"
    {
        for (int i = 0; i < 5; i++)
        {
            parTexts[i].text = "?";
            scoreTexts[i].text = "?";
            outlineScoreTexts[i].text = "?";
            outlineScoreTexts[i].color = new Color(0,0,0,0);
        }
    }
    void OnScore(string score, int relPar, int totalShots, Color scoreColor) //Called by onScore delegate from LevelScoring, sets the text required for the current level
    {
        if(currentGalaxy == null) { return; }
        if (!GameManager.LevelManager.procedural) //Procedural processes differently because the scene name is always the same
        {
            string sceneName = SceneManager.GetActiveScene().name;
            for (int i = 0; i < currentGalaxy.Length; i++)
            {
                if (sceneName == currentGalaxy[i])
                {
                    //Setting text strings and colours
                    scoreTexts[i].text = relPar.ToString();
                    outlineScoreTexts[i].text = relPar.ToString();
                    scoreTexts[i].color = scoreColor;
                    parTexts[i].text = (totalShots - relPar).ToString();
                    break;
                }
            }
        }
        else //Procedural processing
        {
            for (int i = 0; i < currentProceduralGalaxy.Length; i++)
            {
                if (GameManager.LevelManager.levelParams == currentProceduralGalaxy[i]) //ADD == OPERAND FUNCTIONALITY PLEASE, THIS IS PROBABLY SUFFICIENT BUT COULD CAUSE ISSUES
                {
                    i--; //because level manager has already switched to the next params
                    //Setting text strings and colours
                    scoreTexts[i].text = relPar.ToString();
                    outlineScoreTexts[i].text = relPar.ToString();
                    scoreTexts[i].color = scoreColor;
                    parTexts[i].text = (totalShots - relPar).ToString();
                    break;
                }
            }
        }
    }
    void SetGalaxyLevels(string[] galaxy, GalaxyData theme) //Sets the current galaxy levels
    {
        currentGalaxy = galaxy;
    }
    void SetProceduralGalaxyLevels(string[] galaxy, LevelParams[] galaxyLevels, GalaxyData theme) //Sets the current galaxy levels, we also know it cannot be procedural
    {
        currentProceduralGalaxy = galaxyLevels;
        currentGalaxy = galaxy;
    }
    private void OnEnable()
    {
        ProceduralGalaxyGeneration.initProceduralGalaxy += SetProceduralGalaxyLevels;//Invoked by procedural galaxy generator in awake after creating the solar systems | (string[] levels, LevelParams[] levelParams, GalaxyData theme)
        GalaxyLevels.initGalaxy += SetGalaxyLevels;         //Invoked by galaxy levels in start | (string[] levels)
        LevelScoring.onScore += OnScore;
    }
    private void OnDisable()
    {
        GalaxyLevels.initGalaxy -= SetGalaxyLevels;         //Invoked by galaxy levels in start | (string[] levels)
        ProceduralGalaxyGeneration.initProceduralGalaxy -= SetProceduralGalaxyLevels;
        LevelScoring.onScore -= OnScore;
    }
}
