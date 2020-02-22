using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UImanager : MonoBehaviour
{
    public GameObject[] DisabledOnTrace;//Warning: objects will be reenabled after trace ends
    public polygonTracer tracerRef;
    public GameObject AssembleButton;
    public void OnTraceStart()//change this as soon as possible
    {
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
        if (!tracerRef.canAssemble()) AssembleButton.SetActive(false);
    }

    public void OnPlaneBuild()
    {

    }
    public void OnFailedPlaneBuild()
    {
        
    }
    
}
