using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Cinemachine;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

[ExecuteAlways]
public class PlayerController : MonoBehaviour
{
    public Transform surfBlockParent;

    // locomotion
    public float moveWidth;
    public float rotateSpeed = 15.0f;

    private float _moveX;
    public float moveX
    {
        get
        {
            return _moveX;
        }
        set
        {
            if (GameSystem.GetGameManager() == null)
            {
                _moveX = Remap(value, 0.0f, 1.0f, -1.0f, 1.0f);
                return;
            }
            
            bool bCanMove = GameSystem.GetGameManager().gameHasStarted && !GameSystem.GetGameManager().gameHasEnded;
            
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                bCanMove = true;
            }
#endif

            if (bCanMove)
            {
                _moveX = Remap(value, 0.0f, 1.0f, -1.0f, 1.0f);
            }
        }
    }

    private Vector3 targetPosition;
    private Vector3 targetRotation;

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

        transform.position = targetPosition;

        if (Math.Abs(targetRotation.magnitude - Double.Epsilon) > 0.01f)
        {
            transform.forward = Vector3.Slerp(transform.forward, targetRotation, rotateSpeed * Time.deltaTime);
        }
    }

    private void UpdatePositionAndRotation()
    {
        Vector3 trackPosition = GameSystem.GetTrackController().GetTrackPosition();
        Vector3 trackTangent = GameSystem.GetTrackController().GetTrackTangent();
        Vector3 cross = Vector3.Cross(Vector3.up, trackTangent);
        targetPosition = trackPosition + (cross * (moveX * moveWidth));
        targetRotation = Vector3.Slerp(targetRotation, (trackPosition + trackTangent) - trackPosition, 0.1f);
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