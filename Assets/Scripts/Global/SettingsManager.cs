using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.Rendering;

[ExecuteInEditMode]
public class SettingsManager : MonoBehaviour
{
    [System.Serializable]
    class SettingsSlider 
    {
        public Slider slider;
        public Button[] resetBtns;
        [Tooltip("Optional variable, displays the value of the slider.")]
        public TextMeshProUGUI valueText;
        public float defaultValue;
    }
    [System.Serializable]
    class OptionScroll
    {
        public Button leftBtn;
        public Button rightBtn;
        public Button[] resetBtns;
        public TextMeshProUGUI optionTxt;
        public int currentValue;
        public int defaultValueIndex;
        public string[] options;
    }
    [System.Serializable]
    class SettingsToggle
    {
        public Button toggleBtn;
        public Image toggleImg;
        public bool currentValue;
        public bool defaultValue;
        public Button[] resetBtns;
    }
    [SerializeField] Button applySettings;
    [SerializeField] Button backBtn;
    [Header("Main Settings")]
    [SerializeField] Button audioTabBtn;
    [SerializeField] Button gameTabBtn;
    [SerializeField] Button graphicsTabBtn;
    [SerializeField] TextMeshProUGUI[] tabBtnSelected;
    [SerializeField] CanvasGroup[] settingsTabs;
    [Header("Audio Elements")]
    [SerializeField] SettingsSlider masterVolElement;
    [SerializeField] SettingsSlider sfxVolElement;
    [SerializeField] SettingsSlider interfaceVolElement;
    [SerializeField] SettingsSlider musicVolElement;
    [Header("Game Elements")]
    [SerializeField] OptionScroll windowModeElement;
    [SerializeField] SettingsToggle customCursorTgl;
    [SerializeField] SettingsToggle hintsTgl;
    [SerializeField] SettingsToggle uiGlowTgl;
    [Header("Graphics Elements")]
    [SerializeField] SettingsSlider frameLimitElement;
    [SerializeField] OptionScroll screenResElement;
    [SerializeField] OptionScroll planetResElement;
    [SerializeField] OptionScroll antiAliasingElement;
    [SerializeField] SettingsToggle vsyncTgl;

    Canvas settingsCanvas;
    Animator settingsAnim;
    bool menuOpen = false;
    int currentTab;
    float transitionDuration = 0.05f;

    public static UserSettings userSettings = new UserSettings();

    public delegate void EnableSettingsMenu(bool enable);
    public static EnableSettingsMenu enableSettingsMenu;
    public delegate void ApplySettings();
    public static ApplySettings applySettingsVar;

