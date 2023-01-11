using UnityEngine;
using UnityEngine.Audio;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

public class AudioProperties : ScriptableObject
{
    private const string AudioPropertiesPath = "Data/AudioProperties";
    public static AudioProperties Get()
    {
        AudioProperties res = Resources.Load<AudioProperties>(AudioPropertiesPath);
#if UNITY_EDITOR
        if (res == null)
        {
            string resPath = Path.Combine(Application.dataPath, "Resources", "Data");
            if (!Directory.Exists(resPath))
            {
                Directory.CreateDirectory(resPath);
            }

            res = ScriptableObject.CreateInstance<AudioProperties>();
            AssetDatabase.CreateAsset(res, Path.Combine("Assets", "Resources", "Data", "AudioProperties.asset" ));
        }
#endif
        return res;
    }

    [Header("Mixer / Settings")] 
    public AudioMixerGroup MasterMixerGroup;
    public AudioMixerGroup MusicMixerGroup;
    public AudioMixerGroup EffectsMixerGroup;
    public AudioMixerGroup UIMixerGroup;

    [Header("Music")]
    public AudioClip MenuMusicClip;
    public AudioClip GameMusicClip;
    public AudioClip LevelCompleteClip;
    public AudioClip LevelFailClip;

    [Header("UI")] 
    public AudioClip ButtonClickClip;
    public AudioClip StarPopupClip;
    public AudioClip CoinCountClip;
    public AudioClip CoinCountCompleteClip;
    public AudioClip PlayPopupClip;

    [Header("Effects")]
    public AudioClip[] CarrotCollectClips;
    public AudioClip BoxCollectClip;
    public AudioClip CheckpointGrantedClip;
}