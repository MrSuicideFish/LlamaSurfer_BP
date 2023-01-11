using System.Collections.Generic;
using DG.Tweening;
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
    private AudioSource _musicSource = null;

    private AudioSource CreateSource(string sourceName = "AudioManagerSource")
    {
        AudioSource source = new GameObject(sourceName).AddComponent<AudioSource>();
        source.transform.SetParent(_inst.transform);
        source.mute = GameSettings.IsAudioEnabled() == false;
        source.volume = 0.5f;
        return source;
    }
    private AudioSource GetOrCreateAudioSource(BPAudioTrack track)
    {
        if (track != BPAudioTrack.Music)
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
                source = CreateSource();
                _sources.Add(source);
            }
            return source;
        }
        else
        {
            if (_musicSource == null)
            {
                return _musicSource = CreateSource("MUSIC");
            }

            return _musicSource;
        }
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
        AudioSource source = GetOrCreateAudioSource(track);
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

    public void StopMusic()
    {
        GetOrCreateAudioSource(BPAudioTrack.Music).Stop();
    }

    public void ToggleAudioEnabled(bool audioEnabled)
    {
        _musicSource.mute = audioEnabled == false;
        foreach (AudioSource s in _sources)
        {
            s.mute = audioEnabled == false;
        }
    }
}
