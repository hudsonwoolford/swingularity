using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static LevelManager LevelManager { get; private set; }
    public static PauseManager PauseManager { get; private set; }
    public static SettingsManager SettingsManager { get; private set; }
    public static LevelSettingsManager LevelSettingsManager { get; private set; }
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);

        LevelManager = GetComponentInChildren<LevelManager>(true);
        PauseManager = GetComponentInChildren<PauseManager>(true);
        SettingsManager = GetComponentInChildren<SettingsManager>(true);
        LevelSettingsManager = GetComponentInChildren<LevelSettingsManager>(true);
    }
    private void Start()
    {
        GetComponentInChildren<EventSystem>().enabled = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
