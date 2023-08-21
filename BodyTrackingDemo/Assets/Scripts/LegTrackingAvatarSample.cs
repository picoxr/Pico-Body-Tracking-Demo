using System.Collections;
using System.Collections.Generic;
using Unity.XR.PXR;
using UnityEngine;

public class LegTrackingAvatarSample : MonoBehaviour
{
    
    public List<Transform> BonesList = new List<Transform>(new Transform[(int)BodyTrackerRole.ROLE_NUM]);

    public Dictionary<int, Quaternion> mRotationDic = new Dictionary<int, Quaternion>();

    public float[] SkeletonLens= new float[11];     

    [HideInInspector]
    public int LeftTouchGroundAction;
    [HideInInspector]
    public int RightTouchGroundAction;

    //public Text InfoText;
    private BodyTrackerResult m_BodyTrackerResult;
    private double mDisplayTime;

    private Vector3 m_JointPosition;
    private Quaternion m_JointRotation;
    //private Vector3 rightFootPosition;
    //private Vector3 leftAnklePos;
    //private Vector3 rightAnklePos;
    //private Vector3 footPosition;
    //private int lastLeftAction;
    //private int lastRightAction;


    // Start is called before the first frame update
    void Start()
    {
        m_BodyTrackerResult = new BodyTrackerResult();
        m_BodyTrackerResult.trackingdata = new BodyTrackerTransform[24];

        for (int i = 0; i < BonesList.Count; i++)
        {
            if (BonesList[i] != null)
            {
                mRotationDic.Add(i, BonesList[i].rotation);
            }
        }
        //lastLeftAction = 1000;
        //lastRightAction = 1000;
    }

    // Update is called once per frame
    void Update()
    {
        mDisplayTime = PXR_System.GetPredictedDisplayTime();
        PXR_Input.GetBodyTrackingPose(mDisplayTime, ref m_BodyTrackerResult);
        
        m_JointPosition.x = (float)m_BodyTrackerResult.trackingdata[0].localpose.PosX;
        m_JointPosition.y = (float)m_BodyTrackerResult.trackingdata[0].localpose.PosY;
        m_JointPosition.z = (float)m_BodyTrackerResult.trackingdata[0].localpose.PosZ;
        //Debug.Log("[LegTrackingMode] hip position: " + m_JointPosition);
        BonesList[0].position = m_JointPosition;

        //string frameContent = null;
        for (int i = 0; i < BonesList.Count; i++)
        {
            if (BonesList[i] != null)
            {
                //position.x = (float)m_BodyTrackerResult.trackingdata[i].localpose.PosX;
                //position.y = (float)m_BodyTrackerResult.trackingdata[i].localpose.PosY;
                //position.z = (float)m_BodyTrackerResult.trackingdata[i].localpose.PosZ;

                m_JointRotation.x = (float)m_BodyTrackerResult.trackingdata[i].localpose.RotQx;
                m_JointRotation.y = (float)m_BodyTrackerResult.trackingdata[i].localpose.RotQy;
                m_JointRotation.z = (float)m_BodyTrackerResult.trackingdata[i].localpose.RotQz;
                m_JointRotation.w = (float)m_BodyTrackerResult.trackingdata[i].localpose.RotQw;
                
                //BonesList[i].position = position;
                //BonesList[i].rotation = rotation;

                BonesList[i].rotation = m_JointRotation * mRotationDic[i];
            }
        }

        //update left and right feet actions
        LeftTouchGroundAction = (int)m_BodyTrackerResult.trackingdata[7].Action;
        RightTouchGroundAction = (int)m_BodyTrackerResult.trackingdata[8].Action;

    }

