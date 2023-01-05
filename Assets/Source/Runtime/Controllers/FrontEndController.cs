using System;
using DG.Tweening;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UI;

public class FrontEndController : MonoBehaviour
{
    public Image logo;
    public void Start()
    {
        logo.DOColor(new Color(0,0,0,0), 0.0f).OnComplete(() =>
        {
            logo.DOColor(Color.white, 1.0f).OnComplete(() =>
            {
                LevelLoader.GoToNextLevel();
            });
        });
    }
}
