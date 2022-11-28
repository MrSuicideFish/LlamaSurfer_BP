using UnityEditor;
using UnityEngine;

public class EditorLevelTrackTimeline
{
    private float t = 0.0f;
    private float size = 0.1f;
    private int resolution = 10;
    private float timelineNotchWidth = 4.0f;
    
    public float Draw(float height, float leftValue, float rightValue, float size = 0.1f)
    {
        Rect timelineRect = GUILayoutUtility.GetRect(150, Screen.width, height, height);

        Rect sliderRect = new Rect(timelineRect);
        t = GUI.Slider(sliderRect, t, size, leftValue, rightValue,
            LevelEditorStyles.timelineStyle, LevelEditorStyles.timelineCursorStyle, true, 0);

        for (int i = 1; i <= resolution; i++)
        {
            float perc = i / (float)resolution;

            Rect r = new Rect(timelineRect);
            r.x += timelineRect.width * perc;
            r.width = timelineNotchWidth;

            GUI.Box(r, "", LevelEditorStyles.timelineNotchStyle);
        }

        return t;
    }


}