using System;
using System.Collections;

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
}