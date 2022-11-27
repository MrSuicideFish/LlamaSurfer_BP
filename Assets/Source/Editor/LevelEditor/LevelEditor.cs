using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.SceneManagement;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.EventSystems;
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
        get { return GameSystem.GetTrackController().TrackTime; }

        set { GameSystem.GetTrackController().TrackTime = value; }
    }

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

    }

    public override void OnActivated()
    {
        if (Application.isPlaying) return;
        if (GameSystem.GetTrackController().TrackCount == 0)
        {
            BuildStart();
            BuildFinish();
        }
        
        ReloadObjectRepo();
        ReloadPlatformRepo();
        ReloadGraphicIcons();
        Rebuild();
        SceneView.lastActiveSceneView?.ShowNotification(new GUIContent("Entering Level Editor"), .1f);
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

    private float playerXPosition;
    private void DrawTrackPositionSlider()
    {
        const float slider_space = 15;
        GUILayout.Label($"TrackPosition ({trackPosition})");
        
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

    private void DrawTrackWindow()
    {
        using (new GUILayout.VerticalScope(GUILayout.MaxWidth(200)))
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format("TT Pos: {0}", GameSystem.GetTrackController().GetTrackPosition()));
            sb.AppendLine(string.Format("TT Tan: {0}", GameSystem.GetTrackController().GetTrackTangent()));
            sb.AppendLine(string.Format("TT World Up: {0}", GameSystem.GetTrackController().GetTrackWorldUp()));

            GUILayout.Label(sb.ToString());

            EditorGUI.BeginChangeCheck();

            if (GameSystem.GetTrackController().TrackCount > 2)
            {
                if (GUILayout.Button("Delete")) RemoveLast();
            }
            
            DrawTrackPositionSlider();

            if (GUILayout.Button("Rebuild"))
            {
                Rebuild();
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(GameSystem.GetTrackController().GetPlatformContainer(), "change platforms");
                Undo.RecordObject(GameSystem.GetTrackController(), "change track");
            }
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
            
            GUILayout.FlexibleSpace();
        }
        
        GUILayout.FlexibleSpace();
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
        GUILayout.FlexibleSpace();
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
    
    public override void OnToolGUI(EditorWindow window)
    {
        if (!(window is SceneView sceneView))
            return;

        DrawWorldGizmos();
        
        if (Event.current.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(GetHashCode(), FocusType.Passive));
        }

        if (GameSystem.GetTrackController() != null)
        {
            sceneView.sceneViewState.alwaysRefresh = true;
            
            // move scene camera to the updated game camera's orientation
            // game camera executes in edit mode and uses in-game properties for pos/rot offset from the path.
            // instead of emulating this we take the game camera's orientation. NOTE: FOV/Effects will differ.
            TeleportSceneCamera(Camera.main.transform.position,
                GameSystem.GetPlayer().transform.position - Camera.main.transform.position);
            
            HandleMouseControl(sceneView);
            Handles.BeginGUI();
            
            using (new GUILayout.VerticalScope())
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(15);
                    DrawTrackWindow();
                    GUILayout.Space(15);
                    DrawObjectWindow();
                }
            }

            Handles.EndGUI();
        }
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

    public void BuildStart()
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

    public void BuildFinish()
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
    
    public void BuildNext(int platformIndex)
    {
        Platform newPlatform = PrefabUtility.InstantiatePrefab(_platformRepo[platformIndex]) as Platform;
        if (newPlatform != null)
        {
            GameSystem.GetTrackController().AddPlatform(newPlatform);
            Rebuild();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }

    public void RemoveLast()
    {
        GameSystem.GetTrackController().RemoveLast();
        Rebuild();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    private void Rebuild()
    {
        RebuildPlayerPath();
    }

    private void RebuildPlayerPath()
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
        if (GameSystem.GetTrackController() == null || GameSystem.GetTrackController()._grid == null)
            return Vector3.zero;
        
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        Plane drawPlane = new Plane(Vector3.up, new Vector3(0, 0, 0));
        drawPlane.Raycast(ray, out float enter);
        var point = ray.GetPoint(enter);
        Vector3Int samplePosInt =
            new Vector3Int(Mathf.FloorToInt(point.x), Mathf.FloorToInt(point.y), Mathf.FloorToInt(point.z));

        return GameSystem.GetTrackController()._grid.GetCellCenterWorld(samplePosInt);
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