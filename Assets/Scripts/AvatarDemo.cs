using System.Collections;
using System.Collections.Generic;
using RootMotion.Demos;
using RootMotion.FinalIK;
using UnityEngine;

public class AvatarDemo : MonoBehaviour
{
    public VRIKCalibrationController controller;
    void Start()
    {
        controller = GetComponent<VRIKCalibrationController>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M) /*|| InputBridge.Instance.AButtonDown*/)
        {
            Debug.Log("Avatar Demo");
            CalibrationIK();
        }

    }

    public void CalibrationIK()
    {
        VRIKCalibrator.Calibrate(controller.ik, controller.settings, controller.headTracker, controller.bodyTracker, controller.leftHandTracker, controller.rightHandTracker, controller.leftFootTracker, controller.rightFootTracker);

    }
}
