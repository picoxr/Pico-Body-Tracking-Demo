using System;
using System.Collections.Generic;
using Pico.Platform;
using Pico.Platform.Models;
using UnityEngine;
using Unity.XR.PXR;
using UnityEngine.XR;
using CommonUsages = UnityEngine.XR.CommonUsages;

namespace BodyTrackingDemo
{
    public class LegTrackingModeSceneManager : MonoBehaviour
    {
        public static LegTrackingModeSceneManager Instance;
        
        public LegTrackingModeUIManager LegTrackingUIManager;
        public DancePadsManager DancePadManager;
        public GameObject DancePadUI;
        public GameObject MotionTrackerUI;
        public GameObject DisplaySettingUI;
        public GameObject RecorderUI;
        public GameObject MirrorObj;
        public GameObject XROrigin;
        public GameObject Avatar;
        
        [SerializeField] private GameObject stepOnToeEffect;
        [SerializeField] private GameObject stepOnHeelEffect;

        [HideInInspector] public LegTrackingDemoState m_CurrentLegTrackingDemoState;

        private GameObject m_AvatarObj;
        private Transform m_AvatarLeftFoot;
        private Transform m_AvatarRightFoot;
        private int m_LeftFootStepOnAction;
        private int m_RightFootStepOnAction;
        private int m_LeftFootStepOnLastAction;
        private int m_RightFootStepOnLastAction;

        private bool m_SwiftCalibratedState;
        private LegTrackingAvatarSample _legTrackingAvatarSample;
        private float _startFootHeight;
        private float _startXROriginY;
        
        public enum LegTrackingDemoState
        {
            START,
            CALIBRATING,
            CALIBRATED,
            PLAYING,
        }

        private static List<XRInputSubsystem> s_InputSubsystems = new List<XRInputSubsystem>();

        private void Awake()
        {
            Instance = this;

            _startXROriginY = XROrigin.transform.localPosition.y;

            MirrorObj.SetActive(false);
            DancePadUI.SetActive(false);
            MotionTrackerUI.SetActive(false);
            DisplaySettingUI.SetActive(false);
            RecorderUI.SetActive(false);
            DancePadManager.gameObject.SetActive(false);
            Avatar.SetActive(false);
        }

        // Start is called before the first frame update
        void Start()
        {
            CoreService.Initialize();
            UserService.RequestUserPermissions(Permissions.SportsUserInfo).OnComplete((permissionRsp) =>
            {
                if (permissionRsp.IsError)
                {
                    Debug.LogWarning($"UserService.RequestUserPermissions: Failed, msg = {permissionRsp.Error}");
                    return;
                }
                
                Debug.Log("Request SportsUserInfo Permissions Successfully");
            });

            UpdateFitnessBandState();
            // SubsystemManager.GetInstances(s_InputSubsystems);
            // foreach (var t in s_InputSubsystems)
            // {
            //     t.TryRecenter();
            // }
        }

        // Update is called once per frame
        void LateUpdate()
        {
            if (m_AvatarObj != null)
            {
                if (InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool right_pressed) && right_pressed ||
                    InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool left_pressed) && left_pressed)
                {
                    // PXR_Input.OpenFitnessBandCalibrationAPP();
                    LegTrackingUIManager.startMenu.SetActive(true);
                    LegTrackingUIManager.btnContinue.gameObject.SetActive(true);
                }

                m_LeftFootStepOnAction = _legTrackingAvatarSample.LeftTouchGroundAction;
                m_RightFootStepOnAction = _legTrackingAvatarSample.RightTouchGroundAction;
                DancePadManager.DancePadHoleStepOnDetection(m_AvatarLeftFoot.position, m_AvatarRightFoot.position, m_LeftFootStepOnAction, m_RightFootStepOnAction, m_LeftFootStepOnLastAction, m_RightFootStepOnLastAction);

