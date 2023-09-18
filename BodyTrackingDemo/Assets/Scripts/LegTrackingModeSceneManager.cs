using System;
using System.Collections;
using System.Collections.Generic;
using Pico.Platform;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.XR;

namespace BodyTrackingDemo
{
    public class LegTrackingModeSceneManager : MonoBehaviour
    {
        public enum LegTrackingDemoState
        {
            START,
            CALIBRATING,
            CALIBRATED,
            PLAYING
        }

        public static LegTrackingModeSceneManager Instance;

        private static List<XRInputSubsystem> s_InputSubsystems = new();

        public LegTrackingModeUIManager startCanvas;
        public DancePadsManager DancePadManager;
        public GameObject RecorderUI;
        public GameObject gameCanvas;
        public GameObject XROrigin;
        public GameObject Avatar;

        [SerializeField] private GameObject stepOnLeftToeEffect;
        [SerializeField] private GameObject stepOnLeftHeelEffect;
        [SerializeField] private GameObject stepOnToeEffect;
        [SerializeField] private GameObject stepOnHeelEffect;

        [SerializeField] private AudioSource stepOnToeSFX;
        [SerializeField] private AudioSource stepOnHeelSFX;


        [HideInInspector] public LegTrackingDemoState m_CurrentLegTrackingDemoState;
        private LegTrackingAvatarSample _legTrackingAvatarSample;
        private float _startFootHeight;
        private float _startXROriginY;
        private Transform m_AvatarLeftFoot;

        private GameObject m_AvatarObj;
        private Transform m_AvatarRightFoot;
        private int m_LeftFootStepOnAction;
        private int m_LeftFootStepOnLastAction;
        private int m_RightFootStepOnAction;
        private int m_RightFootStepOnLastAction;

        private bool m_SwiftCalibratedState;
        private float _avatarScale;

        private void Awake()
        {
            Instance = this;

            _startXROriginY = XROrigin.transform.localPosition.y;
            
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
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus) UpdateFitnessBandState();
        }
        
        // Update is called once per frame
        private void LateUpdate()
        {
            if (m_CurrentLegTrackingDemoState == LegTrackingDemoState.PLAYING)
            {
                m_LeftFootStepOnAction = _legTrackingAvatarSample.LeftTouchGroundAction;
                m_RightFootStepOnAction = _legTrackingAvatarSample.RightTouchGroundAction;
                DancePadManager.DancePadHoleStepOnDetection(m_AvatarLeftFoot.position, m_AvatarRightFoot.position, m_LeftFootStepOnAction, m_RightFootStepOnAction, m_LeftFootStepOnLastAction, m_RightFootStepOnLastAction);

                if ((m_LeftFootStepOnAction & (int) BodyActionList.PxrTouchGround) != 0 && (m_LeftFootStepOnLastAction & (int) BodyActionList.PxrTouchGround) == 0)
                {
                    PlayStepOnEffect(BodyActionList.PxrTouchGround, _legTrackingAvatarSample.LeftFootBone, stepOnLeftHeelEffect);
                }
                if ((m_LeftFootStepOnAction & (int) BodyActionList.PxrTouchGroundToe) != 0 && (m_LeftFootStepOnLastAction & (int) BodyActionList.PxrTouchGroundToe) == 0)
                {
                    PlayStepOnEffect(BodyActionList.PxrTouchGroundToe, _legTrackingAvatarSample.LeftFootToeBone, stepOnLeftToeEffect);
                }
                m_LeftFootStepOnLastAction = m_LeftFootStepOnAction;

                if ((m_RightFootStepOnAction & (int) BodyActionList.PxrTouchGround) != 0 && (m_RightFootStepOnLastAction & (int) BodyActionList.PxrTouchGround) == 0)
                {
                    PlayStepOnEffect(BodyActionList.PxrTouchGround, _legTrackingAvatarSample.RightFootBone, stepOnHeelEffect);
                }
                if ((m_RightFootStepOnAction & (int) BodyActionList.PxrTouchGroundToe) != 0 && (m_RightFootStepOnLastAction & (int) BodyActionList.PxrTouchGroundToe) == 0)
                {
                    PlayStepOnEffect(BodyActionList.PxrTouchGroundToe, _legTrackingAvatarSample.RightFootToeBone, stepOnToeEffect);
                }
                m_RightFootStepOnLastAction = m_RightFootStepOnAction;
            }
        }

        private void OnApplicationFocus(bool focus)
        {
#if UNITY_EDITOR
            // return;
#endif
            if (focus)
            {
                if (m_CurrentLegTrackingDemoState == LegTrackingDemoState.START) return;

                UpdateFitnessBandState();
            }
        }

