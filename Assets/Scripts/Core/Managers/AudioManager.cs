using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    [Serializable]
    public struct Clips
    {
        public string Name;
        public AudioClip Clip;
        [Range(0f, 1f)] public float Volume;
        public bool AutoPlay;
        public bool Loop;
    }

    [SerializeField] private List<Clips> _audioClips;

    private Dictionary<string, AudioSource> _audioSources = new Dictionary<string, AudioSource>();

    public static AudioManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        foreach (var audioClip in _audioClips)
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.clip = audioClip.Clip;
            source.volume = audioClip.Volume;

            if (audioClip.Loop)
            {
                source.loop = true;
            }
            if (audioClip.AutoPlay)
            {
                source.Play();
            }

            _audioSources.Add(audioClip.Name, source);
        }
    }
        
    public void Play(string name)
    {
        var source = _audioSources[name];
        source.Play();
    }
}
