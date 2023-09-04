using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public enum AudioChannel
{
    SFX,
    MUSIC
}

public class AudioVolumeSlider : MonoBehaviour
{

    [SerializeField]
    AudioChannel channel;

    Slider slider;

    static public event UnityAction<AudioChannel, float> OnVolumeChange;

    void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void Start()
    {
        if(PlayerPrefs.HasKey(channel.ToString() + " Volume"))
        {
            slider.value = PlayerPrefs.GetFloat(channel.ToString() + " Volume");
        }
        OnVolumeChange?.Invoke(channel, slider.value);
        PlayerPrefs.SetFloat(channel.ToString() + " Volume", slider.value);
    }

    private void OnEnable()
    {
        slider.onValueChanged.AddListener(RespondToValueChanged);
    }

    private void OnDisable()
    {
        slider.onValueChanged.RemoveListener(RespondToValueChanged);
    }

    private void RespondToValueChanged(float value)
    {
        Debug.Log(channel.ToString() + " Volume: " + value);
        OnVolumeChange?.Invoke(channel, value);
        PlayerPrefs.SetFloat(channel.ToString() + " Volume", value);
    }
}
