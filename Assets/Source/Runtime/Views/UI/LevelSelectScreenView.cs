
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
        if (GameManager.Instance.gameHasStarted 
            && GameManager.Instance.gameHasEnded)
        {
            if (GameManager.Instance.playerHasFailed)
            {
                GameUIManager.Instance.GoToScreen(GameUIManager.GameScreenID.GameFail);
            }
            else
            {
                GameUIManager.Instance.GoToScreen(GameUIManager.GameScreenID.GameSuccess);
            }
        }
    }
}