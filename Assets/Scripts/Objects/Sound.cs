using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public string name;

    [HideInInspector]
    public AudioSource source;
    public AudioClip clip;
    
    [Range(0,1)]
    public float volume;
    [Range(-4f, 2f)]
    public float pitchAdjustment;

    public bool loop;

    public void InitializeAudioSource(AudioSource audioSource)
    {
        source = audioSource;
        source.clip = clip;
        source.volume = volume;
        source.pitch = 1 + pitchAdjustment;

        source.playOnAwake = false;
        source.loop = loop;
    }
}
