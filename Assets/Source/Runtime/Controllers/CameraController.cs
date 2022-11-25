using System;
using Cinemachine;
using UnityEngine;

[ExecuteAlways]
public class CameraController : MonoBehaviour
{
    public static CameraController Instance;
    
    public CinemachineVirtualCamera mainTrackCamera;
    public CinemachinePath cameraPath;

    public float yOffsetPerBlock = 1.5f;
    private const float MinYOffset = 6.31f; 
    
    private CinemachineTrackedDolly _cameraDolly;

    private Vector3 targetTrackPos;
    private Vector3 targetTrackRot;
    public Vector3 positionOffset;
    
    private void OnEnable()
    {
        Instance = this;
    }
    
    private void Start()
    {
        Instance = this;
#if UNITY_EDITOR
        cameraPath = FindObjectOfType<CinemachinePath>();
        if (cameraPath == null)
        {
            GameObject cameraPathObj = new GameObject("Camera_Path");
            cameraPath = cameraPathObj.AddComponent<CinemachinePath>();
        }
#endif
    }

    private void Update()
    {
        UpdateTrackTarget();
        mainTrackCamera.transform.position = targetTrackPos;
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

        /*
        GetCameraDolly().m_PathPosition = Mathf.SmoothDamp(GetCameraDolly().m_PathPosition, targetTrackPos,
            ref currentVelocity, moveSmoothTime * Time.deltaTime);

        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            GetCameraDolly().m_PathOffset.y =
                Mathf.Lerp(GetCameraDolly().m_PathOffset.y,
                    Mathf.Clamp(yOffsetPerBlock * GameManager.Instance.playerController.BlockCount(), MinYOffset,
                        Mathf.Infinity), 0.05f);
        }
        */

        //GetCameraDolly().m_PathPosition = targetTrackPos;


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