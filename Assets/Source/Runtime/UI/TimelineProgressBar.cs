using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class TimelineProgressBar : MonoBehaviour
{
    public Image finishFlag;
    public Image checkpointFlag;
    public Image background;
    public Image fill;

    private List<Image> checkpointFlags;

    private void OnEnable()
    {

        if (checkpointFlag != null && fill != null)
        {
            // hide main checkpoint
            checkpointFlag.gameObject.SetActive(false);

            ClearAllCheckpoints();
            SetupCheckpoints();
        }
    }

    private void OnDisable()
    {
        ClearAllCheckpoints();
    }

    private void SetupCheckpoints()
    {
        TrackController track = GameSystem.GetTrackController();
        if (track != null)
        {
            float[] checkpoints = track.GetCheckpoints();
            if (checkpoints != null && checkpoints.Length > 0)
            {
                for (int i = 0; i < checkpoints.Length; i++)
                {
                    AddCheckpoint(checkpoints[i]);
                }
            }
        }
    }

    private void AddCheckpoint(float trackTime)
    {
        if (checkpointFlags == null)
        {
            checkpointFlags = new List<Image>();
        }
        
        Image newFlag = Instantiate(checkpointFlag);
        newFlag.gameObject.SetActive(true);
        newFlag.transform.SetParent(this.transform, false);
        newFlag.transform.localPosition = Vector3.zero;
        
        // use fill img for dimensions
        //fill.rectTransform.rect.size;
        Vector2 flagPosition = fill.rectTransform.rect.position;
        flagPosition.x = Mathf.Lerp(flagPosition.x, flagPosition.x + fill.rectTransform.rect.width, trackTime);
        flagPosition.y += (fill.rectTransform.rect.height / 2.0f) * 3.0f;
        
        newFlag.rectTransform.anchoredPosition = flagPosition;

        checkpointFlags.Add(newFlag);
    }

    [ContextMenu("Clear all checkpoints")]
    private void ClearAllCheckpoints()
    {
        if (checkpointFlags != null)
        {
            for (int i = 0; i < checkpointFlags.Count; i++)
            {
#if UNITY_EDITOR
                GameObject.DestroyImmediate(checkpointFlags[i].gameObject);
#else
                GameObject.Destroy(checkpointFlags[i].gameObject);
#endif
            }
            checkpointFlags.Clear();
        }
    }

    [ContextMenu("Set some checkpoints")]
    public void Debug_SetSomeCheckpoints()
    {
        OnEnable();
        AddCheckpoint(0.333f);
        AddCheckpoint(0.5f);
        AddCheckpoint(0.8f);
        AddCheckpoint(0.1f);
    }

    private void Update()
    {
        if (fill != null && GameSystem.GetTrackController() != null)
        {
            //fill.fillAmount = GameSystem.GetTrackController().TrackTime / 1.0f;
        }
    }

#if UNITY_EDITIOR
    public void OnValidate()
    {

    }
#endif
}
