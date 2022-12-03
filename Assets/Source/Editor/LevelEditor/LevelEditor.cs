using System;
using System.Collections.Generic;
using System.Text;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.Overlays;
using UnityEditor.SceneManagement;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public enum EPaintMode
{
    Paint,
    Erase
}

public enum EBuildMode
{
    Platform,
    Entity
}

[EditorTool("Level Editor", typeof(TrackController))]
public class LevelEditor : EditorTool
{
    private LevelEditorWindow _editorWindow;
    private Vector3 _paintPosition;
    private WorldObjectLayer _objectContainer;
    private WorldObjectBase _objectToPaint;
    private EditorLevelTrackTimeline _timeline;
    private EditorLevelTrackTimeline _playerX_Slider;
    
    public static EPaintMode PaintMode { get; set; }
    public static EBuildMode BuildMode { get; set; }

    [MenuItem("Broken Pixel/Open Level Editor")]
    private static void OpenLevelEditor()
    {
        TrackController trackInst = TrackController.FindObjectOfType<TrackController>();
        if (trackInst == null)
        {
            Selection.SetActiveObjectWithContext(
                GameSystem.GetTrackController(), GameSystem.GetTrackController());
        }
    }

    public override void OnActivated()
    {
        if (Application.isPlaying) return;
        if (GameSystem.GetTrackController().TrackCount == 0)
        {
            BuildStartPlatform();
            BuildFinishPlatform();
        }
        
        Rebuild();
        SceneView.lastActiveSceneView?.ShowNotification(new GUIContent("Entering Level Editor"), .1f);

        playerXPosition = Mathf.Clamp01(GameSystem.GetPlayer().moveX);
        System.Type inspectorType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow");
        
        // initialize editor window
        _editorWindow = EditorWindow.GetWindow<LevelEditorWindow>(desiredDockNextTo: inspectorType);
        _editorWindow.OnEraseAllEntities.AddListener(GetObjectLayer().EraseAll);
        _editorWindow.OnRebuildTrack.AddListener(Rebuild);
        _editorWindow.OnPaintEntityChanged.AddListener((obj) => { _objectToPaint = obj; });
        _editorWindow.OnAddPlatform.AddListener(BuildNextPlatform);
        _editorWindow.OnDeletePlatform.AddListener(RemoveLastPlatform);

        
        Color timelineColor = new Color();
        timelineColor.r = 0.1f;
        timelineColor.g = 0.1f;
        timelineColor.b = 0.15f;
        timelineColor.a = 1.0f;

        Color cursorColor = new Color();
        cursorColor.r = 0.8f;
        cursorColor.g = 0.0f;
        cursorColor.b = 0.4f;
        cursorColor.a = 1.0f;

        Color notchColor = new Color();
        notchColor.r = 0.2f;
        notchColor.g = 0.2f;
        notchColor.b = 0.25f;
        notchColor.a = 1.0f;
        
        _timeline = new EditorLevelTrackTimeline(25, 
            0.0f, 1.0f,
            25, timelineColor, cursorColor, notchColor);

        timelineColor.r *= 0.7f;
        timelineColor.g *= 0.7f;
        timelineColor.b *= 0.7f;
        cursorColor.g = 0.3f;
        cursorColor.b = 0.4f;
        notchColor.r *= 1.5f;
        notchColor.g *= 1.5f;
        notchColor.b *= 1.5f;
        _playerX_Slider = new EditorLevelTrackTimeline(10, 
            0.0f, 1.0f,
            25, timelineColor, cursorColor, notchColor);
    }

    private void OnDisable()
    {
        if (_editorWindow != null)
        {
            _editorWindow.Close();
        }
    }

    // The second "context" argument accepts an EditorWindow type.
    [Shortcut("Open Level Editor", typeof(SceneView), KeyCode.P)]
    static void LevelEditorShortcut()
    {
        if (Selection.GetFiltered<TrackController>(SelectionMode.TopLevel).Length > 0)
            ToolManager.SetActiveTool<LevelEditor>();
        else
            Debug.Log("No track selected!");
    }

