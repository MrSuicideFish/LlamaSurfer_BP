using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public int points;

    public bool gameHasStarted { get; private set; }
    public bool gameHasEnded { get; private set; }
    public bool playerHasFailed { get; private set; }

    public int startCheckpoint { get; private set; } = -1;

    public void Start()
    {
        TrackController track = GameSystem.GetTrackController();
        if (track != null)
        {
            track.OnTrackEnd.RemoveListener(OnTrackEnded);
            track.OnTrackEnd.AddListener(OnTrackEnded);
            track.Restart();

            GameUIManager.Instance.RebuildUI();
            GameUIManager.Instance.GoToScreen(GameUIManager.GameScreenID.PreGame);
        
            // try get last checkpoint
            int lastCheckpoint = PlayerPrefs.GetInt(PlayerData.DataKey.LastCheckpoint, 0);
            if (lastCheckpoint > 0)
            {
                float lastCheckpointTime = track.Checkpoints[lastCheckpoint];
                track.SetTrackTime(lastCheckpointTime);
                startCheckpoint = lastCheckpoint;
            }
            else
            {
                startCheckpoint = -1;
                GameSystem.GetTrackController().SetTrackTime(0.0f);
            }
        }
        
        AdsManager.ShowBanner();
    }

    private void OnTrackEnded()
    {
        EndGame(isWin: true);
    }

    public void StartGame()
    {
        GameSystem.GetTrackController().Play();
        gameHasStarted = true;
        GameUIManager.Instance.HideAllScreens();
        GameUIManager.Instance.ToggleControlPanel(false);
        GameUIManager.Instance.ToggleInGameHeader(true);
        
        LevelCfg currentLevel = LevelCfgDb.GetCurrentLevel();
        if (currentLevel != null)
        {
            Analytics.LevelStart(currentLevel.sceneIndex);
        }
        
        string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments,
            Environment.SpecialFolderOption.None);
        ScreenCapture.CaptureScreenshot(Path.Combine(path, "screenshot_0.png"));
    }

    public void EndGame(bool isWin)
    {
        gameHasEnded = true;
        GameSystem.GetTrackController().Pause();
        GameUIManager.Instance.ToggleInGameHeader(true);
        GameUIManager.Instance.ToggleControlPanel(true);
        
        AdsManager.LoadInterstitial();
        
        if (isWin)
        {
            playerHasFailed = false;
            ClearCheckpoint();
            GameUIManager.Instance.GoToScreen(GameUIManager.GameScreenID.GameSuccess);

            LevelCfg currentLevel = LevelCfgDb.GetCurrentLevel();
            if (currentLevel != null)
            {
                Analytics.LevelComplete(currentLevel.sceneIndex, points, currentLevel.maxPoints);
            }
        }
        else
        {
            playerHasFailed = true;

            if (ShouldShowHardDeath())
            {
                GameUIManager.Instance.GoToScreen(GameUIManager.GameScreenID.GameHardFail);
            }
            else
            {
                GameUIManager.Instance.GoToScreen(GameUIManager.GameScreenID.GameSoftFail);
            }

            LevelCfg currentLevel = LevelCfgDb.GetCurrentLevel();
            if (currentLevel != null)
            {
                int checkpoint = PlayerPrefs.GetInt(PlayerData.DataKey.LastCheckpoint, 0);
                Analytics.LevelFailed(currentLevel.sceneIndex, startCheckpoint, checkpoint, points,
                    currentLevel.maxPoints);
            }
        }
    }
    
    public void AddCheckpoint(int checkpointIndex)
    {
        //const string checkpoint
        PlayerPrefs.SetInt(PlayerData.DataKey.LastCheckpoint, checkpointIndex);
        Debug.Log($"Added checkpoint #{checkpointIndex+1}");
        
        LevelCfg currentLevel = LevelCfgDb.GetCurrentLevel();
        if (currentLevel != null)
        {
            Analytics.CheckpointReached(currentLevel.sceneIndex, checkpointIndex+1);
        }
    }

    public void ClearCheckpoint()
    {
        PlayerPrefs.DeleteKey(PlayerData.DataKey.LastCheckpoint);
    }

    public bool ShouldShowHardDeath()
    {
        return PlayerData.GetData<int>(PlayerData.DataKey.HeartCount, 0) == 0;
    }

    public void AddPoints(int amount)
    {
        if (amount > 0)
        {
            points += amount;
            Events.OnPointsUpdated.Invoke(points);
        }
    }

    public void SetPoints(int amount)
    {
        if (amount >= 0)
        {
            points = amount;
            Events.OnPointsUpdated.Invoke(points);
        }
    }

    public void GivePlayerBlock()
    {
        SurfBlockView newViewRes = GameSystem.GetPlayer().surfBlockParent.GetChild(0).GetComponent<SurfBlockView>();
        if (newViewRes != null)
        {
            SurfBlockView newView = GameObject.Instantiate(newViewRes, GameSystem.GetPlayer().surfBlockParent);
            newView.transform.localPosition = new Vector3(0, GameSystem.GetPlayer().BlockCount()-1, 0);
            newView.transform.localEulerAngles = Vector3.zero;
            newView.transform.SetAsLastSibling();

            Events.OnPlayerBlockAdded.Invoke(GameSystem.GetPlayer().surfBlockParent.childCount);
        }
    }

    public void RemovePlayerBlock(SurfBlockView view)
    {
        SurfBlockView[] blocks = GameSystem.GetPlayer().surfBlockParent.gameObject.GetComponentsInChildren<SurfBlockView>();

        SurfBlockView targetBlock = null;
        for (int i = 0; i < blocks.Length; i++)
        {
            if (blocks[i].GetInstanceID() == view.GetInstanceID())
            {
                targetBlock = blocks[i];
                break;
            }
        }
        if (targetBlock != null)
        {
            targetBlock.Detatch();
            if (GameSystem.GetPlayer().surfBlockParent.childCount == 0)
            {
                EndGame(false);
            }
            
            Events.OnPlayerBlockRemoved.Invoke(GameSystem.GetPlayer().surfBlockParent.childCount);
        }
    }
}