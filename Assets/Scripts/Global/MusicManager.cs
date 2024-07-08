using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    [SerializeField] AudioSource musicSource;
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
    [SerializeField] string[] menuScenes;
    [SerializeField] MusicData menuMusic;
    [SerializeField] MusicData[] galaxyClips;
    MusicData currentMusic;
    void Awake()
    {
        if (musicSource == null) {musicSource = GetComponent<AudioSource>();}   
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
        if((musicSource.time >= currentMusic.musicClip.length - currentMusic.loopEndOffset) || (musicSource.time < 0.5f && !musicSource.isPlaying))
        {
            musicSource.Play();
            musicSource.time = currentMusic.loopBeginOffset;
        }
    }
    void PlayGalaxyMusic(MusicData musicData)
    {
        StopMusic();
        currentMusic = musicData;
        musicSource.time = 0;
        musicSource.clip = musicData.musicClip;
        musicSource.volume = musicData.volume;
        musicSource.Play();
    }
    void StopMusic()
    {
        musicSource.Stop();
    }
    void OnLoadScene(Scene scene, LoadSceneMode mode)
    {
        for(int i = 0; i < menuScenes.Length; i++)
        {
            if(menuScenes[i] == scene.name && musicSource.clip != menuMusic.musicClip)
            {
                PlayGalaxyMusic(menuMusic);
                return;
            }
            else if (menuScenes[i] == scene.name && musicSource.clip == menuMusic.musicClip)
            {
                return;
            }
        }
        currentMusic = CurrentMusic();
        if (currentMusic.musicClip != musicSource.clip)
        {
            PlayGalaxyMusic(currentMusic);
        }
    }
    void UpdateSourceVolume()
    {
        musicSource.volume = SettingsManager.musicVolume;
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
