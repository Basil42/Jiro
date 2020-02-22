using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventListener : MonoBehaviour
{
    public UnityEvent Response;
    public GameEvent Event;
    private void OnEnable()
    {
        Event.AddListener(this);
    }
    private void OnDisable()
    {
        Event.RemoveListener(this);
    }
    public void OnRaise()
    {
        Response.Invoke();
    }
}
