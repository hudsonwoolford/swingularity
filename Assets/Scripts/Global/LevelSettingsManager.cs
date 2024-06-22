using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Linq;
using UnityEngine.SceneManagement;

public class LevelSettingsManager : MonoBehaviour
{
    [SerializeField] Slider difficultySlider;
    [SerializeField] TextMeshProUGUI difficultyText;
    [SerializeField] TMP_Dropdown typeDropdown;
    [SerializeField] TMP_InputField seedInput;
    [SerializeField] Toggle randomSeedToggle;
    [SerializeField] LevelGenerator.SpecialType[] specialTypes;
    [SerializeField] string levelGenScene = "LevelGen";
    public delegate void LevelSettingChange(LevelManager.LevelParams levelParams);
    public static LevelSettingChange onLevelSettingChange;

    Canvas levenGenSettingsCanvas;
    // Start is called before the first frame update
    void Start()
    {
        randomSeedToggle.isOn = true;
        levenGenSettingsCanvas = GetComponent<Canvas>();
        levenGenSettingsCanvas.enabled = false;
        ValueUpdate();
        PopulateDropdown();
    }
    void PopulateDropdown() //Populates the dropdown UI with planet types
    {
        string[] enumNames = Enum.GetNames(typeof(ColorSchemeStaticReference.PlanetType));

        typeDropdown.ClearOptions();

        typeDropdown.AddOptions(enumNames.ToList());
        typeDropdown.value = 0;
        typeDropdown.captionText.text = enumNames[0];
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public void RegenerateLevel() //Loads the level gen scene
    {
        SceneManager.LoadScene(levelGenScene);
        PauseManager.unpauseGame?.Invoke();
        levenGenSettingsCanvas.enabled = false;
    }
    public void RandomizeSeedInput() //Randomizes the seed input field
    {
        seedInput.text = Mathf.RoundToInt(UnityEngine.Random.value * System.DateTime.Now.Millisecond + System.DateTime.Now.Millisecond * System.DateTime.Now.Second * UnityEngine.Random.Range(-1000f, 1000f) + UnityEngine.Random.value * Time.time).ToString();
    }
    public void ValueUpdate() //Called by all buttons when a value is updated, triggers event with updated level params
    {
        onLevelSettingChange?.Invoke(SynthesizeParams());
        difficultyText.text = difficultySlider.value.ToString();
    }
    LevelManager.LevelParams SynthesizeParams() //Synthesizes parameters from UI into levlel params
    {
        return new LevelManager.LevelParams(seedInput.text.GetHashCode(), (int)difficultySlider.value, 2, new ColorSchemeStaticReference.PlanetType[] { (ColorSchemeStaticReference.PlanetType)(typeDropdown.value)}, specialTypes);
    }
    void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        if(scene.name == levelGenScene && randomSeedToggle.isOn)
        {
            RandomizeSeedInput();
        }
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
    }
}
