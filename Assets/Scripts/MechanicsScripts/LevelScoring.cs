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
    [SerializeField] Button hackModeBtn;
    [SerializeField] Button resetBtn;
    [SerializeField] Color positive, neutral, negative;

    Coroutine corountineStore;
    bool hackMode;

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
        resetBtn.onClick.AddListener(() => ResetButton());
    }
    private void Update(){
        levelCanvas.enabled = !LevelManager.levelComplete;
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
    IEnumerator ResetNotification(){
        yield return new WaitForSeconds(1.5f);
        EnableOrDisableResetUI(true);
    }
    void ResetButton(){
        if(LevelManager.ballInput){
            ShootBall.triggerReset?.Invoke();
        }
    }
    void EnableOrDisableResetUI(bool enable){
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
        canvasAnimator.Play("Slide Out");
        if (numShots == 1)
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
        
        corountineStore = StartCoroutine(ResetNotification());
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
    IEnumerator LevelFailed()
    {
        ShootBall.death?.Invoke(3);
        HoleEnd();
        yield return new WaitForSeconds(1.25f);
        ShootBall.onHole?.Invoke();
    }
    void ResetHole()
    {
        numShots = 0;
        numShotsText.text = numShots.ToString();
        numShotsText.color = positive;
    }

    void Pause(){
        canvasAnimator.Play("Slide Out");
    }
    void UnPause(){
        canvasAnimator.Play("Slide In1");

    }
    void OnBallStill()
    {
        if(corountineStore != null){
            StopCoroutine(corountineStore);    
        }

        if(hackModeBtn.enabled != true){
            EnableOrDisableResetUI(false);
        }
        if(numShots == 12 && !LevelManager.levelComplete) { StartCoroutine(LevelFailed()); }
    }
    private void OnEnable()
    {
        ShootBall.onStill += OnBallStill;
        LevelGenerator.levelGenerated += CheckPar;
        ShootBall.onShoot += IncrementScore;
        ShootBall.onHole += HoleEnd;
        ShootBall.triggerReset += ResetHole;
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
        ShootBall.triggerReset -= ResetHole;
        PauseManager.pauseGame -= Pause;
        PauseManager.unpauseGame -= UnPause;
        HackMode.onHackMode -= HackModeButton;

    }
}