    public override void OnToolGUI(EditorWindow window)
    {
        if (!(window is SceneView sceneView))
            return;

        if (_playerX_Slider == null
            || _timeline == null
            || !GameSystem.IsInitialized()) return;

        sceneView.sceneViewState.alwaysRefresh = true;
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        DrawWorldGizmos();

        // Handle mouse position query
        // & adding/removing entities on mouse down
        // Only do this if the mouse isn't too far
        // towards the bottom of the screen.
        float mouseY = Event.current.mousePosition.y;
        float screenHeight = sceneView.camera.pixelRect.height;
        if (mouseY < screenHeight - 50)
        {
            HandleMouseControl(sceneView);
        }

        Handles.BeginGUI();
        GUILayout.FlexibleSpace();
        using (new GUILayout.HorizontalScope(LevelEditorStyles.editorConsoleText))
        {
            GUILayout.Label(string.Format("Time: {0}", GameSystem.GetTrackController().TrackTime));
            GUILayout.Label(string.Format("TT Pos: {0}", GameSystem.GetTrackController().GetTrackPosition()));
            GUILayout.Label(string.Format("TT Tan: {0}", GameSystem.GetTrackController().GetTrackTangent()));
            GUILayout.Label(string.Format("TT World Up: {0}", GameSystem.GetTrackController().GetTrackWorldUp()));
            GUILayout.FlexibleSpace();
        }

        float newMoveX = _playerX_Slider.Draw(playerXPosition);
        if (!playerXPosition.Equals(newMoveX))
        {
            playerXPosition = newMoveX;
            GameSystem.GetPlayer().moveX = playerXPosition;
        }

        GameSystem.GetTrackController().TrackTime = _timeline.Draw(GameSystem.GetTrackController().TrackTime);
        Handles.EndGUI();

        // move scene camera to the updated game camera's orientation
        // game camera executes in edit mode and uses in-game properties for pos/rot offset from the path.
        // instead of emulating this we take the game camera's orientation. NOTE: FOV/Effects will differ.
        TeleportSceneCamera(Camera.main.transform.position,
            GameSystem.GetPlayer().transform.position - Camera.main.transform.position);
    }
    
