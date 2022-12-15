using System;
using System.Collections;
using UnityEngine.Audio;
using UnityEngine.UI;

public class GameOptionsView : GameScreenView
{
    public Toggle BGM_ToggleON;
    public Toggle SFX_ToggleON;

    public void Start()
    {
        BGM_ToggleON.onValueChanged.AddListener((value =>
        {
            
        }));
        
        SFX_ToggleON.onValueChanged.AddListener((value =>
        {
            
        }));
    }

    public override IEnumerator OnShow()
    {
        yield break;
    }

    public override IEnumerator OnHide()
    {
        yield break;
    }

    public void Done()
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
        }else if (!GameSystem.GetGameManager().gameHasStarted)
        {
            GameUIManager.Instance.GoToScreen(GameUIManager.GameScreenID.PreGame);
        }
    }
}