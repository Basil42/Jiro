using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;

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
    private TraceDirection limiter;
    [Header("Settings")]
    public float tracingStep = 1.0f;//spatial period of tracing sampling
    public float closingDistance = 1.0f;//maximum distance the game will try to close the loop of the players tracing
    public float DefaultTopLimit = 500f;
    public float DefaultBotLimit = -500f;
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
    public MeshFilter meshtester;
    
    public IEnumerator Trace()
    {
        
        StartTrace.Raise();
        if(limiter == direction)
        {
            TopLimit = DefaultTopLimit;
            BotLimit = DefaultBotLimit;
        }
        Vector3 previousposition = _cam.ScreenToWorldPoint(Input.mousePosition +Vector3.forward*10.0f);
        Vector3 currentPosition;
        List<Vector3> points = new List<Vector3>();
        if(!(previousposition.y > TopLimit || previousposition.y < BotLimit)) points.Add(previousposition);
        else
        {
            line.positionCount = 0;
            StopTrace.Raise();
            yield break;
        }
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
                if (frontTracePoints == null)findLimits(points,TraceDirection.side);
                sideTracePoints = points.ToArray();
                break;
            case (TraceDirection.front):
                if (sideTracePoints == null) findLimits(points, TraceDirection.front);
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

    private void findLimits(List<Vector3> points,TraceDirection direction)
    {
        TopLimit = DefaultBotLimit;
        BotLimit = DefaultTopLimit;

        foreach (var point in points)
        {
            if (point.y > TopLimit) TopLimit = point.y;
            else if (point.y < BotLimit) BotLimit = point.y;
        }
        limiter = direction;
    }

    private bool AssembleMesh()
    {
        if (sideTracePoints == null || frontTracePoints == null /*|| wingTracePoints== null*/)
        {
            FailBuildingPlane.Raise();
            return false;
        }
        //build side "cylinder"
        Mesh sidemesh = new Mesh();
        assembleViewmesh(sidemesh);
        //rotate the side mesh
        List<Vector3> rotatedVert = new List<Vector3>();
        rotatedVert.Capacity = sidemesh.vertexCount;
        List<Vector3> rawVert = new List<Vector3>();
        rawVert.Capacity = sidemesh.vertexCount;
        sidemesh.GetVertices(rawVert);
        Quaternion rotation = Quaternion.AngleAxis(90, Vector3.up);
        for(int i = 0; i < sidemesh.vertexCount; i++)
        {
            rotatedVert[i] = rotation * rawVert[i];
        }
        //only do uvs now if absolutely necessary
        //center the whole thing by translating it by the coordinates of the average vertex
        Mesh frontmesh = new Mesh();
        assembleViewmesh(frontmesh);
        
        Mesh resultMesh = new Mesh();
        AddProjection(resultMesh, sidemesh, frontmesh,Vector3.left);
        AddProjection(resultMesh, frontmesh, sidemesh,Vector3.forward);
        //front mesh
        PlaneBuilt.Raise();
        return true;
    }

    private void AddProjection(Mesh outputMesh, Mesh projectedMesh, Mesh projectionTarget,Vector3 direction)
    {
        meshtester.mesh = projectionTarget;
        LayerMask targetlayer = LayerMask.NameToLayer("meshHolder");
        Vector3 projDirection = 
        foreach (var vertex in projectedMesh.vertices)
        {
            if(Physics.Raycast(origin: vertex,direction))
        }
    }

    private void assembleViewmesh(Mesh sidemesh)
    {
        var sideVert = new Vector3[(sideTracePoints.Length * 2)];
        var sidetris = new int[(sideTracePoints.Length * 2) * 3];
        //populate vertices array
        {
            //trace to vertices
            sideTracePoints.CopyTo(sideVert, 0);
            for (int i = sideTracePoints.Length - 1; i >= 0; i--)
            {
                sideVert[i].z = -50;
            }
            //duplication
            sideTracePoints.CopyTo(sideVert, sideTracePoints.Length);
            for (int i = sideTracePoints.Length; i < sideVert.Length; i++)
            {
                sideVert[i].z = 50;
            }
        }

        //set up triangles
        int length = sideTracePoints.Length;
        for (int i = 0; i < length - 1; i++)
        {
            sidetris[i * 6] = i;
            sidetris[(i * 6) + 1] = i + 1;
            sidetris[(i * 6) + 2] = i + length;
            sidetris[(i * 6) + 3] = i + 1;
            sidetris[(i * 6) + 4] = i + length + 1;
            sidetris[(i * 6) + 5] = i + length;
        }
        //closing the loop
        sidetris[(length - 1) * 6] = length - 1;
        sidetris[((length - 1) * 6) + 1] = 0;
        sidetris[((length - 1) * 6) + 2] = length - 1 + length;
        sidetris[((length - 1) * 6) + 3] = 0;
        sidetris[((length - 1) * 6) + 4] = length;
        sidetris[((length - 1) * 6) + 5] = length - 1 + length;
        sidemesh.SetVertices(sideVert);
        sidemesh.SetTriangles(sidetris, 0, true);
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
        return !(sideTracePoints == null || frontTracePoints == null /*|| wingTracePoints == null*/);
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
    public float getTopLimit()
    {
        return TopLimit;
    }
    public float getBotlimit()
    {
        return BotLimit;
    }
}
