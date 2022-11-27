using System;
using UnityEngine;

public class DragToMove : MonoBehaviour
{
    private Touch touch;
    private PlayerController _player;

    private void Update()
    {
        if (_player == null)
        {
            _player = GameSystem.GetPlayer();
        }
        
        if (Input.touchCount > 0)
        {
            touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
            {
                // remap touch position to -1.0 - 1.0 range for playerController
                _player.moveX = touch.position.x / Screen.width;
            }
        }
    }
}