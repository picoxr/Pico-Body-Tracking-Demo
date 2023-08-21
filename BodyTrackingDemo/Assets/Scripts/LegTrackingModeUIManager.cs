using UnityEngine;
using UnityEngine.UI;
using Unity.XR.PXR;

namespace BodyTrackingDemo
{
    public class LegTrackingModeUIManager : MonoBehaviour
    {
        public GameObject startMenu;
        public Button btnContinue;
        public Toggle fullBodyTrackingToggle;

        private void Awake()
        {
            btnContinue.onClick.AddListener(OnContinue);
        }

        private void OnContinue()
        {
            startMenu.SetActive(false);
            LegTrackingModeSceneManager.Instance.StartGame();
        }

        private void Start()
        {
            int bodyTrackMode = PlayerPrefManager.Instance.PlayerPrefData.bodyTrackMode;
            fullBodyTrackingToggle.isOn = bodyTrackMode == 1;
            PXR_Input.SetSwiftMode(bodyTrackMode);
        }

        public void OnDemoStart()
        {
            startMenu.SetActive(false);
            PXR_Input.OpenFitnessBandCalibrationAPP();
            LegTrackingModeSceneManager.Instance.m_CurrentLegTrackingDemoState = LegTrackingModeSceneManager.LegTrackingDemoState.CALIBRATING;
        }

        public void OnFullBodyTrackingToggleValueChange(bool enable)
        {
            Debug.Log("[DragonTest] FullBodyTracking = " + enable);
            PlayerPrefManager.Instance.PlayerPrefData.bodyTrackMode = enable ? 1 : 0;
            PXR_Input.SetSwiftMode(enable ? 1 : 0);
        }
    }
}