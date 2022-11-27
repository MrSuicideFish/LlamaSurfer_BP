using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTrail : MonoBehaviour
{
    public TrailRenderer trail;

    private void Start()
    {
        // move trail to bottom
        trail.transform.position = transform.position + (Vector3.up * 0.1f);
    }

    private void Update()
    {
        trail.emitting = GameSystem.GetPlayer().IsGrounded();
    }
}
