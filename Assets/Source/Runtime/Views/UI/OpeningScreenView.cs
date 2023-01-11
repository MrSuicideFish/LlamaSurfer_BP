using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class OpeningScreenView : GameScreenView
{
    public Button resetDataButton;
    public void Play()
    {
        BPAudioManager.Instance.Play(AudioProperties.Get().ButtonClickClip, false, BPAudioTrack.UI);
        GameUIManager.Instance.HideAllScreens();
        LevelLoader.GoToProgressionLevel();
    }

    public override IEnumerator OnShow()
    {
        resetDataButton.gameObject.SetActive(Debug.isDebugBuild);
        yield break;
    }

    public void ResetPlayerData()
    {
        PlayerData.ResetPlayerData();
    }
}