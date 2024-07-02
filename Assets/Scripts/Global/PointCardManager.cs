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
    [SerializeField] TextMeshProUGUI[] shotsTexts; //Text that displays play shots for a level, 5 in array
    [SerializeField] TextMeshProUGUI[] outlineScoreTexts; //Text that provides coloured outlines to the score text, 5 in array
    string[] currentGalaxy; //An array of scene names in order that correspond to which galaxy you are in
    LevelManager.LevelParams[] currentProceduralGalaxy; //An array of scene names in order that correspond to which galaxy you are in
    [SerializeField] Color positive, neutral, negative;
    [SerializeField] Color holeInOne;

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
        for (int i = 0; i < 6; i++)
        {
            parTexts[i].text = "?";
            scoreTexts[i].text = "?";
            shotsTexts[i].text = "?";
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
                    shotsTexts[i].text = totalShots.ToString();
                    Color col = neutral;
                    if(totalShots < (totalShots - relPar)){
                        col = positive;
                    }
                    else if (totalShots > (totalShots - relPar)){
                        col = negative;
                    }

                    shotsTexts[i].color = col;
                    outlineScoreTexts[i].text = relPar.ToString();
                    scoreTexts[i].color = col;
                    parTexts[i].text = (totalShots - relPar).ToString();
                    
                    int scoreSum = 0, shotSum = 0, parSum = 0;
                    for(int j = 0; j < 5; j++){
                        if (scoreTexts[j].text != "?"){
                            scoreSum += int.Parse(scoreTexts[j].text);
                        }
                        if (shotsTexts[j].text != "?"){
                            shotSum += int.Parse(shotsTexts[j].text);
                        }
                        if (parTexts[j].text != "?"){
                            parSum += int.Parse(parTexts[j].text);
                        }
                    }

                    scoreTexts[5].text = scoreSum.ToString();
                    Color scoreCol = neutral;
                    if(shotSum < parSum){
                        scoreCol = positive;
                    }
                    else if (shotSum > parSum){
                        scoreCol = negative;
                    }
                    
                    scoreTexts[5].color = scoreCol;
                    shotsTexts[5].text = shotSum.ToString();
                    shotsTexts[5].color = scoreCol;
                    parTexts[5].text = parSum.ToString();
                    break;
                }
            }
        }
        else //Procedural processing
        {
            int i = GameManager.LevelManager.levelIndex;
            //Setting text strings and colours
            shotsTexts[i].text = totalShots.ToString();
            scoreTexts[i].text = relPar.ToString();
            outlineScoreTexts[i].text = relPar.ToString();
            Color col = neutral;
            if(totalShots < (totalShots - relPar)){
                col = positive;
            }
            else if (totalShots > (totalShots - relPar)){
                col = negative;
            }
            scoreTexts[i].color = col;
            shotsTexts[i].color = col;

            parTexts[i].text = (totalShots - relPar).ToString();

            int scoreSum = 0, shotSum = 0, parSum = 0;
            for(int j = 0; j < 5; j++){
                if (scoreTexts[j].text != "?"){
                    scoreSum += int.Parse(scoreTexts[j].text);
                }
                if (shotsTexts[j].text != "?"){
                    shotSum += int.Parse(shotsTexts[j].text);
                }
                if (parTexts[j].text != "?"){
                    parSum += int.Parse(parTexts[j].text);
                }
            }

            Color scoreCol = neutral;
            if(shotSum < parSum){
                scoreCol = positive;
            }
            else if (shotSum > parSum){
                scoreCol = negative;
            }


            scoreTexts[5].color = scoreCol;
            shotsTexts[5].color = scoreCol;
            scoreTexts[5].text = scoreSum.ToString();
            shotsTexts[5].text = shotSum.ToString();
            parTexts[5].text = parSum.ToString();
        }
    }
    void SetGalaxyLevels(string[] galaxy, GalaxyData theme) //Sets the current galaxy levels
    {
        ClearPointCard();
        currentGalaxy = galaxy;
    }
    void SetProceduralGalaxyLevels(string[] galaxy, LevelParams[] galaxyLevels, GalaxyData theme) //Sets the current galaxy levels, we also know it cannot be procedural
    {
        currentProceduralGalaxy = galaxyLevels;
        currentGalaxy = galaxy;
    }
    private void OnEnable()
    {
        GalaxyGenerator.initProceduralGalaxy += SetProceduralGalaxyLevels;//Invoked by procedural galaxy generator in awake after creating the solar systems | (string[] levels, LevelParams[] levelParams, GalaxyData theme)
        GalaxyGenerator.initGalaxy += SetGalaxyLevels;         //Invoked by galaxy levels in start | (string[] levels)
        LevelScoring.onScore += OnScore;
    }
    private void OnDisable()
    {
        GalaxyGenerator.initGalaxy -= SetGalaxyLevels;         //Invoked by galaxy levels in start | (string[] levels)
        GalaxyGenerator.initProceduralGalaxy -= SetProceduralGalaxyLevels;
        LevelScoring.onScore -= OnScore;
    }
}
