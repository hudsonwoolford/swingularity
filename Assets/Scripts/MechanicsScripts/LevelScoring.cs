using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

public class LevelScoring : MonoBehaviour
{
    [Header("Scoring")]
    [SerializeField] int par;
    [SerializeField] List<Score> scores = new List<Score>();
    [Header("UI")]
    //[SerializeField] Animator canvasAnimator;
    [SerializeField] Transform sectorParent;
    [SerializeField] Color underColor = Color.white, parColor = Color.white, overColor = Color.white, doneColor = Color.white;
    [SerializeField] TextMeshProUGUI parText;
    [SerializeField] TextMeshProUGUI numShotsText;
    [Serializable]
    struct Score
    {
        public string name;
        public int scoreRelToPar;
    }
    Image[] sectors;
    int numShots;
    string currentScore;

    public delegate void OnScore(string score, int scoreRelToPar, int totalShots);
    public static OnScore onScore;

    private void Start()
    {
        //holeScoreText.gameObject.SetActive(false);
        parText.text = "Par: " + par.ToString();
        numShotsText.text = "Shots: " + numShots.ToString();
        sectors = sectorParent.GetComponentsInChildren<Image>();
        for (int i = 0; i < sectors.Length; i++)
        {
            sectors[i].color = i < par-1 ? underColor : i > par-1 ? overColor : parColor;
        }
    }
    void HoleEnd()
    {
        if(numShots == 1)
        {
            currentScore = "Hole in One!";
            onScore?.Invoke(currentScore, numShots - par, numShots);
            return;
        }
        bool namedScore = false;
        foreach(Score score in scores)
        {
            if(par + score.scoreRelToPar == numShots) 
            {
                namedScore = true;
                currentScore = score.name;
                break;
            }
        }
        if(!namedScore) 
        { 
            currentScore = numShots.ToString() + " Shots";
        }
        onScore?.Invoke(currentScore, numShots - par, numShots);


    }
    void IncrementScore()
    {
        sectors[numShots].color = doneColor;
        numShots++;
        numShots = Mathf.Clamp(numShots, 0, sectors.Length-1);
        numShotsText.text = "Shots: " + numShots.ToString();
    }
    void ResetHole()
    {
        numShots = 0;
        numShotsText.text = "Shots: " + numShots.ToString();
    }
    private void OnEnable()
    {
        ShootBall.onShoot += IncrementScore;
        ShootBall.onHole += HoleEnd;
        ShootBall.triggerReset += ResetHole;
    }
    private void OnDisable()
    {
        ShootBall.onShoot -= IncrementScore;
        ShootBall.onHole -= HoleEnd;
        ShootBall.triggerReset -= ResetHole;

    }
}
