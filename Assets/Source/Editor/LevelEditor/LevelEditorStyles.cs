using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class LevelEditorStyles
{
    private const int blockSize = 100;

    private static Dictionary<int, Texture2D> _texture_cache = new Dictionary<int, Texture2D>();

    private static Texture2D GetOrCreateTexture(Color color)
    {
        int hashCode = color.GetHashCode();
        if (!_texture_cache.ContainsKey(hashCode))
        {
            Texture2D tex = new Texture2D(blockSize, blockSize);
            Color[] colors = new Color[blockSize * blockSize];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = color;
            }

            tex.SetPixels(0, 0, blockSize, blockSize, colors);
            tex.Apply();
            
            _texture_cache.Add(hashCode, tex);
        }
        
        return _texture_cache[hashCode];
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

    public static GUIStyle GetBuildModeButtonStyle(bool pressed)
    {
        return pressed ? buildModeButtonPressed : buildModeButton;
    }

    public static GUIStyle timelineStyle
    {
        get
        {
            GUIStyle style = new GUIStyle();
            style.normal.background = GetOrCreateTexture(Color.grey * 0.8f);
            return style;
        }
    }

    public static GUIStyle timelineCursorStyle
    {
        get
        {
            GUIStyle style = new GUIStyle();
            style.normal.background = GetOrCreateTexture(new Color(0.5f, 0.5f,1.0f));
            return style;
        }
    }

    public static GUIStyle timelineNotchStyle
    {
        get
        {
            GUIStyle style = new GUIStyle();
            style.normal.background = GetOrCreateTexture(Color.white * 0.5f);
            return style;
        }
    }
}