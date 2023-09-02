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
    SortedSet<AudioClip> sfxPlaying;

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
            sfxPlaying = new SortedSet<AudioClip>();
            for(int i = 0; i < resources.Length; i++)
            {
                audioResources.Add(resources[i].name, resources[i]);
            }
            transform.parent = null;
            DontDestroyOnLoad(this.gameObject);
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
            StartCoroutine(ExpressSoundLifetime(clip));
        }
    }

    IEnumerator ExpressSoundLifetime(AudioClip clip)
    {
        float length = clip.length;
        sfxPlaying.Add(clip);
        AudioSource sfxChild = Instantiate(sfxChildPrefab, transform).GetComponent<AudioSource>();
        sfxChild.clip = clip;
        sfxChild.Play();
        yield return new WaitForSeconds(length);
        Destroy(sfxChild.gameObject);
        sfxPlaying.Remove(clip);
    }
}
