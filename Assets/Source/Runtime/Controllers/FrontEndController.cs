using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class FrontEndController : MonoBehaviour
{
    public Image logo;
    public VideoPlayer videoPlayer;
    public IEnumerator Start()
    {
        GameUIManager.Instance.HideAllScreens();
        yield return DoPublisherIntro();
        yield return new WaitForSeconds(2.0f);
        BPAudioManager.Instance.Play(AudioProperties.Get().MenuMusicClip, true, BPAudioTrack.Music);
        yield return DoIntroStart();
        yield return new WaitForSeconds(2.5f);
        GameApplicationHandle.Initialize();
        //LevelLoader.GoToNextLevel(() => { });
    }

    private IEnumerator DoPublisherIntro()
    {
        GameUIManager.Instance.HideAllScreens();
        videoPlayer.Stop();
        videoPlayer.gameObject.SetActive(false);
        logo.gameObject.SetActive(true);
        logo.DOColor(new Color(0, 0, 0, 0), 0.0f).OnComplete(() =>
        {
            logo.DOColor(Color.white, 1.0f);
        });
        
        yield return null;
    }

    private IEnumerator DoIntroStart()
    {
        GameUIManager.Instance.HideAllScreens();
        logo.gameObject.SetActive(false);
        videoPlayer.gameObject.SetActive(true);
        videoPlayer.Play();
        yield return null;
    }
}
