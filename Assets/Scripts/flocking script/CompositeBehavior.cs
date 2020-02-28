using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "compositeBehavior", menuName = "Flocking/composite")]
public class CompositeBehavior : FlockBehavior
{
    public FlockBehavior[] behaviors;
    public float[] weights;
    public override Vector3 CalculateMove(planeBehavior plane, List<Transform> context, Flock flock)
    {
        if (weights.Length != behaviors.Length){
            Debug.LogError("Array length mismatch in " + name, this);
            return Vector3.zero;
        }
        Vector3 move = Vector3.zero;
        Vector3 partialMove;
        for (int i = 0; i < behaviors.Length; i++)
        {
            partialMove = behaviors[i].CalculateMove(plane, context, flock) * weights[i];
            if(partialMove != Vector3.zero)
            {
                if(partialMove.sqrMagnitude > weights[i] * weights[i])
                {
                    partialMove.Normalize();
                    partialMove *= weights[i];
                }
                move += partialMove;
            }
        }
        return move;
    }
    
}
