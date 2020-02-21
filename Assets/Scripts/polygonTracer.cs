using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class polygonTracer : MonoBehaviour
{
    public enum TraceDirection
    {
        side,
        front,
        wings
    }
    [HideInInspector]public TraceDirection direction;
    [Header("Settings")]
    public float tracingStep = 1.0f;//spatial period of tracing sampling
    public float closingDistance = 1.0f;//maximum distance the game will try to close the loop of the players tracing
    bool isTracing = false;
    Mesh mesh;
    Vector3[] sideTracePoints;
    Vector3[] frontTracePoints;
    Vector3[] WingTracePoints;
    private LineRenderer line;
    public IEnumerator Trace()
    {
        isTracing = true;
        Vector3 previousposition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        List<Vector3> points = new List<Vector3>();
        points.Add(previousposition);
        while (Input.GetMouseButton(0))//keep looping as long as the user keeps the button pressed
        {
            if(Vector3.Distance(previousposition, Camera.main.ScreenToWorldPoint(Input.mousePosition)) > tracingStep)//add a point in the tracing
            {
                previousposition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                points.Add(previousposition);
                line.SetPositions(points.ToArray());
            }
            yield return null;
        }

        switch (direction)
        {
            case (TraceDirection.side):
                sideTracePoints = points.ToArray();
                break;
            case (TraceDirection.front):
                frontTracePoints = points.ToArray();
                break;
            case (TraceDirection.wings):
                WingTracePoints = points.ToArray();
                break;
            default:
                Debug.LogError("Invalid tracing direction/type");
                break;

        }
        isTracing = false;// swap this for an event?
    }

    public Mesh GetMesh()//this should give a copy, not a reference
    {
        return Instantiate(mesh);
    }
}
