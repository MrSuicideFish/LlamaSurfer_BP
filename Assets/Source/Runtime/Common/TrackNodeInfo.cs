using System;
using UnityEngine;

[Serializable]
public class TrackNodeInfo
{
    public float time;
    public bool isCheckpoint;

    public Platform platform;
    
    public TrackNodeInfo(){}
    public TrackNodeInfo(Platform _platform)
    {
        platform = _platform;
    }
}