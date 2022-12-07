using System.Linq;
using UnityEditor;
using UnityEngine;

public class LevelEditorWindow : EditorWindow
{
    private Texture[] _buildModeIcons;
    private Texture[] _paintModeIcons;
    public LevelEditorEvent OnRebuildTrack;
    public LevelEditorEvent<EBuildMode> OnBuildModeChanged;
    
    // Entities
    public LevelEditorEvent OnEraseAllEntities;
    public LevelEditorEvent<EPaintMode> OnPaintModeChanged;
    public LevelEditorEvent<WorldObjectBase> OnPaintEntityChanged;

    private WorldObjectBase[] _objectRepo;
    private Texture2D[] _objectThumbnails;
    private Vector2 _entityViewScrollPos;
    
    // Platforms
    public LevelEditorEvent<Platform> OnAddPlatform;
    public LevelEditorEvent OnDeletePlatform;
    
    private Platform[] _platformRepo;
    private Texture2D[] _platformThumbnails;
    private Vector2 _platformViewScrollPos;
    
    private void OnGUI()
    {
        using (new GUILayout.HorizontalScope())
        {
            if (GUILayout.Toggle(LevelEditor.BuildMode == EBuildMode.Platform, 
                    EditorGUIUtility.IconContent("d_ToggleUVOverlay@2x"),
                    LevelEditorStyles.GetToggleButtonStyle(LevelEditor.BuildMode == EBuildMode.Platform),
                    GUILayout.Height(64)))
            {
                LevelEditor.BuildMode = EBuildMode.Platform;
                if (OnBuildModeChanged != null)
                {
                    OnBuildModeChanged.Invoke(EBuildMode.Platform);
                }
            }
            
            if (GUILayout.Toggle(LevelEditor.BuildMode == EBuildMode.Entity, 
                    EditorGUIUtility.IconContent("d_LightmapEditor.WindowTitle@2x"),
                    LevelEditorStyles.GetToggleButtonStyle(LevelEditor.BuildMode == EBuildMode.Entity),
                    GUILayout.Height(64)))
            {
                LevelEditor.BuildMode = EBuildMode.Entity;
                if (OnBuildModeChanged != null)
                {
                    OnBuildModeChanged.Invoke(EBuildMode.Entity);
                }
            }
        }

        GUILayout.Space(20);

        switch (LevelEditor.BuildMode)
        {
            case EBuildMode.Platform:
                DrawPlatformsPanel();
                break;
            case EBuildMode.Entity:
                DrawEntitiesPanel();
                break;
        }
    }

    private void OnEnable()
    {
        
        OnRebuildTrack = new LevelEditorEvent();
        OnBuildModeChanged = new LevelEditorEvent<EBuildMode>();
        OnEraseAllEntities = new LevelEditorEvent();
        OnPaintModeChanged = new LevelEditorEvent<EPaintMode>();
        OnPaintEntityChanged = new LevelEditorEvent<WorldObjectBase>();
        OnAddPlatform = new LevelEditorEvent<Platform>();
        OnDeletePlatform = new LevelEditorEvent();
        
        ReloadObjectRepo();
        ReloadPlatformRepo();
        ReloadGraphicIcons();
    }

