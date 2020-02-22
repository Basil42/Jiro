using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEvent",menuName = "Event")]
public class GameEvent : ScriptableObject
{
    List<EventListener> listeners = new List<EventListener>();

    public void Raise()
    {
        foreach(var listener in listeners)
        {
            listener.OnRaise();
        }
    }
    public void AddListener(EventListener listener)
    {
        listeners.Add(listener);
    }
    public void RemoveListener(EventListener listener)
    {
        listeners.Remove(listener);
    }
}
