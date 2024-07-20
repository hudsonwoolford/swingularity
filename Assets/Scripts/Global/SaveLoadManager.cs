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
        public UniverseParams universeParams = new UniverseParams();
        public GalaxyParams[] allGalaxies = new GalaxyParams[1];
        public float totalTime = 0f;
        public int totalShots = 0;
        public int supernovae = 0;
    }
    class GlobalSave
    {
        public UserSettings userSettings = new UserSettings();
        public int totalSupernovae = 0;
    }
    [SerializeField] string saveFolder; //Folder containing saves
    [SerializeField] string settingsSaveFolder; //Folder containing settings
    [SerializeField] string[] nonLevelScenes; //Folder containing settings
    public static UniverseSave currentUniverseSave { get; private set; }
    GlobalSave currentGlobalSave;
    public static FileInfo[] saves { get; private set; }
    bool inLevel;

    private void Awake()
    {
        if (!Directory.Exists(Application.dataPath + saveFolder))
        {
            Directory.CreateDirectory(Application.dataPath + saveFolder);
        }
        if (!Directory.Exists(Application.dataPath + settingsSaveFolder) || !File.Exists(Application.dataPath + settingsSaveFolder + "UserSettings.txt"))
        {
            Directory.CreateDirectory(Application.dataPath + settingsSaveFolder);
            GameManager.SettingsManager.SetDefaultVariables();
            CreateGlobalSave();
        }
        RetrieveUniverseSaves();
        LoadSettings();
    }
    void Start()
    {

    }
    void Update()
    {
        if(inLevel && currentUniverseSave != null && !PauseManager.paused)
        {
            currentUniverseSave.totalTime += Time.unscaledDeltaTime;
        }
    }
    void RetrieveUniverseSaves()
    {
        DirectoryInfo saveInfo = new DirectoryInfo(Application.dataPath + saveFolder);
        FileInfo[] saveFiles = saveInfo.GetFiles();
        List<FileInfo> trackRealSave = new List<FileInfo>();
        for (int i = 0; i < saveFiles.Length; i++)
        {
            if (saveFiles[i].Name == "UserSettings.txt") { continue; }
            if (saveFiles[i].Extension == ".meta") { continue; }
            if (!saveFiles[i].Exists) { continue; }
            trackRealSave.Add(saveFiles[i]);
        }
        saves = trackRealSave.ToArray();
    }
    #region Setting Saving
    void SettingsEnabled(bool enable)
    {
        if (enable) { LoadSettings(); }
        else { SaveGlobalSave(); }
    }
    void CreateGlobalSave()
    {
        Debug.Log("create global");
        GlobalSave settingsSave = new GlobalSave
        {
            userSettings = new UserSettings(),
            totalSupernovae = 0
        };

        string jsonSave = JsonUtility.ToJson(settingsSave, true);

        File.WriteAllText(Application.dataPath + settingsSaveFolder + "UserSettings.txt", jsonSave);

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
        currentGlobalSave = settingsSave;
    }
    void SaveGlobalSave()
    {
        Debug.Log("save global");
        if (currentGlobalSave == null) { Debug.LogError("No current global save, creating new one."); CreateGlobalSave(); }
        currentGlobalSave.userSettings = SettingsManager.userSettings.DeepCopy();

        string jsonSave = JsonUtility.ToJson(currentGlobalSave, true);

        File.WriteAllText(Application.dataPath + settingsSaveFolder + "UserSettings.txt", jsonSave);

        #if UNITY_EDITOR
                AssetDatabase.Refresh();
        #endif
    }
    void LoadSettings()
    {
        Debug.Log("load global");
        string path = Application.dataPath + settingsSaveFolder + "UserSettings.txt";
        if (File.Exists(path))
        {
            string jsonSave = File.ReadAllText(path);
            GlobalSave loadedSave = JsonUtility.FromJson<GlobalSave>(jsonSave);

            currentGlobalSave = loadedSave;
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
        if (currentUniverseSave == null || currentUniverseSave.universeParams == new UniverseParams()) { return; }
        currentUniverseSave.universeParams = GameManager.LevelManager.currentUniverse;
        currentUniverseSave.allGalaxies = GameManager.LevelManager.currentProceduralUniverse;
        currentUniverseSave.supernovae = currentUniverseSave.universeParams.numStars * currentUniverseSave.universeParams.relativeDifficulty;

        string jsonSave = JsonUtility.ToJson(currentUniverseSave, true);

        File.WriteAllText(Application.dataPath + saveFolder + currentUniverseSave.universeParams.name + ".txt", jsonSave);
        File.SetLastAccessTime(Application.dataPath + saveFolder + currentUniverseSave.universeParams.name + ".txt", System.DateTime.Now);
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
        currentUniverseSave = save;

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

            currentUniverseSave = loadedSave;

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
    public void IncrementSupernovae(int increment)
    {
        currentGlobalSave.totalSupernovae += increment;
    }
    void OnSceneLoad(Scene scene, LoadSceneMode loadMode)
    {
        bool matchNonLevel = false;
        foreach (string s in nonLevelScenes)
        {
            if(s == SceneManager.GetActiveScene().name)
            {
                matchNonLevel = true;
            }
        }
        inLevel = !matchNonLevel;
        Save();
    }
    void IncrementNumShots()
    {
        if (currentUniverseSave != null) { currentUniverseSave.totalShots++; }
    }
    private void OnEnable()
    {
        ShootBall.onShoot += IncrementNumShots;
        SceneManager.sceneLoaded += OnSceneLoad;
        SettingsManager.enableSettingsMenu += SettingsEnabled;
        SettingsManager.applySettingsVar += SaveGlobalSave;
    }
    private void OnDisable()
    {
        ShootBall.onShoot -= IncrementNumShots;
        SceneManager.sceneLoaded -= OnSceneLoad;
        SettingsManager.enableSettingsMenu -= SettingsEnabled;
        SettingsManager.applySettingsVar -= SaveGlobalSave;

    }
}
