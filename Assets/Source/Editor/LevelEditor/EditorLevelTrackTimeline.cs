using UnityEditor;
using UnityEngine;

public class EditorLevelTrackTimeline
{
    private int resolution = 25;
    private float cursorSize = 0.1f;
    private float timelineNotchWidth = 2.0f;
    private float height = 15;

    private float leftValue, rightValue;

    public GUIStyle timelineStyle;
    public GUIStyle timelineCursorStyle;
    public GUIStyle timelineNotchStyle;

    private Color timelineColor;
    private Color timelineCursorColor;
    private Color timelineNotchColor;

    public EditorLevelTrackTimeline(int resolution, float leftValue, float rightValue, float height,
        Color timelineColor, Color timelineCursorColor, Color timelineNotchColor)
    {
        this.resolution = resolution;
        this.height = height;
        this.leftValue = leftValue;
        this.rightValue = rightValue;

        this.timelineColor = timelineColor;
        this.timelineCursorColor = timelineCursorColor;
        this.timelineNotchColor = timelineNotchColor;
        InitGUI();
    }

    private void InitGUI()
    {
        cursorSize = Mathf.Lerp(leftValue, rightValue, 1.0f / resolution);
        timelineStyle = new GUIStyle();
        timelineStyle.normal.background = LevelEditorStyles.GetOrCreateTexture(timelineColor);
        
        timelineCursorStyle = new GUIStyle();
        timelineCursorStyle.normal.background = LevelEditorStyles.GetOrCreateTexture(timelineCursorColor);
        
        timelineNotchStyle = new GUIStyle();
        timelineNotchStyle.normal.background = LevelEditorStyles.GetOrCreateTexture(timelineNotchColor);
    }
    
    public float Draw(float value)
    {
        InitGUI();
        
        Rect timelineRect = GUILayoutUtility.GetRect(150, Screen.width, height, height);

        Rect sliderRect = new Rect(timelineRect);
        value = GUI.Slider(sliderRect, value, cursorSize, leftValue, rightValue,
            timelineStyle, timelineCursorStyle, true, 0);

        for (int i = 1; i <= resolution; i++)
        {
            float perc = i / (float)resolution;

            Rect r = new Rect(timelineRect);
            r.x += timelineRect.width * perc;
            r.width = timelineNotchWidth;

            GUI.Box(r, "", timelineNotchStyle);
        }

        return value;
    }
}