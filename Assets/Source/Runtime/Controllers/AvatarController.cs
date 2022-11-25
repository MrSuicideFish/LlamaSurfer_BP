using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarController : MonoBehaviour
{
    public Transform surfBlocksParent;
    private Animator _avatarAnimator;

    private void Awake()
    {
        Events.OnPlayerBlockAdded.RemoveListener(data => OnBlockAdded());
        Events.OnPlayerBlockAdded.AddListener(data => OnBlockAdded());
        
        Events.OnPlayerBlockRemoved.RemoveListener(data => OnBlockRemoved());
        Events.OnPlayerBlockRemoved.AddListener(data => OnBlockRemoved());
    }

    private void MoveAvatarToTop()
    {
        Vector3 localpos = Vector3.zero;
        foreach (Transform child in surfBlocksParent)
        {
            if (child.localPosition.y > localpos.y)
            {
                localpos = child.localPosition;
            }
        }

        localpos.y += 0.5f;
        this.transform.localPosition = localpos;
    }

    private void OnBlockAdded()
    {
        MoveAvatarToTop();
    }

    private void OnBlockRemoved()
    {
        
    }
}