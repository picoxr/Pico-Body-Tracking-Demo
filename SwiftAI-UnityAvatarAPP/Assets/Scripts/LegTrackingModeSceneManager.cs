using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.PXR;
using UnityEngine.XR;

public class LegTrackingModeSceneManager : MonoBehaviour
{
    public static LegTrackingModeSceneManager Instance;
    public LegTrackingModeUIManager LegTrackingUIManager;
    public LegTrackingDemoState CurrentLegTrackingDemoState;

    public GameObject MirrorObj;

    private GameObject m_AvatarObj;
    private bool m_SwiftCalibratedState;

    public enum LegTrackingDemoState
    {
        START,
        CALIBRATING,
        CALIBRATED,
        PLAYING,
    }

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        CurrentLegTrackingDemoState = LegTrackingDemoState.START;
        MirrorObj.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (CurrentLegTrackingDemoState == LegTrackingDemoState.PLAYING)
        {
            if (InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool right_pressed) && right_pressed ||
                InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool left_pressed) && left_pressed)
            {
                PXR_Input.OpenFitnessBandCalibrationAPP();
            }
        }
    }

    private void OnApplicationFocus(bool focus)
    {
        Debug.Log("[SwiftDemoTest] Application focus: " + focus);
        

        if (focus)
        {
            if (CurrentLegTrackingDemoState == LegTrackingDemoState.START) return;

            //Update Swift calibration state after resuming
            int calibrated = -1;
            PXR_Input.GetFitnessBandCalibState(ref calibrated);
            m_SwiftCalibratedState = calibrated == 1;
            Debug.Log("[SwiftDemoTest] Swift calibrated: " + calibrated);
            if (CurrentLegTrackingDemoState == LegTrackingDemoState.CALIBRATING && m_SwiftCalibratedState)
            {
                CurrentLegTrackingDemoState = LegTrackingDemoState.CALIBRATED;

                //load avatar
                MirrorObj.SetActive(true);
                LoadAvatar();
                //SetControllersActive(false);
            }
            else
            {
                LegTrackingUIManager.StartMenu.SetActive(true);
            }
        }
    }


    private void LoadAvatar()
    {
        Debug.Log("[LegDemoTest] LoadAvatar");
        if (m_AvatarObj != null)
        {
            GameObject temp = m_AvatarObj;
            m_AvatarObj = null;
            GameObject.Destroy(temp);
        }

        // current logic use only one Avatar model for each gender.
        string avatar_name = "ClothRun_175 Variant LegTracking";
        Debug.Log("[LegDemoTest] Models/" + avatar_name);
        GameObject avatarObj = Resources.Load<GameObject>("Models/" + avatar_name);
        m_AvatarObj = Instantiate(avatarObj, Vector3.zero, Quaternion.identity);
        float scale = 1; // use 1 for leg tracking mode
        m_AvatarObj.transform.localScale = new Vector3(scale, scale, scale);
        Debug.Log("[LegDemoTest] m_AvatarObj" + m_AvatarObj);
        m_AvatarObj.SetActive(true);

        CurrentLegTrackingDemoState = LegTrackingDemoState.PLAYING;
    }
}
