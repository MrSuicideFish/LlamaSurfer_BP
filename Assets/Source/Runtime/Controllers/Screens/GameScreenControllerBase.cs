using System.Collections;
using UnityEngine;

public class GameScreenControllerBase
{
    protected GameScreenView _view;
    
    protected GameScreenControllerBase(GameScreenView view)
    {
        _view = view;
    }

    public Coroutine Show()
    {
        return GameApplicationHandle.BeginRoutine(_view.OnShow());
    }

    public Coroutine Hide()
    {
        return GameApplicationHandle.BeginRoutine(_view.OnHide());
    }
}