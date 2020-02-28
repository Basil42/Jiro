using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "cohesionbehavior", menuName = "Flocking/cohesion")]
public class CohesionBehavior : FlockBehavior
{
    Vector3 currentVelocity;
    public float agentSmoothTime = 0.5f;
    public override Vector3 CalculateMove(planeBehavior plane, List<Transform> context, Flock flock)
    {
        if (context.Count == 0) return Vector3.zero;
        Vector3 cohesionMove = Vector3.zero;
        foreach (var transform in context)
        {
            cohesionMove += transform.position;
        }
        cohesionMove /= context.Count;

        cohesionMove -= plane.transform.position;
        
        cohesionMove = Vector3.SmoothDamp(plane.transform.forward, cohesionMove, ref currentVelocity, agentSmoothTime);
        return cohesionMove;
    }
}
