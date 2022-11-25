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
        }else if (!GameManager.Instance.gameHasStarted)
        {
            GameUIManager.Instance.GoToScreen(GameUIManager.GameScreenID.PreGame);
        }
    }
}