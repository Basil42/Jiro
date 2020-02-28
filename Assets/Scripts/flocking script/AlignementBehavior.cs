using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "alignementbehavior",menuName = "Flocking/alignement")]
public class AlignementBehavior : FlockBehavior
{
    Vector3 currentVelocity;
    public override Vector3 CalculateMove(planeBehavior plane, List<Transform> context, Flock flock)
    {
        if (context.Count == 0) return Vector3.zero;
        Vector3 AlignMove = plane.transform.forward;
        foreach (var transform in context)
        {
            AlignMove += transform.forward;
        }
        AlignMove /= context.Count;
        AlignMove = Vector3.SmoothDamp(plane.transform.forward, AlignMove, ref currentVelocity, 0.3f);
        
        return AlignMove;
    }
}
