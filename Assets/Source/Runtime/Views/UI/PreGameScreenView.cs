using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PreGameScreenView : GameScreenView
{
    public UILevelButton[] levelButtons;
    public TextMeshProUGUI levelNumText;
    public TextMeshProUGUI carrotCountText;
    
    public override IEnumerator OnShow()
    {
        LevelCfg levelCfg = LevelCfgDb.GetCurrentLevel();
        if (levelCfg != null)
        {
            int currentLevel = levelCfg.sceneIndex;
            int round = Mathf.FloorToInt(currentLevel / 5) * 5;
            int next = round;

            if (round == 0)
            {
                round++;
                next++;
            }
        
            for (int i = 0; i < levelButtons.Length; i++)
            {
                levelButtons[i].SetLevel(next, next <= currentLevel);
                next++;
            }

            levelNumText.text = $"Level {currentLevel}";
            carrotCountText.text = levelCfg.maxPoints.ToString();
        }
        else
        {
            levelNumText.text = "Error";
            carrotCountText.text = "Error";
        }
        
        yield return base.OnShow();
    }

    public override IEnumerator OnHide()
    {
        yield return base.OnHide();
    }

    private void Update()
    {
        if (GameSystem.GetGameManager().gameHasStarted) return;
        if (Input.touchCount > 0)
        {
            if (Input.GetTouch(0).position.y < Screen.height / 2.0f)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Moved)
                {
                    GameSystem.GetGameManager().StartGame();
                }
            }
        }
    }
}
