using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
    [Header("Debug")]
    public float currentStep;
    
    public IEnumerator Trace()
    {
        isTracing = true;
        Vector3 previousposition = Camera.main.ScreenToWorldPoint(Input.mousePosition +Vector3.forward*10.0f);
        Vector3 currentPosition;
        List<Vector3> points = new List<Vector3>();
        points.Add(previousposition);
        while (Input.GetMouseButton(0))//keep looping as long as the user keeps the button pressed
        {
            currentPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition + Vector3.forward * 10.0f);
            currentStep = Vector3.Distance(previousposition, currentPosition);
            if (currentStep > tracingStep)//add a point in the tracing
            {
                previousposition =currentPosition;
                points.Add(previousposition);
                line.positionCount = points.Count;
                line.SetPositions(points.ToArray());//not ideal, should probably pre allocate the memory for this
                
            }
            yield return null;
        }
        if (Vector3.Distance(points[points.Count - 1], points[0]) > closingDistance)
        {
            line.positionCount = 0;
            yield break;
        }
        line.positionCount++;
        line.SetPosition(line.positionCount - 1, line.GetPosition(0));
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
        Debug.Log("trace done with " + line.positionCount + " points.");
    }

    public Mesh GetMesh()//this should give a copy, not a reference
    {
        return Instantiate(mesh);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) StartCoroutine(Trace());

    }
    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        
    }
    public void setMode(TraceDirection mode)
    {
        direction = mode;
        switch (direction)
        {
            case TraceDirection.side:
                if(sideTracePoints != null)
                {

                }
                else
                {

                }
                break;
            case TraceDirection.front:
                break;
            case TraceDirection.wings:
                break;
            default:
                break;
        }
    }
        
}
