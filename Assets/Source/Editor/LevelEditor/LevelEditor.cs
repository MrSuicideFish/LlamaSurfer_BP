using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cinemachine;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.SceneManagement;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Splines;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

[EditorTool("Level Editor", typeof(TrackController))]
public class LevelEditor : EditorTool
{
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

    // Gameplay Systems
    private TrackController _track;

    // Entity Repositories
    private Platform[] _platformRepo;
    private Texture2D[] _platformThumbnails;
    private WorldObjectBase[] _objectRepo;
    private Texture2D[] _objectThumbnails;
    
    // Paint Properties
    private EBuildMode _buildMode;
    private Texture[] _worldBuildIcons;
    private Texture[] _paintModeIcons;
    
    private Vector3 _paintPosition;
    private int _objectToPaint;
    private EPaintMode _paintMode;

    // Level Ed Containers
    private WorldObjectLayer _objectContainer;

    public float trackPosition
    {
        get { return GetCameraTrack().TrackTime; }

        set { GetCameraTrack().TrackTime = value; }
    }

    public void OnEnable()
    {
        if (GetCameraTrack().TrackCount == 0)
        {
            BuildStart();
            BuildFinish();
        }

        Rebuild();
        ReloadObjectRepo();
        ReloadPlatformRepo();
    }

    public override void OnActivated()
    {
        SceneView.lastActiveSceneView?.ShowNotification(new GUIContent("Entering Level Editor"), .1f);
        ReloadObjectRepo();
        ReloadPlatformRepo();
        ReloadGraphicIcons();
    }

