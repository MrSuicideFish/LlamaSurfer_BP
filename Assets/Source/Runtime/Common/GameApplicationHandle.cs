using System;
using System.Collections;
using Unity.VisualScripting;
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
        Debug.Log("Application Handle Loaded");
        PlayerData.Load();
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