using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeaderInGame : MonoBehaviour
{
    public TextMeshProUGUI pointsText;
    
    [Header("Sprites")]
    public Sprite   heartOnSprite,
                    heartOffSprite;

    public Image[] heartObjects;

    private void OnEnable()
    {
        if (heartObjects == null) return;
        int playerHeartCount = PlayerData.GetData<int>(PlayerData.DataKey.HeartCount, 3);
        for (int i = 0; i < heartObjects.Length; i++)
        {
            heartObjects[i].sprite = i < playerHeartCount ? heartOnSprite : heartOffSprite;
        }
    }
}