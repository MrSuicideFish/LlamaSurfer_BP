using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelLoader
{
    private const int DEFAULT_LEVEL_BUILD_IDX = 1;
    private const string LoadingScreenSceneName = "LevelLoadScene";

    public static void GoToLevel(int levelBuildIndex, Action onLoadComplete = null)
    {
        try
        {
            LevelCfg nextLevel = LevelCfgDb.GetLevelByBuildIndex(levelBuildIndex);
            if (nextLevel != null)
            {
                GameApplicationHandle.BeginRoutine(DoLevelLoad(nextLevel.sceneIndex, onLoadComplete));
            }
            else
            {
                Debug.Log("Failed to load progression level. Level Cfg is NULL");
            }
        }
        catch (Exception e)
        {
            
        }
    }
    
    public static void GoToProgressionLevel(Action onLoadComplete = null)
    {
        try
        {
            LevelCfg finalLevel = LevelCfgDb.Instance.levelsCfgs[LevelCfgDb.Instance.levelsCfgs.Length - 1];
            if (finalLevel != null)
            {
                int progressLevelIdx = PlayerData.GetData<int>(PlayerData.DataKey.LastLevelCompleted, 1);
                progressLevelIdx = Mathf.Clamp(progressLevelIdx + 1, 1, finalLevel.sceneIndex);

                LevelCfg nextLevel = LevelCfgDb.GetLevelByBuildIndex(progressLevelIdx);
                if (nextLevel != null)
                {
                    GameApplicationHandle.BeginRoutine(DoLevelLoad(nextLevel.sceneIndex, onLoadComplete));
                }
                else
                {
                    Debug.Log("Failed to load progression level. Level Cfg is NULL");
                }
            }
        }
        catch (Exception e)
        {
            onLoadComplete?.Invoke();
        }

    }
    
    public static void GoToNextLevel(Action onLoadComplete = null)
    {
        try
        {
            LevelCfg currentLevel = LevelCfgDb.GetCurrentLevel();
            if (currentLevel != null)
            {
                LevelCfg nextLevel = LevelCfgDb.GetLevelByBuildIndex(currentLevel.sceneIndex + 1);
                if (nextLevel != null)
                {
                    GameApplicationHandle.BeginRoutine(DoLevelLoad(nextLevel.sceneIndex, onLoadComplete));
                }
                else
                {
                    Debug.Log("Failed to load next level. Next level Cfg is NULL");
                }
            }
            else
            {
                GameApplicationHandle.BeginRoutine(DoLevelLoad(DEFAULT_LEVEL_BUILD_IDX, onLoadComplete));
            }
        }
        catch (Exception e)
        {
            
        }
    }

    public static void RestartLevel(Action onLoadComplete = null)
    {
        LevelCfg loadedLevel = LevelCfgDb.GetCurrentLevel();
        if (loadedLevel != null)
        {
            int buildIndex = SceneManager.GetActiveScene().buildIndex;
            if (buildIndex >= 0)
            {
                GameApplicationHandle.BeginRoutine(DoLevelLoad(buildIndex, onLoadComplete));
            }
        }
        else
        {
            GameApplicationHandle.BeginRoutine(DoLevelLoad(DEFAULT_LEVEL_BUILD_IDX, onLoadComplete));
        }
    }


    private static IEnumerator DoLevelLoad(int buildIndex, Action onComplete = null)
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        
        AsyncOperation loadingScene = SceneManager.LoadSceneAsync(LoadingScreenSceneName, LoadSceneMode.Additive);
        while (!loadingScene.isDone) yield return null;

        AsyncOperation unloadCurrent = SceneManager.UnloadSceneAsync(currentScene);
        while (!unloadCurrent.isDone) yield return null;

        AsyncOperation newLevel = SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Single);
        while (!newLevel.isDone) yield return null;

        onComplete?.Invoke();
    }
}