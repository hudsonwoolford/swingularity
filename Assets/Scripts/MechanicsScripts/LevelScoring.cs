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
    [SerializeField] TextMeshProUGUI powerText;
    [SerializeField] TextMeshProUGUI angleText;
    [SerializeField] Button hackModeBtn;
    [SerializeField] Color positive, neutral, negative;

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
        parText.text = par.ToString();
        numShotsText.text = numShots.ToString();
        numShotsText.color = positive;
        hackModeBtn.onClick.AddListener(() => Camera.main.GetComponent<HackMode>().EnterHackMode());
    }
    private void Update(){
        if(ShootBall.liningUpShot){
            powerText.text = Mathf.RoundToInt(ShootBall.shootPower).ToString() + "%";
            if(ShootBall.shootPower < 25){
                powerText.color = negative;
            }
            else{
                powerText.color = Color.white;
            }
            if(ShootBall.shootAngle >= 140 || ShootBall.shootAngle <= -140){
                angleText.color = negative;
            }
            else{
                angleText.color = Color.white;
            }
            if(ShootBall.shootAngle + 90 > 180){
                angleText.text = Mathf.RoundToInt(ShootBall.shootAngle - 270).ToString() + "°";
            }
            else{
                angleText.text = Mathf.RoundToInt(ShootBall.shootAngle + 90).ToString() + "°";
            }
        }
        else{
            angleText.color = Color.white;
            powerText.color = Color.white;
            angleText.text = "——°";
            powerText.text = "——%";
        }
    }

    void SetGalaxyAndHole(){
        if(GameManager.LevelManager.procedural) //Procedural level pars
        {
            holeNoText.text = "#" + (GameManager.LevelManager.levelIndex + 1).ToString();
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

        numShotsText.text = numShots.ToString();
        if(numShots < par){
            numShotsText.color = positive;
        }
        else if(numShots > par){
            numShotsText.color = negative;
        }
        else{
            numShotsText.color = neutral;
        }
        if(numShots == 12)
        {
            StartCoroutine(LevelFailed());
        }
    }
    IEnumerator LevelFailed()
    {
        ShootBall.death?.Invoke(3);
        yield return new WaitForSeconds(1.25f);
        ShootBall.onHole?.Invoke();
        HoleEnd();
    }
    void ResetHole()
    {
        numShots = 0;
        numShotsText.text = numShots.ToString();
        numShotsText.color = positive;
    }

    void Pause(){
        angleText.transform.parent.gameObject.SetActive(false);
        numShotsText.transform.parent.gameObject.SetActive(false);
    }
    void UnPause(){
        angleText.transform.parent.gameObject.SetActive(true);
        numShotsText.transform.parent.gameObject.SetActive(true);
    }
    
    private void OnEnable()
    {
        LevelGenerator.levelGenerated += CheckPar;
        ShootBall.onShoot += IncrementScore;
        ShootBall.onHole += HoleEnd;
        ShootBall.triggerReset += ResetHole;
        PauseManager.pauseGame += Pause;
        PauseManager.unpauseGame += UnPause;
    }
    private void OnDisable()
    {
        LevelGenerator.levelGenerated -= CheckPar;
        ShootBall.onShoot -= IncrementScore;
        ShootBall.onHole -= HoleEnd;
        ShootBall.triggerReset -= ResetHole;
        PauseManager.pauseGame -= Pause;
        PauseManager.unpauseGame -= UnPause;
    }
}
