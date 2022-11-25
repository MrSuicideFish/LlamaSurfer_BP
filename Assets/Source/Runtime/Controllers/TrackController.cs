using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using DG.Tweening;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;

[ExecuteAlways]
public class TrackController : MonoBehaviour
{
    public static TrackController Instance;
    
    public SplineContainer spline;
    public Grid _grid { get; private set; }
    
    private const float MIN_TRACK_POSITION = 0.0001f;
    private const float MAX_TRACK_POSITION = 0.9999f;
    public float trackSpeed = 1.0f;

    public UnityEvent OnTrackEnd;

    public int TrackCount => _platforms.Count;
    
    // track playback
    private bool _isPlaying;
    private float _trackTime;
    public float TrackTime
    {
        get => _trackTime;
        set => _trackTime = Mathf.Clamp(value, MIN_TRACK_POSITION, MAX_TRACK_POSITION);
    }
    public bool IsPlaying => _isPlaying;
    public bool HasEnded => TrackTime >= 1.0f;

    public float StartTime { get; private set; }
    
    // path targeting
    private GameObject _pathPositionTarget;
    private CinemachineTargetGroup _pathTargetCineGrp;
    public float pathTargetLead;
    
    // platforms
    public List<Platform> _platforms;
    public Platform _startPlatform;
    public Platform _finishPlatform;

    public void OnValidate()
    {
        for (int i = 0; i < _platforms.Count; i++)
        {
            if (_platforms[i] == null)
            {
                _platforms.RemoveAt(i);
            }
        }

        MoveFinishPlatformToEnd();
    }

    public void OnEnable()
    {
        Instance = this;
        _grid = this.GetComponent<Grid>();
        if (_grid == null)
        {
            _grid = this.AddComponent<Grid>();
        }

        if (spline == null)
        {
            spline = this.AddComponent<SplineContainer>();
        }

        if (spline.CalculateLength() == 0)
        {
            spline.Spline.Add(new BezierKnot(
                float3.zero, float3.zero, float3.zero, Quaternion.identity));
        }
        
        if (_pathPositionTarget == null)
        {
            _pathPositionTarget = GameObject
                .FindGameObjectWithTag("PathTarget");
            if (_pathPositionTarget == null)
            {
                _pathPositionTarget = new GameObject("_PATH_TARGET");
                _pathPositionTarget.gameObject.tag = "PathTarget";
            }
        }

        _pathTargetCineGrp = _pathPositionTarget
            .GetComponent<CinemachineTargetGroup>();
        if (!_pathTargetCineGrp)
        {
            _pathTargetCineGrp = _pathPositionTarget
                .AddComponent<CinemachineTargetGroup>();
        }

        if (_platforms == null || _platforms.Count > 0)
        {
            _platforms = new List<Platform>(
                GetPlatformContainer().GetComponentsInChildren<Platform>());
        }
        
#if UNITY_EDITOR
        MoveFinishPlatformToEnd();
#endif

        RefreshCameraGroupTarget();
        Events.OnPlayerBlockAdded.RemoveListener(data => RefreshCameraGroupTarget());
        Events.OnPlayerBlockAdded.AddListener(data => RefreshCameraGroupTarget());
            
        Events.OnPlayerBlockRemoved.RemoveListener(data => RefreshCameraGroupTarget());
        Events.OnPlayerBlockRemoved.AddListener(data => RefreshCameraGroupTarget());
    }

    public void AddStartPlatform(Platform newPlatform)
    {
        if (_startPlatform != null)
        {
            return;
        }
        
        newPlatform.transform.position = Vector3.zero;
        newPlatform.transform.forward = Vector3.forward;
        _platforms.Add(newPlatform);
        
        newPlatform.transform.SetParent(GetPlatformContainer());
        _startPlatform = newPlatform;
    }

    public void AddFinishPlatform(Platform newPlatform)
    {
        if (_finishPlatform != null)
        {
            return;
        }

        Platform lastPlatform = _platforms[GetLastPlatformIndex()];
        if (lastPlatform != null)
        {
            newPlatform.transform.position = lastPlatform.platformExit.position;
            newPlatform.transform.forward = lastPlatform.platformExit.forward;
        }

        newPlatform.transform.SetParent(GetPlatformContainer());
        _platforms.Add(newPlatform);
        _finishPlatform = newPlatform;
    }

    public void AddPlatform(Platform newPlatform)
    {
        Platform lastPlatform = _platforms[GetLastPlatformIndex()];
        newPlatform.transform.position = lastPlatform.platformExit.position;
        newPlatform.transform.forward = lastPlatform.platformExit.forward;
        newPlatform.transform.SetParent(GetPlatformContainer());
        _platforms.Add(newPlatform);
        
        MoveFinishPlatformToEnd();
    }

    public int GetLastPlatformIndex()
    {
        int result = 0;
        for (int i = _platforms.Count - 1; i >= 0; i--)
        {
            if (_platforms[i] != null && !_platforms[i].isFinishPlatform)
            {
                result = i;
                break;
            }
        }
        return result;
    }

    private int GetFinishPlatformIndex()
    {
        int result = 0;
        for (int i = 0; i < _platforms.Count; i++)
        {
            if (_platforms[i] != null && _platforms[i].isFinishPlatform)
            {
                result = i;
                break;
            }
        }
        return result;
    }

