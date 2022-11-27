using UnityEngine;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

public class CameraProperties : ScriptableObject
{
    private const string CameraPropertiesPath = "Data/CameraProperties";
    public static CameraProperties Get()
    {
        CameraProperties res = Resources.Load<CameraProperties>(CameraPropertiesPath);
        
#if UNITY_EDITOR
        if (res == null)
        {
            string resPath = Path.Combine(Application.dataPath, "Resources", "Data");
            if (!Directory.Exists(resPath))
            {
                Directory.CreateDirectory(resPath);
            }
            res = ScriptableObject.CreateInstance<CameraProperties>();
            AssetDatabase.CreateAsset(res, Path.Combine("Assets", "Resources", "Data", "CameraProperties.asset" ));
        }
#endif

        return res;
    }
    
    #if UNITY_EDITOR
    [MenuItem("Broken Pixel/Open Camera Properties")]
    private static void OpenCameraProperties()
    {
        EditorGUIUtility.PingObject(Get());
        Selection.activeObject = Get();
    }
    #endif
    
    [Header("Body")]
    public float fieldOfView = 60.0f;

    public float verticalArmLength = 0.4f;
    [Range(-1.0f, 1.0f)] public float cameraSide = 1.0f;
    public float cameraDistance = 2.0f;
    
    public Vector3 bodyOffset = Vector3.zero;
    public Vector3 bodyDamping = Vector3.zero;
    
    [Header("Aim")]
    public Vector3 aimOffset = Vector3.zero;

}