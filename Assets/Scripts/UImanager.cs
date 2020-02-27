using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UImanager : MonoBehaviour
{
    public GameObject[] DisabledOnTrace;//Warning: objects will be reenabled after trace ends
    public polygonTracer tracerRef;
    public GameObject AssembleButton;
    public GameObject limiterPlane;
    private Material limiterPlaneMat;
    public void OnTraceStart()//change this as soon as possible
    {
        refreshLimitPlane();
        foreach (var gameObject in DisabledOnTrace)
        {
            gameObject.SetActive(false);
        }
    }
    public void OnTraceStop()
    {
        foreach (var gameObject in DisabledOnTrace)
        {
            gameObject.SetActive(true);
        }
        if (!tracerRef.canAssemble())
        {
            AssembleButton.SetActive(false);
            Debug.Log("can't assemble.");
        }
        refreshLimitPlane();
    }

    private void refreshLimitPlane()
    {
        limiterPlaneMat.SetVector("limitBounds", new Vector4(tracerRef.getBotlimit(), tracerRef.getTopLimit(), 0f, 0f));
    }

    public void OnPlaneBuild()
    {

    }
    public void OnFailedPlaneBuild()
    {

    }
    private void Awake()
    {
        limiterPlaneMat = limiterPlane.GetComponent<Renderer>().material;
    }
    private void Start()
    {
        refreshLimitPlane();
    }
    public void OnRefreshLimitDisplay()
    {
        refreshLimitPlane();
    }
}
