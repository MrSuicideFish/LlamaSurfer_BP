using UnityEngine;

public class OpeningScreenView : GameScreenView
{
    public void Play()
    {
        BPAudioManager.Instance.Play(AudioProperties.Get().ButtonClickClip, false, BPAudioTrack.UI);
        LevelLoader.GoToProgressionLevel();
    }
}