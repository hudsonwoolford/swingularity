using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelScoring : MonoBehaviour
{
    [Header("Scoring")]
    [SerializeField] int par;
    [SerializeField] List<Score> scores = new List<Score>();
    [Header("UI")]
    //[SerializeField] Animator canvasAnimator;
    [SerializeField] TextMeshProUGUI parText;
    [SerializeField] TextMeshProUGUI numShotsText;
    [Serializable]
    struct Score
    {
        public string name;
        public int scoreRelToPar;
    }
    int numShots;
    string currentScore;

    public delegate void OnScore(string score, int scoreRelToPar);
    public static OnScore onScore;

    private void Start()
    {
        //holeScoreText.gameObject.SetActive(false);
        parText.text = "Par: " + par.ToString();
        numShotsText.text = "Shots: " + numShots.ToString();
    }
    void HoleEnd()
    {
        if(numShots == 1)
        {
            currentScore = "Hole in One!";
            onScore?.Invoke(currentScore, numShots - par);
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
        onScore?.Invoke(currentScore, numShots - par);


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