    private void DrawTrackWindow()
    {
        using (new GUILayout.VerticalScope(GUILayout.MaxWidth(200)))
        {
            EditorGUI.BeginChangeCheck();

            DrawTrackPositionSlider();
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(GameSystem.GetTrackController().GetPlatformContainer(), "change platforms");
                Undo.RecordObject(GameSystem.GetTrackController(), "change track");
            }
        }
    }
    
    private float playerXPosition;
    private void DrawTrackPositionSlider()
    {
        const float slider_space = 15;
        
        GUILayout.Label($"TrackPosition ({GameSystem.GetTrackController().TrackTime})");
        GUILayout.Space(slider_space);
        
        float newTrackTime = GUILayout.HorizontalSlider(GameSystem.GetTrackController().TrackTime, 0.0f, 1.0f);
        if (!GameSystem.GetTrackController().TrackTime.Equals(newTrackTime))
        {
            GameSystem.GetTrackController().SetTrackTime(newTrackTime, fireTrackEvents: false);
        }

        GUILayout.Space(slider_space);
        
        float newMoveX = GUILayout.HorizontalSlider(playerXPosition, 0.0f, 1.0f);
        if (!playerXPosition.Equals(newMoveX))
        {
            playerXPosition = newMoveX;
            GameSystem.GetPlayer().moveX = playerXPosition;
        }
        
        GUILayout.Space(slider_space);
        using (new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button("<<")) GameSystem.GetTrackController().Step(-(1.0f / GameSystem.GetTrackController().spline.Spline.GetLength()));
            if (GUILayout.Button(">>")) GameSystem.GetTrackController().Step((1.0f / GameSystem.GetTrackController().spline.Spline.GetLength()));
        }
    }
    
    private void DrawWorldGizmos()
    {
        const float gizmoThickness = 6.0f;
        
        Vector3 trackWorldPos = GameSystem.GetTrackController().GetTrackPosition();
        Vector3 trackTangent = GameSystem.GetTrackController().GetTrackTangent();
        Vector3 worldUp = GameSystem.GetTrackController().GetTrackWorldUp();

        Color oldColor = Handles.color;

        Handles.color = Color.green;
        
        // tangent
        Handles.DrawLine(trackWorldPos, trackWorldPos + trackTangent, gizmoThickness);

        // world up
        Handles.color = Color.red;
        Handles.DrawLine(trackWorldPos, trackWorldPos + worldUp, gizmoThickness/2.0f);
        // reset handles color
        Handles.color = oldColor;
    }

    private WorldObjectLayer GetObjectLayer()
    {
        if (_objectContainer == null)
        {
            GameObject existing = GameObject.FindGameObjectWithTag("WorldObjectLayer");
            if (existing != null)
            {
                _objectContainer = existing.GetComponent<WorldObjectLayer>();
            }
            else
            {
                _objectContainer = new GameObject("_OBJECTS").AddComponent<WorldObjectLayer>();
                _objectContainer.gameObject.tag = "WorldObjectLayer";
            }
        }

        return _objectContainer;
    }
    
    #region Platforms
    public void BuildStartPlatform()
    {
        const string startPlatformPath = "Level/Platforms/Platform_Start";
        Object platformRes = Resources.Load<Platform>(startPlatformPath);
        if (platformRes != null)
        {
            Platform p = PrefabUtility.InstantiatePrefab(platformRes) as Platform;
            GameSystem.GetTrackController().AddStartPlatform(p);
            Rebuild();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }

    public void BuildFinishPlatform()
    {
        const string startPlatformPath = "Level/Platforms/Platform_Finish";
        Object platformRes = Resources.Load<Platform>(startPlatformPath);
        if (platformRes != null)
        {
            Platform p = PrefabUtility.InstantiatePrefab(platformRes) as Platform;
            GameSystem.GetTrackController().AddFinishPlatform(p);
            Rebuild();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }
    
    public void BuildNextPlatform(Platform platform)
    {
        Platform newPlatform = PrefabUtility.InstantiatePrefab(platform) as Platform;
        if (newPlatform != null)
        {
            GameSystem.GetTrackController().AddPlatform(newPlatform);
            Rebuild();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }

    public void RemoveLastPlatform()
    {
        GameSystem.GetTrackController().RemoveLast();
        Rebuild();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }
    #endregion

    private void Rebuild()
    {
        GameSystem.GetTrackController().ClearTrackNodes();
        
        for (int i = 0; i < GameSystem.GetTrackController()._platforms.Count; i++)
        {
            Platform platform = GameSystem.GetTrackController()._platforms[i];
            
            if (platform != null)
            {
                Vector3 lastPos = Vector3.zero;
                Vector3 currentPos = Vector3.zero;
                Vector3 nextPos = Vector3.zero;
                Quaternion currentRotation = Quaternion.identity;

                for (int j = 0; j < platform.trackPoints.Length; j++)
                {
                    Transform currentTP = platform.trackPoints[j];
                    bool isLast = (j == platform.trackPoints.Length - 1);
                
                    // current
                    currentPos = currentTP.position;
                    currentRotation = currentTP.rotation;
                
                    // last/next
                    nextPos = !isLast ? platform.trackPoints[j + 1].position : currentPos;
                    lastPos = (j - 1 >= 0) ? platform.trackPoints[j - 1].position : currentPos;

                    TrackNodeInfo newTrackNode = new TrackNodeInfo(
                        currentPos, currentRotation, float3.zero, float3.zero);
                    GameSystem.GetTrackController().AddTrackNode(newTrackNode);
                }   
            }
        }
    }

    private void HandleMouseControl(SceneView sceneView)
    {
        if (BuildMode != EBuildMode.Entity) return;
        
        var worldSamplePos = SampleMousePosition();

        float size = 0.5f;
        DrawPaintPreviewGridRect(worldSamplePos, size,
            PaintMode == EPaintMode.Paint ? Color.blue : Color.red,
            Color.black);
        
        // paint / erase
        if ((Event.current.type == EventType.MouseDown 
             || Event.current.type == EventType.MouseDrag) && Event.current.button == 0)
        {
            if (PaintMode == EPaintMode.Paint)
            {
                if (!ObjectExistsAtPosition(worldSamplePos))
                {
                    AddWorldObject(worldSamplePos, Vector3.forward);
                }
                else if(_objectToPaint.canStack)
                {
                    StackWorldObject(worldSamplePos, Vector3.forward);
                }
            }
            else if (PaintMode == EPaintMode.Erase)
            {
                RemoveWorldObject(worldSamplePos);
            }
        }
    }

    private void DrawPaintPreviewGridRect(Vector3 position, float size, Color faceColor, Color outlineColor)
    {
        PaintGridRect(position, size, faceColor, outlineColor);
    }

    private void PaintGridRect(Vector3 position, float size, Color faceColor, Color outlineColor)
    {
        Vector3[] verts = new Vector3[]
        {
            new Vector3(position.x - size, position.y, position.z - size),
            new Vector3(position.x - size, position.y, position.z + size),
            new Vector3(position.x + size, position.y, position.z + size),
            new Vector3(position.x + size, position.y, position.z - size)
        };
        
        Handles.DrawSolidRectangleWithOutline(verts, faceColor, outlineColor);
    }

    private Vector3 SampleMousePosition()
    {
        if (GameSystem.GetTrackController() == null || GameSystem.GetTrackController()._grid == null)
            return Vector3.zero;
        
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        Plane drawPlane = new Plane(Vector3.up, new Vector3(0, 0, 0));
        drawPlane.Raycast(ray, out float enter);
        var point = ray.GetPoint(enter);
        Vector3Int samplePosInt =
            new Vector3Int(Mathf.RoundToInt(point.x-0.5f), Mathf.RoundToInt(point.y), Mathf.RoundToInt(point.z-0.5f));

        return GameSystem.GetTrackController()._grid.GetCellCenterWorld(samplePosInt);
    }

    #region Entity
    private void AddWorldObject(Vector3 position, Vector3 rotation)
    {
        if (_objectToPaint != null)
        {
            string id = _objectToPaint.gameObject.name;
            WorldObjectBase newObj = PrefabUtility.InstantiatePrefab(_objectToPaint) as WorldObjectBase;
            if (newObj != null)
            {
                newObj.transform.position = position;
                newObj.transform.eulerAngles = rotation;
                GetObjectLayer().AddObject(position, newObj);
            }
        }
    }

    private void StackWorldObject(Vector3 position, Vector3 rotation)
    {
        List<WorldObjectBase> objs = new List<WorldObjectBase>();
        GetObjectLayer().GetObjectsAtPosition(position, ref objs);
        
        if (_objectToPaint != null)
        {
            if (objs.Count == 0)
            {
                AddWorldObject(position, rotation);
                return;
            }
            
            string id = _objectToPaint.gameObject.name;
            WorldObjectBase newObj = PrefabUtility.InstantiatePrefab(_objectToPaint) as WorldObjectBase;
            if (newObj != null)
            {
                var pos = position;
                pos.y += objs.Count;
                newObj.transform.position = pos;
                newObj.transform.eulerAngles = rotation;
                GetObjectLayer().AddObject(position, newObj);
            }
        }
    }

    private void RemoveWorldObject(Vector3 position)
    {
        GetObjectLayer().RemoveObject(position);
    }

    private bool ObjectExistsAtPosition(Vector3 position)
    {
        List<WorldObjectBase> objsAtPos = null;
        GetObjectLayer().GetObjectsAtPosition(position, ref objsAtPos);
        return objsAtPos != null && objsAtPos.Count > 0;
    }
    #endregion
    
    public static void TeleportSceneCamera(Vector3 cam_position, Vector3 cam_forward)
    {
        //UnityEditor.EditorApplication.ExecuteMenuItem("Window/Scene");

        // Can't set transform of camera :(
        // It internally updates every frame:
        //      cam.position = pivot + rotation * new Vector3(0, 0, -cameraDistance)
        // Info: https://forum.unity.com/threads/moving-scene-scene_view-camera-from-editor-script.64920/#post-3388397
        // But we can align it to an object! Source: http://answers.unity.com/answers/256969/scene_view.html
        var scene_view = UnityEditor.SceneView.lastActiveSceneView;
        if (scene_view != null)
        {
            scene_view.orthographic = false;

            var target = scene_view.camera;
            target.transform.position = cam_position;
            target.transform.rotation = Quaternion.LookRotation(cam_forward);
            scene_view.AlignViewToObject(target.transform);
        }
    }
}