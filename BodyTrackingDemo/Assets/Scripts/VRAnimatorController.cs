using System.Collections;
using System.Collections.Generic;
using RootMotion.FinalIK;
using UnityEngine;

public class VRAnimatorController : MonoBehaviour
{
    public float speedTreshold = 0.1f;
    [Range(0, 1)]
    public float smoothing = 1;
    private Animator animator;
    private Vector3 previousPos;
    private Vector3 previousRot;
    private VRIK ik;
    public bool isInit;
    // private PhotonView view;
    private VRRig rig;
    public bool hasFace;
    public int faceID;

    void Start()
    {
        Init();
    }

    public void Init()
    {
        rig = GetComponent<VRRig>();
        animator = GetComponent<Animator>();
        ik = GetComponent<VRIK>();
        previousPos = ik.solver.spine.headTarget.position;
        // isInit = true;
    }
    void Update()
    {
        // if (!isInit) return;
        // if (!view.IsMine) return;
        if (Time.frameCount % 12 == 0)
        {
            Vector3 headsetSpeed = (rig.head.parent.position - previousPos) / (Time.deltaTime * 12);
            headsetSpeed.y = 0;
            Vector3 headsetLocalSpeed = transform.InverseTransformDirection(headsetSpeed);
            previousPos = rig.head.parent.position;
            float previousDirectionX = animator.GetFloat("DirectionX");
            float previousDirectionY = animator.GetFloat("DirectionY");
            animator.SetBool("isMoving", headsetLocalSpeed.magnitude > speedTreshold);
            animator.SetFloat("DirectionX", Mathf.Lerp(previousDirectionX, Mathf.Clamp(headsetLocalSpeed.x, -1, 1), smoothing));
            animator.SetFloat("DirectionY", Mathf.Lerp(previousDirectionY, Mathf.Clamp(headsetLocalSpeed.z, -1, 1), smoothing));
        }
    }

    public void Event_LeftStep()
    {

    }

    public void Event_RightStep()
    {

    }
}