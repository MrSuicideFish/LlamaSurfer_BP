using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.UI;

public class GameplayView : GameScreenView
{
    public Image progressBar;
    public Image pointImage;
    public TextMeshProUGUI pointText;

    private Tween _popPointsTween;
    private Tween _counterTween;

    private void Start()
    {
        _popPointsTween = pointText.rectTransform.DOPunchScale(Vector3.one * 2.0f, 0.3f, vibrato: 1, elasticity: 0.1f);
    }

    public override IEnumerator OnShow()
    {
        _popPointsTween = DOTween.Sequence();
        
        yield break;
    }
    
    public override IEnumerator OnHide()
    {
        yield break;
    }

    public void SetPointsTo(int to)
    {
        if (int.TryParse(pointText.text, out int counter))
        {
            _counterTween = DOTween.To(() => counter, (x) => counter = x, to, 0.2f).OnUpdate(() =>
            {
                pointText.SetText(counter.ToString());
            });
            _popPointsTween.Restart(false);
        }
    }

    public void Update()
    {
        if (TrackController.Instance != null)
        {
            progressBar.fillAmount = TrackController.Instance.TrackTime;   
        }
    }
}