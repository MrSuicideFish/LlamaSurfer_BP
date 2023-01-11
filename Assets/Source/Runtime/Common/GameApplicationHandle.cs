using System;
using System.Collections;
using Firebase;
using GooglePlayGames;
using UnityEngine;

public class GameApplicationHandle : MonoBehaviour
{
    private static GameApplicationHandle inst;

    private static GameApplicationHandle Instance
    {
        get
        {
            if (inst == null)
            {
                inst = new GameObject("_APP_HANDLE").AddComponent<GameApplicationHandle>();
                DontDestroyOnLoad(inst);
            }

            return inst;
        }
    }
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void OnApplicationStart()
    {
        PlayerPrefs.DeleteAll();
        PlayerData.Load();
        GameUIManager.Instance.HideAllScreens();
    }

    private void OnApplicationQuit()
    {
        if (Instance != null)
        {
            Instance.StopAllCoroutines();
        }
        
        AdsManager.Shutdown();
        GC.Collect(100, GCCollectionMode.Forced);
    }

    public static Coroutine BeginRoutine(IEnumerator routine)
    {
        return Instance.StartCoroutine(routine);
    }

    public static void EndRoutine(IEnumerator routine)
    {
        Instance.StartCoroutine(routine);
    }


    public static bool HasInitialized { get; private set; }

    public static void Initialize()
    {
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
        HasInitialized = true;
    }
}