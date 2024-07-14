using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif
using static LevelManager;

public class SaveLoadManager : MonoBehaviour
{
    public class UniverseSave
    {
        public UniverseParams universeParams;
        public GalaxyParams[] allGalaxies;
    }
    class SettingsSave
    {
        public UserSettings userSettings = new UserSettings();
    }
    [SerializeField] string saveFolder; //Folder containing saves
    [SerializeField] string settingsSaveFolder; //Folder containing settings

    public static FileInfo[] saves {get;private set;}

    private void Awake()
    {
        if(!Directory.Exists(Application.dataPath + saveFolder))
        {
            Directory.CreateDirectory(Application.dataPath + saveFolder);
        }
        if (!Directory.Exists(Application.dataPath + settingsSaveFolder))
        {
            Directory.CreateDirectory(Application.dataPath + settingsSaveFolder);
            GameManager.SettingsManager.SetDefaultVariables();
            SaveSettings();
        }
        RetrieveUniverseSaves();
        LoadSettings();
    }
    void Start()
    {
        
    }
    void Update()
    {

    }
    void RetrieveUniverseSaves()
    {
        DirectoryInfo saveInfo = new DirectoryInfo(Application.dataPath + saveFolder);
        FileInfo[] saveFiles = saveInfo.GetFiles();
        List<FileInfo> trackRealSave = new List<FileInfo>();
        for (int i = 0; i < saveFiles.Length; i++)
        {
            if (saveFiles[i].Name == "UserSettings.txt") { continue; }
            if (saveFiles[i].Extension == ".meta" ){ continue; }
            if (!saveFiles[i].Exists) { continue; }
            trackRealSave.Add(saveFiles[i]);
        }
        saves = trackRealSave.ToArray();
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

        File.WriteAllText(Application.dataPath + settingsSaveFolder + "UserSettings.txt", jsonSave);

        #if UNITY_EDITOR
        AssetDatabase.Refresh();
        #endif
    }
    void LoadSettings()
    {
        string path = Application.dataPath + settingsSaveFolder + "UserSettings.txt";
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
    public void Save()
    {
        UniverseSave save = new UniverseSave
        {
            universeParams = GameManager.LevelManager.currentUniverse,
            allGalaxies = GameManager.LevelManager.currentProceduralUniverse
        };
        if(save.universeParams == new UniverseParams()) { return; }
        string jsonSave = JsonUtility.ToJson(save, true);

        File.WriteAllText(Application.dataPath + saveFolder + save.universeParams.name + ".txt", jsonSave);
        File.SetLastAccessTime(Application.dataPath + saveFolder + save.universeParams.name + ".txt", System.DateTime.Now);
        #if UNITY_EDITOR
                AssetDatabase.Refresh();
        #endif
    }
    public void CreateSave(string fileName, UniverseSave save)
    {
        string jsonSave = JsonUtility.ToJson(save, true);

        File.WriteAllText(Application.dataPath + saveFolder + fileName, jsonSave);
        #if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif

        setUniverse?.Invoke(save.universeParams);
        setGalaxies?.Invoke(save.allGalaxies);
        RetrieveUniverseSaves();
    }
    public void Load(string fileName)
    {
        string path = Application.dataPath + saveFolder + fileName;
        if (File.Exists(path))
        {
            string jsonSave = File.ReadAllText(path);
            UniverseSave loadedSave = JsonUtility.FromJson<UniverseSave>(jsonSave);

            setUniverse?.Invoke(loadedSave.universeParams);
            setGalaxies?.Invoke(loadedSave.allGalaxies);
        }
        else
        {
            Debug.LogWarning("Save file '" + path + "' does not exist. Please provide a valid save name.");
        }
    }
    #endregion
    public UniverseSave RetrieveSave(string fileName)
    {
        string path = Application.dataPath + saveFolder + fileName;
        if (File.Exists(path))
        {
            string jsonSave = File.ReadAllText(path);
            return JsonUtility.FromJson<UniverseSave>(jsonSave);
        }
        else
        {
            return null;
        }
    }
    public void DeleteSave(string fileName)
    {
        File.Delete(Application.dataPath + saveFolder + fileName);
        File.Delete(Application.dataPath + saveFolder + fileName + ".meta");
        #if UNITY_EDITOR
                AssetDatabase.Refresh();
        #endif
        RetrieveUniverseSaves();
    }
    void OnSceneLoad(Scene scene, LoadSceneMode loadMode)
    {
        Save();
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
        SettingsManager.enableSettingsMenu += SettingsEnabled;
        SettingsManager.applySettingsVar += SaveSettings;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
        SettingsManager.enableSettingsMenu -= SettingsEnabled;
        SettingsManager.applySettingsVar -= SaveSettings;

    }
}
