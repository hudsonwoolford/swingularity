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
    [SerializeField] Color positive, neutral, negative;
    [SerializeField] Color holeInOne;
    public delegate void GalaxyScoring(int scoreSum, int shotsSum, int parSum, bool starCollected);
    public static GalaxyScoring onGalaxyScore;
    bool galaxyStarCollected;
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
        if (!GameManager.LevelManager.procedural) //Procedural processes differently because the scene name is always the same
        {
            int i = GameManager.LevelManager.levelIndex;

            SetPointCardVar(i, relPar, totalShots);
        }
        else //Procedural processing
        {
            int i = GameManager.LevelManager.levelIndex;
            SetPointCardVar (i, relPar, totalShots);

            if(i == 4)
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
                onGalaxyScore?.Invoke(scoreSum, shotSum, parSum, galaxyStarCollected);
            }
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
    void NewProceduralSystem(string[] galaxyLevels, LevelManager.LevelParams[] galaxyLevelParams, GalaxyParams galaxyParams, GalaxyData theme)
    {
        galaxyStarCollected = false;
        ClearPointCard();
    }
    void CollectibleCollected()
    {
        galaxyStarCollected = true;
    }
    private void OnEnable()
    {
        StarCollectible.onStarCollected += CollectibleCollected;
        GalaxyGenerator.initProceduralSystem += NewProceduralSystem;
        LevelScoring.onScore += OnScore;
    }
    private void OnDisable()
    {
        StarCollectible.onStarCollected -= CollectibleCollected;
        GalaxyGenerator.initProceduralSystem -= NewProceduralSystem;
        LevelScoring.onScore -= OnScore;
    }
}
