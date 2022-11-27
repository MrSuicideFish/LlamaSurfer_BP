using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;

[ExecuteAlways]
public class TrackController : MonoBehaviour
{
    private const float MIN_TRACK_POSITION = 0.0001f;
    private const float MAX_TRACK_POSITION = 0.9999f;
    
    public SplineContainer spline;
    public Grid _grid;
    
    // platforms
    public List<Platform> _platforms;
    public Platform _startPlatform;
    public Platform _finishPlatform;
    
    public UnityEvent OnTrackStart;
    public UnityEvent OnTrackEnd;
    
    // cache
    private Vector3 _trackPosition;
    private Vector3 _trackTangent;
    private Vector3 _trackWorldUp;

    // playback
    private float _trackTime;
    public float TrackTime
    {
        get => _trackTime;
        set
        {
            _trackTime = Mathf.Clamp(value, MIN_TRACK_POSITION, MAX_TRACK_POSITION);
            _trackPosition = spline.EvaluatePosition(_trackTime);
            if (float.IsNaN(_trackPosition.x) || float.IsNaN(_trackPosition.y) || float.IsNaN(_trackPosition.z))
            {
                _trackPosition = Vector3.zero;
            }
        
            _trackTangent = ((Vector3) spline.EvaluateTangent(_trackTime)).normalized;
            _trackWorldUp = spline.EvaluateUpVector(_trackTime);
        }
    }
    
    private bool _isPlaying;
    public bool IsPlaying => _isPlaying;
    public bool HasEnded => TrackTime >= 1.0f && !_isPlaying;
    public float StartTime { get; private set; }
    public float EndTime { get; private set; }

    public void OnEnable()
    {
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

#if UNITY_EDITOR
        if (_platforms == null || _platforms.Count == 0)
        {
            _platforms = new List<Platform>(
                GetPlatformContainer().GetComponentsInChildren<Platform>());
        }
        
        MoveFinishPlatformToEnd();
#endif
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
        if (OnTrackStart != null)
        {
            OnTrackStart.Invoke();
        }
    }

    public void Pause()
    {
        _isPlaying = false;
    }

    public void End()
    {
        TrackTime = MAX_TRACK_POSITION;
        Pause();
        EndTime = Time.time;
        if (OnTrackEnd != null)
        {
            OnTrackEnd.Invoke();
        }
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
        TrackTime += (1.0f / spline.Spline.GetLength()) * GameProperties.Get().trackPlaybackSpeed * Time.deltaTime;
        if (TrackTime >= MAX_TRACK_POSITION)
        {
            End();
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
    
    public Vector3 GetTrackPosition()
    {
        return _trackPosition;
    }

    public Vector3 GetTrackTangent()
    {
        return _trackTangent;
    }

    public Vector3 GetTrackWorldUp()
    {
        return _trackWorldUp;
    }

    
    
#if UNITY_EDITOR
    public int TrackCount
    {
        get
        {
            return GetPlatformContainer().childCount;
        }
    }
    
    public void OnValidate()
    {
        /*
        for (int i = 0; i < _platforms.Count; i++)
        {
            if (_platforms[i] == null)
            {
                _platforms.RemoveAt(i);
            }
        }*/

        MoveFinishPlatformToEnd();
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
            MoveFinishPlatformToEnd();
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
    
    private void OnDrawGizmos()
    {
        // Ensure continuous Update calls.
        if (!Application.isPlaying)
        {
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            UnityEditor.SceneView.RepaintAll();
        }
    }
#endif
}