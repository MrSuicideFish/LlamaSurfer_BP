using UnityEngine;

public class GameSettings
{
    private const string AudioEnabledKey = "AudioEnabled";

    public static void ToggleAudio(bool isEnabled)
    {
        PlayerData.SetData(PlayerData.DataKey.IsAudioEnabled, isEnabled);
        BPAudioManager.Instance.ToggleAudioEnabled(isEnabled);
        PlayerPrefs.Save();
    }
    public static bool IsAudioEnabled()
    {
        return PlayerData.GetData<bool>(PlayerData.DataKey.IsAudioEnabled, true);
    }
}