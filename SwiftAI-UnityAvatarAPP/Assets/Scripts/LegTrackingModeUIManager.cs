using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.XR.PXR;

public class LegTrackingModeUIManager : MonoBehaviour
{
    public GameObject StartMenu;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnDemoStart()
    {
        StartMenu.SetActive(false);
        PXR_Input.OpenFitnessBandCalibrationAPP();
        LegTrackingModeSceneManager.Instance.CurrentLegTrackingDemoState = LegTrackingModeSceneManager.LegTrackingDemoState.CALIBRATING;
    }
}