    // Start is called before the first frame update
    void Start()
    {
        if(userSettings == null) { userSettings = new UserSettings(); }
        if(!Application.isPlaying) { return; }
        settingsCanvas = GetComponent<Canvas>();
        settingsCanvas.enabled = false;
        settingsAnim = GetComponent<Animator>();
        currentTab = 0;

        SetButtonFunctions();
        for (int i = 0; i < tabBtnSelected.Length; i++)
        {
            if (i == currentTab) { tabBtnSelected[i].enabled = true; }
            else { tabBtnSelected[i].enabled = false; }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!Application.isPlaying)
        {
            //default audio values
            if (sfxVolElement.slider != null) { sfxVolElement.slider.value = sfxVolElement.defaultValue; }
            if (interfaceVolElement.slider != null) { interfaceVolElement.slider.value = interfaceVolElement.defaultValue; }
            if (musicVolElement.slider != null) { musicVolElement.slider.value = musicVolElement.defaultValue; }
            if (masterVolElement.slider != null) { masterVolElement.slider.value = masterVolElement.defaultValue; }

            //default game values
            if (customCursorTgl.toggleImg != null) { customCursorTgl.toggleImg.enabled = customCursorTgl.defaultValue; }
            if (hintsTgl.toggleImg != null) { hintsTgl.toggleImg.enabled = hintsTgl.defaultValue; }
            if (uiGlowTgl.toggleImg != null) { uiGlowTgl.toggleImg.enabled = uiGlowTgl.defaultValue; }

            //default graphic values
            if (frameLimitElement.slider != null) { frameLimitElement.slider.value = frameLimitElement.defaultValue; if (frameLimitElement.valueText != null) { frameLimitElement.valueText.text = frameLimitElement.slider.value.ToString(); } }
            if (planetResElement.optionTxt != null) { planetResElement.optionTxt.text = planetResElement.options[planetResElement.defaultValueIndex]; }
            if (screenResElement.optionTxt != null) { screenResElement.optionTxt.text = screenResElement.options[screenResElement.defaultValueIndex]; }
            if (antiAliasingElement.optionTxt != null) { antiAliasingElement.optionTxt.text = antiAliasingElement.options[antiAliasingElement.defaultValueIndex]; }
            if (vsyncTgl.toggleImg != null) { vsyncTgl.toggleImg.enabled = vsyncTgl.defaultValue; }
        }
        else
        {
            if(menuOpen && Input.GetKeyDown(KeyCode.Escape) && !PauseManager.paused) //pause manager handles this itself
            {
                enableSettingsMenu?.Invoke(false);
            }
        }
    }
    #region Setting Variables
    public void LoadUIElements(UserSettings userSettings)
    {
        SetSliderValue(userSettings.masterVolume, ref masterVolElement);
        SetSliderValue(userSettings.sfxVolume / userSettings.masterVolume, ref sfxVolElement);
        SetSliderValue(userSettings.interfaceVolume / userSettings.masterVolume, ref interfaceVolElement);
        SetSliderValue(userSettings.musicVolume / userSettings.masterVolume, ref musicVolElement);

        SetScrollOption(userSettings.windowMode, ref windowModeElement);
        windowModeElement.currentValue = userSettings.windowMode;
        customCursorTgl.currentValue = userSettings.customCursor;
        hintsTgl.currentValue = userSettings.hints;
        uiGlowTgl.currentValue = userSettings.uiGlow;

        int planetResIndex = planetResElement.defaultValueIndex;
        for (int i = 0; i < planetResElement.options.Length; i++)
        {
            if (userSettings.planetRes == int.Parse(planetResElement.options[i]))
            {
                planetResIndex = i;
                break;
            }
        }
        planetResElement.currentValue = planetResIndex;
        int screenResIndex = screenResElement.defaultValueIndex;
        for (int i = 0; i < screenResElement.options.Length; i++)
        {
            if(userSettings.screenRes == ConvertScreenRes(screenResElement.options[i]))
            {
                screenResIndex = i;
                break;
            }
        }
        screenResElement.currentValue = screenResIndex;
        frameLimitElement.slider.value = userSettings.frameLimit;
        antiAliasingElement.currentValue = userSettings.antiAliasing;
        vsyncTgl.currentValue = userSettings.vsync;
        SetAllVariables();
    }
    void SetAllVariables() //Applies all variables from settings menu into public static variables
    {
        SetAudioVariables();
        SetGameVariables();
        SetGraphicVariables();
        applySettingsVar?.Invoke();
    }
    void SetAudioVariables() //Applies only audio variables
    {
        userSettings.masterVolume = masterVolElement.slider.value;
        userSettings.sfxVolume = sfxVolElement.slider.value * masterVolElement.slider.value;
        userSettings.interfaceVolume = interfaceVolElement.slider.value * masterVolElement.slider.value;
        userSettings.musicVolume = musicVolElement.slider.value * masterVolElement.slider.value;
    }
    void SetGameVariables() //Applies only game variables
    {
        userSettings.windowMode = windowModeElement.currentValue;
        switch (userSettings.windowMode)
        {
            case 0:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                Screen.fullScreen = true;
                break;
            case 1:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                Screen.fullScreen = false;
                break;
            default:
                break;
        }
        userSettings.customCursor = customCursorTgl.currentValue;
        userSettings.hints = hintsTgl.currentValue;
        userSettings.uiGlow = uiGlowTgl.currentValue;
    }

