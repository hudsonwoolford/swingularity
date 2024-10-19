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
    public void PlayInterface(AudioObj audio)
    {
        interfaceSrc.PlayOneShot(audio.clip, audio.inherentVol);
    }
    public void PlaySFX(AudioObj audio, AudioSource src, float dynamicAudioScalar)
    {
        src.volume = SettingsManager.userSettings.sfxVolume * dynamicAudioScalar;
        src.PlayOneShot(audio.clip, audio.inherentVol);
    }
    public void PlaySFX(AudioObj audio)
    {
        sfxSrc.PlayOneShot(audio.clip, audio.inherentVol);
    }
    public void PlaySFX(AudioObj audio, float dynamicAudioScalar)
    {

        sfxSrc.PlayOneShot(audio.clip, audio.inherentVol * dynamicAudioScalar);
    }
    private void ManageSRCSettings()
    {
        sfxSrc.volume = SettingsManager.userSettings.sfxVolume;
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
