using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    [SerializeField] AudioSource musicSource1;
    [SerializeField] AudioSource musicSource2;
    [SerializeField] AudioSource currentSource;
    [System.Serializable]
    struct MusicData
    {
        public string name;
        public ColorSchemeStaticReference.PlanetType type;
        public AudioClip musicClip;
        public float loopEndOffset;
        public float loopBeginOffset;
        public float volume;
    }
    [Header("Music Settings")]
    [SerializeField] float crossFadeDuration;
    [SerializeField] AnimationCurve crossFadeOut;
    [SerializeField] AnimationCurve crossFadeIn;
    [Header("Music Clips")]
    [SerializeField] string[] menuScenes;
    [SerializeField] MusicData menuMusic;
    [SerializeField] MusicData[] galaxyClips;
    MusicData currentMusic;
    void Awake()
    {
        if (musicSource1 == null || musicSource2 == null) { AudioSource[] sources = GetComponents<AudioSource>(); musicSource1 = sources[0]; musicSource2 = sources[1]; }
        currentSource = musicSource1;
    }
    // Start is called before the first frame update
    void Start()
    {
        UpdateSourceVolume();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }
    void Update()
    {
        ManageCurrentMusic();
    }
    MusicData CurrentMusic()
    {
        //check if menu music
        string scene = SceneManager.GetActiveScene().name;
        for (int i = 0; i < menuScenes.Length; i++)
        {
            if (menuScenes[i] == scene)
            {
                return menuMusic;
            }
        }
        //check for galaxy type music
        for (int i = 0; i<galaxyClips.Length; i++)
        {
            if(galaxyClips[i].type == GameManager.LevelManager.currentTheme.type)
            {
                return galaxyClips[i];
            }
        }
        return galaxyClips[0];
    }
    void ManageCurrentMusic()
    {
        if((currentSource.time >= currentMusic.musicClip.length - currentMusic.loopEndOffset) || (currentSource.time < 0.5f && !currentSource.isPlaying))
        {
            PlayGalaxyMusic(currentMusic, currentSource);
            currentSource.Play();
            currentSource.time = currentMusic.loopBeginOffset;
        }
    }
    void PlayGalaxyMusic(MusicData musicData, AudioSource s)
    {
        currentMusic = musicData;
        s.time = 0;
        s.clip = musicData.musicClip;
        s.volume = musicData.volume;
        s.Play();
    }
    void StopMusic(AudioSource s)
    {
        s.Stop();
    }
    IEnumerator CrossFadeToMusic(MusicData md)
    {
        AudioSource prevS = currentSource;
        AudioSource newS = prevS == musicSource1 ? musicSource2 : musicSource1;
        PlayGalaxyMusic(md, newS);
        float prevV = currentSource.volume;
        float newV = newS.volume;
        newS.volume = 0;
        currentSource = newS;
        float timer = 0;
        while(timer<crossFadeDuration)
        {
            prevS.volume = prevV * crossFadeOut.Evaluate(timer/crossFadeDuration);
            newS.volume = newV * crossFadeIn.Evaluate(timer/crossFadeDuration);

            timer += Time.unscaledDeltaTime;
            yield return null;
        }
        newS.volume = newV;
        StopMusic(prevS);
        currentMusic = CurrentMusic();
    }
    void OnLoadScene(Scene scene, LoadSceneMode mode)
    {
        for(int i = 0; i < menuScenes.Length; i++)
        {
            if(menuScenes[i] == scene.name && currentSource.clip != menuMusic.musicClip)
            {
                StartCoroutine(CrossFadeToMusic(menuMusic));
                return;
            }
            else if (menuScenes[i] == scene.name && currentSource.clip == menuMusic.musicClip)
            {
                return;
            }
        }
        currentMusic = CurrentMusic();
        if (currentMusic.musicClip != currentSource.clip)
        {
            StartCoroutine(CrossFadeToMusic(currentMusic));
        }
    }
    void UpdateSourceVolume()
    {
        musicSource1.volume = SettingsManager.userSettings.musicVolume;
        musicSource2.volume = SettingsManager.userSettings.musicVolume;
    }
    void OnEnable()
    {
        SettingsManager.applySettingsVar += UpdateSourceVolume;
        SceneManager.sceneLoaded += OnLoadScene;
    }
    void OnDisable()
    {
        SettingsManager.applySettingsVar -= UpdateSourceVolume;
        SceneManager.sceneLoaded -= OnLoadScene;
    }
}