    private void ReloadGraphicIcons()
    {
        _paintModeIcons = new Texture[]
        {
            EditorGUIUtility.IconContent("d_Grid.PaintTool@2x").image,
            EditorGUIUtility.IconContent("d_Grid.EraserTool@2x").image
        };
            
        _worldBuildIcons = new Texture[]
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

    // The second "context" argument accepts an EditorWindow type.
    [Shortcut("Open Level Editor", typeof(SceneView), KeyCode.P)]
    static void LevelEditorShortcut()
    {
        if (Selection.GetFiltered<TrackController>(SelectionMode.TopLevel).Length > 0)
            ToolManager.SetActiveTool<LevelEditor>();
        else
            Debug.Log("No track selected!");
    }

    private void DrawTrackWindow()
    {
        using (new GUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.MaxWidth(200)))
        {
            GUILayout.Space(10);
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format("TT Pos: {0}", _track.GetTrackPosition()));
            sb.AppendLine(string.Format("TT Tan: {0}", _track.GetTrackTangent()));
            sb.AppendLine(string.Format("TT World Up: {0}", _track.GetTrackWorldUp()));

            GUILayout.Label(sb.ToString());

            EditorGUI.BeginChangeCheck();

            if (GetCameraTrack().TrackCount > 2)
            {
                if (GUILayout.Button("Delete")) RemoveLast();
            }

            GUILayout.Label($"TrackPosition ({trackPosition})");
            float newTrackTime = GUILayout.HorizontalSlider(GetCameraTrack().TrackTime, 0.0f, 1.0f);
            if (!GetCameraTrack().TrackTime.Equals(newTrackTime))
            {
                GetCameraTrack().SetTrackTime(newTrackTime, fireTrackEvents: false);
            }
            GUILayout.Space(10);
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("<<")) GetCameraTrack().Step(-(1.0f / GetCameraTrack().spline.Spline.GetLength()));
                if (GUILayout.Button(">>")) GetCameraTrack().Step((1.0f / GetCameraTrack().spline.Spline.GetLength()));
            }

            if (GUILayout.Button("Rebuild"))
            {
                Rebuild();
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(GetCameraTrack().GetPlatformContainer(), "change platforms");
                Undo.RecordObject(GetCameraTrack(), "change track");
            }

            GUILayout.Space(10);
        }
    }

    private Vector2 _entityViewScrollPos;
    private Vector2 _platformViewScrollPos;

    private void DrawObjectWindow()
    {
        using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
        {
            using (new GUILayout.VerticalScope())
            {
                int selection = GUILayout.SelectionGrid(_buildMode.GetHashCode(), _worldBuildIcons, 1, GUILayout.Width(60), GUILayout.Height(100));
                if (selection != _buildMode.GetHashCode())
                {
                    _buildMode = (EBuildMode)selection;
                }
            }
        
            switch (_buildMode)
            {
                case EBuildMode.Entity:
                    DrawEntitiesPanel();
                    break;
                case EBuildMode.Platform:
                    DrawPlatformsPanel();
                    break;
            }
        }
    }

    private void DrawEntitiesPanel()
    {
        using (new GUILayout.VerticalScope())
        {
            int selection = GUILayout.SelectionGrid(_paintMode.GetHashCode(), _paintModeIcons, 1);
            if (selection != _paintMode.GetHashCode())
            {
                _paintMode = (EPaintMode) selection;
            }
            
            if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("d_TreeEditor.Trash"))))
            {
                GetObjectLayer().EraseAll();
            }
        }
        using (new GUILayout.ScrollViewScope(_entityViewScrollPos))
        {
            _objectToPaint = GUILayout.SelectionGrid(_objectToPaint, _objectThumbnails, 10);
        }
    }
    
    private void DrawPlatformsPanel()
    {
        using (new GUILayout.HorizontalScope())
        {
            for (int i = 0; i < _platformRepo.Length; i++)
            {
                if (GUILayout.Button(_platformThumbnails[i], GUILayout.Width(100), GUILayout.Height(100)))
                {
                    BuildNext(i);
                }
            }
            GUILayout.FlexibleSpace();
        }
    }

    private void DrawWorldGizmos()
    {
        const float gizmoThickness = 6.0f;
        
        Vector3 trackWorldPos = _track.GetTrackPosition();
        Vector3 trackTangent = _track.GetTrackTangent();
        Vector3 worldUp = _track.GetTrackWorldUp();

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
    
    public override void OnToolGUI(EditorWindow window)
    {
        if (!(window is SceneView sceneView))
            return;

        DrawWorldGizmos();
        
        if (Event.current.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(GetHashCode(), FocusType.Passive));
        }

        if (GetCameraTrack() != null)
        {
            sceneView.sceneViewState.alwaysRefresh = true;
            TeleportSceneCamera(Camera.main.transform.position, Camera.main.transform.forward);
            HandleMouseControl(sceneView);
            
            Handles.BeginGUI();
            
            using (new GUILayout.VerticalScope())
            {
                GUILayout.FlexibleSpace();

                GUILayout.Space(10);
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(10);
                    DrawTrackWindow();
                    DrawObjectWindow();
                }
            }

            Handles.EndGUI();
            CameraController.Instance.UpdateTrackTarget();
        }
    }

    private TrackController GetCameraTrack()
    {
        if (_track == null)
        {
            _track = Object.FindObjectOfType<TrackController>();
            if (_track == null)
            {
                CreateNewTrack();
            }
        }

        return _track;
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

    private void CreateNewTrack()
    {
        GameObject splineObj = new GameObject("_LEVEL_TRACK");
        splineObj.tag = "TrackController";
        _track = splineObj.AddComponent<TrackController>();
        _track.spline.Spline.Add(new BezierKnot(new float3(0, 0, 0)));
    }

    public void BuildStart()
    {
        const string startPlatformPath = "Level/Platforms/Platform_Start";
        Object platformRes = Resources.Load<Platform>(startPlatformPath);
        if (platformRes != null)
        {
            Platform p = PrefabUtility.InstantiatePrefab(platformRes) as Platform;
            GetCameraTrack().AddStartPlatform(p);
            Rebuild();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }

    public void BuildFinish()
    {
        const string startPlatformPath = "Level/Platforms/Platform_Finish";
        Object platformRes = Resources.Load<Platform>(startPlatformPath);
        if (platformRes != null)
        {
            Platform p = PrefabUtility.InstantiatePrefab(platformRes) as Platform;
            GetCameraTrack().AddFinishPlatform(p);
            Rebuild();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }
    
    public void BuildNext(int platformIndex)
    {
        Platform newPlatform = PrefabUtility.InstantiatePrefab(_platformRepo[platformIndex]) as Platform;
        if (newPlatform != null)
        {
            GetCameraTrack().AddPlatform(newPlatform);
            Rebuild();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }

    public void RemoveLast()
    {
        GetCameraTrack().RemoveLast();
        Rebuild();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    private void Rebuild()
    {
        RebuildPlayerPath();
        RebuildCameraPath();
    }

    private void RebuildPlayerPath()
    {
        GetCameraTrack().ClearTrackNodes();
        
        for (int i = 0; i < GetCameraTrack()._platforms.Count; i++)
        {
            Platform platform = GetCameraTrack()._platforms[i];
            
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
                    GetCameraTrack().AddTrackNode(newTrackNode);
                }   
            }
        }
    }

    private void RebuildCameraPath()
    {
        // rebuild camera path
        CinemachinePath _cameraPath = FindObjectOfType<CinemachinePath>();
        if (_cameraPath == null)
        {
            GameObject cameraPathObj = new GameObject("Camera_Path");
            _cameraPath = cameraPathObj.AddComponent<CinemachinePath>();
        }
        
        _cameraPath.m_Waypoints.Initialize();
        _cameraPath.m_Waypoints = new CinemachinePath.Waypoint[GetCameraTrack().spline.Spline.Count];
        for (int i = 0; i < _cameraPath.m_Waypoints.Length; i++)
        {
            _cameraPath.m_Waypoints[i].position = GetCameraTrack().spline.Spline[i].Position;
            _cameraPath.m_Waypoints[i].tangent = GetCameraTrack().spline.Spline[i].TangentOut;
            _cameraPath.m_Waypoints[i].roll = 0.0f;
        }
    }

    private void HandleMouseControl(SceneView sceneView)
    {
        if (_buildMode != EBuildMode.Entity) return;
        float mouseY = Event.current.mousePosition.y;
        float screenHeight = sceneView.camera.pixelRect.height;
        if (mouseY < screenHeight - 220)    
        {
            var worldSamplePos = SampleMousePosition();

            float size = 0.5f;
            DrawPaintPreviewGridRect(worldSamplePos, size,
                _paintMode == EPaintMode.Paint ? Color.blue : Color.red,
                Color.black);
        
            // paint / erase
            if ((Event.current.type == EventType.MouseDown 
                || Event.current.type == EventType.MouseDrag) && Event.current.button == 0)
            {
                if (_paintMode == EPaintMode.Paint)
                {
                    if (!ObjectExistsAtPosition(worldSamplePos))
                    {
                        AddWorldObject(_objectToPaint, worldSamplePos, Vector3.forward);
                    }
                    else if(GetPaintObject().canStack)
                    {
                        StackWorldObject(_objectToPaint, worldSamplePos, Vector3.forward);
                    }
                }
                else if (_paintMode == EPaintMode.Erase)
                {
                    RemoveWorldObject(worldSamplePos);
                }
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
        if (GetCameraTrack() == null || GetCameraTrack()._grid == null)
            return Vector3.zero;
        
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        Plane drawPlane = new Plane(Vector3.up, new Vector3(0, 0, 0));
        drawPlane.Raycast(ray, out float enter);
        var point = ray.GetPoint(enter);
        Vector3Int samplePosInt =
            new Vector3Int(Mathf.FloorToInt(point.x), Mathf.FloorToInt(point.y), Mathf.FloorToInt(point.z));

        return GetCameraTrack()._grid.GetCellCenterWorld(samplePosInt);
    }

    private void AddWorldObject(int objIndex, Vector3 position, Vector3 rotation)
    {
        if (objIndex < 0 || objIndex >= _objectRepo.Length)
        {
            return;
        }

        WorldObjectBase obj = _objectRepo[objIndex];
        if (obj != null)
        {
            string id = obj.gameObject.name;
            obj = PrefabUtility.InstantiatePrefab(obj) as WorldObjectBase;
            if (obj != null)
            {
                obj.transform.position = position;
                obj.transform.eulerAngles = rotation;
                GetObjectLayer().AddObject(position, obj);
            }
        }
    }

    private void StackWorldObject(int objIndex, Vector3 position, Vector3 rotation)
    {
        if (objIndex < 0 || objIndex >= _objectRepo.Length)
        {
            return;
        }
        
        List<WorldObjectBase> objs = new List<WorldObjectBase>();
        GetObjectLayer().GetObjectsAtPosition(position, ref objs);

        if (objs.Count == 0)
        {
            AddWorldObject(objIndex, position, rotation);
            return;
        }
        
        WorldObjectBase obj = _objectRepo[objIndex];
        if (obj != null)
        {
            string id = obj.gameObject.name;
            obj = PrefabUtility.InstantiatePrefab(obj) as WorldObjectBase;
            if (obj != null)
            {
                var pos = position;
                pos.y += objs.Count;
                obj.transform.position = pos;
                obj.transform.eulerAngles = rotation;
                GetObjectLayer().AddObject(position, obj);
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

    private WorldObjectBase GetPaintObject()
    {
        if (_objectToPaint < 0 || _objectToPaint >= _objectRepo.Length)
            return null;
        return _objectRepo[_objectToPaint];
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

    //Returns 'true' if we touched or hovering on Unity UI element.
    public bool IsPointerOverUIElement()
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults());
    }


    //Returns 'true' if we touched or hovering on Unity UI element.
    private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
    {
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == LayerMask.NameToLayer("UI"))
                return true;
        }

        return false;
    }


    //Gets all event system raycast results of current mouse or touch position.
    static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }
}