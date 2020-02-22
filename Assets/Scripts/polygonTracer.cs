using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

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
    public float DefaultTopLimit = 500;
    public float DefaultBotLimit = -500;
    Mesh mesh;
    Vector3[] sideTracePoints;
    Vector3[] frontTracePoints;
    Vector3[] wingTracePoints;
    
    private float TopLimit = 500;
    private float BotLimit = -500;
    private LineRenderer line;
    private Camera _cam ;
    [Header("Events")]
    public GameEvent StartTrace;
    public GameEvent StopTrace;
    public GameEvent PlaneBuilt;
    public GameEvent FailBuildingPlane;
    [Header("Debug")]
    public float currentStep;
    
    public IEnumerator Trace()
    {
        
        StartTrace.Raise();
        Vector3 previousposition = _cam.ScreenToWorldPoint(Input.mousePosition +Vector3.forward*10.0f);
        Vector3 currentPosition;
        List<Vector3> points = new List<Vector3>();
        points.Add(previousposition);
        while (Input.GetMouseButton(0))//keep looping as long as the user keeps the button pressed
        {
            currentPosition = _cam.ScreenToWorldPoint(Input.mousePosition + Vector3.forward * 10.0f);
            currentStep = Vector3.Distance(previousposition, currentPosition);
            if (currentStep > tracingStep && !(currentPosition.y>TopLimit || currentPosition.y<BotLimit))//add a point in the tracing
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
            StopTrace.Raise();
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
                wingTracePoints = points.ToArray();
                break;
            default:
                Debug.LogError("Invalid tracing direction/type");
                break;

        }
        
        StopTrace.Raise();
        Debug.Log("trace done with " + line.positionCount + " points.");
    }
    private bool AssembleMesh()
    {
        if (sideTracePoints==null || frontTracePoints==null || wingTracePoints== null)
        {
            FailBuildingPlane.Raise();
            return false;
        }
        PlaneBuilt.Raise();
        return true;
    }
    public Mesh GetMesh()//this should give a copy, not a reference
    {
        if (mesh == null) return new Mesh();//return empty mesh if called before a mesh is built
        return Instantiate(mesh);
    }
    public void TryAssemble()
    {
        AssembleMesh();
        
    }
    public bool canAssemble()
    {
        return !(sideTracePoints == null || frontTracePoints == null || wingTracePoints == null);
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) StartCoroutine(Trace());

    }
    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        TopLimit = DefaultTopLimit;
        BotLimit = DefaultBotLimit;
        _cam = Camera.main;
    }
    private void setMode(TraceDirection mode)
    {
        direction = mode;
        switch (direction)
        {
            case TraceDirection.side:
                if(sideTracePoints != null)
                {
                    line.positionCount = sideTracePoints.Length;
                    line.SetPositions(sideTracePoints);
                }
                else
                {
                    line.positionCount = 0;
                }
                break;
            case TraceDirection.front:
                if (frontTracePoints != null)
                {
                    line.positionCount = frontTracePoints.Length;
                    line.SetPositions(frontTracePoints);
                }
                else
                {
                    line.positionCount = 0;
                }
                break;
            case TraceDirection.wings:
                if (wingTracePoints != null)
                {
                    line.positionCount = wingTracePoints.Length;
                    line.SetPositions(wingTracePoints);
                }
                else
                {
                    line.positionCount = 0;
                }
                break;
            default:
                break;
        }
    }
    public void setFrontView()
    {
        setMode(TraceDirection.front);
    }
    public void setSideView()
    {
        setMode(TraceDirection.side);
    }
    public void setWingView()
    {
        setMode(TraceDirection.wings);
    }

    public void Clear()
    {
        sideTracePoints = null;
        frontTracePoints = null;
        wingTracePoints = null;
        line.positionCount = 0;
        TopLimit = DefaultTopLimit;
        BotLimit = DefaultBotLimit;
    }
}
