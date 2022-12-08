using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class LevelEditorEvent : UnityEvent
{
    
}

public class LevelEditorEvent<T> : UnityEvent<T>
{
}

public class LevelEditorEvent<T0, T1> : UnityEvent<T0, T1>
{
    
}