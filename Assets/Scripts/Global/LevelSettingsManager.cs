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
    [SerializeField] LevelGenStaticReferences levelStatic;
    [SerializeField] string levelGenScene = "LevelGen";
    public delegate void LevelSettingChange(LevelManager.LevelParams levelParams);
    public static LevelSettingChange onLevelSettingChange;

    Canvas levenGenSettingsCanvas;
    // Start is called before the first frame update
    void Start()
    {
        randomSeedToggle.isOn = false;
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
    public void UIMatchParams()
    {
        LevelManager.LevelParams lp = GameManager.LevelManager.levelParams;
        difficultySlider.value = lp.difficulty;
        difficultyText.text = difficultySlider.value.ToString();
        seedInput.text = lp.seed.ToString();
        typeDropdown.value = (int)lp.themes[0];
        onLevelSettingChange?.Invoke(lp);
    }
    LevelManager.LevelParams SynthesizeParams() //Synthesizes parameters from UI into levlel params
    {
        return new LevelManager.LevelParams(ConvertTextToSeed(seedInput.text), (int)difficultySlider.value, GameManager.LevelManager.levelParams.levelPar, new ColorSchemeStaticReference.PlanetType[] { (ColorSchemeStaticReference.PlanetType)(typeDropdown.value)}, specialTypes, false);
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
public static int ConvertTextToSeed(string text)
{
    if(text == null) { return 0; }

    int convertedSeed = 0;
    int signMult = 1;
    int multiplier = 1;

    for (int i = text.Length - 1; i >= 0; i--)
    {
        char currentChar = text[i];

        if (currentChar == '-')
        {
            signMult *= -1;
        }
        else if (char.IsDigit(currentChar))
        {
            convertedSeed += (currentChar - '0') * multiplier;
        }
        else
        {
            // Using some constant to avoid using GetHashCode, which can vary across different runtime sessions
            int addSeed = (int)currentChar;
            convertedSeed += addSeed * multiplier;
        }

        multiplier *= 10;
    }

    return convertedSeed * signMult;
}

}
