
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName="Levels/Create Level Config DB")]
public class LevelCfgDb : ScriptableObject
{
    private static LevelCfgDb instance;
    public static LevelCfgDb Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<LevelCfgDb>("Database/LevelCfgDB");
            }
            return instance;
        }
    }
    
    public LevelCfg[] levelsCfgs;

    public static LevelCfg GetCurrentLevel()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        foreach (LevelCfg cfg in Instance.levelsCfgs)
        {
            if (activeScene.buildIndex == cfg.sceneIndex)
            {
                return cfg;
            }
        }

        return null;
    }

    public static LevelCfg GetLevelByBuildIndex(int sceneIndex)
    {
        foreach (LevelCfg cfg in instance.levelsCfgs)
        {
            if (cfg.sceneIndex == sceneIndex)
            {
                return cfg;
            }
        }

        return null;
    }
}