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


    public static float sfxVolume;
    public static float interfaceVolume;
    public static float musicVolume;

    public static bool customCursor;
    public static bool hints;
    public static bool uiGlow;

    public static int frameLimit;
    public static bool vsync;
    
    // Start is called before the first frame update
    void Start()
    {
        settingsCanvas = GetComponent<Canvas>();
        settingsCanvas.enabled = false;
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

        }
    }

    void SetAllVariables()
    {
        SetAudioVariables();
        SetGameVariables();
        SetGraphicVariables();
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
}
