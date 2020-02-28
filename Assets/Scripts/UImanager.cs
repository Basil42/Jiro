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
    public float LookUpY;
    public float LookDownY;
    public GameObject wall;
    public GameObject[] DisabledOnLookUp;
    public GameObject[] EnabledOnlookUp;
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
    public void OnLookup()
    {
        StartCoroutine(LookUp());
    }
    public void OnLookDown()
    {
        StartCoroutine(LookDown());
    }
    IEnumerator LookDown()
    {
        foreach (var item in EnabledOnlookUp)
        {
            item.SetActive(false);
        }
        Vector3 pos = wall.transform.position;
        for(float i = 0.0f;i < 1; i += 0.005f)
        {
            pos.y = Mathf.SmoothStep(LookUpY, LookDownY, i);
            wall.transform.position = pos;
            yield return null;
        }
        foreach (var item in DisabledOnLookUp)
        {
            item.SetActive(true);
        }
        tracerRef.gameObject.SetActive(true);
        OnTraceStop();
    }
    IEnumerator LookUp()
    {
        tracerRef.gameObject.SetActive(false);
        foreach (var item in DisabledOnLookUp)
        {
            item.SetActive(false);
        }
        Vector3 pos = wall.transform.position;
        for (float i = 0.0f; i < 1; i += 0.005f)
        {
            pos.y = Mathf.SmoothStep(LookDownY, LookUpY, i);
            wall.transform.position = pos;
            yield return null;
        }
        foreach (var item in EnabledOnlookUp)
        {
            item.SetActive(true);
        }
    }
}
