using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Cinemachine;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

[ExecuteAlways]
public class PlayerController : MonoBehaviour
{
    public CharacterController charController;

    public Transform surfBlockParent;

    // locomotion
    public float moveSpeed;
    public float moveWidth;
    
    public float moveX { get; private set; }

    private Vector3 targetPosition;
    private Vector3 targetRotation;
    private Vector3 lastCross;
    public float moveSmoothTime;

    public void Move(float position)
    {
        if (GameManager.Instance.gameHasStarted && !GameManager.Instance.gameHasEnded)
        {
            moveX = Remap(position, 0.0f, 1.0f, -1.0f, 1.0f);
        }
    }
    
    public static float Remap (float from, float fromMin, float fromMax, float toMin,  float toMax)
    {
        var fromAbs  =  from - fromMin;
        var fromMaxAbs = fromMax - fromMin;
       
        var normal = fromAbs / fromMaxAbs;
 
        var toMaxAbs = toMax - toMin;
        var toAbs = toMaxAbs * normal;
 
        var to = toAbs + toMin;
       
        return to;
    }
    
    private void Update()
    {
        UpdatePositionAndRotation();

        charController.transform.position = targetPosition;

        if (Math.Abs(targetRotation.magnitude - Double.Epsilon) > 0.01f)
        {
            transform.forward = targetRotation;
        }
    }

    private void UpdatePositionAndRotation()
    {
        Vector3 trackPosition = TrackController.Instance.GetTrackPosition();
        Vector3 trackTangent = TrackController.Instance.GetTrackTangent();
        Vector3 cross = Vector3.Cross(Vector3.up, trackTangent);
        targetPosition = trackPosition + (cross * (moveX * moveWidth));
        targetRotation = Vector3.Slerp(targetRotation, (trackPosition + trackTangent) - trackPosition, 0.1f);
        lastCross = cross;
    }

    public int BlockCount()
    {
        return surfBlockParent.childCount;
    }
    
    public bool IsGrounded()
    {
        return surfBlockParent.childCount > 0
               && surfBlockParent.GetChild(0).localPosition.y < 0.1f;
    }
}