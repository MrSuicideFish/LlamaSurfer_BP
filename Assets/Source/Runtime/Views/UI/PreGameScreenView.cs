using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreGameScreenView : GameScreenView
{
    public override IEnumerator OnShow()
    {
        yield break;
    }

    public override IEnumerator OnHide()
    {
        yield break;
    }

    private void Update()
    {
        if (GameSystem.GetGameManager().gameHasStarted) return;
        if (Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                GameSystem.GetGameManager().StartGame();
            }
        }
    }
}
