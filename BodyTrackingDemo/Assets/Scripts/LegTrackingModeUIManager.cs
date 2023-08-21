using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.XR.PXR;

public class LegTrackingModeUIManager : MonoBehaviour
{
    public GameObject StartMenu;
    public Toggle FullBodyTrackingToggle;
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
        LegTrackingModeSceneManager.Instance.m_CurrentLegTrackingDemoState = LegTrackingModeSceneManager.LegTrackingDemoState.CALIBRATING;
    }

    public void OnFullBodyTrackingToggleValueChange(bool enable)
    {
        Debug.Log("[DragonTest] FullBodyTracking = " + enable);
        PXR_Input.SetSwiftMode(enable ? 1 : 0);
    }
}
