using TMPro;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.UI;

namespace BodyTrackingDemo
{
    public class UIBodyTrackerSetting : MonoBehaviour
    {
        public Button btnCalibration;
        public TMP_Dropdown dropdownMode;
        public Slider sliderSensitivity;
        public TextMeshProUGUI textSensitivityValue;
        public Slider sliderHeight;
        public TextMeshProUGUI textHeightValue;

        private void Awake()
        {
            btnCalibration.onClick.AddListener(OnCalibration);
            dropdownMode.onValueChanged.AddListener(OnModeChanged);
            sliderSensitivity.onValueChanged.AddListener(OnSensitivityChanged);
            sliderHeight.onValueChanged.AddListener(OnHeightChanged);
        }

        private void Start()
        {
            dropdownMode.value = PlayerPrefManager.Instance.PlayerPrefData.bodyTrackMode;
            sliderSensitivity.value = PlayerPrefManager.Instance.PlayerPrefData.steppingSensitivity;
            sliderHeight.value = PlayerPrefManager.Instance.PlayerPrefData.height;
            
            textSensitivityValue.text = sliderSensitivity.value.ToString("f2");
            textHeightValue.text = sliderHeight.value.ToString("f0");
        }
        
        private void OnHeightChanged(float value)
        {
            PlayerPrefManager.Instance.PlayerPrefData.height = value;
            textHeightValue.text = value.ToString("f0");
        }
        
        private void OnSensitivityChanged(float value)
        {
            textSensitivityValue.text = value.ToString("f2");
            PlayerPrefManager.Instance.PlayerPrefData.steppingSensitivity = value;
            Debug.Log($"UIBodyTrackerSetting.OnSensitivityChanged: value = {value}");
        }

        public void OnModeChanged(int modeIdx)
        {
            PlayerPrefManager.Instance.PlayerPrefData.bodyTrackMode = modeIdx;
            PXR_Input.SetSwiftMode(modeIdx);
            Debug.Log($"UIBodyTrackerSetting.OnModeChanged: modeIdx = {modeIdx}");
        }

        private void OnCalibration()
        {
            PXR_Input.OpenFitnessBandCalibrationAPP();
            LegTrackingModeSceneManager.Instance.m_CurrentLegTrackingDemoState = LegTrackingModeSceneManager.LegTrackingDemoState.CALIBRATING;
            LegTrackingModeSceneManager.Instance.HideAvatar();
        }
    }
}