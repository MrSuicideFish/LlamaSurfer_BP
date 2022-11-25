using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName="Levels/Create Level Config")]
public class LevelCfg : ScriptableObject
{
    public int sceneIndex;
    public int maxPoints;

    [ContextMenu("Populate From Current Scene")]
    public void PopulateFromCurrentScene()
    {
        Scene active = SceneManager.GetActiveScene();
        sceneIndex = active.buildIndex;
        
        PointsOnTouch[] pointObjects = FindObjectsOfType<PointsOnTouch>();
        Debug.Log(pointObjects.Length);
        maxPoints = 0;
        foreach (PointsOnTouch obj in pointObjects)
        {
            maxPoints += obj.value;
        }
    }
}