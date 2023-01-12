
using System;
using System.Collections;
using UnityEngine.UI;

public class LevelSelectScreenView : GameScreenView
{
    public Button[] levelButtons;
    
    public override IEnumerator OnShow()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int level = i + 1;
            Button btn = levelButtons[i];
            if (btn != null)
            {
                btn.onClick.AddListener((() =>
                {
                    GoToLevel(level);
                }));
                
                int lastLevelCompleted = PlayerData.GetData<int>(PlayerData.DataKey.LastLevelCompleted, 0);
                btn.interactable = level <= lastLevelCompleted;
            }
        }

        return base.OnShow();
    }

    public override IEnumerator OnHide()
    {
        yield return base.OnHide();
    }

    public void GoToLevel(int level)
    {
        LevelCfg currentLevel = LevelCfgDb.GetCurrentLevel();
        if (currentLevel != null)
        {
            AdRequestInfo adRequest = new AdRequestInfo()
            {
                OnAdComplete = () =>
                {
                    GameUIManager.Instance.HideAllScreens();
                    LevelLoader.GoToLevel(level);
                },
                OnAdShowFailed = () =>
                {
                    GameUIManager.Instance.HideAllScreens();
                    LevelLoader.GoToLevel(level);
                }
            };
            
            AdsManager.ShowInterstitial(adRequest);
        }
    }

    public void Back()
    {
        if (!GameSystem.GetGameManager().gameHasStarted)
        {
            GameUIManager.Instance.GoToScreen(GameUIManager.GameScreenID.PreGame);
            GameUIManager.Instance.ToggleControlPanel(false);
        }
        else if(GameSystem.GetGameManager().gameHasEnded)
        {
            if (GameSystem.GetGameManager().playerHasFailed)
            {
                if (GameSystem.GetGameManager().ShouldShowHardDeath())
                {
                    GameUIManager.Instance.GoToScreen(GameUIManager.GameScreenID.GameHardFail);
                }
                else
                {
                    GameUIManager.Instance.GoToScreen(GameUIManager.GameScreenID.GameSoftFail);
                }
            }
            else
            {
                GameUIManager.Instance.GoToScreen(GameUIManager.GameScreenID.GameSuccess);
            }
        }
    }
}