    /// <summary>
    /// For Demo Avatar use only
    /// </summary>
    [ContextMenu("AutoBindAvatarBones")]
    public void FindBonesReference()
    {
        BonesList[0] = GameObject.Find(this.name + "/RIG/DeformationSystem/Root/GlobalScale/Hips").transform;
        BonesList[1] = GameObject.Find(this.name + "/RIG/DeformationSystem/Root/GlobalScale/Hips/UpLeg_L").transform;
        BonesList[2] = GameObject.Find(this.name + "/RIG/DeformationSystem/Root/GlobalScale/Hips/UpLeg_R").transform;
        BonesList[3] = GameObject.Find(this.name + "/RIG/DeformationSystem/Root/GlobalScale/Hips/Spine1").transform;
        BonesList[4] = GameObject.Find(this.name + "/RIG/DeformationSystem/Root/GlobalScale/Hips/UpLeg_L/Leg_L").transform;
        BonesList[5] = GameObject.Find(this.name + "/RIG/DeformationSystem/Root/GlobalScale/Hips/UpLeg_R/Leg_R").transform;
        BonesList[6] = GameObject.Find(this.name + "/RIG/DeformationSystem/Root/GlobalScale/Hips/Spine1/Spine2").transform;
        BonesList[7] = GameObject.Find(this.name + "/RIG/DeformationSystem/Root/GlobalScale/Hips/UpLeg_L/Leg_L/Foot_L").transform;
        BonesList[8] = GameObject.Find(this.name + "/RIG/DeformationSystem/Root/GlobalScale/Hips/UpLeg_R/Leg_R/Foot_R").transform;
        BonesList[9] = GameObject.Find(this.name + "/RIG/DeformationSystem/Root/GlobalScale/Hips/Spine1/Spine2/Chest").transform;
        BonesList[10] = GameObject.Find(this.name + "/RIG/DeformationSystem/Root/GlobalScale/Hips/UpLeg_L/Leg_L/Foot_L/Foot_rotate_L/HeelToes_L/Toes_rotate_L/Toes_L").transform;
        BonesList[11] = GameObject.Find(this.name + "/RIG/DeformationSystem/Root/GlobalScale/Hips/UpLeg_R/Leg_R/Foot_R/Foot_rotate_R/HeelToes_R/Toes_rotate_R/Toes_R").transform;
        BonesList[12] = GameObject.Find(this.name + "/RIG/DeformationSystem/Root/GlobalScale/Hips/Spine1/Spine2/Chest/Neck").transform;
        BonesList[13] = GameObject.Find(this.name + "/RIG/DeformationSystem/Root/GlobalScale/Hips/Spine1/Spine2/Chest/Shoulder_L").transform;
        BonesList[14] = GameObject.Find(this.name + "/RIG/DeformationSystem/Root/GlobalScale/Hips/Spine1/Spine2/Chest/Shoulder_R").transform;
        BonesList[15] = GameObject.Find(this.name + "/RIG/DeformationSystem/Root/GlobalScale/Hips/Spine1/Spine2/Chest/Neck/Head").transform;
        BonesList[16] = GameObject.Find(this.name + "/RIG/DeformationSystem/Root/GlobalScale/Hips/Spine1/Spine2/Chest/Shoulder_L/Arm_L").transform;
        BonesList[17] = GameObject.Find(this.name + "/RIG/DeformationSystem/Root/GlobalScale/Hips/Spine1/Spine2/Chest/Shoulder_R/Arm_R").transform;
        BonesList[18] = GameObject.Find(this.name + "/RIG/DeformationSystem/Root/GlobalScale/Hips/Spine1/Spine2/Chest/Shoulder_L/Arm_L/Arm_L_twist/ForeArm_L").transform;
        BonesList[19] = GameObject.Find(this.name + "/RIG/DeformationSystem/Root/GlobalScale/Hips/Spine1/Spine2/Chest/Shoulder_R/Arm_R/Arm_R_twist/ForeArm_R").transform;
        BonesList[20] = GameObject.Find(this.name + "/RIG/DeformationSystem/Root/GlobalScale/Hips/Spine1/Spine2/Chest/Shoulder_L/Arm_L/Arm_L_twist/ForeArm_L/ForeArm_L_twist01/ForeArm_L_twist02/Hand_L").transform;
        BonesList[21] = GameObject.Find(this.name + "/RIG/DeformationSystem/Root/GlobalScale/Hips/Spine1/Spine2/Chest/Shoulder_R/Arm_R/Arm_R_twist/ForeArm_R/ForeArm_R_twist01/ForeArm_R_twist02/Hand_R").transform;
    }

    //For demo Avatar only
    [ContextMenu("AutoFindAvatarBonesLenth")]
    public void FindBonesLength()
    {
        SkeletonLens[0] = 0.2f; //HeadLen
        SkeletonLens[1] = (BonesList[12].position - BonesList[15].position).magnitude; //NeckLen
        SkeletonLens[2] = (BonesList[12].position - (BonesList[0].position + BonesList[3].position) * 0.5f).magnitude; //TorsoLen
        SkeletonLens[3] = ((BonesList[0].position + BonesList[3].position) * 0.5f - (BonesList[1].position + BonesList[2].position) * 0.5f).magnitude; //HipLen
        SkeletonLens[4] = (BonesList[1].position - BonesList[4].position).magnitude; //UpperLegLen
        SkeletonLens[5] = (BonesList[4].position - BonesList[7].position).magnitude; //LowerLegLen
        SkeletonLens[6] = (BonesList[7].position - BonesList[10].position).magnitude; //FootLen
        SkeletonLens[7] = (BonesList[16].position - BonesList[17].position).magnitude; //ShoulderLen
        SkeletonLens[8] = (BonesList[16].position - BonesList[18].position).magnitude; //UpperArmLen
        SkeletonLens[9] = (BonesList[18].position - BonesList[20].position).magnitude; //LowerArmLen
        SkeletonLens[10] = 0.169f; //HandLen
    }


}
