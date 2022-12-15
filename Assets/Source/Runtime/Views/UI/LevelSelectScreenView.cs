
using System;
using System.Collections;

public class LevelSelectScreenView : GameScreenView
{
    public override IEnumerator OnShow()
    {
        yield break;
    }

    public override IEnumerator OnHide()
    {
        yield break;
    }

    public void GoToLevel(int level)
    {
        
    }

    public void Back()
    {
        if (GameSystem.GetGameManager().gameHasStarted 
            && GameSystem.GetGameManager().gameHasEnded)
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