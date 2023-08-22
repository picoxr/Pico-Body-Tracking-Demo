using UnityEngine;
using Unity.XR.PXR;
using UnityEngine.XR;

namespace BodyTrackingDemo
{
    public class LegTrackingModeSceneManager : MonoBehaviour
    {
        public static LegTrackingModeSceneManager Instance;
        public LegTrackingModeUIManager LegTrackingUIManager;
        public DancePadsManager DancePadManager;
        public GameObject DancePadUI;
        public GameObject MirrorObj;
        [HideInInspector] public LegTrackingDemoState m_CurrentLegTrackingDemoState;



        private GameObject m_AvatarObj;
        private Transform m_AvatarLeftFoot;
        private Transform m_AvatarRightFoot;
        private int m_LeftFootStepOnAction;
        private int m_RightFootStepOnAction;

        private bool m_SwiftCalibratedState;
        private LegTrackingAvatarSample _legTrackingAvatarSample;
        private float _startFootHeight;

        public enum LegTrackingDemoState
        {
            START,
            CALIBRATING,
            CALIBRATED,
            PLAYING,
        }

        // Start is called before the first frame update
        void Start()
        {
            Instance = this;

            m_CurrentLegTrackingDemoState = LegTrackingDemoState.START;
            MirrorObj.SetActive(false);
            DancePadUI.SetActive(false);
            DancePadManager.gameObject.SetActive(false);
        }

        // Update is called once per frame
        void LateUpdate()
        {
            if (m_CurrentLegTrackingDemoState == LegTrackingDemoState.PLAYING)
            {
                if (InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool right_pressed) && right_pressed ||
                    InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool left_pressed) && left_pressed)
                {
                    PXR_Input.OpenFitnessBandCalibrationAPP();
                }

                if (m_AvatarObj != null)
                {
                    m_LeftFootStepOnAction = _legTrackingAvatarSample.LeftTouchGroundAction;
                    m_RightFootStepOnAction = _legTrackingAvatarSample.RightTouchGroundAction;
                    if (m_LeftFootStepOnAction == 1 || m_RightFootStepOnAction == 1)
                    {
                        DancePadManager.DancePadHoleStepOnDetection(m_AvatarLeftFoot.position, m_AvatarRightFoot.position, m_LeftFootStepOnAction, m_RightFootStepOnAction);
                    }
                }
#if UNITY_EDITOR
                //For editor test only
                DancePadManager.DancePadHoleStepOnDetection(GameObject.Find("GameObject").transform.position, GameObject.Find("GameObjectRight").transform.position);
#endif
            }
        }

        private void OnApplicationFocus(bool focus)
        {
            Debug.Log("[SwiftDemoTest] Application focus: " + focus);

            if (focus)
            {
                if (m_CurrentLegTrackingDemoState == LegTrackingDemoState.START) return;

                //Update Swift calibration state after resuming
                int calibrated = -1;
                PXR_Input.GetFitnessBandCalibState(ref calibrated);
                m_SwiftCalibratedState = calibrated == 1;
                Debug.Log("[SwiftDemoTest] Swift calibrated: " + calibrated);
                if (m_SwiftCalibratedState)
                {
                    m_CurrentLegTrackingDemoState = LegTrackingDemoState.CALIBRATED;

                    //load avatar
                    MirrorObj.SetActive(true);
                    DancePadUI.SetActive(true);
                    DancePadManager.gameObject.SetActive(true);
                    LoadAvatar();
                    //SetControllersActive(false);
                    
                    PXR_Input.SetSwiftMode(PlayerPrefManager.Instance.PlayerPrefData.bodyTrackMode);
                }
                else
                {
                    PxrFitnessBandConnectState connectState = new PxrFitnessBandConnectState();
                    PXR_Input.GetFitnessBandConnectState(ref connectState);
                    
                    LegTrackingUIManager.startMenu.SetActive(true);
                    LegTrackingUIManager.btnContinue.gameObject.SetActive(connectState.num == 2 && m_SwiftCalibratedState);
                }
            }
        }

        public void StartGame()
        {
            m_CurrentLegTrackingDemoState = LegTrackingDemoState.CALIBRATED;

            //load avatar
            MirrorObj.SetActive(true);
            DancePadUI.SetActive(true);
            DancePadManager.gameObject.SetActive(true);
            LoadAvatar();
        }

        private void LoadAvatar()
        {
            if (m_AvatarObj != null)
            {
                GameObject temp = m_AvatarObj;
                m_AvatarObj = null;
                GameObject.Destroy(temp);
            }

            // current logic use only one Avatar model for each gender.
            string avatar_name = "ClothRun_175 Variant LegTracking";
            
            GameObject avatarObj = Resources.Load<GameObject>("Prefabs/" + avatar_name);
            m_AvatarObj = Instantiate(avatarObj, Vector3.zero, Quaternion.identity);
            float scale = 1; // use 1 for leg tracking mode
            m_AvatarObj.transform.localScale = new Vector3(scale, scale, scale);
            m_AvatarObj.SetActive(true);
            
            _legTrackingAvatarSample = m_AvatarObj.GetComponent<LegTrackingAvatarSample>();
            m_AvatarLeftFoot = _legTrackingAvatarSample.BonesList[7];
            m_AvatarRightFoot = _legTrackingAvatarSample.BonesList[8];
            
            _startFootHeight = Mathf.Min(m_AvatarLeftFoot.transform.position.y, m_AvatarRightFoot.transform.position.y);

            m_CurrentLegTrackingDemoState = LegTrackingDemoState.PLAYING;
            
            Debug.Log($"LegTrackingModeSceneManager.LoadAvatar: Model = {avatar_name}, StartFootHeight = {_startFootHeight}");
        }
    }
}