                if (_legTrackingAvatarSample.LeftToeTouchGroundAction * 0.001f >= PlayerPrefManager.Instance.PlayerPrefData.steppingSensitivity)
                {
                    if ((m_LeftFootStepOnAction & (int)BodyActionList.PxrTouchGround) != 0 && (m_LeftFootStepOnLastAction & (int)BodyActionList.PxrTouchGround) ==0)
                    {
                        PlayStepOnEffect(m_LeftFootStepOnAction, PlayerPrefManager.Instance.PlayerPrefData.steppingEffect, _legTrackingAvatarSample.LeftFootBone.position);
                        m_LeftFootStepOnLastAction |= (int)BodyActionList.PxrTouchGround;
                    }
                    
                    if ((m_LeftFootStepOnAction & (int)BodyActionList.PxrTouchGroundToe) != 0 && (m_LeftFootStepOnLastAction & (int)BodyActionList.PxrTouchGroundToe) ==0)
                    {
                        PlayStepOnEffect(m_LeftFootStepOnAction, PlayerPrefManager.Instance.PlayerPrefData.steppingEffect, _legTrackingAvatarSample.LeftFootToeBone.position);
                        m_LeftFootStepOnLastAction |= (int)BodyActionList.PxrTouchGroundToe;
                    }
                }
                else
                {
                    m_LeftFootStepOnLastAction &= m_LeftFootStepOnAction;
                }

