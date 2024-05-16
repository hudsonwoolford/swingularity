using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public delegate void PauseGame();
    public PauseGame pauseGame;
    public delegate void UnpauseGame();
    public UnpauseGame unpauseGame;

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
            foreach (string s in dontPauseScene)
            {
                if (SceneManager.GetActiveScene().name == s)
                {
                    return;
                }
            }
            pauseGame?.Invoke();
        }
    }
    void Pause()
    {
        Time.timeScale = 0;
        pausecanvas.enabled = true;
    }
    void Unpause()
    {
        Time.timeScale = 1;
        pausecanvas.enabled = false;
    }
    public void ResumeButton()
    {
        unpauseGame.Invoke();
    }
    public void MenuButton()
    {
        unpauseGame.Invoke();
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
