using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = System.Random;

public class GameUIManager
{
    public enum GameScreenID
    {
        LevelSelect,
        Options,
        PreGame,
        Game,
        GameFail,
        GameSuccess
    }

    private static GameUIManager _instance;
    public static GameUIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameUIManagerView viewRes = Resources.Load<GameUIManagerView>("UI/GameUIView");
                if (viewRes != null)
                {
                    GameUIManagerView managerView = GameUIManagerView.Instantiate(viewRes);
                    if (managerView != null)
                    {
                        _instance = new GameUIManager(managerView);
                        GameObject.DontDestroyOnLoad(managerView.gameObject);
                    }
                }
            }
            return _instance;
        }
    }
    
    private GameUIManagerView _view { get; set; }
    private GameScreenID _lastScreen { get; set; }
    private GameScreenID _activeScreen { get; set; }
    private Dictionary<GameScreenID, GameScreenControllerBase> _screens { get; set; }
    
    private GameUIManager(GameUIManagerView view)
    {
        this._view = view;
        RebuildUI();
    }

    ~GameUIManager()
    {
        _instance = null;
        GC.Collect();
    }

    public void RebuildUI()
    {
        _screens = new Dictionary<GameScreenID, GameScreenControllerBase>();
        
        // build screens
        OptionsScreenController optionsController = new OptionsScreenController(
            _view.GetScreenView(GameScreenID.Options));
        _screens.Add(GameScreenID.Options, optionsController);

        LevelSelectScreenController levelSelectController = new LevelSelectScreenController(
            _view.GetScreenView(GameScreenID.Options));
        _screens.Add(GameScreenID.LevelSelect, levelSelectController);

        PreGameScreenController pregameController = new PreGameScreenController(
            _view.GetScreenView(GameScreenID.PreGame));
        _screens.Add(GameScreenID.PreGame, pregameController);

        GameplayScreenController gameplayController = new GameplayScreenController(
            _view.GetScreenView(GameScreenID.Game));
        _screens.Add(GameScreenID.Game, gameplayController);
                        
        LevelCompleteScreenController levelCompleteController = new LevelCompleteScreenController(
            _view.GetScreenView(GameScreenID.GameSuccess));
        _screens.Add(GameScreenID.GameSuccess, levelCompleteController);
                        
        LevelFailedScreenController levelFailController = new LevelFailedScreenController(
            _view.GetScreenView(GameScreenID.GameFail));
        _screens.Add(GameScreenID.GameFail, levelFailController);

        GC.Collect(UnityEngine.Random.Range(1000, 9999), GCCollectionMode.Optimized);
    }

    public void GoToScreen(GameScreenID screenId)
    {
        GameApplicationHandle.BeginRoutine(ChangeScreensRoutine(screenId));
    }

    private IEnumerator ChangeScreensRoutine(GameScreenID screenId)
    {
        _lastScreen = _activeScreen;
        
        yield return _screens[_activeScreen].Hide();
        _view.ToggleScreenView(screenId);
        _activeScreen = screenId;
        yield return _screens[screenId].Show();
    }
}