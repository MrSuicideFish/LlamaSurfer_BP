
using Firebase;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FrontEndController : MonoBehaviour
{
    public Image logo;
    public IEnumerator Start()
    {
        logo.DOColor(new Color(0,0,0,0), 0.0f).OnComplete(() =>
        {
            logo.DOColor(Color.white, 1.0f).OnComplete(() =>
            {
                logo.DOColor(Color.white, 0.5f).OnComplete(() =>
                {
                    PlayGamesPlatform.Instance.Authenticate((signInStatus =>
                    {
                        AdsManager.Initialize();
                        LevelLoader.GoToNextLevel(() =>
                        {
                            // init firebase
                            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
                            {
                                var dependencyStatus = task.Result;
                                if (dependencyStatus == DependencyStatus.Available)
                                {
                                    Analytics.FireAppStart();
                                }
                            });
                        });
   
                    }));
                });
            });
        });

        yield return null;
    }
}
