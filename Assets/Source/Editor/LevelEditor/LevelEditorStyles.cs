using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class LevelEditorStyles
{
    private const int blockSize = 100;

    private static Dictionary<int, Texture2D> _texture_cache = new Dictionary<int, Texture2D>();

    public static Texture2D GetOrCreateTexture(Color color)
    {
        Texture2D tex = new Texture2D(blockSize, blockSize);
        Color[] colors = new Color[blockSize * blockSize];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = color;
        }

        tex.SetPixels(0, 0, blockSize, blockSize, colors);
        tex.Apply();

        return tex;
    }

    public static GUIStyle buildModeButton
    {
        get
        {
            GUIStyle style = GUI.skin.button;
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 13;
            return style;
        }
    }
    
    public static GUIStyle buildModeButtonPressed
    {
        get
        {
            GUIStyle style = GUI.skin.button;
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 13;
            return style;
        }
    }

    public static GUIStyle GetToggleButtonStyle(bool pressed)
    {
        return pressed ? buildModeButtonPressed : buildModeButton;
    }
    
    public static GUIStyle editorConsoleText
    {
        get
        {
            GUIStyle style = GUI.skin.label;
            style.normal.textColor = Color.blue;
            return style;
        }
    }
}