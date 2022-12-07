using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;


[ExecuteAlways]
public class TrackController : MonoBehaviour
{
    public const float MIN_TRACK_POSITION = 0.0001f;
    public const float MAX_TRACK_POSITION = 0.9999f;
    
    public SplineContainer spline;
    public Grid _grid;
    
    // platforms
    public List<TrackNodeInfo> _platforms;
    public Platform _startPlatform;
    public Platform _finishPlatform;
    public float[] _checkpoints;
    
    public UnityEvent OnTrackStart;
    public UnityEvent OnTrackEnd;
    
    // cache
    private Vector3 _trackPosition = Vector3.zero;
    private Vector3 _trackTangent = Vector3.zero;
    private Vector3 _trackWorldUp = Vector3.zero;
    
    public int TrackCount
    {
        get
        {
            if (_platforms == null)
            {
                return 0;
            }
            
            return _platforms.Count;
        }
    }

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