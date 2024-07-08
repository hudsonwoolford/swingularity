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
    [SerializeField] Toggle vsyncTgl;
    [SerializeField] SettingsSlider frameLimit;




    public static float sfxVolume;
    public static float musicVolume;
    
    // Start is called before the first frame update
    void Start()
    {
        
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
}
