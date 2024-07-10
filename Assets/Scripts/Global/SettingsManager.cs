using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    public static float masterVolume;
    public static float sfxVolume;
    public static float interfaceVolume;
    public static float musicVolume;

    public static int windowMode;
    public static bool customCursor;
    public static bool hints;
    public static bool uiGlow;

    public static int frameLimit;
    public static Vector2Int screenRes;
    public static int planetRes;
    public static int antiAliasing;
    public static bool vsync;

    public delegate void EnableSettingsMenu(bool enable);
    public static EnableSettingsMenu enableSettingsMenu;
    public delegate void ApplySettings();
    public static ApplySettings applySettingsVar;

    // Start is called before the first frame update
    void Start()
    {
        if(!Application.isPlaying) { return; }
        settingsCanvas = GetComponent<Canvas>();
        settingsCanvas.enabled = false;
        settingsAnim = GetComponent<Animator>();
        currentTab = 0;

        SetButtonFunctions();
        SetAllVariables();
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
    void SetAllVariables() //Applies all variables from settings menu into public static variables
    {
        SetAudioVariables();
        SetGameVariables();
        SetGraphicVariables();
        applySettingsVar?.Invoke();
    }
    void SetAudioVariables() //Applies only audio variables
    {
        sfxVolume = sfxVolElement.slider.value * masterVolElement.slider.value;
        interfaceVolume = interfaceVolElement.slider.value * masterVolElement.slider.value;
        musicVolume = musicVolElement.slider.value * masterVolElement.slider.value;
    }
    void SetGameVariables() //Applies only game variables
    {
        windowMode = windowModeElement.currentValue;
        switch (windowMode)
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
        customCursor = customCursorTgl.currentValue;
        hints = hintsTgl.currentValue;
        uiGlow = uiGlowTgl.currentValue;
    }

    void SetGraphicVariables() //Applies only graphic variables
    {
        planetRes = int.Parse(planetResElement.options[planetResElement.currentValue]);

        char[] screenResString = screenResElement.options[screenResElement.currentValue].ToCharArray();
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

        screenRes = new Vector2Int(int.Parse(width), int.Parse(height));
        Screen.SetResolution(screenRes.x, screenRes.y, Screen.fullScreenMode);

        frameLimit = Mathf.RoundToInt(frameLimitElement.slider.value);
        Application.targetFrameRate = frameLimit;

        antiAliasing = antiAliasingElement.currentValue;
        QualitySettings.antiAliasing = antiAliasing;

        vsync = vsyncTgl.currentValue;
        QualitySettings.vSyncCount = vsync ? 1 : 0;
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
