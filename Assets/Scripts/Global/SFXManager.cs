using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    [SerializeField] private AudioSource sfxSrc;
    [SerializeField] private AudioSource interfaceSrc;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void PlayInterface(AudioClip clip)
    {

        interfaceSrc.PlayOneShot(clip);
    }
    public void PlaySFX(AudioClip clip, float volModifier)
    {

        sfxSrc.PlayOneShot(clip, volModifier);
    }
    private void ManageSRCSettings()
    {
        sfxSrc.volume = SettingsManager.userSettings.sfxVolume;
        interfaceSrc.volume = SettingsManager.userSettings.interfaceVolume;
    }
    private void OnEnable()
    {
        SettingsManager.applySettingsVar += ManageSRCSettings;
    }
    private void OnDisable()
    {
        SettingsManager.applySettingsVar -= ManageSRCSettings;
    }
}
