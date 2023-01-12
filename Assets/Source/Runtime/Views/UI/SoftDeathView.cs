using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SoftDeathView : GameScreenView
{
    public override IEnumerator OnShow()
    {
        yield return base.OnShow();
    }

    public override IEnumerator OnHide()
    {
        yield return base.OnHide();
    }

    public void RestartAtCheckpoint()
    {
        int heartCount = PlayerData.GetData<int>(PlayerData.DataKey.HeartCount, 3);
        PlayerData.SetData(PlayerData.DataKey.HeartCount, heartCount - 1);
        PlayerData.Save();
        LevelLoader.RestartLevel();
    }

    public void RestartAtCheckpointWithHay()
    {
        int heartCount = PlayerData.GetData<int>(PlayerData.DataKey.HeartCount, 3);
        PlayerData.SetData(PlayerData.DataKey.HeartCount, heartCount - 1);

        AdRequestInfo adRequest = new AdRequestInfo();
        adRequest.OnRewardGranted += () =>
        {
            int bonusCount = PlayerData.GetData<int>(PlayerData.DataKey.BonusBlockCount, 0);
            PlayerData.SetData(PlayerData.DataKey.BonusBlockCount, bonusCount+1);
            PlayerData.Save();
            LevelLoader.RestartLevel();
        };
        
        PlayerData.Save();
        AdsManager.ShowRewarded(adRequest);
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
