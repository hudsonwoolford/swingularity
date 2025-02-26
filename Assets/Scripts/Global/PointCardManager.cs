using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using static LevelManager;
using System.Linq;

public class PointCardManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI[] parTexts; //Text that displays par for a level, 5 in array
    [SerializeField] TextMeshProUGUI[] scoreTexts; //Text that displays play relative score for a level, 5 in array
    [SerializeField] TextMeshProUGUI[] shotsTexts; //Text that displays play shots for a level, 5 in array
    [SerializeField] Color positive, neutral, negative;
    [SerializeField] Color holeInOne;
    [Header("Sound")]
    [SerializeField] AudioObj holeInOneAudio;
    [SerializeField] AudioObj showPointCardAudio;
    public delegate void GalaxyScoring(int scoreSum, int shotsSum, int parSum, bool starCollected, int prevGalaxyScore);
    public static GalaxyScoring onGalaxyScore;
    bool galaxyStarCollected;
    bool lastLevel;

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
            parTexts[i].text = "—";
            scoreTexts[i].color = new Color(1,1,1,1);
            scoreTexts[i].text = "—";
            shotsTexts[i].color = new Color(1,1,1,1);
            shotsTexts[i].text = "—";
        }
    }
    void OnScore(string score, int relPar, int totalShots, Color scoreColor) //Called by onScore delegate from LevelScoring, sets the text required for the current level
    {
        int i = GameManager.LevelManager.levelIndex;
        SetPointCardVar (i, relPar, totalShots);

        if(totalShots == 1) { GameManager.SFXManager.PlaySFX(holeInOneAudio); } 
        else { GameManager.SFXManager.PlaySFX(showPointCardAudio); }

        if (i == 4)
        {
            lastLevel = true;

        }
    }
    void SetPointCardVar(int i, int relPar, int totalShots)
    {
        //Setting text strings and colours
        if (relPar > 0)
        {
            scoreTexts[i].text = "+" + relPar.ToString();
        }
        else
        {
            scoreTexts[i].text = relPar.ToString();
        }
        shotsTexts[i].text = totalShots.ToString();
        Color col = neutral;
        if (totalShots < (totalShots - relPar))
        {
            col = positive;
        }
        else if (totalShots > (totalShots - relPar))
        {
            col = negative;
        }

        shotsTexts[i].color = col;
        scoreTexts[i].color = col;
        parTexts[i].text = (totalShots - relPar).ToString();

        int scoreSum = 0, shotSum = 0, parSum = 0;
        for (int j = 0; j < 5; j++)
        {
            if (scoreTexts[j].text != "—")
            {
                scoreSum += int.Parse(scoreTexts[j].text);
            }
            if (shotsTexts[j].text != "—")
            {
                shotSum += int.Parse(shotsTexts[j].text);
            }
            if (parTexts[j].text != "—")
            {
                parSum += int.Parse(parTexts[j].text);
            }
        }

        Color scoreCol = neutral;
        if (shotSum < parSum)
        {
            scoreCol = positive;
        }
        else if (shotSum > parSum)
        {
            scoreCol = negative;
        }

        scoreTexts[5].color = scoreCol;
        if(scoreSum >=20) { scoreTexts[5].fontSize = 60; }
        else { scoreTexts[5].fontSize =75; }

        if (scoreSum > 0)
        {
            scoreTexts[5].text = "+" + scoreSum.ToString();
        }
        else
        {
            scoreTexts[5].text = scoreSum.ToString();
        }
        shotsTexts[5].text = shotSum.ToString();
        scoreTexts[5].color = scoreCol;
        shotsTexts[5].color = scoreCol;
        parTexts[5].text = parSum.ToString();
    }
    void NewSystem(string[] galaxyLevels, GalaxyParams galaxyParams, GalaxyData theme)
    {
        galaxyStarCollected = false;
        ClearPointCard();
    }
    void NewProceduralSystem(string[] galaxyLevels, LevelManager.LevelParams[] galaxyLevelParams, GalaxyParams galaxyParams, GalaxyData theme)
    {
        galaxyStarCollected = false;
        ClearPointCard();
    }
    void CollectibleCollected()
    {
        galaxyStarCollected = true;
    }
    void ScoreGalaxy()
    {
        int scoreSum = 0, shotSum = 0, parSum = 0;
        for (int j = 0; j < 5; j++)
        {
            if (scoreTexts[j].text != "—")
            {
                scoreSum += int.Parse(scoreTexts[j].text);
            }
            if (shotsTexts[j].text != "—")
            {
                shotSum += int.Parse(shotsTexts[j].text);
            }
            if (parTexts[j].text != "—")
            {
                parSum += int.Parse(parTexts[j].text);
            }
        }


        onGalaxyScore?.Invoke(scoreSum, shotSum, parSum, galaxyStarCollected, SaveLoadManager.currentUniverseSave.allGalaxies[GameManager.LevelManager.galaxyIndex].collectedStars.Count(a => a == true));
        lastLevel = false;
    }
    void OnSceneLoad(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (lastLevel)
        {
            ScoreGalaxy();
        }

    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
        GalaxyGenerator.initSystem += NewSystem;
        GalaxyGenerator.initProceduralSystem += NewProceduralSystem;
        StarCollectible.onStarCollected += CollectibleCollected;
        LevelScoring.onScore += OnScore;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
        GalaxyGenerator.initSystem -= NewSystem;
        StarCollectible.onStarCollected -= CollectibleCollected;
        GalaxyGenerator.initProceduralSystem -= NewProceduralSystem;
        LevelScoring.onScore -= OnScore;
    }
}