    private int selectedEntity = 0;
    private void DrawEntitiesPanel()
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Toggle(LevelEditor.PaintMode == EPaintMode.Paint, _paintModeIcons[0], 
                    LevelEditorStyles.GetToggleButtonStyle(LevelEditor.PaintMode == EPaintMode.Paint),
                    GUILayout.Height(64), GUILayout.Width(150)))
            {
                if (LevelEditor.PaintMode != EPaintMode.Paint)
                {
                    LevelEditor.PaintMode = EPaintMode.Paint;
                    OnPaintModeChanged.Invoke(EPaintMode.Paint);
                }
            }
            if (GUILayout.Toggle(LevelEditor.PaintMode == EPaintMode.Erase, _paintModeIcons[1], 
                    LevelEditorStyles.GetToggleButtonStyle(LevelEditor.PaintMode == EPaintMode.Erase),
                    GUILayout.Height(64), GUILayout.Width(150)))
            {
                if (LevelEditor.PaintMode != EPaintMode.Erase)
                {
                    LevelEditor.PaintMode = EPaintMode.Erase;
                    OnPaintModeChanged.Invoke(EPaintMode.Erase);
                }
            }
            GUILayout.FlexibleSpace();
        }
        
        GUILayout.Space(15);
        
        using (new GUILayout.ScrollViewScope(_entityViewScrollPos, GUI.skin.box))
        {
            using (new GUILayout.HorizontalScope())
            {
                for (int i = 0; i < _objectRepo.Length; i++)
                {
                    if (GUILayout.Toggle(selectedEntity == i, 
                        _objectThumbnails[i], LevelEditorStyles.GetToggleButtonStyle(selectedEntity == i),
                    GUILayout.Width(100), GUILayout.Height(100)))
                    {
                        selectedEntity = i;
                        OnPaintEntityChanged.Invoke(_objectRepo[i]);
                    }
                }
            }
            GUILayout.FlexibleSpace();
        }
        GUIContent deleteAllContent = new GUIContent(EditorGUIUtility.IconContent("d_winbtn_mac_close_a@2x"));
        deleteAllContent.text = "Delete All Objects";
        if (GUILayout.Button(deleteAllContent, GUILayout.Height(64)))
        {
            if (OnEraseAllEntities != null)
            {
                OnEraseAllEntities.Invoke();
            }
        }
    }

    private void DrawPlatformsPanel()
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_Refresh@2x"),
                    GUILayout.Height(64), GUILayout.Width(150)))
            {
                ReloadPlatformRepo();
                ReloadObjectRepo();
                if (OnRebuildTrack != null)
                {
                    OnRebuildTrack.Invoke();
                }
            }
        
            if (GameSystem.GetTrackController().TrackCount > 2)
            {
                if (GUILayout.Button(EditorGUIUtility.IconContent("P4_DeletedLocal@2x"), 
                        GUILayout.Height(64), GUILayout.Width(150)))
                {
                    if (OnDeletePlatform != null)
                    {
                        OnDeletePlatform.Invoke();
                    }
                }
            }
            GUILayout.FlexibleSpace();
        }
        
        GUILayout.Space(15);
        using (new GUILayout.ScrollViewScope(_platformViewScrollPos, "HelpBox"))
        {
            using (new GUILayout.HorizontalScope())
            {
                for (int i = 0; i < _platformRepo.Length; i++)
                {
                    Platform p = _platformRepo[i];
                    using (new GUILayout.VerticalScope())
                    {
                        if (GUILayout.Button(_platformThumbnails[i], GUILayout.Width(100), GUILayout.Height(100)))
                        {
                            if (OnAddPlatform != null)
                            {
                                OnAddPlatform.Invoke(p);
                            }
                        }

                        string platformName = p.gameObject.name;
                        GUILayout.Label(platformName);
                    }

                }
            }
        }
    }

    private void ReloadGraphicIcons()
    {
        _paintModeIcons = new Texture[]
        {
            EditorGUIUtility.IconContent("d_Grid.PaintTool@2x").image,
            EditorGUIUtility.IconContent("d_Grid.EraserTool@2x").image
        };
            
        _buildModeIcons = new Texture[]
        {
            EditorGUIUtility.IconContent("d_GameObject Icon").image,
            EditorGUIUtility.IconContent("d_Light Icon").image
        };
    }
    
    private void ReloadObjectRepo()
    {
        _objectRepo = Resources.LoadAll<WorldObjectBase>("Level/Objects");
        _objectThumbnails = new Texture2D[_objectRepo.Length];
        for (int i = 0; i < _objectRepo.Length; i++)
        {
            _objectThumbnails[i] = AssetPreview.GetAssetPreview(_objectRepo[i].gameObject);
        }
    }

    private void ReloadPlatformRepo()
    {
        _platformRepo = Resources.LoadAll<Platform>("Level/Platforms")
            .Where(x => !x.isStartPlatform && !x.isFinishPlatform).ToArray();
        _platformThumbnails = new Texture2D[_platformRepo.Length];
        for (int i = 0; i < _platformRepo.Length; i++)
        {
            _platformThumbnails[i] = AssetPreview.GetAssetPreview(_platformRepo[i].gameObject);
        }
    }
}
