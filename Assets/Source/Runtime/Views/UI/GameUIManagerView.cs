using System;
using System.Collections.Generic;
using UnityEngine;

public class GameUIManagerView : MonoBehaviour
{
    [Serializable]
    public class GameUIView
    {
        public GameUIManager.GameScreenID id;
        public GameScreenView view;
    }
    
    public GameUIView[] screens;

    public GameScreenView GetScreenView(GameUIManager.GameScreenID screenId)
    {
        for (int i = 0; i < screens.Length; i++)
        {
            if (screens[i].id == screenId)
            {
                return screens[i].view;
            }
        }

        return null;
    }

    public void ToggleScreenView(GameUIManager.GameScreenID screenId)
    {
        for (int i = 0; i < screens.Length; i++)
        {
            GameUIView view = screens[i];
            if (view != null && view.view != null)
            {
                view.view.gameObject.SetActive(view.id == screenId);
            }
        }
    }

    public void GoToNextLevel()
    {
        Debug.Log("Going to next level");
        LevelLoader.GoToNextLevel();
    }
    
    public void ReplayLevel()
    {
        Debug.Log("Replaying level");
        LevelLoader.RestartLevel();
    }

    public void ShowLevelsScreen()
    {
        Debug.Log("Showing levels screen");
        ToggleScreenView(GameUIManager.GameScreenID.LevelSelect);
    }

    public void ShowOptionsScreen()
    {
        Debug.Log("Showing options screen");
        ToggleScreenView(GameUIManager.GameScreenID.Options);
    }
}