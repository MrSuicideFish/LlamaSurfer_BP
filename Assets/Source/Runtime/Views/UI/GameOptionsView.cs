using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameOptionsView : GameScreenView
{
    public Image AudioToggleImg;
    public Sprite volumeOffSprite;
    public Sprite volumeOnSprite;
    
    public void OnEnable()
    {
        if (AudioToggleImg != null)
        {
            AudioToggleImg.sprite = GameSettings.IsAudioMuted() ? volumeOffSprite : volumeOnSprite;
        }
    }

    public void ToggleMuteButton()
    {
        bool isMuted = GameSettings.IsAudioMuted();
        GameSettings.ToggleAudio(!isMuted);
        if (AudioToggleImg != null)
        {
            AudioToggleImg.sprite = !isMuted ? volumeOffSprite : volumeOnSprite;
        }
    }
    
    
    public void PurchaseNoAds()
    {
        
    }

    public void Share()
    {
        
    }

    public void OpenTermsOfUse()
    {
        Application.OpenURL("http://www.brokenpixel.co/terms");
    }

    public void OpenPrivacy()
    {
        Application.OpenURL("http://www.brokenpixel.co/privacy");
    }

    public void OpenBrokenPixelSite()
    {
        Application.OpenURL("http://www.brokenpixel.co");
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
            GameUIManager.Instance.ToggleInGameHeader(true);
            GameUIManager.Instance.ToggleControlPanel(true);
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