    public void RemoveLast()
    {
        if (_platforms == null || TrackCount == 0 
                               || _startPlatform == null 
                               || _finishPlatform == null)
        {
            return;
        }

        if (_platforms.Count <= 2)
        {
            return;
        }

        int last = GetLastPlatformIndex();
        GameObject.DestroyImmediate(_platforms[last].gameObject);
        _platforms.RemoveAt(last);
        MoveFinishPlatformToEnd();
    }

    private void MoveFinishPlatformToEnd()
    {
        if (_platforms.Count <= 2 || _startPlatform == null || _finishPlatform == null)
        {
            return;
        }
        
        Platform lastPlatform = _platforms[GetLastPlatformIndex()];
        if (lastPlatform != null)
        {
            _finishPlatform.transform.position = lastPlatform.platformExit.position;
            _finishPlatform.transform.forward = lastPlatform.platformExit.forward;

            _platforms.RemoveAt(GetFinishPlatformIndex());
            _platforms.Add(_finishPlatform);
            _finishPlatform.transform.SetAsLastSibling();
        }
    }

    public void AddTrackNode(TrackNodeInfo info)
    {
        BezierKnot knot = new BezierKnot(
            info.position, info.tangentIn, 
            info.tangentOut, info.rotation); 
        spline.Spline.Add(knot);
    }

    public void ClearTrackNodes()
    {
        spline.Spline.Clear();
    }
    
    private Vector3 _pathTargetMoveVel;
    private void Update()
    {
        if (_isPlaying)
        {
            Step();
        }
    }

    public void Play()
    {
        _isPlaying = true;
        StartTime = Time.time;
    }

    public void Pause()
    {
        _isPlaying = false;
    }

    public void Restart()
    {
        Pause();
        SetTrackTime(0.0f);
    }

    public void Step(float amount)
    {
        SetTrackTime(TrackTime + amount);
    }

    private void Step()
    {
        TrackTime += (1.0f / spline.Spline.GetLength()) * trackSpeed * Time.deltaTime;
        GetPathTarget().position = GetTrackPositionAt(TrackTime + pathTargetLead);
        
        // track end
        if (TrackTime >= MAX_TRACK_POSITION)
        {
            TrackTime = MAX_TRACK_POSITION;
            Pause();
            
            if (OnTrackEnd != null)
            {
                OnTrackEnd.Invoke();
            }
        }
    }

    public void SetTrackTime(float time, bool fireTrackEvents = true)
    {
        TrackTime = time;

        // track end
        if (TrackTime >= MAX_TRACK_POSITION)
        {
            TrackTime = MAX_TRACK_POSITION;
            Pause();
            
            if (fireTrackEvents)
            {
                OnTrackEnd?.Invoke();
            }
        }
        
        Vector3 pos = spline.EvaluatePosition(TrackTime + pathTargetLead);
        if (float.IsNaN(pos.x) || float.IsNaN(pos.y) || float.IsNaN(pos.z))
        {
            pos = Vector3.zero;
        }

        GetPathTarget().position = pos;
    }

    private Transform GetPathTarget()
    {
        return _pathPositionTarget.transform;
    }

    public Vector3 GetTrackPosition()
    {
        return GetTrackPositionAt(TrackTime);
    }

    public Vector3 GetTrackPositionAt(float time)
    {
        Vector3 result = spline.EvaluatePosition(time);
        if (float.IsNaN(result.x) || float.IsNaN(result.y) || float.IsNaN(result.z))
        {
            result = Vector3.zero;
        }

        return result;
    }

    public Vector3 GetTrackTangent()
    {
        return ((Vector3) spline.EvaluateTangent(TrackTime)).normalized;
    }

    public Vector3 GetTrackWorldUp()
    {
        return spline.EvaluateUpVector(TrackTime);
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        // Ensure continuous Update calls.
        if (!Application.isPlaying)
        {
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            UnityEditor.SceneView.RepaintAll();
        }
#endif
    }

    private void RefreshCameraGroupTarget()
    {
        if (GameManager.Instance != null
            && GameManager.Instance.playerController != null
            && GameManager.Instance.playerController.surfBlockParent != null)
        {
            Transform parent = GameManager.Instance.playerController.surfBlockParent;
            if (parent != null)
            {
                int count = parent.childCount;
                _pathTargetCineGrp.m_Targets =
                    new CinemachineTargetGroup.Target[count];
                for (int i = 0; i < count; i++)
                {
                    _pathTargetCineGrp.m_Targets[i] = new CinemachineTargetGroup.Target()
                    {
                        target = parent.GetChild(i).transform,
                        radius = 0.2f,
                        weight = 0.1f
                    };
                }
            }
        }
    }
    
    private Transform _platformContainer;
    public Transform GetPlatformContainer()
    {
        if (_platformContainer == null)
        {
            GameObject existing = GameObject.FindGameObjectWithTag("PlatformContainer");
            if (existing != null)
            {
                _platformContainer = existing.transform;
            }
            else
            {
                _platformContainer = new GameObject("_PLATFORMS").transform;
                _platformContainer.gameObject.tag = "PlatformContainer";
            }
        }

        return _platformContainer;
    }
}