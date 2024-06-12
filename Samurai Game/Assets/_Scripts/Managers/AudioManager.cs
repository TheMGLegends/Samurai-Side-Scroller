using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Pool Settings:")]
    [SerializeField] private int sfxCount;
    [SerializeField] private int musicCount;

    [Space(10)]

    [Header("Sound Effects:")]
    [SerializeField] private List<string> clipNames = new();
    [SerializeField] private List<AudioClip> audioClips = new();

    [Space(10)]

    [Header("Audio Source Prefab:")]
    [SerializeField] private GameObject audioSourcePrefab;

    private readonly Dictionary<string, AudioClip> audioLib = new();
    private readonly List<AudioSource> sfxAudioSources = new();
    private readonly List<AudioSource> musicAudioSources = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        for (int i = 0; i < clipNames.Count; i++)
        {
            // INFO: Add audio clips with their corresponding names to the audio library
            audioLib.Add(clipNames[i], audioClips[i]);
        }

        // INFO: Parent Locations for SFX and Music Audio Sources
        Transform sfxSources = transform.GetChild(0);
        Transform musicSources = transform.GetChild(1);

        // INFO: Pool Audio Sources for SFX
        for (int i = 0; i < sfxCount; i++)
        {
            AudioSource audioSource = Instantiate(audioSourcePrefab).GetComponent<AudioSource>();
            audioSource.transform.SetParent(sfxSources);
            audioSource.transform.name = "SFX Audio Source " + i;
            sfxAudioSources.Add(audioSource);
        }

        // INFO: Pool Audio Sources for Music
        for (int i = 0; i < musicCount; i++)
        {
            AudioSource audioSource = Instantiate(audioSourcePrefab).GetComponent<AudioSource>();
            audioSource.transform.SetParent(musicSources);
            audioSource.transform.name = "Music Audio Source " + i;
            musicAudioSources.Add(audioSource);
        }
    }

    public void PlaySFX(string clipName, float volume = 1.0f)
    {
        if (audioLib.ContainsKey(clipName))
        {
            foreach (AudioSource audioSource in sfxAudioSources)
            {
                if (!audioSource.isPlaying)
                {
                    audioSource.PlayOneShot(audioLib[clipName], volume);
                    return;
                }
            }
        }
        else
        {
            Debug.LogWarning("AudioManager::PlaySFX - Audio clip called " + clipName + " could not be found!");
        }
    }

    public void PlayMusic(string clipName, float volume = 1.0f)
    {
        if (audioLib.ContainsKey(clipName))
        {
            foreach (AudioSource audioSource in musicAudioSources)
            {
                if (!audioSource.isPlaying)
                {
                    audioSource.PlayOneShot(audioLib[clipName], volume);
                    return;
                }
            }
        }
        else
        {
            Debug.LogWarning("AudioManager::PlayMusic - Audio clip called " + clipName + " could not be found!");
        }
    }
}
