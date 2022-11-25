using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public PlayerController playerController;
    public int points;

    public bool gameHasStarted { get; private set; }
    public bool gameHasEnded { get; private set; }
    public bool playerHasFailed { get; private set; }

    public void Awake()
    {
        Instance = this;
        TrackController.Instance.OnTrackEnd.RemoveListener(OnTrackEnded);
        TrackController.Instance.OnTrackEnd.AddListener(OnTrackEnded);
        TrackController.Instance.Restart();

        GameUIManager.Instance.RebuildUI();
        GameUIManager.Instance.GoToScreen(GameUIManager.GameScreenID.PreGame);
    }

    private void OnTrackEnded()
    {
        EndGame(isWin: true);
    }

    public void StartGame()
    {
        TrackController.Instance.SetTrackTime(0.0f);
        TrackController.Instance.Play();
        gameHasStarted = true;
        GameUIManager.Instance.GoToScreen(GameUIManager.GameScreenID.Game);
    }

    public void EndGame(bool isWin)
    {
        gameHasEnded = true;
        TrackController.Instance.Pause();
        if (isWin)
        {
            playerHasFailed = false;
            GameUIManager.Instance.GoToScreen(GameUIManager.GameScreenID.GameSuccess);
        }
        else
        {
            playerHasFailed = true;
            
            GameUIManager.Instance.GoToScreen(GameUIManager.GameScreenID.GameFail);
        }
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
        SurfBlockView newViewRes = playerController.surfBlockParent.GetChild(0).GetComponent<SurfBlockView>();
        if (newViewRes != null)
        {
            SurfBlockView newView = GameObject.Instantiate(newViewRes, playerController.surfBlockParent);
            newView.transform.localPosition = new Vector3(0, playerController.BlockCount()-1, 0);
            newView.transform.localEulerAngles = Vector3.zero;
            newView.transform.SetAsLastSibling();

            Events.OnPlayerBlockAdded.Invoke(playerController.surfBlockParent.childCount);
        }
    }

    public void RemovePlayerBlock(SurfBlockView view)
    {
        SurfBlockView[] blocks = playerController.surfBlockParent.gameObject.GetComponentsInChildren<SurfBlockView>();

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
            if (playerController.surfBlockParent.childCount == 0)
            {
                EndGame(false);
            }
            
            Events.OnPlayerBlockRemoved.Invoke(playerController.surfBlockParent.childCount);
        }
    }
}