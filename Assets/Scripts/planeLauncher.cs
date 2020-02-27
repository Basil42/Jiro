using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class planeLauncher : MonoBehaviour
{
    //[HideInInspector]public Mesh meshRef;
    public GameObject planeprefab;
    public polygonTracer tracer;
    public void OnPlaneBuilt()
    {
        GameObject plane = Instantiate(planeprefab);
        plane.GetComponent<MeshFilter>().mesh = tracer.GetMesh();
    }
}
