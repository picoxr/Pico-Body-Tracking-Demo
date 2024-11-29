using System;
using System.Collections;
using System.Collections.Generic;
using Pico.Platform;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.XR;

namespace BodyTrackingDemo
{
    public class BodyTrackingManager : MonoBehaviour
    {
        public enum BodyTrackingDemoState
        {
            START,
            CALIBRATING,
            CALIBRATED,
            PLAYING
        }

        public static BodyTrackingManager Instance;

        private static List<XRInputSubsystem> s_InputSubsystems = new();

        public UIStartup startCanvas;
        public DancePadsManager DancePadManager;
        public GameObject RecorderUI;
        public GameObject gameCanvas;
        public GameObject XROrigin;
        public GameObject Avatar;

        [SerializeField] private GameObject stepOnLeftToeEffect;
        [SerializeField] private GameObject stepOnLeftHeelEffect;
        [SerializeField] private GameObject stepOnToeEffect;
        [SerializeField] private GameObject stepOnHeelEffect;
        [SerializeField] private GameObject stepOnRightEffect;
        [SerializeField] private GameObject stepOnLeftEffect;

        [SerializeField] private AudioSource stepOnToeSFX;
        [SerializeField] private AudioSource stepOnHeelSFX;


        [HideInInspector] public BodyTrackingDemoState m_CurrentBodyTrackingDemoState;
        [HideInInspector] public static int SteppingEffectType;
        private BodyTrackerSampler _bodyTrackerSampler;
        private float _startFootHeight;
        private float _startXROriginY;
        private float _initXROriginY;
        private Transform m_AvatarLeftFoot;

        private GameObject m_AvatarObj;
        private Transform m_AvatarRightFoot;
        private int m_LeftFootStepOnAction;
        private int m_LeftFootStepOnLastAction;
        private int m_RightFootStepOnAction;
        private int m_RightFootStepOnLastAction;

        private bool m_MotionTrackerCalibrateState;
        private float _avatarScale;
        private GameObject _curLeftHeelVFX;
        private GameObject _curLeftToeVFX;
        private GameObject _curRightHeelVFX;
        private GameObject _curRightToeVFX;

        private void Awake()
        {
            Instance = this;

            _initXROriginY = _startXROriginY = XROrigin.transform.localPosition.y;
            
            RecorderUI.SetActive(false);
            DancePadManager.gameObject.SetActive(false);
            Avatar.SetActive(false);
            gameCanvas.SetActive(false);
            
            CoreService.Initialize();
        }

        // Start is called before the first frame update
        private void Start()
        {
            SubsystemManager.GetInstances(s_InputSubsystems);
            foreach (var t in s_InputSubsystems)
            {
                t.TryRecenter();
            }
            PXR_MotionTracking.CheckMotionTrackerModeAndNumber(MotionTrackerMode.BodyTracking, MotionTrackerNum.TWO);
            UpdateMotionTrackerState();

            // StartBodyTracking must be called, otherwise the body tracking will stop working (no update data) after 5mins.
            BodyTrackingBoneLength boneLength = new BodyTrackingBoneLength();
            PXR_MotionTracking.StartBodyTracking((BodyTrackingMode)PlayerPrefManager.Instance.PlayerPrefData.bodyTrackMode, boneLength);

            SteppingEffectType = PlayerPrefManager.Instance.PlayerPrefData.steppingEffect;
            PXR_MotionTracking.BodyTrackingAction += OnBodyTrackingStepAction;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus)
            {
                UpdateMotionTrackerState();
            }
        }
        
