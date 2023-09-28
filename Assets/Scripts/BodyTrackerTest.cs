using System.Collections;
using System.Collections.Generic;
using RootMotion.Demos;
using UnityEngine;
using Unity.XR.PXR;
using UnityEngine.UI;
public class BodyTrackerTest : MonoBehaviour
{

    BodyTrackerResult bodyTrackerResult = new BodyTrackerResult();

    public Transform headAnchor;
    public Transform rightFootAnchor;
    public Transform leftFootAnchor;
    public Transform modelScale;

    private Vector3 tempRightFootPosition;
    private Vector3 tempLeftFootPosition;
    public VRIKCalibrationController controller;

    private bool bodytrackingenable = true;
    // Start is called before the first frame update
    void Start()
    {
         bodyTrackerResult.trackingdata = new BodyTrackerTransform[24];
         bodyTrackerResult.trackingdata[7].localpose = new BodyTrackerTransPose();
         bodyTrackerResult.trackingdata[8].localpose = new BodyTrackerTransPose();

        // PXR_Input.SetBodyTrackingStaticCalibState
        // PXR_Input.GetBodyTrackingImuData
    }

    // Update is called once per frame
    void Update()
    {
        if (bodytrackingenable)
        {
            PXR_Input.GetBodyTrackingPose(0f,ref bodyTrackerResult);
            // Debug.Log("GetBodyTrackingPose " + bodyTrackerResult.rightAnkle.pose.PosX);
            // Debug.Log("GetBodyTrackingPose " + bodyTrackerResult.leftAnkle.pose.PosX);
            tempRightFootPosition = new Vector3((float)bodyTrackerResult.trackingdata[8].localpose.PosX, (float)bodyTrackerResult.trackingdata[8].localpose.PosY, (float)bodyTrackerResult.trackingdata[8].localpose.PosZ);
            rightFootAnchor.position = modelScale.localScale.x * (tempRightFootPosition - headAnchor.position) + headAnchor.position + controller.settings.headOffset;
            // rightFootAnchor.position = new Vector3((float)bodyTrackerResult.rightAnkle.pose.PosX, (float)bodyTrackerResult.rightAnkle.pose.PosY, (float)bodyTrackerResult.rightAnkle.pose.PosZ);
            // Debug.Log("GetBodyTrackingPose " + rightFootAnchor.position);
            rightFootAnchor.rotation = new Quaternion((float)bodyTrackerResult.trackingdata[8].localpose.RotQx, (float)bodyTrackerResult.trackingdata[8].localpose.RotQy, (float)bodyTrackerResult.trackingdata[8].localpose.RotQz, (float)bodyTrackerResult.trackingdata[8].localpose.RotQw);
            // Debug.Log("GetBodyTrackingPose " + rightFootAnchor.rotation);
            tempLeftFootPosition = new Vector3((float)bodyTrackerResult.trackingdata[7].localpose.PosX, (float)bodyTrackerResult.trackingdata[7].localpose.PosY, (float)bodyTrackerResult.trackingdata[7].localpose.PosZ);
            leftFootAnchor.position = modelScale.localScale.x * (tempLeftFootPosition - headAnchor.position) + headAnchor.position + controller.settings.headOffset;
            // Debug.Log("GetBodyTrackingPose " + leftFootAnchor.position);
            leftFootAnchor.rotation = new Quaternion((float)bodyTrackerResult.trackingdata[7].localpose.RotQx, (float)bodyTrackerResult.trackingdata[7].localpose.RotQy, (float)bodyTrackerResult.trackingdata[7].localpose.RotQz, (float)bodyTrackerResult.trackingdata[7].localpose.RotQw);
            // Debug.Log("GetBodyTrackingPose " + leftFootAnchor.rotation);

        }
    }

    /// <summary>
    /// Enable body tracking data update
    /// </summary>
    public void EnableBodyTracking() {
        bodytrackingenable = true;
    }

    /// <summary>
    /// Disable body tracking data update
    /// </summary>
    public void DisableBodyTracking() {
        bodytrackingenable = false;
    }
}
