using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sound Data")]
    [SerializeField] private List<SoundData> soundDataList = new List<SoundData>();

    private Dictionary<SoundType, SoundData> soundDictionary;
    private List<AudioSource> audioSourcePool = new List<AudioSource>();
    private AudioSource musicAudioSource;
    private float masterVolume = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSoundDictionary();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        PlayBackgroundMusic();
    }

    private void InitializeSoundDictionary()
    {
        soundDictionary = new Dictionary<SoundType, SoundData>();
        foreach (var soundData in soundDataList)
        {
            if (!soundDictionary.ContainsKey(soundData.soundType))
            {
                soundDictionary.Add(soundData.soundType, soundData);
            }
        }
    }

    public void PlaySound(SoundType soundType)
    {
        if (soundDictionary.TryGetValue(soundType, out SoundData soundData))
        {
            if (soundType == SoundType.Music_Background)
            {
                PlayMusic(soundData);
            }
            else
            {
                PlaySFX(soundData);
            }
        }
    }

    private void PlaySFX(SoundData soundData)
    {
        if (soundData.audioClip != null)
        {
            AudioSource audioSource = GetAvailableAudioSource();
            audioSource.clip = soundData.audioClip;
            audioSource.volume = soundData.volume * masterVolume;
            audioSource.loop = soundData.loop;
            audioSource.Play();
        }
    }

    private void PlayMusic(SoundData soundData)
    {
        if (musicAudioSource == null)
        {
            musicAudioSource = gameObject.AddComponent<AudioSource>();
        }

        if (soundData.audioClip != null)
        {
            musicAudioSource.clip = soundData.audioClip;
            musicAudioSource.volume = soundData.volume * masterVolume;
            musicAudioSource.loop = soundData.loop;
            musicAudioSource.Play();
        }
    }

    private AudioSource GetAvailableAudioSource()
    {
        foreach (var audioSource in audioSourcePool)
        {
            if (!audioSource.isPlaying)
            {
                return audioSource;
            }
        }

        AudioSource newAudioSource = gameObject.AddComponent<AudioSource>();
        audioSourcePool.Add(newAudioSource);
        return newAudioSource;
    }

    private void PlayBackgroundMusic()
    {
        PlaySound(SoundType.Music_Background);
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = volume;
        
        foreach (var audioSource in audioSourcePool)
        {
            if (audioSource.isPlaying && soundDictionary.ContainsValue(GetSoundDataFromClip(audioSource.clip)))
            {
                SoundData soundData = GetSoundDataFromClip(audioSource.clip);
                audioSource.volume = soundData.volume * masterVolume;
            }
        }

        if (musicAudioSource != null && soundDictionary.TryGetValue(SoundType.Music_Background, out SoundData musicData))
        {
            musicAudioSource.volume = musicData.volume * masterVolume;
        }
    }

    private SoundData GetSoundDataFromClip(AudioClip clip)
    {
        foreach (var soundData in soundDictionary.Values)
        {
            if (soundData.audioClip == clip)
                return soundData;
        }
        return null;
    }

    public float GetMasterVolume()
    {
        return masterVolume;
    }
}