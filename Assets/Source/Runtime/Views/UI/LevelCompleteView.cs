using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class LevelCompleteView : GameScreenView
{
    public TextMeshProUGUI pointCountText;
    public Image playButton;
    public RectTransform contentWindow;
    public Image[] stars;

    private int currentPoints;
    private int maxPoints;

    private Tween DoCountPoints()
    {
        // do points counting
        currentPoints = 0;
        int targetPoints = GameManager.Instance.points;
        if (LevelCfgDb.GetCurrentLevel() != null)
        {
            maxPoints = LevelCfgDb.GetCurrentLevel().maxPoints;
        }
        else
        {
            maxPoints = 100;
        }

        return DOTween.To(
                () => currentPoints, x => currentPoints = x, targetPoints, 1.3f).SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                pointCountText.rectTransform.DOPunchScale(Vector3.one * 1.2f, 0.3f, 1, 0.1f);
            });
    }

    private Tween DoPresentStars(int count)
    {
        Sequence seq = DOTween.Sequence();
        
        // move all stars to top of screen
        foreach(Image star in stars)
        {
            star.rectTransform.anchoredPosition = contentWindow.rect.position;
            star.color = new Color(1, 1, 1, 0.0f);
        }

        if (count > stars.Length)
        {
            count = stars.Length;
        }

        for (int i = 0; i < count; i++)
        {
            Sequence starSeq = DOTween.Sequence();
            if (starSeq != null)
            {
                starSeq.Join(stars[i].DOColor(new Color(1, 1, 1, 1), 0.1f));
                starSeq.Join(stars[i].rectTransform.DOAnchorPos(new Vector2(0,0), 0.1f));
                starSeq.Append(stars[i].rectTransform.DOPunchScale(Vector3.one * 1.2f, 0.1f, vibrato: 1, elasticity: 0.1f));
                seq.Append(starSeq);
            }
        }
        
        return seq;
    }

    private void HidePlayButton()
    {
        foreach (Image img in playButton.GetComponentsInChildren<Image>())
        {
            img.DOColor(new Color(1, 1, 1, 0), 0.0f);
        }
    }

    private Tween ShowPlayButton()
    {
        Sequence seq = DOTween.Sequence();
        
        foreach (Image img in playButton.GetComponentsInChildren<Image>())
        {
            seq.Join(img.DOColor(new Color(1, 1, 1, 1), 0.5f));
        }

        seq.Join(playButton.rectTransform.DOPunchScale(Vector3.one * 2.0f, 0.3f, vibrato: 1, elasticity: 0.1f));

        return seq;
    }

    public override IEnumerator OnShow()
    {
        HidePlayButton();
        contentWindow.anchoredPosition = new Vector2(0, -3000);

        Sequence lvlCompleteSeq = DOTween.Sequence();
        lvlCompleteSeq.Append(contentWindow.DOAnchorPos(Vector2.zero, 1.4f, false).SetEase(Ease.OutBounce));
        lvlCompleteSeq.Append(DoCountPoints());

        float completePerc = (float)GameManager.Instance.points / (float) maxPoints;
        int starCount = Mathf.RoundToInt(Mathf.Lerp(1, 5, completePerc));
        lvlCompleteSeq.Append(DoPresentStars(starCount));
        lvlCompleteSeq.Play();
        
        yield return new WaitForSeconds(2);
        ShowPlayButton().Play();
    }
    
    public double RoundDown(double number, int decimalPlaces)
    {
        return Math.Floor(number * Math.Pow(10, decimalPlaces)) / Math.Pow(10, decimalPlaces);
    }

    public override IEnumerator OnHide()
    {
        yield break;
    }

    private void Update()
    {
        pointCountText.SetText("{0} / {1}", currentPoints, maxPoints);
    }
}