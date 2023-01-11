 using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameOptionsView : GameScreenView
{
    public Image AudioToggleImg;
    public Sprite volumeOffSprite;
    public Sprite volumeOnSprite;

    public void ToggleMuteButton()
    {
        bool audioEnabled = GameSettings.IsAudioEnabled();
        GameSettings.ToggleAudio(!audioEnabled);
        if (AudioToggleImg != null)
        {
            AudioToggleImg.sprite = audioEnabled ? volumeOffSprite : volumeOnSprite;
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
        if (AudioToggleImg != null)
        {
            AudioToggleImg.sprite = GameSettings.IsAudioEnabled() ? volumeOnSprite : volumeOffSprite;
        }
        
        yield break;
    }

    public override IEnumerator OnHide()
    {
        yield break;
    }

    public void Done()
    {
        PlayerData.Save();
        BPAudioManager.Instance.Play(AudioProperties.Get().ButtonClickClip, false, BPAudioTrack.UI);
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