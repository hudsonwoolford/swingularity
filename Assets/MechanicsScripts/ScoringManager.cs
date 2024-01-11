using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoringManager : MonoBehaviour
{
    [Header("Scoring")]
    [SerializeField] int par;
    [SerializeField] List<Score> scores = new List<Score>();
    [Header("UI")]
    [SerializeField] Animator canvasAnimator;
    [SerializeField] TextMeshProUGUI parText;
    [SerializeField] TextMeshProUGUI holeScoreText, numShotsText;
    [Serializable]
    struct Score
    {
        public string name;
        public int scoreRelToPar;
    }
    int numShots;

    private void Start()
    {
        holeScoreText.gameObject.SetActive(false);
        parText.text = "Par: " + par.ToString();
        numShotsText.text = "Shots: " + numShots.ToString();
    }
    void HoleEnd()
    {
        if(numShots == 1)
        {
            holeScoreText.text = "Hole in One!";
            canvasAnimator.Play("ShowScore");
            return;
        }
        bool namedScore = false;
        foreach(Score score in scores)
        {
            if(par + score.scoreRelToPar == numShots) 
            {
                namedScore = true;
                holeScoreText.text = score.name;
                break;
            }
        }
        if(!namedScore) 
        { 
            holeScoreText.text = numShots.ToString() + " Shots";
        }
        canvasAnimator.Play("ShowScore");


    }
    void IncrementScore()
    {
        numShots++;
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
