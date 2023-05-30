using System.Collections;
using System.Collections.Generic;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class SimpleSample : MonoBehaviour
{
    [HideInInspector]
    public List<Transform> BonesList = new List<Transform>(new Transform[(int)BodyTrackerRole.ROLE_NUM]);

    public Dictionary<int, Quaternion> mRotationDic = new Dictionary<int, Quaternion>();

    public float[] SkeletonLens= new float[11];

    public GameObject leftexplotionEffect;
    public GameObject rightexplotionEffect;

    //public Text InfoText;
    private BodyTrackerResult mBodyTrackerResult;
    private double mDisplayTime;

    private Vector3 position;
    private Quaternion rotation;
    private Vector3 rightFootPosition;

    private string logPath;

    private Vector3 leftAnklePos;
    private Vector3 rightAnklePos;

    private Vector3 footPosition;

    private int lastLeftAction;
    private int lastRightAction;


    // Start is called before the first frame update
    void Start()
    {
        mBodyTrackerResult = new BodyTrackerResult();
        mBodyTrackerResult.trackingdata = new BodyTrackerTransform[24];

        logPath = DemoProcessController.Instance.GetLogPath();

        for (int i = 0; i < BonesList.Count; i++)
        {
            if (BonesList[i] != null)
            {
                mRotationDic.Add(i, BonesList[i].rotation);
            }
        }
        lastLeftAction = 1000;
        lastRightAction = 1000;
    }

    // Update is called once per frame
    void Update()
    {
        mDisplayTime = PXR_System.GetPredictedDisplayTime();
        PXR_Input.GetBodyTrackingPose(mDisplayTime, ref mBodyTrackerResult);
        // InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 rightPotion);
        #region screen log
        // Debug.Log("Left contoller Position:" + rightPotion);
        // position.x = (float)mBodyTrackerResult.trackingdata[22].localpose.PosX;
        // position.y = (float)mBodyTrackerResult.trackingdata[22].localpose.PosY;
        // position.z = (float)mBodyTrackerResult.trackingdata[22].localpose.PosZ;
        // Debug.Log("Left hand Position:" + position);

        //leftAnklePos.x = (float)mBodyTrackerResult.trackingdata[7].localpose.PosX;
        //leftAnklePos.y = (float)mBodyTrackerResult.trackingdata[7].localpose.PosY;
        //leftAnklePos.z = -(float)mBodyTrackerResult.trackingdata[7].localpose.PosZ;

        //rightAnklePos.x = (float)mBodyTrackerResult.trackingdata[8].localpose.PosX;
        //rightAnklePos.y = (float)mBodyTrackerResult.trackingdata[8].localpose.PosY;
        //rightAnklePos.z = -(float)mBodyTrackerResult.trackingdata[8].localpose.PosZ;

        //InfoText.text = string.Format("leftAnkle: ({0}, {1}, {2})\nrightAnkle: ({3}, {4}, {5})\ndistance: {6}",
        //    leftAnklePos.x,
        //    leftAnklePos.y,
        //    leftAnklePos.z,
        //    rightAnklePos.x,
        //    rightAnklePos.y,
        //    rightAnklePos.z,
        //    Mathf.Abs(leftAnklePos.x - rightAnklePos.x));

        #endregion

        position.x = (float)mBodyTrackerResult.trackingdata[0].localpose.PosX;
        position.y = (float)mBodyTrackerResult.trackingdata[0].localpose.PosY;
        position.z = (float)mBodyTrackerResult.trackingdata[0].localpose.PosZ;
        Debug.Log("[BodyTrackingMode] hip position: " + position);

        rightFootPosition.x = (float)mBodyTrackerResult.trackingdata[11].localpose.PosX;
        rightFootPosition.y = (float)mBodyTrackerResult.trackingdata[11].localpose.PosY;
        rightFootPosition.z = (float)mBodyTrackerResult.trackingdata[11].localpose.PosZ;
        //Debug.Log("right foot position: " + rightFootPosition);


        //if (DemoProcessController.Instance.DiffPosition == Vector3.zero && rightFootPosition != Vector3.zero)
        //{
        //    DemoProcessController.Instance.DiffPosition = new Vector3(0, /*rightFootPosition.y - */0.0005f * DemoProcessController.Instance.PlayerHeight, 0);
        //    //�㷨���ҽŽ�ֺ�߶�ֵ - 0.05f(����ֵ, ��Ϊ�ǵ�ǰ�ҽŽ�ֺ�㷨�߶������߶ȵĲ�)

        //    //Debug.LogFormat("[SwiftDemoTest] Hips.positionY = {0}, Toes.positionY = {1}", BonesList[0].position.y, rightFootPosition.y);
        //    //Debug.Log("[SwiftDemoTest]diff position: " + DemoProcessController.Instance.DiffPosition);
        //}
        if(DemoProcessController.loadavatar)
        {
            footPosition = new Vector3(0, rightFootPosition.y, 0);
            DemoProcessController.loadavatar = false;
        }
        BonesList[0].position = position + DemoProcessController.Instance.DiffPosition - footPosition;
        //Debug.Log("model hip: " + BonesList[0].position);

        if (DemoProcessController.effectText.GetComponent<Text>().text == "Click A button to Disable/Enable stomping effects. Current stomping: Enabled.")
        {
            if ((int)mBodyTrackerResult.trackingdata[11].Action > 500 && lastRightAction <= 500)
            {
                GameObject effectnode = Instantiate(leftexplotionEffect, null);
                effectnode.transform.position = BonesList[11].position;
            }

            if ((int)mBodyTrackerResult.trackingdata[10].Action > 500 && lastLeftAction <= 500)
            {
                GameObject effectnode = Instantiate(rightexplotionEffect, null);
                effectnode.transform.position = BonesList[10].position;
            }
        }
        lastLeftAction = (int)mBodyTrackerResult.trackingdata[10].Action;
        lastRightAction = (int)mBodyTrackerResult.trackingdata[11].Action;

        string frameContent = null;
        for (int i = 0; i < BonesList.Count; i++)
        {
            if (BonesList[i] != null)
            {
                position.x = (float)mBodyTrackerResult.trackingdata[i].localpose.PosX;
                position.y = (float)mBodyTrackerResult.trackingdata[i].localpose.PosY;
                position.z = (float)mBodyTrackerResult.trackingdata[i].localpose.PosZ;

                rotation.x = (float)mBodyTrackerResult.trackingdata[i].localpose.RotQx;
                rotation.y = (float)mBodyTrackerResult.trackingdata[i].localpose.RotQy;
                rotation.z = (float)mBodyTrackerResult.trackingdata[i].localpose.RotQz;
                rotation.w = (float)mBodyTrackerResult.trackingdata[i].localpose.RotQw;


                //BonesList[i].position = position;
                //BonesList[i].rotation = rotation;

                BonesList[i].rotation = rotation * mRotationDic[i];
                if (i < 22)
                    DemoProcessController.Instance.SkeletonNodes[i].position = BonesList[i].position;
            }
            frameContent += string.Format("{0}|{1},{2},{3}|{4},{5},{6},{7}\n",
                i,
                mBodyTrackerResult.trackingdata[i].localpose.PosX,
                mBodyTrackerResult.trackingdata[i].localpose.PosY,
                mBodyTrackerResult.trackingdata[i].localpose.PosZ,
                mBodyTrackerResult.trackingdata[i].localpose.RotQx,
                mBodyTrackerResult.trackingdata[i].localpose.RotQy,
                mBodyTrackerResult.trackingdata[i].localpose.RotQz,
                mBodyTrackerResult.trackingdata[i].localpose.RotQw);
        }
#if !UNITY_EDITOR
        frameContent += (mBodyTrackerResult.trackingdata[3].velo[0] + "\n#\n");
        System.IO.File.AppendAllText(logPath, frameContent);
#endif

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
