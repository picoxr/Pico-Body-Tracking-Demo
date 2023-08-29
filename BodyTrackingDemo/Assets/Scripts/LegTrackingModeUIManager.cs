using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.XR.PXR;

namespace BodyTrackingDemo
{
    public class LegTrackingModeUIManager : MonoBehaviour
    {
        public GameObject startMenu;
        public Button btnContinue;
        public TMP_Dropdown dropdownMode;

        private void Awake()
        {
            btnContinue.onClick.AddListener(OnContinue);
            dropdownMode.onValueChanged.AddListener(OnModeChanged);
        }

        private void Start()
        {
            dropdownMode.value = PlayerPrefManager.Instance.PlayerPrefData.bodyTrackMode;
        }

        private void OnModeChanged(int modeIdx)
        {
            PlayerPrefManager.Instance.PlayerPrefData.bodyTrackMode = modeIdx;
            PXR_Input.SetSwiftMode(modeIdx);
            Debug.Log($"LegTrackingModeUIManager.OnModeChanged: modeIdx = {modeIdx}");
        }

        private void OnContinue()
        {
            startMenu.SetActive(false);
            LegTrackingModeSceneManager.Instance.StartGame();
        }

        public void OnDemoStart()
        {
            startMenu.SetActive(false);
            PXR_Input.OpenFitnessBandCalibrationAPP();
            LegTrackingModeSceneManager.Instance.m_CurrentLegTrackingDemoState = LegTrackingModeSceneManager.LegTrackingDemoState.CALIBRATING;
        }
    }
}