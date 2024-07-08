using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteInEditMode]
public class SettingsManager : MonoBehaviour
{
    [System.Serializable]
    struct SettingsSlider
    {
        public Slider slider;
        public Button[] resetBtns;
        public float defaultValue;
    }
    [System.Serializable]
    struct SettingsChoice
    {
    }
    [SerializeField] Button applySettings;
    [Header("Main Settings")]
    [SerializeField] Button audioTabBtn;
    [SerializeField] Button gameTabBtn;
    [SerializeField] Button graphicsTabBtn;
    [SerializeField] CanvasGroup[] settingsTabs;
    [Header("Audio Elements")]
    [SerializeField] SettingsSlider sfxVolElement;
    [SerializeField] SettingsSlider interfaceVolElement;
    [SerializeField] SettingsSlider musicVolElement;
    [Header("Game Elements")]
    [SerializeField] Toggle customCursorTgl;
    [SerializeField] Toggle hintsTgl;
    [SerializeField] Toggle uiGlowTgl;
    [Header("Graphics Elements")]
    [SerializeField] SettingsSlider frameLimitElement;
    [SerializeField] Toggle vsyncTgl;

    Canvas settingsCanvas;
    Animator settingsAnim;
    bool menuOpen = false;
    int currentTab;
    float transitionDuration = 0.3f;


    public static float sfxVolume;
    public static float interfaceVolume;
    public static float musicVolume;

    public static bool customCursor;
    public static bool hints;
    public static bool uiGlow;

    public static int frameLimit;
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

        SetAllVariables();
        SetButtonFunctions();
    }

    // Update is called once per frame
    void Update()
    {
        if(!Application.isPlaying)
        {
            if (sfxVolElement.slider != null) { sfxVolElement.slider.value = sfxVolElement.defaultValue; }
            if (interfaceVolElement.slider != null) { interfaceVolElement.slider.value = interfaceVolElement.defaultValue; }
            if (musicVolElement.slider != null) { musicVolElement.slider.value = musicVolElement.defaultValue; }
        }
        else
        {
            if(menuOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                enableSettingsMenu?.Invoke(false);
            }
        }
    }

    void SetAllVariables()
    {
        SetAudioVariables();
        SetGameVariables();
        applySettingsVar?.Invoke();
    }
    void SetAudioVariables()
    {
        sfxVolume = sfxVolElement.slider.value;
        interfaceVolume = interfaceVolElement.slider.value;
        musicVolume = musicVolElement.slider.value;
    }
    void SetGameVariables()
    {
        customCursor = customCursorTgl.isOn;
        hints = hintsTgl.isOn;
        uiGlow = uiGlowTgl.isOn;
    }
    void SetGraphicVariables()
    {
        vsync = vsyncTgl.isOn;
        frameLimit = Mathf.RoundToInt(frameLimitElement.slider.value);
    }
    void EnableMenu(bool enable)
    {
        menuOpen = enable;
        if(enable) { settingsAnim.Play("ShowSettingsMenu"); }
        else { settingsAnim.Play("HideSettingsMenu"); }
    }
    void ResetSlider(SettingsSlider ss)
    {
        ss.slider.value = ss.defaultValue;
    }
    IEnumerator ChangeTab(int tabIndex)
    {
        int prevIndex = currentTab;
        currentTab = tabIndex;
        float timeOfTransition = 0;
        while(timeOfTransition<transitionDuration)
        {
            float alpha = Mathf.Lerp(0, 1, timeOfTransition / transitionDuration);
            settingsTabs[prevIndex].alpha = 1 - alpha;
            settingsTabs[currentTab].alpha = alpha;

            timeOfTransition += Time.deltaTime;
            yield return null;
        }
        settingsTabs[prevIndex].alpha = 0;
        settingsTabs[currentTab].alpha = 1;
        settingsTabs[prevIndex].interactable = false;
        settingsTabs[prevIndex].blocksRaycasts = false;
        settingsTabs[currentTab].interactable = true;
        settingsTabs[currentTab].blocksRaycasts = true;
    }
    void SetButtonFunctions()
    {
        applySettings.onClick.AddListener(() => SetAllVariables());

        SettingsSlider[] sSliders = new SettingsSlider[] { sfxVolElement, interfaceVolElement, musicVolElement, frameLimitElement };
        for (int i = 0; i < sSliders.Length; i++)
        {
            for (int x = 0; x < sSliders[i].resetBtns.Length; x++)
            {
                int currentIndex = i;
                sSliders[i].resetBtns[x].onClick.AddListener(() => ResetSlider(sSliders[currentIndex]));
            }
        }

        audioTabBtn.onClick.AddListener(() => StartCoroutine(ChangeTab(0)));
        gameTabBtn.onClick.AddListener(() => StartCoroutine(ChangeTab(1)));
        graphicsTabBtn.onClick.AddListener(() => StartCoroutine(ChangeTab(2)));
    }
    private void OnEnable()
    {
        enableSettingsMenu += EnableMenu;
    }
    private void OnDisable()
    {
        enableSettingsMenu -= EnableMenu;
    }
}
