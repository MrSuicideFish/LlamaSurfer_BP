using System;
using Cinemachine;
using UnityEngine;

[ExecuteInEditMode]
public class CameraController : MonoBehaviour
{
    private CinemachineBrain cinemachineBrain;
    public CinemachineVirtualCamera mainTrackCamera;

    private Vector3 targetTrackPos;
    private Vector3 targetTrackRot;
    public Vector3 positionOffset;

    private void Start()
    {
        cinemachineBrain = this.GetComponent<CinemachineBrain>();
    }

    private void Update()
    {
        UpdateTrackTarget();
        
        mainTrackCamera.VirtualCameraGameObject.transform.position = targetTrackPos;
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            mainTrackCamera.VirtualCameraGameObject.transform.eulerAngles = GameManager.Instance.playerController.transform.position -
                                                    mainTrackCamera.transform.position;
        }
    }

    public void UpdateTrackTarget()
    {
        Vector3 trackTangent = TrackController.Instance.GetTrackTangent();
        Vector3 cross = Vector3.Cross(Vector3.up, trackTangent);
        
        // evaluate position
        float trackTime = TrackController.Instance.TrackTime + TrackController.Instance.pathTargetLead;
        targetTrackPos = TrackController.Instance.GetTrackPositionAt(trackTime);
        targetTrackPos += positionOffset;
        
        // evaluate rotation
        var targetLookDir = (targetTrackPos - mainTrackCamera.transform.position);
        targetTrackRot = Vector3.Slerp(targetTrackRot, targetLookDir, 0.1f);
    }

    private CinemachineTrackedDolly GetCameraDolly()
    {
        if (mainTrackCamera == null)
        {
            return null;
        }

        return mainTrackCamera.GetCinemachineComponent<CinemachineTrackedDolly>();
    }
}