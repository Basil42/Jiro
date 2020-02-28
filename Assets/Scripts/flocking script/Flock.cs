using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Flock : MonoBehaviour
{
    public planeBehavior planeprefab;
    List<planeBehavior> agents = new List<planeBehavior>();
    public FlockBehavior behavior;

    [Range(1f, 100f)]
    public float SpeedFactor = 10f;

    [Range(1f, 100f)]
    public float SpeedCap = 5f;

    [Range(1f, 10f)]
    public float NeighborRadius = 1.5f;

    [Range(0f, 1f)]
    public float AvoidanceRadiusMultiplier = 0.5f;

    float squareMaxSpeed;
    float squareNeighborRadius;
    float squareAvoidanceRadius;
    public float SquareAvoidanceRadius{get{return squareAvoidanceRadius; }}
    public void AddAgent(planeBehavior agent) 
    {
        agents.Add(agent);
    }
    public void RemoveAgent(planeBehavior agent)
    {
        agents.Remove(agent);
    }
    private void Awake()
    {
        squareMaxSpeed = SpeedCap * SpeedCap;
        squareNeighborRadius = NeighborRadius * NeighborRadius;
        squareAvoidanceRadius = squareNeighborRadius * AvoidanceRadiusMultiplier * AvoidanceRadiusMultiplier;
        planeBehavior.flock = this;
    }
    private void Update()
    {
        foreach(var agent in agents)
        {
            List<Transform> context = getNearbyObject(agent);
            Vector3 move = behavior.CalculateMove(agent, context, this);
            move *= SpeedFactor;
            if(move.sqrMagnitude > squareMaxSpeed)
            {
                move = move.normalized * SpeedCap;
            }
            agent.Move(move);
        }
    }

    private List<Transform> getNearbyObject(planeBehavior agent)
    {
        List<Transform> context = new List<Transform>();
        Collider[] contextcolliders = Physics.OverlapSphere(agent.transform.position, NeighborRadius); 
        foreach(var collider in contextcolliders)
        {
            if(collider != agent._Collider)
            {
                context.Add(collider.transform);
            }
        }
        return context;
    }
}
