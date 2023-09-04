using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioSystem : MonoBehaviour
{
    [SerializeField]
    AudioClip[] resources;

    [SerializeField]
    GameObject sfxChildPrefab;

    static AudioSystem instance = null;
    static public AudioSystem Instance 
    { 
        get 
        {
            return instance;
        } 
    }
    SortedDictionary<string, AudioClip> audioResources;
    List<AudioClip> sfxPlaying;
    List<AudioSource> sourcesPlaying;

    float volume = .5f;

    private void Awake()
    {
        if(instance && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
            audioResources = new SortedDictionary<string, AudioClip>();
            sfxPlaying = new List<AudioClip>();
            sourcesPlaying = new List<AudioSource>();
            for(int i = 0; i < resources.Length; i++)
            {
                audioResources.Add(resources[i].name, resources[i]);
            }
            transform.parent = null;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey(AudioChannel.SFX.ToString() + " Volume"))
        {
            volume = PlayerPrefs.GetFloat(AudioChannel.SFX.ToString() + " Volume");
        }
        else
        {
            PlayerPrefs.SetFloat(AudioChannel.SFX.ToString() + " Volume", 0.5f);
        }
    }

    private void OnDestroy()
    {
        if(instance == this)
        {
            instance = null;
        }
    }

    public void RequestSoundIn(string name, float time)
    {
        Debug.Assert(time > 0, "RequestSoundIn: time parameter must be a positive number.");
        StartCoroutine(Coroutine(name, time));
        IEnumerator Coroutine(string name, float time)
        {
            yield return new WaitForSeconds(time);
            RequestSound(name);
        }
    }

    public void RequestSound(string name)
    {
        AudioClip clip;
        if(audioResources.TryGetValue(name,out clip) && !sfxPlaying.Contains(clip))
        {
            Debug.Log("Audio System: " + name + " RequestSound.");

            StartCoroutine(ExpressSoundLifetime(clip));
        }
        else if(!audioResources.TryGetValue(name, out clip))
        {
            Debug.LogError("Audio System: " + name + " has no matching string.");
        }
        else
        {
            Debug.LogError("Audio System: " + name + " is already playing.");
        }
    }

    IEnumerator ExpressSoundLifetime(AudioClip clip)
    {
        float length = clip.length;
        sfxPlaying.Add(clip);
        AudioSource sfxChild = Instantiate(sfxChildPrefab, transform).GetComponent<AudioSource>();
        sfxChild.clip = clip;
        sfxChild.Play();
        sourcesPlaying.Add(sfxChild);
        yield return new WaitForSeconds(length);
        sourcesPlaying.Remove(sfxChild);
        Destroy(sfxChild.gameObject);
        sfxPlaying.Remove(clip);
    }

    public void Pause()
    {
        foreach(AudioSource audio in GetComponentsInChildren<AudioSource>())
        {
            audio.Pause();
        }
    }

    public void Resume()
    {
        foreach (AudioSource audio in GetComponentsInChildren<AudioSource>())
        {
            audio.UnPause();
        }
    }

    private void OnEnable()
    {
        AudioVolumeSlider.OnVolumeChange += AudioVolumeSlider_OnVolumeChange;
    }

    private void AudioVolumeSlider_OnVolumeChange(AudioChannel arg0, float arg1)
    {
        if (arg0 == AudioChannel.SFX)
        {
            volume = arg1;
            foreach (AudioSource source in sourcesPlaying)
            {
                if (source != null)
                {
                    source.volume = arg1;
                }
            }
            RequestSound("CoinThrow01");
        }
    }

    private void OnDisable()
    {
        AudioVolumeSlider.OnVolumeChange -= AudioVolumeSlider_OnVolumeChange;
    }
}
