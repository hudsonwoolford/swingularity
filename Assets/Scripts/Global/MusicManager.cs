using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        currentMusic = CurrentMusic();
        if(currentMusic.musicClip != musicSource.clip)
        {
            PlayGalaxyMusic(currentMusic);
        }
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
        print(musicSource.time + " | " + (currentMusic.musicClip.length - currentMusic.loopEndOffset) + " | Playing: " + musicSource.isPlaying);
        if(musicSource.time >= currentMusic.musicClip.length - currentMusic.loopEndOffset) 
        {
            musicSource.Play();
            musicSource.time = currentMusic.loopBeginOffset;
        }
        if(musicSource.time<0.5f && musicSource.isPlaying == false)
        {
            musicSource.Play();
            musicSource.time = currentMusic.loopBeginOffset;
        }
    }
    void PlayGalaxyMusic(MusicData musicData)
    {
        StopMusic();
        musicSource.clip = musicData.musicClip;
        musicSource.volume = musicData.volume;
        musicSource.Play();
    }
    void StopMusic()
    {
        musicSource.Stop();
    }
}
