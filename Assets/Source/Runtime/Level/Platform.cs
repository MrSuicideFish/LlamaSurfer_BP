using UnityEngine;

public class Platform : MonoBehaviour
{
    public enum EPlatformType
    {
        Start,
        Finish,
        Checkpoint,
        Normal
    }
    
    // PREFAB DATA, DO NOT REMOVE
    public Transform platformExit;
    public Transform[] trackPoints;

    public EPlatformType PlatformType;
    //------------------------------------------------------------
}