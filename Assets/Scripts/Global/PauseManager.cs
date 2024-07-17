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
    Canvas pausecanvas;
    bool settingsMenu;

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
    }

    // Update is called once per frame
    void Update()
    {
        PauseInput();
    }
    void PauseInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(!paused)
            {
                foreach (string s in dontPauseScene)
                {
                    if (SceneManager.GetActiveScene().name == s)
                    {
                        return;
                    }
                }
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
        pauseAnim.Play("ShowMenu");
        settingsMenu = false;
    }
    void Unpause()
    {
        paused = false;
        Time.timeScale = 1;
        pauseAnim.Play("HideMenu");
        settingsMenu = false;
    }
    public void SettingsButton()
    {
        //settingsCanvas.enabled = true;
        SettingsManager.enableSettingsMenu?.Invoke(true);
        pauseAnim.Play("HideMenu");
        settingsMenu = true;
    }
    void SettingsBackButton(bool enable)
    {
        if(enable || !settingsMenu) { return; }
        pauseAnim.Play("ShowMenu");
        settingsMenu = false;
    }
    void EscapeSettings()
    {
        SettingsManager.enableSettingsMenu?.Invoke(false);
        pauseAnim.Play("ShowMenu");
        settingsMenu = false;
    }
    public void RestartGalaxy()
    {
        initSystem?.Invoke(GameManager.LevelManager.currentGalaxy, GameManager.LevelManager.currentProceduralUniverse[GameManager.LevelManager.galaxyIndex], LevelManager.allGalaxyData[GameManager.LevelManager.currentProceduralUniverse[GameManager.LevelManager.galaxyIndex].galaxyDataIndex]);
        GameManager.SceneHandling.LoadScene(GameManager.LevelManager.currentGalaxy[0], true);
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
    private void OnEnable()
    {
        SettingsManager.enableSettingsMenu += SettingsBackButton;
        pauseGame += Pause;
        unpauseGame += Unpause;
    }
    private void OnDisable()
    {
        SettingsManager.enableSettingsMenu -= SettingsBackButton;
        pauseGame -= Pause;
        unpauseGame -= Unpause;
    }

}
