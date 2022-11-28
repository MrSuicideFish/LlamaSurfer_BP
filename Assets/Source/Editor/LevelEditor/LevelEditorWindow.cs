using System;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

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
            if (GUILayout.Toggle(LevelEditor.BuildMode == EBuildMode.Platform, "Platform", 
                    LevelEditorStyles.GetBuildModeButtonStyle(LevelEditor.BuildMode == EBuildMode.Platform)))
            {
                LevelEditor.BuildMode = EBuildMode.Platform;
                if (OnBuildModeChanged != null)
                {
                    OnBuildModeChanged.Invoke(EBuildMode.Platform);
                }
            }
            
            if (GUILayout.Toggle(LevelEditor.BuildMode == EBuildMode.Entity, "Entity",
                    LevelEditorStyles.GetBuildModeButtonStyle(LevelEditor.BuildMode == EBuildMode.Entity)))
            {
                LevelEditor.BuildMode = EBuildMode.Entity;
                if (OnBuildModeChanged != null)
                {
                    OnBuildModeChanged.Invoke(EBuildMode.Entity);
                }
            }
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
    
    private void DrawObjectWindow()
    {
        if (GUILayout.Button("Rebuild"))
        {
            if (OnRebuildTrack != null)
            {
                OnRebuildTrack.Invoke();
            }
        }
        
        if (GameSystem.GetTrackController().TrackCount > 2)
        {
            if (GUILayout.Button("Delete"))
            {
                if (OnDeletePlatform != null)
                {
                    OnDeletePlatform.Invoke();
                }
            }
        }
        
        using (new GUILayout.VerticalScope())
        {
            int selection = GUILayout.SelectionGrid(LevelEditor.BuildMode.GetHashCode(), _buildModeIcons, 1, GUILayout.Width(60), GUILayout.Height(100));
            if (selection != LevelEditor.BuildMode.GetHashCode())
            {
                LevelEditor.BuildMode = (EBuildMode) selection;
                OnBuildModeChanged.Invoke((EBuildMode) selection);
            }
        }
        
        switch (LevelEditor.BuildMode)
        {
            case EBuildMode.Entity:
                DrawEntitiesPanel();
                break;
            case EBuildMode.Platform:
                DrawPlatformsPanel();
                break;
        }
    }

    private void DrawEntitiesPanel()
    {
        int selection = GUILayout.SelectionGrid(LevelEditor.PaintMode.GetHashCode(), _paintModeIcons, 1);
        if (selection != LevelEditor.PaintMode.GetHashCode())
        {
            LevelEditor.PaintMode = (EPaintMode) selection;
            OnPaintModeChanged.Invoke((EPaintMode)selection);
        }
            
        if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("d_TreeEditor.Trash"))))
        {
            if (OnEraseAllEntities != null)
            {
                OnEraseAllEntities.Invoke();
            }
        }
        
        using (new GUILayout.ScrollViewScope(_entityViewScrollPos))
        {
            for (int i = 0; i < _objectRepo.Length; i++)
            {
                if (GUILayout.Button(_objectThumbnails[i], GUILayout.Width(100), GUILayout.Height(100)))
                {
                    OnPaintEntityChanged.Invoke(_objectRepo[i]);
                }
            }
        }

    }
    
    private void DrawPlatformsPanel()
    {
        for (int i = 0; i < _platformRepo.Length; i++)
        {
            if (GUILayout.Button(_platformThumbnails[i], GUILayout.Width(100), GUILayout.Height(100)))
            {
                if (OnAddPlatform != null)
                {
                    OnAddPlatform.Invoke(_platformRepo[i]);
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
