using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    private void Awake()
    {
        pausecanvas = GetComponent<Canvas>();
    }
    // Start is called before the first frame update
    void Start()
    {
        settingsCanvas.enabled = false;
        pausecanvas.enabled = false;
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
            if(pausecanvas.enabled == false){
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
                unpauseGame?.Invoke();
            }
        }
    }
    void Pause()
    {
        Time.timeScale = 0;
        pausecanvas.enabled = true;
        settingsCanvas.enabled = false;
    }
    void Unpause()
    {
        Time.timeScale = 1;
        pausecanvas.enabled = false;
        settingsCanvas.enabled = false;
    }
    public void SettingsButton()
    {
        //settingsCanvas.enabled = true;
        SettingsManager.enableSettingsMenu?.Invoke(true);
        pausecanvas.enabled = false;
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
        pauseGame += Pause;
        unpauseGame += Unpause;
    }
    private void OnDisable()
    {
        pauseGame -= Pause;
        unpauseGame -= Unpause;
    }

}
