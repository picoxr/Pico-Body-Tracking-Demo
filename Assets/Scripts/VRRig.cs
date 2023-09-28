using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class VRRig : MonoBehaviour
{
    public Transform head;
    public Transform playArea;
    void Start()
    {
        
    }

    void LateUpdate()
    {
        if (playArea != null)
            transform.position = new Vector3(head.position.x, playArea.position.y, head.position.z);
    }
}