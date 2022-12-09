using DG.Tweening;
using UnityEngine;

public class PointsOnTouch : WorldObjectBase
{
    public Animation animation;
    
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameSystem.GetGameManager().AddPoints(value);
            animation.Play("carrot_collect");
        }
    }

    public void OnCollectComplete()
    {
        this.gameObject.SetActive(false);
    }
}