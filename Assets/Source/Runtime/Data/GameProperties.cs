using UnityEngine;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

public class GameProperties : ScriptableObject
{
    private const string GamePropertiesPath = "Data/GameProperties";
    public static GameProperties Get()
    {
        GameProperties res = Resources.Load<GameProperties>(GamePropertiesPath);
        
#if UNITY_EDITOR
        if (res == null)
        {
            string resPath = Path.Combine(Application.dataPath, "Resources", "Data");
            if (!Directory.Exists(resPath))
            {
                Directory.CreateDirectory(resPath);
            }
            res = ScriptableObject.CreateInstance<GameProperties>();
            AssetDatabase.CreateAsset(res, Path.Combine("Assets", "Resources", "Data", "GameProperties.asset" ));
        }
#endif

        return res;
    }

    public float trackPlaybackSpeed = 15.0f;
}