using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static LevelManager;

public class SaveLoadManager : MonoBehaviour
{
    class UniverseSave
    { 
        public UniverseParams universeParams = new UniverseParams();
        public GalaxyParams[] allGalaxies = new GalaxyParams[0];
    }
    class SettingsSave
    { 
        public UserSettings userSettings = new UserSettings();
    }
    [SerializeField] string saveFolder; //Folder containing saves


    private void Awake()
    {


    }
    void Start()
    {
        
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab)) { SaveSettings(); }

    }
    #region Setting Saving
    void SettingsEnabled(bool enable)
    {
        if(enable) { LoadSettings(); }
        else { SaveSettings(); }
    }
    void SaveSettings()
    {
        SettingsSave settingsSave = new SettingsSave
        {
            userSettings = SettingsManager.userSettings.DeepCopy()
        };

        string jsonSave = JsonUtility.ToJson(settingsSave, true);

        File.WriteAllText(Application.dataPath + saveFolder + "UserSettings.txt", jsonSave);
    }
    void LoadSettings()
    {
        string path = Application.dataPath + saveFolder + "UserSettings.txt";
        if (File.Exists(path))
        {
            string jsonSave = File.ReadAllText(path);
            SettingsSave loadedSave = JsonUtility.FromJson<SettingsSave>(jsonSave);

            GameManager.SettingsManager.LoadUIElements(loadedSave.userSettings);
        }
        else
        {
            Debug.LogWarning("Settings save file does not exist. No settings will be loaded.");
        }
    }
    #endregion
    #region Universe Saving
    void Save()
    {
        UniverseSave save = new UniverseSave
        {
            universeParams = GameManager.LevelManager.currentUniverse,
            allGalaxies = GameManager.LevelManager.currentProceduralUniverse
        };

        string jsonSave = JsonUtility.ToJson(save, true);

        File.WriteAllText(Application.dataPath + saveFolder + save.universeParams.name + ".txt", jsonSave);
    }
    void Load()
    {
        string name = "Test";

        string jsonSave = File.ReadAllText(Application.dataPath + saveFolder + name + ".txt");
        UniverseSave loadedSave = JsonUtility.FromJson<UniverseSave>(jsonSave);

        setProceduralUniverse?.Invoke(loadedSave.universeParams);
        setProceduralGalaxies?.Invoke(loadedSave.allGalaxies);
    }
    public void Load(string saveName)
    {
        string path = Application.dataPath + saveFolder + name + ".txt";
        if (File.Exists(path))
        {
            string jsonSave = File.ReadAllText(path);
            UniverseSave loadedSave = JsonUtility.FromJson<UniverseSave>(jsonSave);

            setProceduralUniverse?.Invoke(loadedSave.universeParams);
            setProceduralGalaxies?.Invoke(loadedSave.allGalaxies);
        }
        else
        {
            Debug.LogWarning("Save file '" + path + "' does not exist. Please provide a valid save name.");
        }
    }
    #endregion

    private void OnEnable()
    {
        SettingsManager.enableSettingsMenu += SettingsEnabled;
        SettingsManager.applySettingsVar += SaveSettings;
    }
    private void OnDisable()
    {
        SettingsManager.enableSettingsMenu -= SettingsEnabled;
        SettingsManager.applySettingsVar -= SaveSettings;

    }
}
