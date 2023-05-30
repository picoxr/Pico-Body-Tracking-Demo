using System.Collections;
using System.Collections.Generic;
using RootMotion.FinalIK;
using TMPro;
using UnityEngine;

public class PoseDemo : MonoBehaviour
{

    public VRIK ik;
    public TextMeshProUGUI text;
    public Transform lefteye;
    public Transform righteye;
    void Start()
    {
        text.text += "lefteye  " + lefteye.position.ToString("f2");
        text.text += "\nrighteye  " + righteye.position.ToString("f2");
        text.text += "\nhead  " + ik.references.head.position.ToString("f2");
        text.text += "\nneck  " + ik.references.neck.position.ToString("f2");
        text.text += "\npelvis " + ik.references.pelvis.position.ToString("f2");
        text.text += "\nleftThigh " + ik.references.leftThigh.position.ToString("f2");
        text.text += "\nleftCalf " + ik.references.leftCalf.position.ToString("f2");
        text.text += "\nleftFoot " + ik.references.leftFoot.position.ToString("f2");
        text.text += "\nrightThigh " + ik.references.rightThigh.position.ToString("f2");
        text.text += "\nrightCalf " + ik.references.rightCalf.position.ToString("f2");
        text.text += "\nrightFoot " + ik.references.rightFoot.position.ToString("f2");

        Debug.Log("neck -> leftThigh" + (ik.references.neck.position - ik.references.leftThigh.position).ToString("f3"));
        Debug.Log("neck -> rightThigh" + (ik.references.neck.position - ik.references.rightThigh.position).ToString("f3"));
        Debug.Log("leftThigh -> leftCalf" + (ik.references.leftThigh.position - ik.references.leftCalf.position).ToString("f3"));
        Debug.Log("rightThigh -> rightCalf" + (ik.references.rightThigh.position - ik.references.rightCalf.position).ToString("f3"));
        Debug.Log("leftCalf -> leftFoot" + (ik.references.leftCalf.position - ik.references.leftFoot.position).ToString("f3"));
        Debug.Log("rightCalf -> rightFoot" + (ik.references.rightCalf.position - ik.references.rightFoot.position).ToString("f3"));

        // float distance = (t1.position - t2.position).magnitude;
        // Debug.Log(distance);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
