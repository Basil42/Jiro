using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class planeBehavior : MonoBehaviour
{
    Collider _collider;
    public Collider _Collider { get { return _collider; } }
    public static Flock flock;
    private void Awake()
    {
        _collider = GetComponent<Collider>();
        

    }
    private void Start()
    {
        
    }
    public void Move(Vector3 velocity)
    {
        if (velocity != Vector3.zero)
        {
            transform.forward = velocity;
            transform.position += velocity * Time.deltaTime;
        }
        else
        {
            transform.position += transform.forward * flock.SpeedCap * Time.deltaTime;
        }
    }
    private void OnEnable()
    {
        flock.AddAgent(this);
    }
    private void OnDisable()
    {
        flock.RemoveAgent(this);
    }
}
