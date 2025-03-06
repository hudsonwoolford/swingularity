using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
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
        public string lastSaveTime = "20.10.24 14:35";
    }
    public class GlobalSave
    {
        public UserSettings userSettings = new UserSettings();
        public bool unlockedInfinite;

        //customization
        public int totalSupernovae = 0;
        public string astronautMaterialElement;
        public string golfBallTrailElement;
        public string astronautHatElement;
        public string[] unlockedShopItems;
    }
    [SerializeField] string saveFolder; //Folder containing saves
    [SerializeField] string settingsSaveFolder; //Folder containing settings
    [SerializeField] string[] nonLevelScenes; //Folder containing settings

    public static UniverseSave currentUniverseSave { get; private set; }
    public static GlobalSave currentGlobalSave { get; private set; }
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
        LoadGlobal();
        if(saves.Length > 0)
        {
            Load(saves[0].Name);
        }
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
        if(Input.GetKey(KeyCode.U) && Input.GetKeyDown(KeyCode.I)) { if (SceneManager.GetActiveScene().name == "Main Menu") { currentGlobalSave.unlockedInfinite = !currentGlobalSave.unlockedInfinite; } }
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
        Array.Sort(saves, (s1, s2) => s2.LastWriteTime.CompareTo(s1.LastWriteTime));
    }
    #region Global Saving
    void SettingsEnabled(bool enable)
    {
        if (enable) { LoadGlobal(); }
        else { SaveGlobalSave(); }
    }
    void CreateGlobalSave()
    {
        //default customization settings
        string mce = GameManager.CustomizationManager._DefaultShopItem.name;
        string gbte = mce;
        string ahe = mce;

        //creating actual save
        GlobalSave globalSave = new GlobalSave
        {
            userSettings = new UserSettings(),
            unlockedInfinite = false,

            totalSupernovae = 0,
            astronautMaterialElement = mce,
            golfBallTrailElement = gbte,
            astronautHatElement = ahe
        };

        string jsonSave = JsonUtility.ToJson(globalSave, true);

        File.WriteAllText(Application.dataPath + settingsSaveFolder + "UserSettings.txt", jsonSave);

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
        Debug.Log("Create new save");

        currentGlobalSave = globalSave;
    }
    void SaveGlobalSave()
    {
        if (currentGlobalSave == null) { Debug.LogError("No current global save, creating new one."); CreateGlobalSave(); }
        currentGlobalSave.userSettings = SettingsManager.userSettings.DeepCopy();

        string jsonSave = JsonUtility.ToJson(currentGlobalSave, true);

        File.WriteAllText(Application.dataPath + settingsSaveFolder + "UserSettings.txt", jsonSave);

        #if UNITY_EDITOR
                AssetDatabase.Refresh();
        #endif
    }
    void LoadGlobal()
    {
        string path = Application.dataPath + settingsSaveFolder + "UserSettings.txt";
        if (File.Exists(path))
        {
            string jsonSave = File.ReadAllText(path);
            GlobalSave loadedSave = JsonUtility.FromJson<GlobalSave>(jsonSave);

            currentGlobalSave = loadedSave;
            GameManager.SettingsManager.LoadUIElements(loadedSave.userSettings.DeepCopy());
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
        currentUniverseSave.lastSaveTime = System.DateTime.Now.ToString("dd.MM.yy hh:mm");

        string jsonSave = JsonUtility.ToJson(currentUniverseSave, true);

        File.WriteAllText(Application.dataPath + saveFolder + currentUniverseSave.universeParams.name + ".txt", jsonSave);
        #if UNITY_EDITOR
                AssetDatabase.Refresh();
        #endif
    }
    public void CreateSave(string fileName, UniverseSave save)
    {
        save.lastSaveTime = System.DateTime.Now.ToString("dd.MM.yy hh:mm");
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
    public void UnlockShopItem(string shopItem)
    {
        if (!currentGlobalSave.unlockedShopItems.Contains(shopItem))
        {
            currentGlobalSave.unlockedShopItems = currentGlobalSave.unlockedShopItems.Concat(new string[] { shopItem }).ToArray();
            SaveGlobalSave();
        }
    }
    public void IncrementSupernovae(int increment)
    {
        currentGlobalSave.totalSupernovae += increment;
        SaveGlobalSave();
    }
    #region Delegates
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
    void OnHole()
    {
        if (GameManager.LevelManager.isCampaignFinalLevel) //last level
        {
            currentGlobalSave.unlockedInfinite = true;
            SaveGlobalSave();
        }
    }
    private void OnEnable()
    {
        ShootBall.onHole += OnHole;
        ShootBall.onShoot += IncrementNumShots;
        SceneManager.sceneLoaded += OnSceneLoad;
        SettingsManager.enableSettingsMenu += SettingsEnabled;
        SettingsManager.applySettingsVar += SaveGlobalSave;
    }
    private void OnDisable()
    {
        ShootBall.onHole -= OnHole;
        ShootBall.onShoot -= IncrementNumShots;
        SceneManager.sceneLoaded -= OnSceneLoad;
        SettingsManager.enableSettingsMenu -= SettingsEnabled;
        SettingsManager.applySettingsVar -= SaveGlobalSave;

    }
    #endregion
}
