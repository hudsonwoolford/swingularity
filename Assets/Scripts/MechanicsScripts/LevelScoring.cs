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
    [SerializeField] Animator canvasAnimator;
    [SerializeField] Gradient scoreGradient;
    [SerializeField] Canvas levelCanvas;
    [SerializeField] TextMeshProUGUI parText;
    [SerializeField] TextMeshProUGUI numShotsText;
    [SerializeField] TextMeshProUGUI holeNoText;
    [SerializeField] TextMeshProUGUI powerText;
    [SerializeField] TextMeshProUGUI angleText;
    [SerializeField] TextMeshProUGUI powerTextPercent;
    [SerializeField] TextMeshProUGUI angleTextDegree;
    [SerializeField] Button hackModeBtn;
    [SerializeField] Button resetBtn;
    [SerializeField] Color positive, neutral, negative;
    bool hackMode, ballStill;
    bool resetUI;
    bool levelFailing;
    ShootBall shootBall;

    [Serializable]
    struct Score
    {
        public string name;
        public int scoreRelToPar;
    }
    int numShots;
    string currentScore;
    float timeOfBallRelease;


    public delegate void OnScore(string score, int scoreRelToPar, int totalShots, Color scoreColor);
    public static OnScore onScore;

    private void Start()
    {
        shootBall = GameObject.FindGameObjectWithTag("Player").GetComponent<ShootBall>();
        CheckPar();
        SetGalaxyAndHole();
        parText.text = par.ToString();
        numShotsText.text = numShots.ToString();
        numShotsText.color = positive;
        hackModeBtn.onClick.AddListener(() => Camera.main.GetComponent<HackMode>().EnterHackMode());
        resetBtn.onClick.AddListener(() => ResetButton());

        angleText.color = Color.white;
        powerText.color = Color.white;
        angleText.text = "——";
        powerText.text = "——";
        gameObject.SetActive(!GameManager.PauseManager.hideHUD);
    }
    private void Update(){
        if (shootBall.liningUpShot)
        {
            powerText.text = Mathf.CeilToInt(shootBall.shootPower).ToString();
            if (Mathf.CeilToInt(shootBall.shootPower) <= 5f)
            {
                powerText.color = negative;
                powerTextPercent.color = negative;
            }
            else
            {
                powerText.color = Color.white;
                powerTextPercent.color = Color.white;
            }
            if (shootBall.shootAngle >= 140 || shootBall.shootAngle <= -140)
            {
                angleText.color = negative;
                angleTextDegree.color = negative;
            }
            else
            {
                angleText.color = Color.white;
                angleTextDegree.color = Color.white;
            }
            if (shootBall.shootAngle + 90 > 180)
            {
                angleText.text = Mathf.CeilToInt(shootBall.shootAngle - 270).ToString();
            }
            else
            {
                angleText.text = Mathf.CeilToInt(shootBall.shootAngle + 90).ToString();
            }
        }
        else if (ballStill && Time.time - timeOfBallRelease > 0.1f)
        {
            angleText.color = Color.white;
            powerText.color = Color.white;
            angleTextDegree.color = Color.white;
            powerTextPercent.color = Color.white;
            angleText.text = "——";
            powerText.text = "——";
        }
    }
    #region Level UI
    //IEnumerator ResetNotification(){
    //    timeOfBallRelease = Time.time;
    //    while(Time.time - timeOfBallRelease < ballNotifTime)
    //    {
            
    //        yield return null;
    //    }
    //    EnableOrDisableResetUI(true);
    //}
    void ResetButton(){
        if(LevelManager.ballInput && !LevelManager.levelComplete){
            ShootBall.triggerReset?.Invoke();
        }
    }
    void EnableOrDisableResetUI(bool enable){
        resetUI = enable;
        if(enable){
            canvasAnimator.Play("ResetUI");
        }
        else{
            canvasAnimator.Play("NormalUI");
        }
    }
    void HackModeButton(bool enable, Material mat){
        if(!hackMode){
            ColorBlock colors = hackModeBtn.colors;
            colors.normalColor = GameManager.LevelManager.currentTheme.hackModeColor;
            hackModeBtn.colors = colors;
            hackMode = true;
        }
        else{
            ColorBlock colors = hackModeBtn.colors;
            colors.normalColor = new Color (1, 1, 1, 123f/255f);
            hackModeBtn.colors = colors;
            hackMode = false;
        }
    }
    IEnumerator LevelFailed()
    {
        levelFailing = true;
        ShootBall.death?.Invoke(3);
        yield return new WaitForSeconds(1.25f);
        GameManager.LevelManager.RestartGalaxy();
        levelFailing = false;
    }
    #endregion

    void SetGalaxyAndHole() => holeNoText.text = (GameManager.LevelManager.levelIndex + 1).ToString(); //sets the hole number ui
    void CheckPar()
    {
        if(GameManager.LevelManager.procedural) //Procedural level pars
        {
            par = GameManager.LevelManager.levelParams.levelPar;
        }
    }
    void HoleEnd()
    {
        canvasAnimator.Play("Slide Out");
        if (numShots == 1)
        {
            currentScore = "Hole in One!";
            onScore?.Invoke(currentScore.ToUpper(), numShots - par, numShots, scoreGradient.Evaluate(0));
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
        onScore?.Invoke(currentScore.ToUpper(), numShots - par, numShots, scoreGradient.Evaluate(Mathf.Clamp01(((float)numShots / 5)) * 0.5f + 0.5f));

    }
    void IncrementScore()
    {
        ballStill = false;
        EnableOrDisableResetUI(true);
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
    }
    void Pause(){
        canvasAnimator.Play("Slide Out");
    }
    void UnPause(){
        canvasAnimator.Play("Slide In1");

    }
    void OnBallStill()
    {
        if (numShots == 12 && !LevelManager.levelComplete && !levelFailing) { StartCoroutine(LevelFailed()); }

        ballStill = true;
        if(resetUI){
            EnableOrDisableResetUI(false);
        }
    }
    private void OnEnable()
    {
        ShootBall.onStill += OnBallStill;
        LevelGenerator.levelGenerated += CheckPar;
        ShootBall.onShoot += IncrementScore;
        ShootBall.onHole += HoleEnd;
        PauseManager.pauseGame += Pause;
        PauseManager.unpauseGame += UnPause;
        HackMode.onHackMode += HackModeButton;
    }
    private void OnDisable()
    {
        ShootBall.onStill -= OnBallStill;
        LevelGenerator.levelGenerated -= CheckPar;
        ShootBall.onShoot -= IncrementScore;
        ShootBall.onHole -= HoleEnd;
        PauseManager.pauseGame -= Pause;
        PauseManager.unpauseGame -= UnPause;
        HackMode.onHackMode -= HackModeButton;

    }
}
