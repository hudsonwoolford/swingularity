using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

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
    [SerializeField] Texture2D customCursorTxtr;
    [SerializeField] Button applySettings;
    [SerializeField] Button backBtn;
    [Header("Main Settings")]
    [SerializeField] Button audioTabBtn;
    [SerializeField] Button gameTabBtn;
    [SerializeField] Button graphicsTabBtn;
    [SerializeField] Button[] tabBtns;
    [SerializeField] CanvasGroup[] settingsTabs;
    [SerializeField] Image[] settingsTabHovers;
    [Header("Audio Elements")]
    [SerializeField] SettingsSlider masterVolElement;
    [SerializeField] SettingsSlider sfxVolElement;
    [SerializeField] SettingsSlider musicVolElement;
    [Header("Game Elements")]
    [SerializeField] SettingsToggle customCursorTgl;
    [SerializeField] SettingsToggle hintsTgl;
    [SerializeField] SettingsToggle dialogueTgl;
    [Header("Graphics Elements")]
    [SerializeField] SettingsSlider frameLimitElement;
    [SerializeField] OptionScroll screenResElement;
    [SerializeField] OptionScroll planetResElement;
    [SerializeField] OptionScroll antiAliasingElement;
    [SerializeField] SettingsToggle vsyncTgl;
    GameObject previousSelectedTab;
    Canvas settingsCanvas;
    Animator settingsAnim;
    [SerializeField] EventSystem settingsEventsystem;
    bool menuOpen = false;
    int currentTab;
    float transitionDuration = 0.05f;
    bool appliedSettings;

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
        settingsAnim = GetComponent<Animator>();
        settingsAnim.Play("DisableMenu");
        currentTab = 0;
        previousSelectedTab = audioTabBtn.gameObject;

        SetButtonFunctions();
    }

    // Update is called once per frame
    void Update()
    {
        if(!Application.isPlaying)
        {
            SetDefaultVariables();
        }
        else
        {
            if(menuOpen && Input.GetKeyDown(KeyCode.Escape) && !PauseManager.paused) //pause manager handles this itself
            {
                enableSettingsMenu?.Invoke(false);
            }
        }
        if(menuOpen){
            applySettings.interactable = !appliedSettings;
            int selectedOther = 0;
            for(int i = 0; i < tabBtns.Length; i++){
                if(settingsEventsystem.currentSelectedGameObject != tabBtns[i].gameObject){
                    selectedOther++;
                }
            }

            if(selectedOther == 3){
                previousSelectedTab.GetComponent<Button>().Select();
            }

        }
    }
    #region Setting Variables
    public void SetDefaultVariables()
    {
        SetSlider(masterVolElement.defaultValue, ref masterVolElement);
        SetSlider(sfxVolElement.defaultValue, ref sfxVolElement);
        SetSlider(musicVolElement.defaultValue, ref musicVolElement);

        SetToggleButton(customCursorTgl.defaultValue, ref customCursorTgl);
        SetToggleButton(hintsTgl.defaultValue, ref hintsTgl);
        SetToggleButton(dialogueTgl.defaultValue, ref dialogueTgl);

        SetScrollOption(planetResElement.defaultValueIndex, ref planetResElement);
        SetScrollOption(screenResElement.defaultValueIndex, ref screenResElement);
        SetScrollOption(antiAliasingElement.defaultValueIndex, ref antiAliasingElement);
        SetSliderValue(frameLimitElement.defaultValue, ref frameLimitElement);
        SetSlider(frameLimitElement.defaultValue, ref frameLimitElement);
        SetToggleButton(vsyncTgl.defaultValue, ref vsyncTgl);

    }
    public void LoadUIElements(UserSettings userSettings)
    {
        SetSlider(userSettings.masterVolume, ref masterVolElement);
        float div = userSettings.masterVolume == 0 ? 1 : userSettings.masterVolume;
        SetSlider(userSettings.sfxVolume / div, ref sfxVolElement);
        SetSlider(userSettings.musicVolume / div, ref musicVolElement);

        SetToggleButton(userSettings.customCursor, ref customCursorTgl);
        SetToggleButton(userSettings.hints, ref hintsTgl);
        SetToggleButton(userSettings.dialogue, ref dialogueTgl);

        int planetResIndex = planetResElement.defaultValueIndex;
        for (int i = 0; i < planetResElement.options.Length; i++)
        {
            if (userSettings.planetRes == int.Parse(planetResElement.options[i]))
            {
                planetResIndex = i;
                break;
            }
        }
        SetScrollOption(planetResIndex, ref planetResElement);
        int screenResIndex = screenResElement.defaultValueIndex;
        for (int i = 0; i < screenResElement.options.Length; i++)
        {
            if(userSettings.screenRes == ConvertScreenRes(screenResElement.options[i]))
            {
                screenResIndex = i;
                break;
            }
        }
        SetScrollOption(screenResIndex, ref screenResElement);
        SetScrollOption(userSettings.antiAliasing, ref antiAliasingElement);
        SetSliderValue(userSettings.frameLimit, ref frameLimitElement);
        SetSlider(userSettings.frameLimit, ref frameLimitElement);
        SetToggleButton(userSettings.vsync, ref vsyncTgl);

        SetAllVariables();
    }
    void SetAllVariables() //Applies all variables from settings menu into public static variables
    {
        SetAudioVariables();
        SetGameVariables();
        SetGraphicVariables();
        applySettingsVar?.Invoke();
        appliedSettings = true;
    }
    void SetAudioVariables() //Applies only audio variables
    {
        userSettings.masterVolume = masterVolElement.slider.value;
        userSettings.sfxVolume = sfxVolElement.slider.value * masterVolElement.slider.value;
        userSettings.musicVolume = musicVolElement.slider.value * masterVolElement.slider.value;
    }
    void SetGameVariables() //Applies only game variables
    {
        userSettings.customCursor = customCursorTgl.currentValue;
        if (userSettings.customCursor)
        {
            Cursor.SetCursor(customCursorTxtr, Vector2.zero, CursorMode.Auto);
        }
        else
        {
            Debug.Log("null");
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }


        userSettings.hints = hintsTgl.currentValue;
        userSettings.dialogue = dialogueTgl.currentValue;
    }

    void SetGraphicVariables() //Applies only graphic variables
    {
        userSettings.planetRes = int.Parse(planetResElement.options[planetResElement.currentValue]);

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
            if (screenResString[i] == 'X' || screenResString[i] == ' ')
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
    void SetSlider(float value, ref SettingsSlider ss)
    {
        ss.slider.value = value;
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
    void SetToggleButton(bool value, ref SettingsToggle tg)
    {
        tg.currentValue = value;
        tg.toggleImg.enabled = tg.currentValue;
    }
    void UnappliedSettings()
    {
        appliedSettings = false;
    }
    IEnumerator ChangeTab(int tabIndex) //Changes the tab with a transition animation
    {
        int prevIndex = currentTab;
        currentTab = tabIndex;

        float timeOfTransition = 0;
        // settingsTabHovers[currentTab].color = new Color (1, 1, 1, 0);
        // settingsTabHovers[prevIndex].color = new Color (1, 1, 1, 10f/255f);
            tabBtns[currentTab].Select();
            previousSelectedTab = tabBtns[currentTab].gameObject;

            // for(int i = 0; i < tabBtns.Length; i++){
                
            //     if(i == currentTab){tabBtns[currentTab].interactable = false;}
            //     else{tabBtns[currentTab].interactable = true;}
            // }
        if(prevIndex == currentTab) { yield break; }
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

    }
    void SetButtonFunctions() //Sets all listener functions for buttons
    {
        applySettings.onClick.AddListener(() => SetAllVariables());
        backBtn.onClick.AddListener(() => enableSettingsMenu?.Invoke(false));

        //Slider buttons
        SettingsSlider[] sSliders = new SettingsSlider[] { masterVolElement, sfxVolElement, musicVolElement, frameLimitElement };
        for (int i = 0; i < sSliders.Length; i++)
        {
            for (int x = 0; x < sSliders[i].resetBtns.Length; x++)
            {
                int currentIndex = i; //needs to be initialized in function
                sSliders[i].resetBtns[x].onClick.AddListener(() => ResetSlider(ref sSliders[currentIndex]));
                sSliders[i].resetBtns[x].onClick.AddListener(() => UnappliedSettings());
                sSliders[i].slider.onValueChanged.AddListener(v => UnappliedSettings());
                if (sSliders[i].valueText != null)
                {
                    sSliders[i].slider.onValueChanged.AddListener(v => SetSliderValue(v, ref sSliders[currentIndex]));
                }
            }
        }

        //Option scroll buttons
        OptionScroll[] oScrolls = new OptionScroll[] { planetResElement, screenResElement, antiAliasingElement };
        for (int i = 0;i < oScrolls.Length; i++)
        {
            int currentIndex = i; //needs to be initialized in function
            oScrolls[i].rightBtn.onClick.AddListener(() => ScrollOption(true, ref oScrolls[currentIndex]));
            oScrolls[i].rightBtn.onClick.AddListener(() => UnappliedSettings());
            oScrolls[i].leftBtn.onClick.AddListener(() => ScrollOption(false, ref oScrolls[currentIndex]));
            oScrolls[i].leftBtn.onClick.AddListener(() => UnappliedSettings());

            for (int x = 0; x < oScrolls[i].resetBtns.Length; x++)
            {
                oScrolls[i].resetBtns[x].onClick.AddListener(() => ResetOptionScroll(ref oScrolls[currentIndex]));
                oScrolls[i].resetBtns[x].onClick.AddListener(() => UnappliedSettings());
            }

            oScrolls[i].currentValue = oScrolls[i].defaultValueIndex;
            oScrolls[i].optionTxt.text = oScrolls[i].options[oScrolls[i].currentValue];
        }

        //Toggle buttons
        SettingsToggle[] sToggles = new SettingsToggle[] { customCursorTgl, hintsTgl, dialogueTgl, vsyncTgl };
        for (int i = 0; i < sToggles.Length; i++)
        {
            int currentIndex = i; //needs to be initialized in function
            sToggles[i].toggleBtn.onClick.AddListener(() => ToggleButton(ref sToggles[currentIndex]));
            sToggles[i].toggleBtn.onClick.AddListener(() => UnappliedSettings());
            for (int x = 0; x < sToggles[i].resetBtns.Length; x++)
            {
                sToggles[i].resetBtns[x].onClick.AddListener(() => ResetToggle(ref sToggles[currentIndex]));
                sToggles[i].resetBtns[x].onClick.AddListener(() => UnappliedSettings());
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
    public float masterVolume = 0.5f;
    public float sfxVolume = 0.2f;
    public float musicVolume = 0.3f;

    public bool customCursor = true;
    public bool hints = true;
    public bool dialogue = true;

    public int frameLimit = 240;
    public Vector2Int screenRes = new Vector2Int(1920, 1080);
    public int planetRes = 1024;
    public int antiAliasing = 1;
    public bool vsync = false;
    public UserSettings DeepCopy()
    {
        return new UserSettings
        {
            masterVolume = this.masterVolume,
            sfxVolume = this.sfxVolume,
            musicVolume = this.musicVolume,

            customCursor = this.customCursor,
            hints = this.hints,
            dialogue = this.dialogue,

            frameLimit = this.frameLimit,
            screenRes = this.screenRes,
            planetRes = this.planetRes,
            antiAliasing = this.antiAliasing,
            vsync = this.vsync
        };
    }
}


