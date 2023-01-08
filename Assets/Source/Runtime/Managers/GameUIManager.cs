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
        GameSoftFail,
        GameHardFail,
        GameSuccess,
        Opening
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

        LevelCompleteScreenController levelCompleteController = new LevelCompleteScreenController(
            _view.GetScreenView(GameScreenID.GameSuccess));
        _screens.Add(GameScreenID.GameSuccess, levelCompleteController);
                        
        LevelFailedScreenController levelSoftFailController = new LevelFailedScreenController(
            _view.GetScreenView(GameScreenID.GameSoftFail));
        _screens.Add(GameScreenID.GameSoftFail, levelSoftFailController);
        
        LevelFailedScreenController levelHardFailController = new LevelFailedScreenController(
            _view.GetScreenView(GameScreenID.GameHardFail));
        _screens.Add(GameScreenID.GameHardFail, levelHardFailController);

        OpeningScreenController openingScreenController = new OpeningScreenController(
            _view.GetScreenView(GameScreenID.Opening));
        _screens.Add(GameScreenID.Opening, openingScreenController);
        
        ToggleControlPanel(false);
        ToggleInGameHeader(false);
        
        GC.Collect(UnityEngine.Random.Range(1000, 9999), GCCollectionMode.Optimized);
    }

    public void GoToScreen(GameScreenID screenId)
    {
        GameApplicationHandle.BeginRoutine(ChangeScreensRoutine(screenId));
    }

    public void HideAllScreens()
    {
        _view.HideAllViews();
    }

    public void ToggleControlPanel(bool isOn)
    {
        _view.ToggleControlPanel(isOn);
    }
    
    public void ToggleInGameHeader(bool isOn)
    {
        _view.ToggleInGameHeader(isOn);
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