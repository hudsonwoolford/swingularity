using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PointCardManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI[] parTexts; //Text that displays par for a level, 5 in array
    [SerializeField] TextMeshProUGUI[] scoreTexts; //Text that displays play relative score for a level, 5 in array
    [SerializeField] TextMeshProUGUI[] outlineScoreTexts; //Text that provides coloured outlines to the score text, 5 in array
    string[] currentGalaxy; //An array of scene names in order that correspond to which galaxy you are in

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
        string sceneName = SceneManager.GetActiveScene().name;
        for (int i = 0; i < currentGalaxy.Length; i++)
        {
            if(sceneName == currentGalaxy[i])
            {
                //Setting text strings and colours
                scoreTexts[i].text = relPar.ToString();
                outlineScoreTexts[i].text = relPar.ToString();
                scoreTexts[i].color = scoreColor;
                parTexts[i].text = (totalShots-relPar).ToString();
                break;
            }
        }
    }
    void SetGalaxyLevels(string[] galaxy, GalaxyData theme) //Sets the current galaxy levels
    {
        currentGalaxy = galaxy;
    }
    private void OnEnable()
    {
        GalaxyLevels.initGalaxy += SetGalaxyLevels;         //Invoked by galaxy levels in start | (string[] levels)
        LevelScoring.onScore += OnScore;
    }
    private void OnDisable()
    {
        GalaxyLevels.initGalaxy -= SetGalaxyLevels;         //Invoked by galaxy levels in start | (string[] levels)
        LevelScoring.onScore -= OnScore;
    }
}
