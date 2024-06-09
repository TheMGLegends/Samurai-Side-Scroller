using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Sound Effects:")]
    [SerializeField] private List<string> clipNames = new();
    [SerializeField] private List<AudioClip> audioClips = new();

    [Header("Audio Source Prefab:")]
    [SerializeField] private GameObject audioSourcePrefab;

    private Dictionary<string, AudioClip> audioLib = new();

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
            audioLib.Add(clipNames[i], audioClips[i]);
        }
    }

    public void PlaySFX(string clipName)
    {
        if (audioLib.ContainsKey(clipName))
        {
            AudioSource audioSource = Instantiate(audioSourcePrefab).GetComponent<AudioSource>();
            audioSource.PlayOneShot(audioLib[clipName]);
            Destroy(audioSource.gameObject, audioLib[clipName].length);
        }
        else
        {
            Debug.LogWarning("AudioManager::PlaySFX - Audio clip called " + clipName + " could not be found!");
        }
    }
}