                if (_legTrackingAvatarSample.RightToeTouchGroundAction * 0.001f >= PlayerPrefManager.Instance.PlayerPrefData.steppingSensitivity)
                {
                    if ((m_RightFootStepOnAction & (int)BodyActionList.PxrTouchGround) != 0 && (m_RightFootStepOnLastAction & (int)BodyActionList.PxrTouchGround) ==0)
                    {
                        PlayStepOnEffect(m_RightFootStepOnAction, PlayerPrefManager.Instance.PlayerPrefData.steppingEffect, _legTrackingAvatarSample.RightFootBone.position);
                        m_RightFootStepOnLastAction |= (int)BodyActionList.PxrTouchGround;
                    }

                    if ((m_RightFootStepOnAction & (int)BodyActionList.PxrTouchGroundToe) != 0 && (m_RightFootStepOnLastAction & (int)BodyActionList.PxrTouchGroundToe) ==0)
                    {
                        PlayStepOnEffect(m_RightFootStepOnAction, PlayerPrefManager.Instance.PlayerPrefData.steppingEffect, _legTrackingAvatarSample.RightFootToeBone.position);
                        m_RightFootStepOnLastAction |= (int)BodyActionList.PxrTouchGroundToe;
                    }
                }
                else
                {
                    m_RightFootStepOnLastAction &= m_RightFootStepOnAction;
                }
            }
        }

        private void OnApplicationFocus(bool focus)
        {
#if UNITY_EDITOR
            return;
#endif
            if (focus)
            {
                if (m_CurrentLegTrackingDemoState == LegTrackingDemoState.START) return;

                UpdateFitnessBandState();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus)
            {
                UpdateFitnessBandState();
            }
        }

        private void StartGame(float height)
        {
            m_CurrentLegTrackingDemoState = LegTrackingDemoState.CALIBRATED;

            //load avatar
            MirrorObj.SetActive(true);
            DancePadUI.SetActive(true);
            MotionTrackerUI.SetActive(true);
            DisplaySettingUI.SetActive(true);
            RecorderUI.SetActive(true);
            DancePadManager.gameObject.SetActive(true);
            LoadAvatar(height);
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

            xrOriginPos.y = _startXROriginY + -(_startFootHeight - _legTrackingAvatarSample.soleHeight); 

            XROrigin.transform.localPosition = xrOriginPos;
            _startXROriginY = xrOriginPos.y;

            Debug.Log($"LegTrackingModeSceneManager.AlignGround: StartFootHeight = {_startFootHeight}, xrOriginPos = {xrOriginPos}");
        }
        
        [ContextMenu("LoadAvatar")]
        public void StartGame()
        {
            try
            {
                
                var task = SportService.GetUserInfo();
                if (task != null)
                {
                    task.OnComplete((rsp) =>
                    {
                        if (!rsp.IsError)
                        {
                            PlayerPrefManager.Instance.PlayerPrefData.height = rsp.Data.Stature;
                            Debug.LogWarning($"SportService.GetUserInfo: Success, Height = {rsp.Data.Stature}");
                        }
                        else
                        {
                            Debug.LogWarning($"SportService.GetUserInfo: Failed, msg = {rsp.Error}");
                        }
                
                        StartGame(PlayerPrefManager.Instance.PlayerPrefData.height);
                    });    
                }
                else
                {
                    StartGame(PlayerPrefManager.Instance.PlayerPrefData.height);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                StartGame(PlayerPrefManager.Instance.PlayerPrefData.height);
            }
        }

        private void LoadAvatar(float height)
        {
            m_AvatarObj = Avatar;
            m_AvatarObj.transform.localScale = Vector3.one;
            m_AvatarObj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            m_AvatarObj.SetActive(true);
            
            _legTrackingAvatarSample = m_AvatarObj.GetComponent<LegTrackingAvatarSample>();
            
            m_LeftFootStepOnAction = m_LeftFootStepOnLastAction = _legTrackingAvatarSample.LeftTouchGroundAction;
            m_RightFootStepOnAction = m_RightFootStepOnLastAction = _legTrackingAvatarSample.RightTouchGroundAction;
            
            m_AvatarLeftFoot = _legTrackingAvatarSample.BonesList[10];
            m_AvatarRightFoot = _legTrackingAvatarSample.BonesList[11];

            
            var scale = height * 1.04f / 175;
            _legTrackingAvatarSample.UpdateBonesLength(scale);
            AlignGround();
            
            // SubsystemManager.GetInstances(s_InputSubsystems);
            // foreach (var t in s_InputSubsystems)
            // {
            //     t.TryRecenter();
            // }

            m_CurrentLegTrackingDemoState = LegTrackingDemoState.PLAYING;
            
            Debug.Log($"LegTrackingModeSceneManager.LoadAvatar: Avatar = {m_AvatarObj.name}, height = {height}");
        }
        
        private void UpdateFitnessBandState()
        {
            PXR_Input.SetSwiftMode(PlayerPrefManager.Instance.PlayerPrefData.bodyTrackMode);

            //Update Swift calibration state after resuming
            int calibrated = -1;
            PXR_Input.GetFitnessBandCalibState(ref calibrated);
            m_SwiftCalibratedState = calibrated == 1;
            if (m_SwiftCalibratedState)
            {
                LegTrackingUIManager.startMenu.SetActive(false);
                StartGame();
                Debug.Log($"LegTrackingModeSceneManager.UpdateFitnessBandState: calibrated = {calibrated}");
            }
            else
            {
                if (m_AvatarObj != null && m_AvatarObj.activeSelf)
                {
                    m_AvatarObj.SetActive(false);
                }
                
                PxrFitnessBandConnectState connectState = new PxrFitnessBandConnectState();
                PXR_Input.GetFitnessBandConnectState(ref connectState);
#if UNITY_EDITOR
                connectState.num = 2;
#endif
                LegTrackingUIManager.startMenu.SetActive(true);
                LegTrackingUIManager.btnContinue.gameObject.SetActive(connectState.num == 2);
                    
                Debug.Log($"LegTrackingModeSceneManager.UpdateFitnessBandState: connectedNum = {connectState.num}");
            }
        }
        
        private void PlayStepOnEffect(int action, int effectType, Vector3 pos)
        {
            if (effectType == 0)
            {
                return;
            }
            
            if (action == 0)
            {
                return;
            }

            if (effectType == 1 || effectType == 3)
            {
                if ((action & (int) BodyActionList.PxrTouchGroundToe) != 0)
                {
                    GameObject obj = Instantiate(stepOnToeEffect);
                    obj.SetActive(true);
                    obj.transform.position = pos + new Vector3(0, -0.02f, 0);
                    obj.GetComponent<ParticleSystem>().Play();
                    Debug.Log($"LegTrackingModeSceneManager.PlayStepOnEffect: action = {action}, effectType = {effectType}, pos = {pos}");
                }
            }
            
            if (effectType == 1 || effectType == 2)
            {
                if ((action & (int) BodyActionList.PxrTouchGround) != 0)
                {
                    GameObject obj = Instantiate(stepOnHeelEffect);
                    obj.SetActive(true);
                    obj.transform.position = pos + new Vector3(0, -.08f, 0);
                    obj.GetComponent<ParticleSystem>().Play();
                    Debug.Log($"LegTrackingModeSceneManager.PlayStepOnEffect: action = {action}, effectType = {effectType}, pos = {pos}");
                }
            }
        }
    }
}