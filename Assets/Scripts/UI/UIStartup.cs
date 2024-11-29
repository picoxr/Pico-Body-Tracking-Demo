using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.XR.PXR;

namespace BodyTrackingDemo
{
    public class UIStartup : MonoBehaviour
    {
        public GameObject startMenu;
        public Button btnContinue;
        public TMP_Dropdown dropdownMode;
        public Slider sliderHeight;
        public TextMeshProUGUI textHeightValue;
        public TextMeshProUGUI textRayHint;
        
        private void Awake()
        {
            btnContinue.onClick.AddListener(OnContinue);
            dropdownMode.onValueChanged.AddListener(OnModeChanged);
            sliderHeight.onValueChanged.AddListener(OnHeightChanged);
        }

        private void OnEnable()
        {
            dropdownMode.value = PlayerPrefManager.Instance.PlayerPrefData.bodyTrackMode;
            sliderHeight.value = PlayerPrefManager.Instance.PlayerPrefData.height;
            textRayHint.gameObject.SetActive(PlayerPrefManager.Instance.PlayerPrefData.interactionRayMode == 0);
            
            textHeightValue.text = sliderHeight.value.ToString("f0");
        }

        private void OnHeightChanged(float value)
        {
            PlayerPrefManager.Instance.PlayerPrefData.height = value;
            textHeightValue.text = value.ToString("f0");
        }
        
        private void OnModeChanged(int modeIdx)
        {
            PlayerPrefManager.Instance.PlayerPrefData.bodyTrackMode = modeIdx;
            PXR_Input.SetBodyTrackingMode((BodyTrackingMode)modeIdx);
            Debug.Log($"UIStartup.OnModeChanged: modeIdx = {modeIdx}");
        }

        private void OnContinue()
        {
            startMenu.SetActive(false);
            BodyTrackingManager.Instance.StartGame();
        }

        public void OnDemoStart()
        {
            startMenu.SetActive(false);
            PXR_MotionTracking.StartMotionTrackerCalibApp();
            BodyTrackingManager.Instance.m_CurrentBodyTrackingDemoState = BodyTrackingManager.BodyTrackingDemoState.CALIBRATING;
        }
    }
}