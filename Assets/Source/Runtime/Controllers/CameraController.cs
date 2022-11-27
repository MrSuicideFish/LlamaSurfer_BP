using System;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;

[ExecuteInEditMode]
public class CameraController : MonoBehaviour
{
    private const string _trackCameraTagID = "TRACK_CAMERA";
    [HideInInspector] public CinemachineBrain cinemachineBrain;
    [HideInInspector] public CinemachineVirtualCamera mainTrackCamera;
    [HideInInspector] public Cinemachine3rdPersonFollow body;
    [HideInInspector] public CinemachineComposer aim;

    private void Start()
    {
        if (cinemachineBrain == null)
        {
            cinemachineBrain = this.GetComponent<CinemachineBrain>();
            if (cinemachineBrain == null)
            {
                cinemachineBrain = this.AddComponent<CinemachineBrain>();
            }
        }

        if (mainTrackCamera == null)
        {
            GameObject camObj = GameObject.FindWithTag(_trackCameraTagID);
            if (camObj != null)
            {
                mainTrackCamera = camObj.GetComponent<CinemachineVirtualCamera>();
                if (mainTrackCamera == null)
                {
                    mainTrackCamera = camObj.AddComponent<CinemachineVirtualCamera>();
                }
            }
            else
            {
                camObj = new GameObject("TRACK_CAMERA");
                camObj.tag = _trackCameraTagID;
                
                mainTrackCamera = camObj.AddComponent<CinemachineVirtualCamera>();
            }
        }

        mainTrackCamera.Follow = GameSystem.GetPlayer().transform;
        mainTrackCamera.LookAt = GameSystem.GetPlayer().transform;
        
        body = mainTrackCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        if (body == null)
        {
            body = mainTrackCamera.AddCinemachineComponent<Cinemachine3rdPersonFollow>();
        }

        aim = mainTrackCamera.GetCinemachineComponent<CinemachineComposer>();
        if (aim == null)
        {
            aim = mainTrackCamera.AddCinemachineComponent<CinemachineComposer>();
        }
    }

    private void Update()
    {
        mainTrackCamera.m_Lens.FieldOfView  = CameraProperties.Get().fieldOfView;
        body.VerticalArmLength              = CameraProperties.Get().verticalArmLength;
        body.CameraSide                     = CameraProperties.Get().cameraSide;
        body.CameraDistance                 = CameraProperties.Get().cameraDistance;
        body.ShoulderOffset = CameraProperties.Get().bodyOffset;
        body.Damping = CameraProperties.Get().bodyDamping;
        aim.m_TrackedObjectOffset = CameraProperties.Get().aimOffset;
    }
}