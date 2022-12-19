using System;
using System.Collections;
using Firebase;
using UnityEngine;
using Firebase.Analytics;

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
        
        // init firebase
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                FirebaseApp app = Firebase.FirebaseApp.DefaultInstance;
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format("Firebase failed to initialize! {0}", dependencyStatus));
            }
            
            Analytics.FireAppStart();
        });
    }

    private void OnApplicationQuit()
    {
        if (Instance != null)
        {
            Instance.StopAllCoroutines();
        }
        
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
}