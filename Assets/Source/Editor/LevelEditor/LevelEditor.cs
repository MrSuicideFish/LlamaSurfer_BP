using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.SceneManagement;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;
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

    public void OnEnable()
    {
        InitGUI();
    }

    public override void OnActivated()
    {
        if (!GameSystem.GetTrackController() == null) return;
        if (Application.isPlaying) return;
        if (TrackCount == 0)
        {
            BuildStartPlatform();
            BuildFinishPlatform();
        }
        
        SceneView.lastActiveSceneView?.ShowNotification(new GUIContent("Entering Level Editor"), .1f);

        playerXPosition = Mathf.Clamp01(GameSystem.GetPlayer().moveX);
        System.Type inspectorType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow");
        
        // initialize editor window
        _editorWindow = EditorWindow.GetWindow<LevelEditorWindow>(desiredDockNextTo: inspectorType);
        _editorWindow.OnEraseAllEntities.AddListener(GetObjectLayer().EraseAll);
        _editorWindow.OnPaintEntityChanged.AddListener((obj) => { _objectToPaint = obj; });
        _editorWindow.OnAddPlatform.AddListener(BuildNextPlatform);
        _editorWindow.OnDeletePlatform.AddListener(RemoveLast);
        InitGUI();
    }

    private void InitGUI()
    {
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
        if (!(window is SceneView sceneView)
            || !GameSystem.GetTrackController() == null)
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
                Undo.RecordObject(GetPlatformContainer(), "change platforms");
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
        TrackController track = GameSystem.GetTrackController();
        if (track != null)
        {
            const string startPlatformPath = "Level/Platforms/Platform_Start";
            Object resource = Resources.Load<Platform>(startPlatformPath);
            if (resource != null)
            {
                Platform startPlatform = PrefabUtility.InstantiatePrefab(resource) as Platform;
                AddPlatform(startPlatform);
            }
        }
    }

    public void BuildFinishPlatform()
    {
        TrackController track = GameSystem.GetTrackController();
        if (track != null)
        {
            const string finishPlatformPath = "Level/Platforms/Platform_Finish";
            Object resource = Resources.Load<Platform>(finishPlatformPath);
            if (resource != null)
            {
                Platform finishPlatform = PrefabUtility.InstantiatePrefab(resource) as Platform;
                AddPlatform(finishPlatform);
            }
        }
    }
    
    public void BuildNextPlatform(Platform resource)
    {
        TrackController track = GameSystem.GetTrackController();
        if (track != null)
        {
            Platform newPlatform = PrefabUtility.InstantiatePrefab(resource) as Platform;
            if (newPlatform != null)
            {
                AddPlatform(newPlatform);
            }
        }
    }


    private Transform _platformContainer;
    public Transform GetPlatformContainer()
    {
        if (_platformContainer == null)
        {
            GameObject existing = GameObject.FindGameObjectWithTag("PlatformContainer");
            if (existing != null)
            {
                _platformContainer = existing.transform;
            }
            else
            {
                _platformContainer = new GameObject("_PLATFORMS").transform;
                _platformContainer.gameObject.tag = "PlatformContainer";
            }
        }

        return _platformContainer;
    }

    public void AddPlatform(Platform newPlatform)
    {
        TrackController track = GameSystem.GetTrackController();
        if (track != null)
        {
            if (track._platforms == null)
            {
                track._platforms = new List<TrackNodeInfo>();
            }

            Platform lastPlatform = null;
            TrackNodeInfo newNode = new TrackNodeInfo();
            newNode.platform = newPlatform;
            newNode.isCheckpoint = newPlatform.PlatformType == Platform.EPlatformType.Checkpoint;
            newPlatform.transform.SetParent(GetPlatformContainer());
            
            switch (newPlatform.PlatformType)
            {
                case Platform.EPlatformType.Start:
                    newPlatform.transform.position = GetPlatformContainer().position;
                    newPlatform.transform.forward = GetPlatformContainer().forward;
                    newNode.time = 0.0f;
                    track._startPlatform = newNode;
                    track._platforms.Add(newNode);
                    track.spline.Spline.Clear();
                    AppendPlatformKnots(newPlatform);
                    break;
                case Platform.EPlatformType.Finish:
                    lastPlatform = track._platforms[GetLastPlatformIndex()].platform;
                    newPlatform.transform.position = lastPlatform.platformExit.position;
                    newPlatform.transform.forward = lastPlatform.platformExit.forward;
                    newNode.time = 1.0f;
                    track._finishPlatform = newNode;
                    track._platforms.Add(newNode);
                    AppendPlatformKnots(newPlatform);
                    break;
                default:
                    lastPlatform = track._platforms[GetLastPlatformIndex()].platform;
                    RemoveFinishPlatform();
                    newPlatform.transform.position = lastPlatform.platformExit.position;
                    newPlatform.transform.forward = lastPlatform.platformExit.forward;
                    AppendPlatformKnots(newPlatform);
                    track._platforms.Add(newNode);
                    BuildFinishPlatform();
                    break;
            }
            
            ValidateNodeTime();
            Scene currentScene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(currentScene);
        }
    }

    private void OnValidate()
    {
        ValidateNodeTime();
        InitGUI();
    }

    private void ValidateNodeTime()
    {
        TrackController track = GameSystem.GetTrackController();
        if (track != null)
        {
            for (int i = 0; i < track._platforms.Count; i++)
            {
                TrackNodeInfo node = track._platforms[i];
                node.time = track.GetTrackTimeByPosition(node.platform.trackPoints[0].position);
            }
        }

    }

    public int GetLastPlatformIndex()
    {
        TrackController track = GameSystem.GetTrackController();
        int result = 0;
        for (int i = track._platforms.Count - 1; i >= 0; i--)
        {
            if (track._platforms[i] != null && track._platforms[i].platform.PlatformType != Platform.EPlatformType.Finish)
            {
                result = i;
                break;
            }
        }
        return result;
    }

    private int GetFinishPlatformIndex()
    {
        int result = 0;
        TrackController track = GameSystem.GetTrackController();
        if (track != null)
        {
            for (int i = 0; i < track._platforms.Count; i++)
            {
                if (track._platforms[i] != null
                    && track._platforms[i].platform.PlatformType == Platform.EPlatformType.Finish)
                {
                    result = i;
                    break;
                }
            }
        }
        return result;
    }

    public void RemoveLast()
    {
        TrackController track = GameSystem.GetTrackController();
        if (track._platforms == null || TrackCount == 0 
                                     || TrackCount <= 2
                                    || track._startPlatform == null 
                                    || track._finishPlatform == null)
        {
            return;
        }

        int last = GetLastPlatformIndex();
        TrackNodeInfo lastNode = track._platforms[last];
        if (lastNode != null)
        {
            RemovePlatformKnots(lastNode.platform);
            DestroyImmediate(lastNode.platform.gameObject);
            track._platforms.RemoveAt(last);
            
            RemoveFinishPlatform();
            BuildFinishPlatform();
        }
    }

    private void RemoveFinishPlatform()
    {
        TrackController track = GameSystem.GetTrackController();
        if (track != null)
        {
            if (track._platforms.Count < 2 
                || track._startPlatform == null 
                || track._finishPlatform == null)
            {
                return;
            }

            int finishNodeIdx = GetFinishPlatformIndex();
            TrackNodeInfo finishNode = track._platforms[finishNodeIdx];
            if (finishNode != null)
            {
                // remove finish platform
                RemovePlatformKnots(track._finishPlatform.platform);
                DestroyImmediate(track._finishPlatform.platform.gameObject);
                track._platforms.RemoveAt(finishNodeIdx);
                track._finishPlatform = null;
            }
        }
    }
    
    private void RemovePlatformKnots(Platform platform)
    {
        TrackController track = GameSystem.GetTrackController();
        if (track != null)
        {
            for (int i = 0; i < platform.trackPoints.Length; i++)
            {
                Transform point = platform.trackPoints[i];
                BezierKnot knot = new BezierKnot()
                {
                    Position = point.position,
                    Rotation = point.rotation
                };
                track.spline.Spline.Remove(knot);
            }
        }
    }
    
    private void AppendPlatformKnots(Platform platform)
    {
        TrackController track = GameSystem.GetTrackController();
        if (track != null)
        {
            for (int i = 0; i < platform.trackPoints.Length; i++)
            {
                Transform point = platform.trackPoints[i];
                BezierKnot knot = new BezierKnot()
                {
                    Position = point.position,
                    Rotation = point.rotation
                };
                track.spline.Spline.Add(knot);
            }
        }
    }

    private void ClearAllTrackKnots()
    {
        TrackController track = GameSystem.GetTrackController();
        if (track != null)
        {
            track.spline.Spline.Clear();
        }
    }
    #endregion
    
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
    
    public int TrackCount
    {
        get
        {
            return GetPlatformContainer().childCount;
        }
    }
}