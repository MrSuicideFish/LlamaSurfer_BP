using System;
using System.Collections;
using UnityEngine.UI;

public class LevelFailView : GameScreenView
{
    public Button restartWithHeartsButton;
    
    public override IEnumerator OnShow()
    {
        int heartCount = PlayerData.GetData<int>(PlayerData.DataKey.HeartCount, 3);
        restartWithHeartsButton.interactable = heartCount > 0;
        yield break;
    }

    public override IEnumerator OnHide()
    {
        yield break;
    }

    public void RestartWithHeart()
    {
        int heartCount = PlayerData.GetData<int>(PlayerData.DataKey.HeartCount, 3);
        PlayerData.SetData(PlayerData.DataKey.HeartCount, heartCount - 1);
        PlayerData.Save();
        LevelLoader.RestartLevel();
    }

    public void RestartCompletely()
    {
        GameSystem.GetGameManager().ClearCheckpoint();
        AdRequestInfo adRequest = new AdRequestInfo()
        {
            OnAdComplete = () =>
            {
                GameUIManager.Instance.HideAllScreens();
                LevelLoader.RestartLevel();
            },
            OnAdShowFailed = () =>
            {
                GameUIManager.Instance.HideAllScreens();
                LevelLoader.RestartLevel();
            }
        };
            
        AdsManager.ShowInterstitial(adRequest);
    }
}