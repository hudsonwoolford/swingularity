using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static GalaxyGenerator;

public class PauseManager : MonoBehaviour
{
    [SerializeField] Canvas settingsCanvas;
    public delegate void PauseGame();
    public static PauseGame pauseGame;
    public delegate void UnpauseGame();
    public static UnpauseGame unpauseGame;
    
    [SerializeField]
    string[] dontPauseScene;
    [SerializeField]
    Sprite[] pauseButtonSprites;
    Canvas pausecanvas;
    bool settingsMenu;
    bool isDontPauseScene;

    Animator pauseAnim;

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
        pauseAnim.Play("DisablePauseMenu");
    }

    // Update is called once per frame
    void Update()
    {
        if (paused && Time.timeScale != 0) { Time.timeScale = 0; }
        PauseInput();
    }
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
    public void PauseButton()
    {
        if (!paused)
        {
            pauseGame?.Invoke();
        }
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
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
        SettingsManager.enableSettingsMenu += SettingsBackButton;
        pauseGame += Pause;
        unpauseGame += Unpause;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
        SettingsManager.enableSettingsMenu -= SettingsBackButton;
        pauseGame -= Pause;
        unpauseGame -= Unpause;
    }

}
