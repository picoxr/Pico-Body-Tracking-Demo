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
        public GameObject UICanvas;
        public GameObject Enviroment;
        public GameObject XROrigin;
        public GameObject Avatar;

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
        // private float _startEnvironmentY;
        // private float _startDancePadY;
        // private float _startCanvasY;
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

            // _startCanvasY = UICanvas.transform.position.y;
            // _startEnvironmentY = Enviroment.transform.position.y;
            // _startDancePadY = DancePadManager.transform.position.y;
            _startXROriginY = XROrigin.transform.localPosition.y;

            // m_CurrentLegTrackingDemoState = LegTrackingDemoState.START;
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
                // if (m_LeftFootStepOnAction == 1 || m_RightFootStepOnAction == 1)
                {
                    DancePadManager.DancePadHoleStepOnDetection(m_AvatarLeftFoot.position, m_AvatarRightFoot.position, m_LeftFootStepOnAction, m_RightFootStepOnAction, m_LeftFootStepOnLastAction, m_RightFootStepOnLastAction);
                }
                m_LeftFootStepOnLastAction = _legTrackingAvatarSample.LeftTouchGroundAction;
                m_RightFootStepOnLastAction = _legTrackingAvatarSample.RightTouchGroundAction;
                
#if UNITY_EDITOR
                //For editor test only
                DancePadManager.DancePadHoleStepOnDetection(GameObject.Find("GameObject").transform.position, GameObject.Find("GameObjectRight").transform.position);
#endif
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

        public void StartGame()
        {
            m_CurrentLegTrackingDemoState = LegTrackingDemoState.CALIBRATED;

            //load avatar
            MirrorObj.SetActive(true);
            DancePadUI.SetActive(true);
            MotionTrackerUI.SetActive(true);
            DisplaySettingUI.SetActive(true);
            RecorderUI.SetActive(true);
            DancePadManager.gameObject.SetActive(true);
            LoadAvatar();
        }

        public void AlignGround()
        {
            if (m_AvatarObj == null)
            {
                Debug.LogError("There is no loaded avatar!");
                return;
            }
            
            _startFootHeight = Mathf.Min(m_AvatarLeftFoot.transform.position.y, m_AvatarRightFoot.transform.position.y);
            
            // var canvasPos = UICanvas.transform.position;
            // var environmentPos = Enviroment.transform.position;
            // var dancePadPos = DancePadManager.transform.position;
            var xrOriginPos = XROrigin.transform.localPosition;

            // canvasPos.y = _startCanvasY + _startFootHeight - _legTrackingAvatarSample.soleHeight;
            // environmentPos.y = _startEnvironmentY + _startFootHeight - _legTrackingAvatarSample.soleHeight;
            // dancePadPos.y = _startDancePadY + _startFootHeight - _legTrackingAvatarSample.soleHeight;
            xrOriginPos.y = _startXROriginY + -(_startFootHeight - _legTrackingAvatarSample.soleHeight); 

            // UICanvas.transform.position = canvasPos;
            // Enviroment.transform.position = environmentPos;
            // DancePadManager.transform.position = dancePadPos;

            XROrigin.transform.localPosition = xrOriginPos;
            _startXROriginY = xrOriginPos.y;

            Debug.Log($"LegTrackingModeSceneManager.AlignGround: StartFootHeight = {_startFootHeight}, xrOriginPos = {xrOriginPos}");
        }
        
        [ContextMenu("LoadAvatar")]
        private void LoadAvatar()
        {
#if UNITY_EDITOR
            LoadAvatar(PlayerPrefManager.Instance.PlayerPrefData.height);
            //return;
#endif
            SportService.GetUserInfo().OnComplete((rsp) =>
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
                
                LoadAvatar(PlayerPrefManager.Instance.PlayerPrefData.height);
            });
        }

        private void LoadAvatar(float height)
        {
            // if (m_AvatarObj != null)
            // {
            //     GameObject temp = m_AvatarObj;
            //     m_AvatarObj = null;
            //     GameObject.Destroy(temp);
            // }

            m_AvatarObj = Avatar;
            // current logic use only one Avatar model for each gender.
            // string avatar_name = "ClothRun_175 Variant LegTracking";
            //
            // GameObject avatarObj = Resources.Load<GameObject>("Prefabs/" + avatar_name);
            // m_AvatarObj = Instantiate(avatarObj, XROrigin.transform);
            
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
                m_CurrentLegTrackingDemoState = LegTrackingDemoState.CALIBRATED;

                //load avatar
                MirrorObj.SetActive(true);
                DancePadUI.SetActive(true);
                MotionTrackerUI.SetActive(true);
                DisplaySettingUI.SetActive(true);
                RecorderUI.SetActive(true);
                
                DancePadManager.gameObject.SetActive(true);
                LegTrackingUIManager.startMenu.SetActive(false);
                
                LoadAvatar();
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
    }
}