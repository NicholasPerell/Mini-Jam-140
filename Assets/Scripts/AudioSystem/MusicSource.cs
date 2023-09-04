using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicSource : MonoBehaviour
{
    AudioSource source;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey(AudioChannel.MUSIC.ToString() + " Volume"))
        {
            source.volume = PlayerPrefs.GetFloat(AudioChannel.MUSIC.ToString() + " Volume");
        }
        else
        {
            PlayerPrefs.SetFloat(AudioChannel.MUSIC.ToString() + " Volume", 0.5f);
            source.volume = 0.5f;
        }
    }

    private void OnEnable()
    {
        AudioVolumeSlider.OnVolumeChange += AudioVolumeSlider_OnVolumeChange;
    }

    private void AudioVolumeSlider_OnVolumeChange(AudioChannel arg0, float arg1)
    {
        if (arg0 == AudioChannel.MUSIC)
        {
            source.volume = arg1;
        }
    }

    private void OnDisable()
    {
        AudioVolumeSlider.OnVolumeChange -= AudioVolumeSlider_OnVolumeChange;

    }
}