        // Update is called once per frame
        private void LateUpdate()
        {
            if (m_CurrentBodyTrackingDemoState == BodyTrackingDemoState.PLAYING)
            {
                m_LeftFootStepOnAction = _bodyTrackerSampler.LeftTouchGroundAction;
                m_RightFootStepOnAction = _bodyTrackerSampler.RightTouchGroundAction;
                DancePadManager.DancePadHoleStepOnDetection(m_AvatarLeftFoot.position, m_AvatarRightFoot.position, m_LeftFootStepOnAction, m_RightFootStepOnAction, m_LeftFootStepOnLastAction, m_RightFootStepOnLastAction);

                if (DancePadManager.IsDancePadGamePlaying) return;

                if (SteppingEffectType == 1) return;
                //{
                //    if ((m_LeftFootStepOnAction & (int)BodyActionList.PxrTouchGround) != 0 && (m_LeftFootStepOnLastAction & (int)BodyActionList.PxrTouchGround) == 0 ||
                //        (m_LeftFootStepOnAction & (int)BodyActionList.PxrTouchGroundToe) != 0 && (m_LeftFootStepOnLastAction & (int)BodyActionList.PxrTouchGroundToe) == 0)
                //    {
                //        if (_curLeftToeVFX != null)
                //        {
                //            var particle = _curLeftToeVFX.GetComponentInChildren<ParticleSystem>();
                //            particle.Stop(true);
                //        }
                //        _curLeftToeVFX = PlayStepOnEffect(BodyActionList.PxrTouchGroundToe, _bodyTrackerSampler.LeftFootToeBone, stepOnLeftEffect);
                //    }

                //    if ((m_RightFootStepOnAction & (int)BodyActionList.PxrTouchGround) != 0 && (m_RightFootStepOnLastAction & (int)BodyActionList.PxrTouchGround) == 0 ||
                //        (m_RightFootStepOnAction & (int)BodyActionList.PxrTouchGroundToe) != 0 && (m_RightFootStepOnLastAction & (int)BodyActionList.PxrTouchGroundToe) == 0)
                //    {
                //        if (_curRightToeVFX != null)
                //        {
                //            var particle = _curRightToeVFX.GetComponentInChildren<ParticleSystem>();
                //            particle.Stop(true);
                //        }
                //        _curRightToeVFX = PlayStepOnEffect(BodyActionList.PxrTouchGroundToe, _bodyTrackerSampler.RightFootToeBone, stepOnRightEffect);
                //    }
                //}
                //else
                //{
                if (SteppingEffectType == 2 || SteppingEffectType == 3)
                {
                    if ((m_LeftFootStepOnAction & (int)BodyActionList.PxrTouchGround) != 0 && (m_LeftFootStepOnLastAction & (int)BodyActionList.PxrTouchGround) == 0)
                    {
                        if (_curLeftHeelVFX != null)
                        {
                            var particle = _curLeftHeelVFX.GetComponentInChildren<ParticleSystem>();
                            particle.Stop(true);
                        }
                        _curLeftHeelVFX = PlayStepOnEffect(BodyActionList.PxrTouchGround, _bodyTrackerSampler.LeftFootBone, stepOnLeftHeelEffect);
                    }

                    if ((m_RightFootStepOnAction & (int)BodyActionList.PxrTouchGround) != 0 && (m_RightFootStepOnLastAction & (int)BodyActionList.PxrTouchGround) == 0)
                    {
                        if (_curRightHeelVFX != null)
                        {
                            var particle = _curRightHeelVFX.GetComponentInChildren<ParticleSystem>();
                            particle.Stop(true);
                        }
                        _curRightHeelVFX = PlayStepOnEffect(BodyActionList.PxrTouchGround, _bodyTrackerSampler.RightFootBone, stepOnHeelEffect);
                    }
                }

                if (SteppingEffectType == 2 || SteppingEffectType == 4)
                {
                    if ((m_LeftFootStepOnAction & (int)BodyActionList.PxrTouchGroundToe) != 0 && (m_LeftFootStepOnLastAction & (int)BodyActionList.PxrTouchGroundToe) == 0)
                    {
                        if (_curLeftToeVFX != null)
                        {
                            var particle = _curLeftToeVFX.GetComponentInChildren<ParticleSystem>();
                            particle.Stop(true);
                        }
                        _curLeftToeVFX = PlayStepOnEffect(BodyActionList.PxrTouchGroundToe, _bodyTrackerSampler.LeftFootToeBone, stepOnLeftToeEffect);
                    }

                    if ((m_RightFootStepOnAction & (int)BodyActionList.PxrTouchGroundToe) != 0 && (m_RightFootStepOnLastAction & (int)BodyActionList.PxrTouchGroundToe) == 0)
                    {
                        if (_curLeftToeVFX != null)
                        {
                            var particle = _curLeftToeVFX.GetComponentInChildren<ParticleSystem>();
                            particle.Stop(true);
                        }
                        _curRightToeVFX = PlayStepOnEffect(BodyActionList.PxrTouchGroundToe, _bodyTrackerSampler.RightFootToeBone, stepOnToeEffect);
                    }
                }
            }

            m_LeftFootStepOnLastAction = m_LeftFootStepOnAction;
            m_RightFootStepOnLastAction = m_RightFootStepOnAction;
            //}
        }