    void SetGraphicVariables() //Applies only graphic variables
    {
        userSettings.planetRes = int.Parse(planetResElement.options[planetResElement.currentValue]);
        print(userSettings.planetRes); 

        userSettings.screenRes = ConvertScreenRes(screenResElement.options[screenResElement.currentValue]);
        Screen.SetResolution(userSettings.screenRes.x, userSettings.screenRes.y, Screen.fullScreenMode);

        userSettings.frameLimit = Mathf.RoundToInt(frameLimitElement.slider.value);
        Application.targetFrameRate = userSettings.frameLimit;

        userSettings.antiAliasing = antiAliasingElement.currentValue;
        QualitySettings.antiAliasing = userSettings.antiAliasing;

        userSettings.vsync = vsyncTgl.currentValue;
        QualitySettings.vSyncCount = userSettings.vsync ? 1 : 0;
    }
    Vector2Int ConvertScreenRes(string option)
    {
        char[] screenResString = option.ToCharArray();
        string width = "";
        string height = "";
        ref string currentEdit = ref width;
        for (int i = 0; i < screenResString.Length; i++)
        {
            if (screenResString[i] == '/' || screenResString[i] == ' ')
            {
                currentEdit = ref height;
                continue;
            }
            currentEdit += screenResString[i];
        }
        return new Vector2Int(int.Parse(width), int.Parse(height));
    }
    #endregion
    #region Element Functions
    void EnableMenu(bool enable) //Enables the settings menu by animation
    {
        menuOpen = enable;
        if(enable) { settingsAnim.Play("ShowSettingsMenu"); }
        else { settingsAnim.Play("HideSettingsMenu"); }
    }
    void ResetSlider(ref SettingsSlider ss) //Resets a slider value to default
    {
        ss.slider.value = ss.defaultValue;
    }
    void ResetOptionScroll(ref OptionScroll os)
    {
        os.optionTxt.text = os.options[os.defaultValueIndex];
        os.currentValue = os.defaultValueIndex;
    }
    void ResetToggle(ref SettingsToggle tg)
    {
        tg.currentValue = tg.defaultValue;
        tg.toggleImg.enabled = tg.currentValue;
    }
    void SetSliderValue(float value, ref SettingsSlider ss)
    {
        ss.valueText.text = value.ToString();
    }
    void ScrollOption(bool right, ref OptionScroll os)
    {
        os.currentValue += right ? 1 : -1;
        os.currentValue = LoopValue(os.currentValue, 0, os.options.Length);
        os.optionTxt.text = os.options[os.currentValue];
    }
    void SetScrollOption(int value, ref OptionScroll os)
    {
        os.currentValue = value;
        os.currentValue = LoopValue(os.currentValue, 0, os.options.Length);
        os.optionTxt.text = os.options[os.currentValue];
    }
    void ToggleButton(ref SettingsToggle tg)
    {
        tg.currentValue = !tg.currentValue;
        tg.toggleImg.enabled = tg.currentValue;
    }
    IEnumerator ChangeTab(int tabIndex) //Changes the tab with a transition animation
    {
        int prevIndex = currentTab;
        currentTab = tabIndex;
        if(prevIndex == currentTab) { yield break; }
        float timeOfTransition = 0;
        while(timeOfTransition<transitionDuration)
        {
            float alpha = Mathf.Lerp(0, 1, timeOfTransition / transitionDuration);
            settingsTabs[prevIndex].alpha = 1 - alpha;
            settingsTabs[currentTab].alpha = alpha;

            timeOfTransition += Time.unscaledDeltaTime;
            yield return null;
        }
        settingsTabs[prevIndex].alpha = 0;
        settingsTabs[currentTab].alpha = 1;
        settingsTabs[prevIndex].interactable = false;
        settingsTabs[prevIndex].blocksRaycasts = false;
        settingsTabs[currentTab].interactable = true;
        settingsTabs[currentTab].blocksRaycasts = true;
        for (int i = 0; i < tabBtnSelected.Length; i++)
        {
            if(i == currentTab) { tabBtnSelected[i].enabled = true; }
            else { tabBtnSelected[i].enabled = false; }
        }
    }
    void SetButtonFunctions() //Sets all listener functions for buttons
    {
        applySettings.onClick.AddListener(() => SetAllVariables());
        backBtn.onClick.AddListener(() => enableSettingsMenu?.Invoke(false));

        //Slider buttons
        SettingsSlider[] sSliders = new SettingsSlider[] { masterVolElement, sfxVolElement, interfaceVolElement, musicVolElement, frameLimitElement };
        for (int i = 0; i < sSliders.Length; i++)
        {
            for (int x = 0; x < sSliders[i].resetBtns.Length; x++)
            {
                int currentIndex = i; //needs to be initialized in function
                sSliders[i].resetBtns[x].onClick.AddListener(() => ResetSlider(ref sSliders[currentIndex]));
                if (sSliders[i].valueText != null)
                {
                    sSliders[i].slider.onValueChanged.AddListener(v => SetSliderValue(v, ref sSliders[currentIndex]));
                }
            }
        }

        //Option scroll buttons
        OptionScroll[] oScrolls = new OptionScroll[] {  windowModeElement, planetResElement, screenResElement, antiAliasingElement };
        for (int i = 0;i < oScrolls.Length; i++)
        {
            int currentIndex = i; //needs to be initialized in function
            oScrolls[i].rightBtn.onClick.AddListener(() => ScrollOption(true, ref oScrolls[currentIndex]));
            oScrolls[i].leftBtn.onClick.AddListener(() => ScrollOption(false, ref oScrolls[currentIndex]));
            for (int x = 0; x < oScrolls[i].resetBtns.Length; x++)
            {
                oScrolls[i].resetBtns[x].onClick.AddListener(() => ResetOptionScroll(ref oScrolls[currentIndex]));
            }

            oScrolls[i].currentValue = oScrolls[i].defaultValueIndex;
            oScrolls[i].optionTxt.text = oScrolls[i].options[oScrolls[i].currentValue];
        }

        //Toggle buttons
        SettingsToggle[] sToggles = new SettingsToggle[] { customCursorTgl, hintsTgl, uiGlowTgl, vsyncTgl };
        for (int i = 0; i < sToggles.Length; i++)
        {
            int currentIndex = i; //needs to be initialized in function
            sToggles[i].toggleBtn.onClick.AddListener(() => ToggleButton(ref sToggles[currentIndex]));
            for (int x = 0; x < sToggles[i].resetBtns.Length; x++)
            {
                sToggles[i].resetBtns[x].onClick.AddListener(() => ResetToggle(ref sToggles[currentIndex]));
            }

            sToggles[i].currentValue = sToggles[i].defaultValue;
            sToggles[i].toggleImg.enabled = sToggles[i].currentValue;
        }

        //tab buttons
        audioTabBtn.onClick.AddListener(() => StartCoroutine(ChangeTab(0)));
        gameTabBtn.onClick.AddListener(() => StartCoroutine(ChangeTab(1)));
        graphicsTabBtn.onClick.AddListener(() => StartCoroutine(ChangeTab(2)));
    }
    #endregion
    private void OnEnable()
    {
        enableSettingsMenu += EnableMenu;
    }
    private void OnDisable()
    {
        enableSettingsMenu -= EnableMenu;
    }
    int LoopValue(int value, int min, int max) //utility to loop values
    {
        int range = max - min;
        if (range <= 0) return min; // Handle edge case if min and max are equal or max is less than min

        int loopedValue = (value - min) % range;
        if (loopedValue < 0)
            loopedValue += range;

        return min + loopedValue;
    }
}
[System.Serializable]
public class UserSettings
{
    public float masterVolume;
    public float sfxVolume;
    public float interfaceVolume;
    public float musicVolume;

    public int windowMode;
    public bool customCursor;
    public bool hints;
    public bool uiGlow;

    public int frameLimit;
    public Vector2Int screenRes;
    public int planetRes;
    public int antiAliasing;
    public bool vsync;
    public UserSettings DeepCopy()
    {
        return new UserSettings
        {
            masterVolume = this.masterVolume,
            sfxVolume = this.sfxVolume,
            interfaceVolume = this.interfaceVolume,
            musicVolume = this.musicVolume,

            windowMode = this.windowMode,
            customCursor = this.customCursor,
            hints = this.hints,
            uiGlow = this.uiGlow,

            frameLimit = this.frameLimit,
            screenRes = this.screenRes,
            planetRes = this.planetRes,
            antiAliasing = this.antiAliasing,
            vsync = this.vsync
        };
    }
}


