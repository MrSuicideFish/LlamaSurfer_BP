using UnityEditor;
using UnityEngine;

/// <summary>
/// Linked-list platforms
/// </summary>
public class Platform : MonoBehaviour
{
    [HideInInspector] public int _id;
    
    public Transform platformExit;
    public Transform[] trackPoints;

    public bool isStartPlatform;
    public bool isFinishPlatform;

    /*
    #if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        if (trackPoints != null && trackPoints.Length > 0)
        {
            for (int i = 0; i < trackPoints.Length; i++)
            {
                Transform point = trackPoints[i];
                if (point != null)
                {
                    EditorGUI.BeginChangeCheck();
                    Quaternion rot = Handles.RotationHandle(point.rotation, Vector3.zero);
                    Vector3 pos = Handles.PositionHandle(point.position, rot);

                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(point, "Modified track point");
                        
                    }

                    //Handles.DrawWireCube(point.position, Vector3.one);
                    //Handles.ArrowHandleCap(0, point.position, point.rotation, 2.0f, EventType.Repaint);
                }
            }
        }
    }
    #endif
    */
}