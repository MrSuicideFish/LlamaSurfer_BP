using System;
using UnityEngine;

public class DragToMove : MonoBehaviour
{
    private Touch touch;

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
            {
                // remap touch position to -1.0 - 1.0 range for playerController
                GameManager.Instance.playerController.Move(touch.position.x / Screen.width);
            }
        }
    }
}