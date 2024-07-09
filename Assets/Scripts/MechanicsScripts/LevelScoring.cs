using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LevelScoring : MonoBehaviour
{
    [Header("Scoring")]
    [SerializeField] int par;
    [SerializeField] List<Score> scores = new List<Score>();
    [Header("UI")]
    //[SerializeField] Animator canvasAnimator;
    [SerializeField] Gradient scoreGradient;
    [SerializeField] TextMeshProUGUI parText;
    [SerializeField] TextMeshProUGUI numShotsText;
    [SerializeField] TextMeshProUGUI holeNoText;
    [SerializeField] TextMeshProUGUI GalaxyText;
    [Serializable]
    struct Score
    {
        public string name;
        public int scoreRelToPar;
    }
    int numShots;
    string currentScore;

    public delegate void OnScore(string score, int scoreRelToPar, int totalShots, Color scoreColor);
    public static OnScore onScore;

    private void Start()
    {
        CheckPar();
        SetGalaxyAndHole();
        parText.text = "Par: " + par.ToString();
        numShotsText.text = "Shots: " + numShots.ToString();
    }
    void SetGalaxyAndHole(){
        if(GameManager.LevelManager.procedural) //Procedural level pars
        {
            holeNoText.text = "Hole: " + (GameManager.LevelManager.levelIndex + 1).ToString();
            //Need something here to determine name of galaxy
        }
    }
    void CheckPar()
    {
        if(GameManager.LevelManager.procedural) //Procedural level pars
        {
            par = GameManager.LevelManager.levelParams.levelPar;
        }
    }
    void HoleEnd()
    {
        if(numShots == 1)
        {
            currentScore = "Hole in One!";
            onScore?.Invoke(currentScore, numShots - par, numShots, scoreGradient.Evaluate(0));
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
        onScore?.Invoke(currentScore, numShots - par, numShots, scoreGradient.Evaluate(Mathf.Clamp01(((float)numShots / 5)) * 0.5f + 0.5f));

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
        LevelGenerator.levelGenerated+=CheckPar;
        ShootBall.onShoot += IncrementScore;
        ShootBall.onHole += HoleEnd;
        ShootBall.triggerReset += ResetHole;
    }
    private void OnDisable()
    {
        LevelGenerator.levelGenerated+=CheckPar;
        ShootBall.onShoot -= IncrementScore;
        ShootBall.onHole -= HoleEnd;
        ShootBall.triggerReset -= ResetHole;

    }
}
