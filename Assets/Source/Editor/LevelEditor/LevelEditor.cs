using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.SceneManagement;
using UnityEditor.ShortcutManagement;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.EventSystems;
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
        
        EditorApplication.playModeStateChanged -= OnPlayStateChanged;
        EditorApplication.playModeStateChanged += OnPlayStateChanged;
    }
    
    private void OnDisable()
    {
        InitGUI();
        if (_editorWindow != null)
        {
            _editorWindow.Close();
        }
    }

    private void OnPlayStateChanged(PlayModeStateChange mode)
    {
        InitGUI();
        if (_editorWindow != null)
        {
            _editorWindow.Close();
        }
    }

    public override void OnActivated()
    {
        if (TrackCount == 0)
        {
            BuildStartPlatform();
            BuildFinishPlatform();
        }
        
        SceneView.lastActiveSceneView?.ShowNotification(new GUIContent("Entering Level Editor"), .1f);

        InitGUI();
        
        // initialize editor window
        System.Type inspectorType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow");
        _editorWindow = EditorWindow.GetWindow<LevelEditorWindow>(desiredDockNextTo: inspectorType);
        _editorWindow.OnEraseAllEntities.RemoveAllListeners();
        _editorWindow.OnEraseAllEntities.AddListener(GetObjectLayer().EraseAll);
        
        _editorWindow.OnPaintEntityChanged.RemoveAllListeners();
        _editorWindow.OnPaintEntityChanged.AddListener((obj) => { _objectToPaint = obj; });
        
        _editorWindow.OnAddPlatform.RemoveAllListeners();
        _editorWindow.OnAddPlatform.AddListener(BuildNextPlatform);
        
        _editorWindow.OnDeletePlatform.RemoveAllListeners();
        _editorWindow.OnDeletePlatform.AddListener(RemoveLast);

        RebuildAllTrackKnots();
    }

    public void OnDestroy()
    {
        if (_editorWindow != null)
        {
            _editorWindow.Close();
        }
    }

    readonly Color timelineBackgroundColor = new Color(0.1f, 0.1f, 0.15f, 1.0f);
    readonly Color xSliderBackgroundColor = new Color(0.07f, 0.07f, 0.105f, 1.0f);
    
    readonly Color timelineCursorColor = new Color(0.8f, 0.0f, 0.4f, 1.0f);
    readonly Color xSliderCursorColor = new Color(0.8f, 0.0f, 0.4f, 1.0f);
    
    readonly Color timelineNotchColor = new Color(0.2f, 0.2f, 0.25f, 1.0f);
    readonly Color xSliderNotchColor = new Color(0.2f, 0.2f, 0.25f, 1.0f);
    
    private void InitGUI()
    {
        _timeline = new EditorLevelTrackTimeline(25, 
            0.0f, 1.0f,
            25, timelineBackgroundColor, timelineCursorColor, timelineNotchColor);
        
        _playerX_Slider = new EditorLevelTrackTimeline(10, 
            0.0f, 1.0f,
            25, xSliderBackgroundColor, xSliderCursorColor, xSliderNotchColor);
        
        playerXPosition = Mathf.Clamp01(GameSystem.GetPlayer().moveX);
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

        if (Screen.height - Event.current.mousePosition.y > 150)
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

    private float playerXPosition = 0.5f;

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
        WorldObjectLayer result = null;
        GameObject existing = GameObject.FindGameObjectWithTag("WorldObjectLayer");
        if (existing != null)
        {
            result = existing.GetComponent<WorldObjectLayer>();
        }
        else
        {
            result = new GameObject("_OBJECTS").AddComponent<WorldObjectLayer>();
            result.gameObject.tag = "WorldObjectLayer";
        }

        return result;
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

    private void RebuildAllTrackKnots()
    {
        ClearAllTrackKnots();
        foreach (var platform in GameSystem.GetTrackController()._platforms)
        {
            AppendPlatformKnots(platform.platform);
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
                GetObjectLayer().AddObject(position, newObj);
                newObj.transform.position = position;
                newObj.transform.forward = rotation;
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
                newObj.transform.eulerAngles = objs[0].transform.eulerAngles;
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

    private const int resolution = 5;
    private const float PATH_DIST_LIMIT = 3;
    private void HandleMouseControl(SceneView sceneView)
    {
        if (BuildMode != EBuildMode.Entity) return;
        
        var mouseSamplePos = SampleMousePosition();
        var paintPosition = mouseSamplePos;
        
        float3 closestPos;
        float closestTime;
        SplineUtility.GetNearestPoint(GameSystem.GetTrackController().spline.Spline, paintPosition, 
            out closestPos,
            out closestTime);
        
        // snap closest time to nearest hundreth or thousandth
        closestTime = Mathf.Round(closestTime * (18f * resolution)) / (18f * resolution);
        
        // get pos a new snapped time
        Vector3 closestTimePositionOnLine = GameSystem.GetTrackController().GetTrackPositionAt(closestTime);
        paintPosition = closestTimePositionOnLine;
        
        // divide X-cross into 6 (3 per curve side)
        Vector3 up = GameSystem.GetTrackController().spline.EvaluateUpVector(closestTime);
        Vector3 tangent = GameSystem.GetTrackController().spline.EvaluateTangent(closestTime);
        Vector3 cross = Vector3.Cross(up, tangent).normalized;

        // determine which x-cross is closest to sample position
        // snap sample pos to nearest cross
        Vector3 mouseDir = (mouseSamplePos - paintPosition);
        Vector3 proj = Vector3.Project(mouseDir, cross);
        proj = Vector3.ClampMagnitude(proj, PATH_DIST_LIMIT);

        float appr = 2;
        float rounded = Mathf.Round(proj.magnitude * appr) / appr;
        Vector3 snappedProj = proj.normalized * rounded;

        paintPosition = paintPosition + snappedProj;
        paintPosition.y = 0.5f;
        Vector3 paintRotation = ((closestTimePositionOnLine + tangent) - closestTimePositionOnLine);

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            if (PaintMode == EPaintMode.Paint)
            {
                if (!ObjectExistsAtPosition(paintPosition))
                {
                    AddWorldObject(paintPosition, paintRotation);
                }
                else if(_objectToPaint.canStack)
                {
                    StackWorldObject(paintPosition, paintRotation);
                }
            }
            else if (PaintMode == EPaintMode.Erase)
            {
                RemoveWorldObject(paintPosition);
            }
        }
        
        Vector3 drawSamplePosition = paintPosition;
        int objCount = GetObjectLayer().ObjectCountAtPosition(paintPosition);
        drawSamplePosition += new Vector3(0, objCount-0.5f, 0);
        DrawPaintPreviewGridRectDotted(paintPosition - new Vector3(0, 0.5f, 0), 1.0f, Quaternion.LookRotation(paintRotation), Color.red, Color.white);
        
        //debugging
        if (PaintMode == EPaintMode.Paint)
        {
            DrawPaintPreviewGridRect(drawSamplePosition, 1.0f, Quaternion.LookRotation(paintRotation), Color.cyan, Color.white);
        }
        else if (PaintMode == EPaintMode.Erase)
        {
            DrawPaintPreviewGridRect(drawSamplePosition, 1.0f, Quaternion.LookRotation(paintRotation), Color.red, Color.white);
        }
    }

    private void DrawPaintPreviewGridRect(Vector3 position, float size, Quaternion rotation, Color faceColor, Color outlineColor)
    {
        size /= 2.0f;
        Vector3[] verts = new Vector3[]
        {
            rotation * (new Vector3(position.x - size, position.y, position.z - size) - position) + position,
            rotation * (new Vector3(position.x - size, position.y, position.z + size) - position) + position,
            rotation * (new Vector3(position.x + size, position.y, position.z + size) - position) + position,
            rotation * (new Vector3(position.x + size, position.y, position.z - size) - position) + position
        };
        
        Handles.DrawSolidRectangleWithOutline(verts, faceColor, outlineColor);
    }
    
    private void DrawPaintPreviewGridRectDotted(Vector3 position, float size, Quaternion rotation, Color faceColor, Color outlineColor)
    {
        size /= 2.0f;
        Vector3[] verts = new Vector3[]
        {
            rotation * (new Vector3(position.x - size, position.y, position.z - size) - position) + position,
            rotation * (new Vector3(position.x - size, position.y, position.z + size) - position) + position,
            rotation * (new Vector3(position.x + size, position.y, position.z + size) - position) + position,
            rotation * (new Vector3(position.x + size, position.y, position.z - size) - position) + position
        };

        Handles.color = outlineColor;
        Handles.DrawDottedLine(verts[0], verts[1], 15.0f);
        Handles.DrawDottedLine(verts[1], verts[2], 15.0f);
        Handles.DrawDottedLine(verts[2], verts[3], 15.0f);
        Handles.DrawDottedLine(verts[3], verts[0], 15.0f);
        Handles.DrawAAPolyLine(verts);
    }

    private Vector3 SampleMousePosition()
    {
        if (GameSystem.GetTrackController() == null || GameSystem.GetTrackController()._grid == null)
            return Vector3.zero;
        
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        Plane drawPlane = new Plane(Vector3.up, new Vector3(0, 0, 0));
        drawPlane.Raycast(ray, out float enter);
        var point = ray.GetPoint(enter);

        return point;
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