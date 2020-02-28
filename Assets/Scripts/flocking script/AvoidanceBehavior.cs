using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "avoidanceBehavior", menuName = "Flocking/avoidance")]
public class AvoidanceBehavior : FlockBehavior
{
    public override Vector3 CalculateMove(planeBehavior plane, List<Transform> context, Flock flock)
    {
        if (context.Count == 0) return Vector3.zero;
        Vector3 AvoidMove = Vector3.zero;
        int nAvoid = 0;
        foreach (var transform in context)
        {
            if(Vector3.SqrMagnitude(transform.position - plane.transform.position) < flock.SquareAvoidanceRadius)
            {
                nAvoid++;
                AvoidMove += plane.transform.position - transform.position;
            }
            
        }
        if (nAvoid > 0) AvoidMove /= nAvoid;
        return AvoidMove;
    }
}
