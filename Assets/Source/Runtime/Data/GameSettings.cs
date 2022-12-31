using UnityEngine;

public class GameSettings
{
    private const string AudioMutedKey = "AudioMuted";

    public static void ToggleAudio(bool isOn)
    {
        PlayerPrefs.SetInt(AudioMutedKey, isOn ? 1 : 0);
    }
    public static bool IsAudioMuted()
    {
        return PlayerPrefs.GetInt(AudioMutedKey, 1) == 0;
    }
}