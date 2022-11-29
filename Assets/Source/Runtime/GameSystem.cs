using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameSystem : MonoBehaviour
{
    private const string _gameSystemID = "_GAME_SYSTEM";
    private const string _gameSystemTagID = "GAME_SYSTEM";
    private static GameSystem _inst;
    
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private PlayerController _player;
    [SerializeField] private TrackController _trackController;
    [SerializeField] private CameraController _camera;

    public static bool IsInitialized()
    {
        return _inst != null;
    }

    private static GameSystem TryGetGameSystem()
    {
        if (_inst == null)
        {
            Debug.Log("Global Game System instance is null, searching for existing instance...");
            GameObject gameSysObj = GameObject.FindWithTag(_gameSystemTagID);
            if (gameSysObj != null)
            {
                _inst = gameSysObj.GetComponent<GameSystem>();
                Debug.Log("Found existing Game System instance!");
            }
            else
            {
#if UNITY_EDITOR
                Debug.Log($"Search for Game System failed. Spawning new one id: {_gameSystemID}");
                GameSystem res = Resources.Load<GameSystem>(_gameSystemID);
                if (res != null)
                {
                    _inst = Object.Instantiate(res);
                }
#endif
            }

            if (_inst._gameManager == null) _inst._gameManager = _inst.GetComponentInChildren<GameManager>();
            if (_inst._player == null) _inst._player = _inst.GetComponentInChildren<PlayerController>();
            if (_inst._trackController == null) _inst._trackController = _inst.GetComponentInChildren<TrackController>();
            if (_inst._camera == null) _inst._camera = _inst.GetComponentInChildren<CameraController>();

            _inst._trackController.transform.SetParent(null, true);
        }
        return _inst;
    }

    public static GameManager GetGameManager()
    {
        return TryGetGameSystem()._gameManager;
    }
    
    public static PlayerController GetPlayer()
    {
        return TryGetGameSystem()._player;
    }

    public static TrackController GetTrackController()
    {
        return TryGetGameSystem()._trackController;
    }

    public static CameraController GetCameraController()
    {
        return TryGetGameSystem()._camera;
    }
}
