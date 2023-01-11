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

        obj.transform.SetParent(this.transform, true);
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
    
    public void GetObjectsAtPosition(Vector3 position, ref List<GameObject> objects)
    {
        if (allObjects == null)
        {
            allObjects = new List<LevelObjectInfo>();
        }

        objects = new List<GameObject>();
        foreach (LevelObjectInfo objs in allObjects)
        {
            if (objs.position == position)
            {
                foreach (var o in objs.objects)
                {
                    objects.Add(o.gameObject);
                }
            }
        }
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

    public int ObjectCountAtPosition(Vector3 position)
    {
        if (allObjects == null)
        {
            allObjects = new List<LevelObjectInfo>();
        }

        int count = 0;
        foreach (LevelObjectInfo objs in allObjects)
        {
            if (objs.position == position)
            {
                count += objs.objects.Count;
            }
        }

        return count;
    }

    public void EraseAll()
    {
        if (allObjects != null && allObjects.Count > 0)
        {
            for (int i = 0; i < allObjects.Count; i++)
            {
                for (int j = 0; j < allObjects[i].objects.Count; j++)
                {
                    DestroyImmediate(allObjects[i].objects[j]);
                }
                allObjects[i].objects.Clear();
            }
            allObjects.Clear();
        }

        for (int i = transform.childCount-1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    #if UNITY_EDITOR
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

            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                List<WorldObjectBase> objects = new List<WorldObjectBase>();
                GetObjectsAtPosition(child.position, ref objects);
                if (objects != null && objects.Count == 0)
                {
                    WorldObjectBase o = child.GetComponent<WorldObjectBase>();
                    AddObject(child.position, o);
                }
            }
        }
    }
#endif

}