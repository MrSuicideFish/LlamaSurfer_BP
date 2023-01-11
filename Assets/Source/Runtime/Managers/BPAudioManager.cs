using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum BPAudioTrack
{
    Master,
    Music,
    SFX,
    UI
}

public class BPAudioManager : MonoBehaviour
{
    private static BPAudioManager _inst;
    public static BPAudioManager Instance
    {
        get
        {
            if (_inst == null)
            {
                _inst = new GameObject("AUDIO_MANAGER").AddComponent<BPAudioManager>();
                DontDestroyOnLoad(_inst);
            }
            return _inst;
        }
    }

    private List<AudioSource> _sources = new List<AudioSource>();
    private AudioSource GetOrCreateAudioSource()
    {
        AudioSource source = null;
        foreach (AudioSource s in _sources)
        {
            if (!s.isPlaying)
            {
                source = s;
                break;
            }
        }

        if (source == null)
        {
            source = new GameObject("AudioManagerSource").AddComponent<AudioSource>();
            source.transform.SetParent(_inst.transform);
            source.mute = GameSettings.IsAudioEnabled() == false;
            source.volume = 0.5f;
            _sources.Add(source);
        }

        return source;
    }

    private AudioMixerGroup GetMixerGroup(BPAudioTrack track)
    {
        switch (track)
        {
            case BPAudioTrack.Master:
                return AudioProperties.Get().MasterMixerGroup;
            case BPAudioTrack.Music:
                return AudioProperties.Get().MusicMixerGroup;
            case BPAudioTrack.SFX:
                return AudioProperties.Get().EffectsMixerGroup;
            case BPAudioTrack.UI:
                return AudioProperties.Get().UIMixerGroup;
        }

        return null;
    }
    
    public void Play(AudioClip clip, bool loop, BPAudioTrack track, float pitch = 1.0f, float volume = 0.5f)
    {
        if (clip == null) return;
        AudioSource source = GetOrCreateAudioSource();
        if (source != null)
        {
            source.clip = clip;
            source.loop = loop;
            source.pitch = pitch;
            source.volume = volume;
            source.outputAudioMixerGroup = GetMixerGroup(track);
            source.Play();
        }
    }

    public void ToggleAudioEnabled(bool audioEnabled)
    {
        foreach (AudioSource s in _sources)
        {
            s.mute = audioEnabled == false;
        }
    }
}