        //private void OnApplicationFocus(bool focus)
        //{
        //    if (focus)
        //    {
        //        UpdateMotionTrackerState();
        //    }
        //}

        private void OnBodyTrackingStepAction(int joint, BodyActionList action)
        {
            if (m_CurrentBodyTrackingDemoState != BodyTrackingDemoState.PLAYING) return;
            if (DancePadManager.IsDancePadGamePlaying) return;
            if (SteppingEffectType != 1) return;
            if (action != BodyActionList.PxrFootDownAction) return;

            if (joint == (int)BodyTrackerRole.LEFT_ANKLE)
            {
                if (_curLeftToeVFX != null)
                {
                    var particle = _curLeftToeVFX.GetComponentInChildren<ParticleSystem>();
                    particle.Stop(true);
                }
                _curLeftToeVFX = PlayStepOnEffect(BodyActionList.PxrTouchGroundToe, _bodyTrackerSampler.LeftFootToeBone, stepOnLeftEffect);
            }

            if (joint == (int)BodyTrackerRole.RIGHT_ANKLE)
            {
                if (_curRightToeVFX != null)
                {
                    var particle = _curRightToeVFX.GetComponentInChildren<ParticleSystem>();
                    particle.Stop(true);
                }
                _curRightToeVFX = PlayStepOnEffect(BodyActionList.PxrTouchGroundToe, _bodyTrackerSampler.RightFootToeBone, stepOnRightEffect);
            }

        }


        private void StartGame(float height)
        {
            m_CurrentBodyTrackingDemoState = BodyTrackingDemoState.CALIBRATED;
            
            var xrOriginPos = XROrigin.transform.localPosition;
            xrOriginPos.y = _startXROriginY = _initXROriginY;
            XROrigin.transform.localPosition = xrOriginPos;

            //load avatar
            StartCoroutine(LoadAvatar(height));
        }

        public void AlignGround()
        {
            if (m_AvatarObj == null)
            {
                Debug.LogError("There is no loaded avatar!");
                return;
            }

            _startFootHeight = Mathf.Min(m_AvatarLeftFoot.transform.position.y, m_AvatarRightFoot.transform.position.y);

            var xrOriginPos = XROrigin.transform.localPosition;

            xrOriginPos.y = _startXROriginY + -(_startFootHeight - _bodyTrackerSampler.soleHeight);

            XROrigin.transform.localPosition = xrOriginPos;
            _startXROriginY = xrOriginPos.y;

            Debug.Log($"BodyTrackingManager.AlignGround: StartFootHeight = {_startFootHeight}, xrOriginPos = {xrOriginPos}");
        }

        [ContextMenu("LoadAvatar")]
        public void StartGame()
        {
            try
            {
                var task = SportService.GetUserInfo();
                if (task != null)
                    task.OnComplete(rsp =>
                    {
                        if (!rsp.IsError)
                        {
                            if (rsp.Data.Stature > 50)
                            {
                                PlayerPrefManager.Instance.PlayerPrefData.height = rsp.Data.Stature;    
                            }
                            Debug.Log($"SportService.GetUserInfo: Success, Height = {rsp.Data.Stature}");
                        }
                        else
                        {
                            Debug.LogWarning($"SportService.GetUserInfo: Failed, msg = {rsp.Error}");
                        }

                        StartGame(PlayerPrefManager.Instance.PlayerPrefData.height);
                    });
                else
                    StartGame(PlayerPrefManager.Instance.PlayerPrefData.height);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                StartGame(PlayerPrefManager.Instance.PlayerPrefData.height);
            }
        }