        private void StartGame(float height)
        {
            m_CurrentLegTrackingDemoState = LegTrackingDemoState.CALIBRATED;

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

            _legTrackingAvatarSample = m_AvatarObj.GetComponent<LegTrackingAvatarSample>();

            m_LeftFootStepOnAction = m_LeftFootStepOnLastAction = _legTrackingAvatarSample.LeftTouchGroundAction;
            m_RightFootStepOnAction = m_RightFootStepOnLastAction = _legTrackingAvatarSample.RightTouchGroundAction;

            m_AvatarLeftFoot = _legTrackingAvatarSample.BonesList[10];
            m_AvatarRightFoot = _legTrackingAvatarSample.BonesList[11];


            _avatarScale = height / 175;
            
            // XROrigin.transform.localScale = Vector3.one * _avatarScale;
            _legTrackingAvatarSample.UpdateBonesLength(_avatarScale);

            Avatar.SetActive(false);
            yield return new WaitForEndOfFrame();
            AlignGround();

            Avatar.SetActive(true);
            gameCanvas.SetActive(true);
#if RECORDER
            RecorderUI.SetActive(true);
#endif
            
            DancePadManager.gameObject.SetActive(true);

            m_CurrentLegTrackingDemoState = LegTrackingDemoState.PLAYING;

            Debug.Log($"LegTrackingModeSceneManager.LoadAvatar: Avatar = {m_AvatarObj.name}, height = {height}");
        }

        private void UpdateFitnessBandState()
        {
            PXR_Input.SetSwiftMode(PlayerPrefManager.Instance.PlayerPrefData.bodyTrackMode);

            //Update Swift calibration state after resuming
            var calibrated = -1;
            PXR_Input.GetFitnessBandCalibState(ref calibrated);
            m_SwiftCalibratedState = calibrated == 1;
            if (m_SwiftCalibratedState)
            {
                startCanvas.startMenu.SetActive(false);
                StartGame();
                Debug.Log($"LegTrackingModeSceneManager.UpdateFitnessBandState: calibrated = {calibrated}");
            }
            else
            {
                if (m_AvatarObj != null && m_AvatarObj.activeSelf) m_AvatarObj.SetActive(false);

                var connectState = new PxrFitnessBandConnectState();
                PXR_Input.GetFitnessBandConnectState(ref connectState);
#if UNITY_EDITOR
                connectState.num = 2;
#endif
                startCanvas.startMenu.SetActive(true);
                startCanvas.btnContinue.gameObject.SetActive(connectState.num == 2);

                Debug.Log($"LegTrackingModeSceneManager.UpdateFitnessBandState: connectedNum = {connectState.num}");
            }
        }

        public void HideAvatar()
        {
            if (m_AvatarObj != null)
            {
                m_AvatarObj.SetActive(false);
            }
        }
        
        private void PlayStepOnEffect(BodyActionList action, Transform joint, GameObject effect)
        {
            int effectType = PlayerPrefManager.Instance.PlayerPrefData.steppingEffect;
            if (effectType == 0) return;

            if (action == 0) return;

            if (DancePadManager.IsDancePadGamePlaying)
            {
                return;
            }

            Vector3 pos = joint.position;
            Quaternion rotation = joint.rotation;
            
            switch (action)
            {
                case BodyActionList.PxrTouchGroundToe:
                {
                    var targetPos = pos + new Vector3(0, -0.02f, 0);
                    if (effectType == 1 || effectType == 3)
                    {
                        var vfx = Instantiate(effect, targetPos, rotation);
                        vfx.transform.localScale = Vector3.one * _avatarScale;
                        vfx.SetActive(true);
                        // var particles = obj.GetComponentsInChildren<ParticleSystem>();
                        // foreach (var particle in particles)
                        // {
                        //     particle.Play();
                        // }
                    }

                    var sfx = Instantiate(stepOnToeSFX, targetPos, Quaternion.identity);
                    sfx.gameObject.SetActive(true);
                    sfx.Play();
                    Debug.Log($"LegTrackingModeSceneManager.PlayStepOnEffect: action = {action}, effectType = {effectType}, targetPos = {targetPos}");
                    break;
                }
                case BodyActionList.PxrTouchGround:
                {
                    var targetPos = pos + new Vector3(0, -.08f, 0);
                    if (effectType == 1 || effectType == 2)
                    {
                        var vfx = Instantiate(effect, targetPos, rotation);
                        vfx.transform.localScale = Vector3.one * _avatarScale;
                        vfx.SetActive(true);
                        // var particles = obj.GetComponentsInChildren<ParticleSystem>();
                        // foreach (var particle in particles)
                        // {
                        //     particle.Play();
                        // }
                    }

                    var sfx = Instantiate(stepOnHeelSFX, targetPos, Quaternion.identity);
                    sfx.gameObject.SetActive(true);
                    sfx.Play();
                    Debug.Log($"LegTrackingModeSceneManager.PlayStepOnEffect: action = {action}, effectType = {effectType}, targetPos = {targetPos}");
                    break;
                }
            }
        }
    }
}