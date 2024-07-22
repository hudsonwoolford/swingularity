using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    enum PauseButtonState
    { 
        Normal,
        Skip,
        Cross
    }

    [SerializeField] Canvas settingsCanvas;
    public delegate void PauseGame();
    public static PauseGame pauseGame;
    public delegate void UnpauseGame();
    public static UnpauseGame unpauseGame;
    
    [SerializeField]
    string[] dontPauseScene;
    [SerializeField] Image pauseImage;
    [SerializeField]
    Sprite[] pauseButtonSprites;
    Canvas pausecanvas;
    bool settingsMenu;
    bool isDontPauseScene;

    Animator pauseAnim;
    PauseButtonState pauseState;

    public static bool paused;

    private void Awake()
    {
        pausecanvas = GetComponent<Canvas>();
        pauseAnim = GetComponent<Animator>();
    }
    // Start is called before the first frame update
    void Start()
    {
        settingsCanvas.enabled = false;
        pauseState = PauseButtonState.Normal;
        pauseAnim.Play("DisablePauseMenu");
    }

    // Update is called once per frame
    void Update()
    {
        if (paused && Time.timeScale != 0) { Time.timeScale = 0; }
        UpdatePauseState();
        PauseInput();
    }
    void Pause()
    {
        paused = true;
        Time.timeScale = 0;
        pauseAnim.Play("ShowPauseMenu");
        settingsMenu = false;
    }
    void Unpause()
    {
        paused = false;
        Time.timeScale = 1;
        pauseAnim.Play("HidePauseMenu");
        settingsMenu = false;
    }
    #region Input & Buttons
    void PauseInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !TIIGER.Instance.dialogueShown && !SceneHandling.loadingScreen && !GameManager.LevelManager.isPointCard)
        {
            if(!paused)
            {
                if(isDontPauseScene) { return; }
                pauseGame?.Invoke();
            }
            else{
                if(!settingsMenu)
                {
                    unpauseGame?.Invoke();
                }
                else if(settingsMenu)
                {
                    EscapeSettings();
                }
            }
        }
    }
    public void PauseButton()
    {
        UpdatePauseState();
        switch (pauseState)
        {
            case PauseButtonState.Normal:
                if (!TIIGER.Instance.dialogueShown && !SceneHandling.loadingScreen && !GameManager.LevelManager.isPointCard)
                {
                    pauseGame?.Invoke();
                }
                break;
            case PauseButtonState.Skip:
                if(TIIGER.Instance != null)
                {
                    TIIGER.Instance.SkipConversation();
                }
                break;
            case PauseButtonState.Cross:
                if (TutorialManager.instance != null)
                {
                    TutorialManager.instance.CompleteTutorial();
                }
                break;
            default:
                break;
        }
        UpdatePauseState();
    }
    public void SettingsButton()
    {
        //settingsCanvas.enabled = true;
        SettingsManager.enableSettingsMenu?.Invoke(true);
        pauseAnim.Play("HidePauseMenu");
        settingsMenu = true;
    }
    void SettingsBackButton(bool enable)
    {
        if(enable || !settingsMenu) { return; }
        pauseAnim.Play("ShowPauseMenu");
        settingsMenu = false;
    }
    void EscapeSettings()
    {
        SettingsManager.enableSettingsMenu?.Invoke(false);
        pauseAnim.Play("ShowPauseMenu");
        settingsMenu = false;
    }
    public void RestartGalaxy()
    {
        GameManager.LevelManager.RestartGalaxy();
        unpauseGame?.Invoke();
    }
    public void ResumeButton()
    {
        unpauseGame?.Invoke();
    }
    public void MenuButton()
    {
        unpauseGame?.Invoke();
    }
    #endregion
    #region Event Functions
    void UpdatePauseState()
    {
        if(TIIGER.Instance.dialogueShown)
        {
            pauseState = PauseButtonState.Skip;
        }
        else
        {
            if(TutorialManager.instance != null)
            {
                pauseState = PauseButtonState.Cross;
            }
            else
            {
                pauseState = PauseButtonState.Normal;
            }
        }
        switch (pauseState) {
            case PauseButtonState.Normal:
                pauseImage.sprite = pauseButtonSprites[0];
                break;
            case PauseButtonState.Skip:
                pauseImage.sprite = pauseButtonSprites[1];
                break;
            case PauseButtonState.Cross:
                pauseImage.sprite = pauseButtonSprites[2];
                break;
            default:
                break;
        }
    }
    void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        foreach (string s in dontPauseScene)
        {
           if(scene.name==s)
           {
                isDontPauseScene = true;
                pausecanvas.enabled = false;
                return;
           }
           isDontPauseScene = false;
        }
        pausecanvas.enabled = true;
    }
    void OnScore(string score, int scoreRelToPar, int totalShots, Color scoreColor)
    {
        pausecanvas.enabled = false;
    }
    private void OnEnable()
    {
        LevelScoring.onScore += OnScore;
        SceneManager.sceneLoaded += OnSceneLoad;
        SettingsManager.enableSettingsMenu += SettingsBackButton;
        pauseGame += Pause;
        unpauseGame += Unpause;
    }
    private void OnDisable()
    {
        LevelScoring.onScore -= OnScore;
        SceneManager.sceneLoaded -= OnSceneLoad;
        SettingsManager.enableSettingsMenu -= SettingsBackButton;
        pauseGame -= Pause;
        unpauseGame -= Unpause;
    }
    #endregion
}
