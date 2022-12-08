using UnityEngine;
using float3 = Unity.Mathematics.float3;

public struct TrackNodeTanInfo
{
    public Vector3 position;
    public float3 tangentIn;
    public float3 tangentOut;
    public Quaternion rotation;

    public TrackNodeTanInfo(Vector3 pos, Quaternion rot, float3 tangentIn, float3 tangentOut)
    {
        position = pos;
        rotation = rot;
        
        this.tangentIn = tangentIn;
        this.tangentOut = tangentOut;

    }
}