using DG.Tweening;
using UnityEngine;

public class PointsOnTouch : WorldObjectBase
{
    public Animation animation;

    private static float lastCollectTime;
    private static int collectStreak = 0;
    private const int MaxCollectStreak = 8;
    private const float collectTimeThreshold = 1.0f;
    
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Time.timeSinceLevelLoad - lastCollectTime <= collectTimeThreshold)
            {
                collectStreak++;
                if (collectStreak >= MaxCollectStreak)
                {
                    collectStreak = MaxCollectStreak;
                }
            }else
            {
                collectStreak = 0;
            }
            
            AudioClip clip = AudioProperties.Get().CarrotCollectClips[collectStreak];
            BPAudioManager.Instance.Play(clip, false, BPAudioTrack.SFX);
            GameSystem.GetGameManager().AddPoints(value);
            animation.Play("carrot_collect");
            lastCollectTime = Time.timeSinceLevelLoad;
        }
    }

    public void OnCollectComplete()
    {
        this.gameObject.SetActive(false);
    }
}