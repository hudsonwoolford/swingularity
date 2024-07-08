using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    [System.Serializable]
    struct SettingsSlider
    {
        public TextMeshProUGUI label;
        public Slider slider;
        public Button resetBtn;
        public float defaultValue;
    }
    [System.Serializable]
    struct SettingsChoice
    {
    }
    [Header("Audio Elements")]
    [SerializeField] SettingsSlider sfxVolElement;
    [SerializeField] SettingsSlider interfaceVolElement;
    [SerializeField] SettingsSlider muiscVolElement;
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
        
    }
}
