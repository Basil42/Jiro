﻿using System.Collections;
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
    public float DefaultTopLimit = 5f;
    public float DefaultBotLimit = -5f;
    public float DefaultLeftLimit = -5f;
    public float DefaultRightLimit = 5f;
    public Mesh mesh;
    Vector3[] sideTracePoints;
    Vector3[] frontTracePoints;
    Vector3[] wingTracePoints;
    
    public float TopLimit = 500;
    public float BotLimit = -500;

    private LineRenderer line;
    private Camera _cam ;
    [Header("Events")]
    public GameEvent StartTrace;
    public GameEvent StopTrace;
    public GameEvent PlaneBuilt;
    public GameEvent FailBuildingPlane;
    [Header("Debug")]
    public float currentStep;
    public MeshFilter meshholder;
    public MeshFilter meshVis;
    
    public IEnumerator Trace()
    {
        
        StartTrace.Raise();
        if(limiter == direction || direction == TraceDirection.wings)
        {
            TopLimit = DefaultTopLimit;
            BotLimit = DefaultBotLimit;
        }
        Vector3 previousposition = _cam.ScreenToWorldPoint(Input.mousePosition +Vector3.forward*10.0f);
        Vector3 currentPosition;
        List<Vector3> points = new List<Vector3>();
        if(!(previousposition.y > TopLimit || previousposition.y < BotLimit || previousposition.x < DefaultLeftLimit|| previousposition.x > DefaultRightLimit)) points.Add(previousposition);
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
            if (currentStep > tracingStep && !(currentPosition.y>TopLimit || currentPosition.y<BotLimit || currentPosition.x < DefaultLeftLimit || currentPosition.x > DefaultRightLimit))//add a point in the tracing
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
            if (direction == TraceDirection.wings && limiter == TraceDirection.front)
            {
                findLimits(frontTracePoints, limiter);
            }else if(direction == TraceDirection.wings && limiter == TraceDirection.side)
            {
                findLimits(sideTracePoints, limiter);
            }
            StopTrace.Raise();
            yield break;
        }
        line.positionCount++;
        line.SetPosition(line.positionCount - 1, line.GetPosition(0));
        switch (direction)
        {
            case (TraceDirection.side):
                if (frontTracePoints == null)findLimits(points.ToArray(),TraceDirection.side);
                sideTracePoints = points.ToArray();
                break;
            case (TraceDirection.front):
                if (sideTracePoints == null) findLimits(points.ToArray(), TraceDirection.front);
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

    private void findLimits(Vector3[] points,TraceDirection direction)
    {
        TopLimit = DefaultTopLimit;
        BotLimit = DefaultBotLimit;
        
        //foreach (var point in points)
        //{
        //    if (point.y > TopLimit) TopLimit = point.y;
        //    else if (point.y < BotLimit) BotLimit = point.y;
        //}
        limiter = direction;
    }

    private bool AssembleMesh()
    {
        if (sideTracePoints == null || frontTracePoints == null || wingTracePoints== null)
        {
            FailBuildingPlane.Raise();
            return false;
        }
        //build side "cylinder"
        Mesh sidemesh = new Mesh();
        assembleViewmesh(sidemesh,TraceDirection.side);
        //rotate the side mesh
        
        
        List<Vector3> rotVert = new List<Vector3>();
        rotVert.Capacity = sidemesh.vertexCount;
        sidemesh.GetVertices(rotVert);
        Quaternion rotation = Quaternion.AngleAxis(90, Vector3.up);
        for(int i = 0; i < sidemesh.vertexCount; i++)
        {
            rotVert[i] = rotation * rotVert[i];
        }
        sidemesh.SetVertices(rotVert);

        //only do uvs now if absolutely necessary
        //center the whole thing by translating it by the coordinates of the average vertex
        Mesh frontmesh = new Mesh();
        assembleViewmesh(frontmesh,TraceDirection.front);
        //create wingmesh
        Mesh wingMesh = new Mesh();
        assembleWingmesh(wingMesh);
        //rotate and place wing mesh
        rotVert = new List<Vector3>();
        rotVert.Capacity = wingMesh.vertexCount;
        wingMesh.GetVertices(rotVert);
        rotation = Quaternion.AngleAxis(90, Vector3.right);//might be actually left or need to be rotated around y as well
        for(int i =0; i < wingMesh.vertexCount; i++)
        {
            rotVert[i] = rotation * rotVert[i];
        }
        wingMesh.SetVertices(rotVert);
        //assemble into single mesh
        Mesh resultMesh = new Mesh();
        mesh = new Mesh();
        Collapsemeshes(resultMesh, sidemesh, frontmesh);
        AssemblePlane(resultMesh, wingMesh, mesh);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        PlaneBuilt.Raise();
        return true;
    }

    private void AssemblePlane(Mesh BodyMesh, Mesh wingMesh, Mesh targetMesh)
    {
        CombineInstance[] combine = new CombineInstance[2];
        combine[1].mesh = BodyMesh;
        combine[1].transform = Matrix4x4.identity;
        BodyMesh.RecalculateBounds();
        combine[0].mesh = wingMesh;
        var wingOffsetValue = BodyMesh.bounds.center.y;
        combine[0].transform = Matrix4x4.Translate(Vector3.up*wingOffsetValue);
        //combine[0]
        
        targetMesh.CombineMeshes(combine);
        
        Debug.Log("body verts: " + BodyMesh.vertexCount + " wing verts :" + wingMesh.vertexCount + " result in " + targetMesh.vertexCount + " of " + (wingMesh.vertexCount + BodyMesh.vertexCount) + " expected.");
        if (targetMesh != mesh) Debug.LogError("mesh not properly assigned.");
    }

    void Collapsemeshes(Mesh outputMesh, Mesh sideMesh, Mesh frontMesh)
    {
        List<Vector3> resultVerts = new List<Vector3>();
        List<int> resultTris = new List<int>();//needs to offset all indices for the second half of it;
        //side on front
        {
            Physics.autoSimulation = false;

            MeshCollider targetcollider = meshholder.GetComponent<MeshCollider>();
            //meshholder.mesh = frontMesh;
            //meshVis.mesh = sideMesh;
            targetcollider.sharedMesh = frontMesh;

            LayerMask targetlayer = LayerMask.NameToLayer("meshHolder");
            List<Vector3> projectedVerts = new List<Vector3>();
            List<int> projTris = new List<int>();
            projTris.AddRange(sideMesh.GetTriangles(0));
            sideMesh.GetVertices(projectedVerts);

            RaycastHit hit;
            bool CompleteMiss = true;
            sideMesh.RecalculateBounds();
            sideMesh.RecalculateNormals();
            sideMesh.RecalculateTangents();
            frontMesh.RecalculateBounds();
            frontMesh.RecalculateNormals();
            frontMesh.RecalculateTangents();
            //yield return new WaitForFixedUpdate();
            //yield return new WaitForFixedUpdate();
            Physics.Simulate(0.1f);
            int hitCount = 0;

            Ray ray = new Ray();
            ray.direction = Vector3.right;
            for (int i = 0; i < projectedVerts.Count / 2; i++)
            {
                ray.origin = projectedVerts[i];

                if (targetcollider.Raycast(ray, out hit, 500f))
                {
                    CompleteMiss = false;
                    //Debug.DrawRay(projectedVerts[i], Vector3.right * 10f, Color.blue, 1f, true);

                    hitCount++;

                    projectedVerts[i] = hit.point;

                }
                else//vertex is outside the defined shape of the other shape
                {
                    //Debug.DrawRay(projectedVerts[i], Vector3.right * 10f, Color.red, 1.0f, true);
                    //remove all triangle involving this vertex
                    for (int t = 0; t < projTris.Count;)
                    {
                        if (projTris[t] == i || projTris[t + 1] == i || projTris[t + 2] == i)
                        {
                            projTris.RemoveRange(t, 3);

                        }
                        else
                        {
                            t += 3;
                        }
                    }
                }
            }

            if (CompleteMiss == true) Debug.Log("All vertices missed the target shape.");
            ray.direction = -Vector3.right;
            for (int i = projectedVerts.Count / 2; i < projectedVerts.Count; i++)
            {
                ray.origin = projectedVerts[i];

                if (targetcollider.Raycast(ray, out hit, 500f))
                {
                    CompleteMiss = false;
                    //Debug.Log("hit");
                    //Debug.DrawRay(projectedVerts[i], -Vector3.right * 10f, Color.blue, 500f, true);
                    hitCount++;
                    projectedVerts[i] = hit.point;
                }
                else//vertex is outside the defined shape of the other shape
                {
                    for (int t = 0; t < projTris.Count;)
                    {
                        if (projTris[t] == i || projTris[t + 1] == i || projTris[t + 2] == i)
                        {
                            projTris.RemoveRange(t, 3);

                        }
                        else
                        {
                            t += 3;
                        }
                    }
                    //Debug.DrawRay(projectedVerts[i], -Vector3.right * 10f, Color.red, 500.0f, true);
                    //remove all triangle involving this vertex
                }
            }
            Debug.Log(hitCount + "hits, out of " + projectedVerts.Count);
            Physics.autoSimulation = true;
            resultVerts.AddRange(projectedVerts);
            resultTris.AddRange(projTris);
        }
        //front on side
        {
            Physics.autoSimulation = false;

            MeshCollider targetcollider = meshholder.GetComponent<MeshCollider>();
            //meshholder.mesh = frontMesh;
            //meshVis.mesh = sideMesh;
            targetcollider.sharedMesh = sideMesh;

            LayerMask targetlayer = LayerMask.NameToLayer("meshHolder");
            List<Vector3> projectedVerts = new List<Vector3>();
            List<int> projTris = new List<int>();
            projTris.AddRange(frontMesh.GetTriangles(0));
            frontMesh.GetVertices(projectedVerts);

            RaycastHit hit;
            bool CompleteMiss = true;
            
            Physics.Simulate(0.1f);
            int hitCount = 0;

            Ray ray = new Ray();
            ray.direction = Vector3.forward;
            for (int i = 0; i < projectedVerts.Count / 2; i++)
            {
                ray.origin = projectedVerts[i];

                if (targetcollider.Raycast(ray, out hit, 500f))
                {
                    CompleteMiss = false;
                    //Debug.DrawRay(projectedVerts[i], Vector3.right * 10f, Color.blue, 1f, true);

                    hitCount++;

                    projectedVerts[i] = hit.point;

                }
                else//vertex is outside the defined shape of the other shape
                {
                    Debug.DrawRay(projectedVerts[i], Vector3.right * 10f, Color.red, 1.0f, true);
                    //remove all triangle involving this vertex
                    for (int t = 0; t < projTris.Count;)
                    {
                        if (projTris[t] == i || projTris[t + 1] == i || projTris[t + 2] == i)
                        {
                            projTris.RemoveRange(t, 3);

                        }
                        else
                        {
                            t += 3;
                        }
                    }
                }
            }

            if (CompleteMiss == true) Debug.Log("All vertices missed the target shape.");
            ray.direction = -Vector3.forward;
            for (int i = projectedVerts.Count / 2; i < projectedVerts.Count; i++)
            {
                ray.origin = projectedVerts[i];

                if (targetcollider.Raycast(ray, out hit, 500f))
                {
                    CompleteMiss = false;
                    //Debug.Log("hit");
                    //Debug.DrawRay(projectedVerts[i], -Vector3.right * 10f, Color.blue, 500f, true);
                    hitCount++;
                    projectedVerts[i] = hit.point;
                }
                else//vertex is outside the defined shape of the other shape
                {
                    for (int t = 0; t < projTris.Count;)
                    {
                        if (projTris[t] == i || projTris[t + 1] == i || projTris[t + 2] == i)
                        {
                            projTris.RemoveRange(t, 3);

                        }
                        else
                        {
                            t += 3;
                        }
                    }
                    //Debug.DrawRay(projectedVerts[i], -Vector3.right * 10f, Color.red, 500.0f, true);
                    //remove all triangle involving this vertex
                }
            }
            Debug.Log(hitCount + "hits, out of " + projectedVerts.Count);
            Physics.autoSimulation = true;
            //offset triangles to new vertex index
            int vertoffset = resultVerts.Count;
            for (int i = 0; i < projTris.Count; i++)
            {
                projTris[i] += vertoffset;
            }

            resultVerts.AddRange(projectedVerts);
            resultTris.AddRange(projTris);
        }
        
        outputMesh.SetVertices(resultVerts);
        outputMesh.SetTriangles(resultTris, 0);

        //outputMesh.RecalculateBounds();
        //outputMesh.RecalculateNormals();
        //outputMesh.RecalculateTangents();
        //outputMesh.Optimize();
        //uv mapping
        Debug.Log(outputMesh.vertices.Length + " vert and " + outputMesh.triangles.Length / 3 + " tris");
    }

    private void assembleViewmesh(Mesh targetMesh,TraceDirection direction)
    {
        Vector3[] TracePoints;
        switch (direction)
        {
            case TraceDirection.side:
                TracePoints = sideTracePoints;
                break;
            case TraceDirection.front:
                TracePoints = frontTracePoints;
                break;
            case TraceDirection.wings:
                TracePoints = wingTracePoints;
                break;
            default:
                TracePoints = new Vector3[0];
                break;
        }
        var Vert = new Vector3[(TracePoints.Length * 2)];
        var tris = new int[(TracePoints.Length * 2) * 3];
        //populate vertices array
        {
            //trace to vertices
            TracePoints.CopyTo(Vert, 0);
            for (int i = TracePoints.Length - 1; i >= 0; i--)
            {
                Vert[i].z = -10;
            }
            //duplication
            TracePoints.CopyTo(Vert, TracePoints.Length);
            for (int i = TracePoints.Length; i < Vert.Length; i++)
            {
                Vert[i].z = 10;
            }
        }

        //set up triangles
        int length = TracePoints.Length;
        for (int i = 0; i < length - 1; i++)
        {
            tris[i * 6] = i;
            tris[(i * 6) + 1] = i + 1;
            tris[(i * 6) + 2] = i + length;
            tris[(i * 6) + 3] = i + 1;
            tris[(i * 6) + 4] = i + length + 1;
            tris[(i * 6) + 5] = i + length;
        }
        //closing the loop
        tris[(length - 1) * 6] = length - 1;
        tris[((length - 1) * 6) + 1] = 0;
        tris[((length - 1) * 6) + 2] = length - 1 + length;
        tris[((length - 1) * 6) + 3] = 0;
        tris[((length - 1) * 6) + 4] = length;
        tris[((length - 1) * 6) + 5] = length - 1 + length;
        targetMesh.SetVertices(Vert);
        targetMesh.SetTriangles(tris, 0, true);
    }
    private void assembleWingmesh(Mesh targetMesh)
    {
        if (wingTracePoints.Length < 3) return;
        int lowerIndex = 1;
        int upperIndex = wingTracePoints.Length - 1;
        bool upperstep = false;
        List<int> resultTris = new List<int>();
        resultTris.Capacity = (wingTracePoints.Length - 2) * 3;
        resultTris.Add(0);
        resultTris.Add(lowerIndex);
        resultTris.Add(upperIndex);
        while(lowerIndex < upperIndex)
        {
            resultTris.Add(upperIndex);
            resultTris.Add(lowerIndex);
            resultTris.Add(upperstep ? --upperIndex : ++lowerIndex);
            upperstep = !upperstep;
        }
        targetMesh.SetVertices(wingTracePoints);
        targetMesh.SetTriangles(resultTris, 0);
        
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
    public float getTopLimit()
    {
        return TopLimit;
    }
    public float getBotlimit()
    {
        return BotLimit;
    }
    public void RaycastTest()
    {
        //if (Physics.Raycast(Vector3.zero, Vector3.left * 10f, float.PositiveInfinity, LayerMask.NameToLayer("meshHolder")))
        //{
        //    Debug.Log("hitRegistered");
        //}
        //else
        //{
        //    Debug.Log("No hit registered");
        //    Debug.DrawRay(Vector3.zero, Vector3.left * 10f, Color.blue,5.0f);
        //}
        Vector3 direc;
        for(float f = 0f; f <= 2*Mathf.PI; f += 0.05f)
        {
            direc = new Vector3(Mathf.Cos(f), Mathf.Sin(f), 0f);
            if (Physics.Raycast(Vector3.zero, direc, LayerMask.NameToLayer("meshHolder")))
            {
                Debug.DrawRay(transform.position, direc * 50f,Color.blue,500f);
            }
            else
            {
                Debug.DrawRay(transform.position, direc * 50f, Color.red,500f);
            }
        }
    }
}