        private IEnumerator LoadAvatar(float height)
        {
            if (height <= 50)
            {
                height = 175;
                Debug.LogWarning($"LoadAvatar: Height = {height} is too small, it be set to 175, please check!");
            }
            
            SubsystemManager.GetInstances(s_InputSubsystems);
            foreach (var t in s_InputSubsystems)
            {
                t.TryRecenter();
            }
            
            m_AvatarObj = Avatar;
            m_AvatarObj.transform.localScale = Vector3.one;
            m_AvatarObj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            m_AvatarObj.SetActive(true);

            _bodyTrackerSampler = m_AvatarObj.GetComponent<BodyTrackerSampler>();

            m_LeftFootStepOnAction = m_LeftFootStepOnLastAction = _bodyTrackerSampler.LeftTouchGroundAction;
            m_RightFootStepOnAction = m_RightFootStepOnLastAction = _bodyTrackerSampler.RightTouchGroundAction;

            m_AvatarLeftFoot = _bodyTrackerSampler.BonesList[10];
            m_AvatarRightFoot = _bodyTrackerSampler.BonesList[11];


            _avatarScale = height / 175;
            
            // XROrigin.transform.localScale = Vector3.one * _avatarScale;
            //_bodyTrackerSampler.UpdateBonesLength(_avatarScale);

            Avatar.SetActive(false);
            yield return new WaitForEndOfFrame();
            AlignGround();

            Avatar.SetActive(true);
            gameCanvas.SetActive(true);
#if RECORDER
            RecorderUI.SetActive(true);
#endif
            
            DancePadManager.gameObject.SetActive(true);

            m_CurrentBodyTrackingDemoState = BodyTrackingDemoState.PLAYING;

            Debug.Log($"BodyTrackingManager.LoadAvatar: Avatar = {m_AvatarObj.name}, height = {height}");
        }

        private void UpdateMotionTrackerState()
        {

            //Update Swift calibration state after resuming
            var calibrated = -1;
            PXR_Input.GetMotionTrackerCalibState(ref calibrated);
            m_MotionTrackerCalibrateState = calibrated == 1;
            if (m_MotionTrackerCalibrateState)
            {
                startCanvas.startMenu.SetActive(false);
                
                StartGame();
                Debug.Log($"BodyTrackingManager.UpdateFitnessBandState: calibrated = {calibrated}");
            }
            else
            {
                if (m_AvatarObj != null && m_AvatarObj.activeSelf) m_AvatarObj.SetActive(false);

                MotionTrackerConnectState connectState = new MotionTrackerConnectState();
                PXR_MotionTracking.GetMotionTrackerConnectStateWithSN(ref connectState);
                
                BodyTrackerResult bodyTrackerResult = new BodyTrackerResult();
                var trackingState = PXR_Input.GetBodyTrackingPose(0, ref bodyTrackerResult);
#if UNITY_EDITOR
                connectState.trackerSum = 2;
                trackingState = 0;
#endif

                startCanvas.startMenu.SetActive(true);
                startCanvas.btnContinue.gameObject.SetActive(connectState.trackerSum >= 2 && trackingState == 0);

                Debug.Log($"BodyTrackingManager.UpdateFitnessBandState: connectedNum = {connectState.trackerSum}, trackingState = {trackingState}");
            }
        }

        public void HideAvatar()
        {
            if (m_AvatarObj != null)
            {
                m_AvatarObj.SetActive(false);
            }
        }
        
        private GameObject PlayStepOnEffect(BodyActionList action, Transform joint, GameObject effect)
        {
            if (action == 0) return null;

            Vector3 pos = joint.position;
            Quaternion rotation = joint.rotation;
            
            switch (action)
            {
                case BodyActionList.PxrTouchGroundToe:
                {
                    var targetPos = pos + new Vector3(0, -0.02f, 0);
                    var vfx = Instantiate(effect, targetPos, rotation);
                    vfx.transform.localScale = Vector3.one * _avatarScale;
                    vfx.SetActive(true);

                    AudioManager.Instance.PlayEffect(stepOnToeSFX, targetPos);
                    Debug.Log($"BodyTrackingManager.PlayStepOnEffect: action = {action}, targetPos = {targetPos}");
                    return vfx;
                }
                case BodyActionList.PxrTouchGround:
                {
                    var targetPos = pos + new Vector3(0, -.08f, 0);
                    var vfx = Instantiate(effect, targetPos, rotation);
                    vfx.transform.localScale = Vector3.one * _avatarScale;
                    vfx.SetActive(true);
                    
                    AudioManager.Instance.PlayEffect(stepOnHeelSFX, targetPos);
                    Debug.Log($"BodyTrackingManager.PlayStepOnEffect: action = {action}, targetPos = {targetPos}");
                    return vfx;
                }
            }

            return null;
        }
    }
}