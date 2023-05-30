using System;
using System.Collections;
using System.Collections.Generic;
using RootMotion.FinalIK;
using TMPro;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI HeadPositionData;
    public TextMeshProUGUI HeadRotationData;
    public TextMeshProUGUI FootPoseData;
    public TextMeshProUGUI VRIKFootPoseData;
    public TextMeshProUGUI VRIKModelScale;

    public Transform Head;
    public Transform LeftHand;
    public Transform RightHand;
    public Transform LeftFoot;
    public Transform RightFoot;

    public VRIK vrik;

    public GameObject[] BodyNodes;
    public GameObject AvatarObj;
    public GameObject FootballObj;
    public GameObject leftfoot;
    public GameObject rightfoot;
    public GameObject ground;

    private bool IsAvatar = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HeadPositionData.text = "HeadPosition " + Head.transform.position.ToString("f3") + "\n LeftHandPosition " + LeftHand.transform.position.ToString("f3") + "\n RightHandPosition " +
        RightHand.transform.position.ToString("f3") + "\n LeftFootPosition " + LeftFoot.transform.position.ToString("f3") + "\n RightFootPosition " + RightFoot.transform.position.ToString("f3");

        HeadRotationData.text = "HeadRotation " + Head.transform.rotation.ToString("f3") + "\n LeftHandRotation " + LeftHand.transform.rotation.ToString("f3") + "\n RightHandRotation " +
        RightHand.transform.rotation.ToString("f3") + "\n LeftFootRotation " + LeftFoot.transform.rotation.ToString("f3") + "\n RightFootRotation " + RightFoot.transform.rotation.ToString("f3");

        FootPoseData.text = "Head LocalPosition " + Head.transform.localPosition.ToString("f3") + "\nHead LocalRotation" + Head.transform.localRotation.ToString("f3") + "\n LeftFoot LocalPositon " +
        LeftFoot.transform.localPosition.ToString("f3") + "\n LeftFoot localRotation " + LeftFoot.transform.localRotation.ToString("f3") + "\n RightFoot LocalPosition " + RightFoot.transform.localPosition.ToString("f3") + "\n RightFoot localRotation " + RightFoot.transform.localRotation.ToString("f3");

        VRIKFootPoseData.text = "VRIK LeftFoot Position \n" + vrik.references.leftFoot.position.ToString("f3") + " \nVRIK LeftFoot Rotation \n" + vrik.references.leftFoot.rotation.ToString("f3") + "\nVRIK RightFoot Position\n" + vrik.references.rightFoot.position.ToString("f3") + "\nVRIK RightFoot Rotation\n" + vrik.references.rightFoot.rotation.ToString("f3");
        VRIKModelScale.text = " Model Scale " + vrik.transform.localScale.x;
        Debug.Log("[SwiftDemo] Avatar Info " + HeadPositionData.text.Replace("\n", "") + " " + HeadRotationData.text.Replace("\n", "") + " " + FootPoseData.text.Replace("\n", "") + " " + VRIKFootPoseData.text.Replace("\n", ""));
    }

    /// <summary>
    /// Switch between Avatar and Nodes
    /// </summary>
    public void OnClickBtnSwitch()
    {
        if (IsAvatar == false)
        {
            IsAvatar = !IsAvatar;
            for (int i = 0; i < BodyNodes.Length; i++)
            {
                BodyNodes[i].transform.localScale = Vector3.zero;
            }
            AvatarObj.SetActive(true);
        }
        else
        {
            IsAvatar = !IsAvatar;
            for (int i = 0; i < BodyNodes.Length; i++)
            {
                BodyNodes[i].transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            }
            AvatarObj.SetActive(false);
        }
    }

    /// <summary>
    /// Reset the scene
    /// </summary>
    public void OnClickBtnReset()
    {
        FootballObj.transform.position = new Vector3(0.2f, 1.0f, 0.2f);
        FootballObj.GetComponent<Rigidbody>().velocity = Vector3.zero;
        FootballObj.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        leftfoot.GetComponent<SphereCollider>().enabled = true;
        leftfoot.GetComponent<Rigidbody>().velocity = Vector3.zero;
        leftfoot.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        leftfoot.transform.localPosition = new Vector3(0, 0, 0.034f);
        leftfoot.transform.localEulerAngles = Vector3.zero;
        rightfoot.GetComponent<SphereCollider>().enabled = true;
        rightfoot.GetComponent<Rigidbody>().velocity = Vector3.zero;
        rightfoot.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        rightfoot.transform.localPosition = new Vector3(0, 0, 0.034f);
        rightfoot.transform.localEulerAngles = Vector3.zero;
        ground.transform.position = new Vector3(ground.transform.position.x, leftfoot.transform.position.y - 0.05f, ground.transform.position.z);
    }

}
