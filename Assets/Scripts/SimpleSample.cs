using System.Collections.Generic;
using Unity.XR.PXR;
using UnityEngine;

namespace BodyTrackingDemo
{
    public class SimpleSample : MonoBehaviour
    {

        public List<Transform> BonesList = new List<Transform>(new Transform[(int) BodyTrackerRole.ROLE_NUM]);

        public Dictionary<int, Quaternion> _startRotationDic = new Dictionary<int, Quaternion>();

        public float soleHeight = 0.022f;
        
        public float[] SkeletonLens = new float[11];

        [HideInInspector] public int LeftTouchGroundAction;
        [HideInInspector] public int RightTouchGroundAction;
        [HideInInspector] public int LeftToeTouchGroundAction;
        [HideInInspector] public int RightToeTouchGroundAction;
        
        public Transform LeftFootBone => BonesList[7];
        public Transform RightFootBone => BonesList[8];
        
        public Transform LeftFootToeBone => BonesList[10];
        public Transform RightFootToeBone => BonesList[11];

        //public Text InfoText;
        private BodyTrackerResult m_BodyTrackerResult;
        private double mDisplayTime;

        private Vector3 m_hipJointPosition;
        private Quaternion m_JointRotation;
        //private Vector3 rightFootPosition;
        //private Vector3 leftAnklePos;
        //private Vector3 rightAnklePos;
        //private Vector3 footPosition;
        //private int lastLeftAction;
        //private int lastRightAction;

        private BodyTrackerJoint[] _joints;
        
        private void Awake()
        {
            m_BodyTrackerResult = new BodyTrackerResult();
            m_BodyTrackerResult.trackingdata = new BodyTrackerTransform[24];
            _joints = new BodyTrackerJoint[BonesList.Count];

            for (int i = 0; i < BonesList.Count; i++)
            {
                if (BonesList[i] != null)
                {
                    _startRotationDic.Add(i, BonesList[i].rotation);

                    var bodyTrackerJoint = BonesList[i].GetComponent<BodyTrackerJoint>();
                    if (bodyTrackerJoint == null)
                    {
                        bodyTrackerJoint = BonesList[i].gameObject.AddComponent<BodyTrackerJoint>();
                        bodyTrackerJoint.bodyTrackerRole = (BodyTrackerRole) i;
                    }

                    _joints[i] = bodyTrackerJoint;
                }
            }

            Update();
        }
        
        // Update is called once per frame
        void Update()
        {
            mDisplayTime = PXR_System.GetPredictedDisplayTime();
            var state = PXR_Input.GetBodyTrackingPose(mDisplayTime, ref m_BodyTrackerResult);
            if (state != 0)
            {
                return;
            }

            m_hipJointPosition = GetPosition(m_BodyTrackerResult.trackingdata[0]);
            BonesList[0].localPosition = m_hipJointPosition;
            
            for (int i = 0; i < BonesList.Count; i++)
            {
                if (BonesList[i] != null)
                {
                    m_JointRotation.x = (float) m_BodyTrackerResult.trackingdata[i].localpose.RotQx;
                    m_JointRotation.y = (float) m_BodyTrackerResult.trackingdata[i].localpose.RotQy;
                    m_JointRotation.z = (float) m_BodyTrackerResult.trackingdata[i].localpose.RotQz;
                    m_JointRotation.w = (float) m_BodyTrackerResult.trackingdata[i].localpose.RotQw;

                    //BonesList[i].position = GetPosition(m_BodyTrackerResult.trackingdata[i]);
                    // BonesList[i].rotation = rotation;

                    BonesList[i].rotation = m_JointRotation * _startRotationDic[i];
                    _joints[i].TrackingData = m_BodyTrackerResult.trackingdata[i];
                }
            }

            //update left and right feet actions
            LeftTouchGroundAction = (int) m_BodyTrackerResult.trackingdata[7].Action;
            RightTouchGroundAction = (int) m_BodyTrackerResult.trackingdata[8].Action;
            LeftToeTouchGroundAction = (int) m_BodyTrackerResult.trackingdata[10].Action;
            RightToeTouchGroundAction = (int) m_BodyTrackerResult.trackingdata[11].Action;
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
            BonesList[20] = GameObject
                .Find(this.name + "/RIG/DeformationSystem/Root/GlobalScale/Hips/Spine1/Spine2/Chest/Shoulder_L/Arm_L/Arm_L_twist/ForeArm_L/ForeArm_L_twist01/ForeArm_L_twist02/Hand_L").transform;
            BonesList[21] = GameObject
                .Find(this.name + "/RIG/DeformationSystem/Root/GlobalScale/Hips/Spine1/Spine2/Chest/Shoulder_R/Arm_R/Arm_R_twist/ForeArm_R/ForeArm_R_twist01/ForeArm_R_twist02/Hand_R").transform;
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
        
            Debug.Log($"SimpleSample.FindBonesLength: NeckLen = {SkeletonLens[1]}, TorsoLen = {SkeletonLens[2]}, HipLen = {SkeletonLens[3]}, UpperLegLen = {SkeletonLens[4]}, LowerLegLen = {SkeletonLens[5]}, FootLen = {SkeletonLens[6]}, ShoulderLen = {SkeletonLens[7]}, UpperArmLen = {SkeletonLens[8]}, LowerArmLen = {SkeletonLens[9]}");
        }

        public void UpdateBonesLength(float scale = 1)
        {
            BonesList[0].localScale = Vector3.one * scale;
            
            SkeletonLens[0] = 0.2f * scale;
            SkeletonLens[1] = 0.169f * scale;
            FindBonesLength();
            SetBonesLength();
            Update();
        }
        
        public void SetBonesLength()
        {
            BodyTrackingBoneLength boneLength = new BodyTrackingBoneLength();
            boneLength.headLen = 100 * SkeletonLens[0];
            boneLength.neckLen = 100 * SkeletonLens[1]; //6.1f;
            boneLength.torsoLen = 100 * SkeletonLens[2]; //37.1f;
            boneLength.hipLen = 100 * SkeletonLens[3]; //9.1f;
            boneLength.upperLegLen = 100 * SkeletonLens[4]; //34.1f;
            boneLength.lowerLegLen = 100 * SkeletonLens[5]; //40.1f;
            boneLength.footLen = 100 * SkeletonLens[6]; //14.1f;
            boneLength.shoulderLen = 100 * SkeletonLens[7]; //27.1f;
            boneLength.upperArmLen = 100 * SkeletonLens[8]; //20.1f;
            boneLength.lowerArmLen = 100 * SkeletonLens[9]; //22.1f;
            boneLength.handLen = 100 * SkeletonLens[10];

            int result = PXR_Input.SetBodyTrackingBoneLength(boneLength);
        
            Debug.Log($"SimpleSample.SetBonesLength: boneLength = {boneLength}, result = {result}");
        }

        public static Vector3 GetPosition(BodyTrackerTransform bodyTrackerTransform)
        {
            return new((float)bodyTrackerTransform.localpose.PosX, (float)bodyTrackerTransform.localpose.PosY, (float)bodyTrackerTransform.localpose.PosZ);
        }
    }
}