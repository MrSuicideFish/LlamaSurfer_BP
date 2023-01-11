using System;
using System.IO;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int points;

    public bool gameHasStarted { get; private set; }
    public bool gameHasEnded { get; private set; }
    public bool playerHasFailed { get; private set; }

    public int startCheckpoint { get; private set; } = -1;

    public void Start()
    {
        if (!GameApplicationHandle.HasInitialized)
        {
            GameApplicationHandle.Initialize();
        }
        else
        {
            AdsManager.LoadRewarded();
            AdsManager.ShowBanner();
            AdsManager.LoadInterstitial();
        }

        BPAudioManager.Instance.Play(AudioProperties.Get().GameMusicClip, true, BPAudioTrack.Music, 1.0f, 0.1f);
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
            if (lastCheckpoint > 0 && lastCheckpoint < track.Checkpoints.Length)
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
        BPAudioManager.Instance.StopMusic();

        AdsManager.LoadInterstitial();
        
        if (isWin)
        {
            BPAudioManager.Instance.Play(AudioProperties.Get().LevelCompleteClip, false, BPAudioTrack.Music);
            playerHasFailed = false;
            ClearCheckpoint();
            LevelCfg currentLevel = LevelCfgDb.GetCurrentLevel();
            if (currentLevel != null)
            {
                int lastLevelCompleted = PlayerData.GetData<int>(PlayerData.DataKey.LastLevelCompleted, 1);
                if (currentLevel.sceneIndex > lastLevelCompleted)
                {
                    PlayerData.SetData(PlayerData.DataKey.LastLevelCompleted, currentLevel.sceneIndex);
                }
                Analytics.LevelComplete(currentLevel.sceneIndex, points, currentLevel.maxPoints);
            }
            
            PlayerData.Save();
            GameUIManager.Instance.GoToScreen(GameUIManager.GameScreenID.GameSuccess);
        }
        else
        {
            BPAudioManager.Instance.Play(AudioProperties.Get().LevelFailClip, false, BPAudioTrack.Music);
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
            BPAudioManager.Instance.Play(AudioProperties.Get().CheckpointGrantedClip, false, BPAudioTrack.SFX);
        }
    }

    public void ClearCheckpoint()
    {
        PlayerPrefs.DeleteKey(PlayerData.DataKey.LastCheckpoint);
    }

    public bool ShouldShowHardDeath()
    {
        if (startCheckpoint == -1 || startCheckpoint == 0) return true;
        return PlayerData.GetData<int>(PlayerData.DataKey.HeartCount, 3) == 0;
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