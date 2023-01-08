
using Firebase;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AdaptivePerformance;
using UnityEngine.UI;
using UnityEngine.Video;

public class FrontEndController : MonoBehaviour
{
    public Image logo;
    public VideoPlayer videoPlayer;
    public IEnumerator Start()
    {
        yield return DoPublisherIntro();
        yield return new WaitForSeconds(2.0f);
        yield return DoIntroStart();
        yield return new WaitForSeconds(2.5f);
        
        PlayGamesPlatform.Instance.Authenticate((signInStatus =>
        {
            AdsManager.Initialize();
        }));
        
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                Analytics.FireAppStart();
            }
        });
        
        AdsManager.LoadInterstitial();
        AdsManager.LoadRewarded();
        AdsManager.ShowBanner();
        GameUIManager.Instance.GoToScreen(GameUIManager.GameScreenID.Opening);
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
