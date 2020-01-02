using System;
using System.Collections;

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public Sound[] sounds;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

       DontDestroyOnLoad(gameObject);

        foreach (Sound sound in sounds)
        {
            sound.InitializeAudioSource(gameObject.AddComponent<AudioSource>());
        }
    }

    public void PlaySound(string name)
    {
        Sound sound = Array.Find(sounds, Sound => Sound.name == name);

        if (sound == null)
        {
            Debug.LogWarning("Sound " + name + " not found");
            return;
        }

        Debug.Log("Playing sound " + name);
        sound.source.Play();
    }
}
