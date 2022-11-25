using UnityEngine.Events;

public class GameEvent<T>
{
    private UnityEvent<T> _event;
    public GameEvent()
    {
        _event = new UnityEvent<T>();
    }

    ~GameEvent()
    {
        _event.RemoveAllListeners();
    }

    public void Invoke(T eventData)
    {
        _event?.Invoke(eventData);
    }

    public void AddListener(UnityAction<T> listener)
    {
        _event.AddListener(listener);
    }
    
    public void RemoveListener(UnityAction<T> listener)
    {
        _event.RemoveListener(listener);
    }
}