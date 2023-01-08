using UnityEngine;

public class OpeningScreenView : GameScreenView
{
    public void Play()
    {
        LevelLoader.GoToNextLevel();
    }
}