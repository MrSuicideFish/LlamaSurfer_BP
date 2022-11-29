using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class LevelObjectInfo
{
    public Vector3 position;
    public List<WorldObjectBase> objects = new List<WorldObjectBase>();
}

public class WorldObjectLayer : MonoBehaviour
{
    public List<LevelObjectInfo> allObjects;

    public void AddObject(Vector3 position, WorldObjectBase obj)
    {
        if (allObjects == null)
        {
            allObjects = new List<LevelObjectInfo>();
        }

        LevelObjectInfo existing = null;
        for (int i = 0; i < allObjects.Count; i++)
        {
            if (allObjects[i].position == position)
            {
                existing = allObjects[i];
            }
        }

        if (existing == null)
        {
            existing = new LevelObjectInfo();
            existing.position = position;
            existing.objects = new List<WorldObjectBase>();
            allObjects.Add(existing);
        }
        
        obj.transform.SetParent(this.transform);
        existing.objects.Add(obj);
    }

    public void RemoveObject(Vector3 position)
    {
        List<WorldObjectBase> objsAtPos = null;
        if (allObjects == null) return;

        for(int i = 0; i < allObjects.Count; i++)
        {
            if (allObjects[i].position == position)
            {
                objsAtPos = allObjects[i].objects;
            }
        }
        
        if (objsAtPos != null && objsAtPos.Count > 0)
        {
            WorldObjectBase lastObj = objsAtPos[^1];
            if (lastObj != null)
            {
                GameObject.DestroyImmediate(lastObj.gameObject);
                objsAtPos.RemoveAt(objsAtPos.Count - 1);
            }
        }
    }

    public GameObject GetObjectAtPosition(Vector3 position)
    {
        if (allObjects == null)
        {
            allObjects = new List<LevelObjectInfo>();
        }

        foreach (LevelObjectInfo obj in allObjects)
        {
            if (obj.position == position)
            {
                return obj.objects[obj.objects.Count].gameObject;
            }
        }

        return null;
    }

    public void GetObjectsAtPosition(Vector3 position, ref List<WorldObjectBase> objects)
    {
        if (allObjects == null)
        {
            allObjects = new List<LevelObjectInfo>();
        }

        objects = null;
        foreach (LevelObjectInfo objs in allObjects)
        {
            if (objs.position == position)
            {
                objects = objs.objects;
            }
        }
    }

    public void EraseAll()
    {
        if (allObjects == null || allObjects.Count == 0) return;
        for (int i = 0; i < allObjects.Count; i++)
        {
            for (int j = 0; j < allObjects[i].objects.Count; j++)
            {
                GameObject.DestroyImmediate(allObjects[i].objects[j]);
            }
            allObjects[i].objects.Clear();
        }
        allObjects.Clear();
        GameObject.DestroyImmediate(gameObject);
    }

    private void OnValidate()
    {
        if (allObjects != null)
        {
            for (int i = 0; i < allObjects.Count; i++)
            {
                List<WorldObjectBase> container = allObjects[i].objects;
                for (int j = 0; j < container.Count; j++)
                {
                    if (container[j] == null)
                    {
                        container.RemoveAt(j);
                    }
                }
            }
        }
    }
}