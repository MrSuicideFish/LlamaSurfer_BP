using System;
using System.Collections;
using System.ComponentModel.Design.Serialization;
using DG.Tweening;
using DG.Tweening.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class LevelCompleteView : GameScreenView
{
    [Header("Star sprites")]
    public Sprite starBrightSprite;
    public Sprite starDimSprite;
    
    [Header("Instance References")]
    public TextMeshProUGUI pointCountText;
    public Image playButton;
    public Image starPrefab;
    public RectTransform starsContainer;

    [Header("Star Animation Parameters")] 
    public float starPunchDuration;
    public int starPunchStrength;
    public float starPunchElasticity;
    public float starPunchDelay;
    
    [Header("Point Count Animation Parameters")]
    public int pointCountDuration;
    public float pointCountPunchSize;
    public float pointCountPunchDuration;
    public int pointCountPunchStrength;
    public float pointCountPunchElasticity;

    private Image[] stars;

    private void OnEnable()
    {
        if (stars == null)
        {
            stars = new Image[5];
            for (int i = 0; i < stars.Length; i++)
            {
                if (i == 0)
                {
                    stars[i] = starPrefab;
                }
                else
                {
                    stars[i] = GameObject.Instantiate(starPrefab);
                    stars[i].transform.SetParent(starsContainer, false);
                }
                stars[i].sprite = starDimSprite;
            }
        }
    }

    [ContextMenu("Test Animations")]
    public void Debug_TestAnimation()
    {
        DoShowStars(3);
        DoCountPoints(117, 160);
    }

    private Tween DoShowStars(int count)
    {
        Sequence seq = DOTween.Sequence();

        for (int i = 0; i < stars.Length; i++)
        {
            if (i < count)
            {
                stars[i].sprite = starBrightSprite;
                seq.Append(stars[i].transform.DOPunchScale(Vector3.one * 3, starPunchDuration, starPunchStrength,
                    starPunchElasticity)).SetDelay(starPunchDelay).OnStart(() =>
                {
                    //BPAudioManager.Instance.Play(AudioProperties.Get().StarPopupClip, false);
                });
            }
            else
            {
                stars[i].sprite = starDimSprite;
            }
        }

        return seq.Play();
    }

    private void DoCountPoints(int count, int total)
    {
        int lastVal = 0;
        DOSetter<float> setter = value =>
        {
            float mod = value - (value % 1.0f);
            if (mod > lastVal)
            {
                BPAudioManager.Instance.Play(AudioProperties.Get().CoinCountClip, false, BPAudioTrack.UI,
                    (1.0f) + (0.5f * (value / total)), 0.2f);
                lastVal = (int) mod;
            }
            
            pointCountText.SetText("{0} / {1}", Mathf.FloorToInt(value), total);
        };

        Tween t = DOTween.To(setter, 0, count, pointCountDuration).SetEase(Ease.InExpo).OnComplete(() =>
        {
            BPAudioManager.Instance.Play(AudioProperties.Get().CoinCountCompleteClip, false, BPAudioTrack.UI);
            pointCountText.rectTransform.DOPunchScale(Vector3.one * pointCountPunchSize, pointCountPunchDuration,
            pointCountPunchStrength, pointCountPunchElasticity);
        });
    }

    private void HidePlayButton()
    {
        foreach (Image img in playButton.GetComponentsInChildren<Image>())
        {
            img.DOColor(new Color(1, 1, 1, 0), 0.0f);
        }
    }

    private void ShowPlayButton()
    {
        foreach (Image img in playButton.GetComponentsInChildren<Image>())
        {
            img.DOColor(new Color(1, 1, 1, 1), 0.0f);
        }
    }

    public override IEnumerator OnShow()
    {
        HidePlayButton();

        int points, maxPoints;
        points = GameSystem.GetGameManager().points;
        maxPoints = LevelCfgDb.GetCurrentLevel().maxPoints;
        DoCountPoints(points,maxPoints);
        
        float completePerc = (float)points / (float) maxPoints;
        int starCount = Mathf.RoundToInt(Mathf.Lerp(1, 5, completePerc));
        DoShowStars(starCount);

        yield return new WaitForSeconds(2);
        ShowPlayButton();
    }
    
    public double RoundDown(double number, int decimalPlaces)
    {
        return Math.Floor(number * Math.Pow(10, decimalPlaces)) / Math.Pow(10, decimalPlaces);
    }

    public override IEnumerator OnHide()
    {
        yield break;
    }
}