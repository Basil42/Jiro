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
        GameObject plane = Instantiate(planeprefab, transform);
        plane.transform.position = transform.position;
        plane.transform.LookAt(transform.position + Random.onUnitSphere);
        plane.GetComponent<MeshFilter>().mesh = tracer.GetMesh();
    }
}
