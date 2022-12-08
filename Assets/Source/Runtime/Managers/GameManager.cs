using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public int points;

    public bool gameHasStarted { get; private set; }
    public bool gameHasEnded { get; private set; }
    public bool playerHasFailed { get; private set; }

    public void Start()
    {
        GameSystem.GetTrackController().OnTrackEnd.RemoveListener(OnTrackEnded);
        GameSystem.GetTrackController().OnTrackEnd.AddListener(OnTrackEnded);
        GameSystem.GetTrackController().Restart();

        GameUIManager.Instance.RebuildUI();
        GameUIManager.Instance.GoToScreen(GameUIManager.GameScreenID.PreGame);
        
        // try get last checkpoint
        float lastCheckpoint = PlayerPrefs.GetFloat(LastCheckpointKey, -1.0f);
        if (lastCheckpoint > 0)
        {
            GameSystem.GetTrackController().SetTrackTime(lastCheckpoint);
        }
        else
        {
            GameSystem.GetTrackController().SetTrackTime(0.0f);
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
        GameUIManager.Instance.GoToScreen(GameUIManager.GameScreenID.Game);
    }

    public void EndGame(bool isWin)
    {
        gameHasEnded = true;
        GameSystem.GetTrackController().Pause();
        if (isWin)
        {
            playerHasFailed = false;
            ClearCheckpoint();
            GameUIManager.Instance.GoToScreen(GameUIManager.GameScreenID.GameSuccess);
        }
        else
        {
            playerHasFailed = true;
            GameUIManager.Instance.GoToScreen(GameUIManager.GameScreenID.GameFail);
        }
    }

    private const string LastCheckpointKey = "LastCheckpoint";
    public void AddCheckpoint(float time)
    {
        //const string checkpoint
        PlayerPrefs.SetFloat(LastCheckpointKey, time);
        Debug.Log($"Added checkpoint at time: {time}");
    }

    public void ClearCheckpoint()
    {
        PlayerPrefs.DeleteKey(LastCheckpointKey);
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