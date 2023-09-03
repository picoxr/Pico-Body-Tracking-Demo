using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BodyTrackingDemo
{
    public class UIRecorder : MonoBehaviour
    {
        public Toggle toggleAutoRecording;
        public Button btnRecording;
        public TextMeshProUGUI textStatus;
        public CameraRecorder cameraRecorder;

        private void Start()
        {
            toggleAutoRecording.onValueChanged.AddListener(OnAutoRecordingChanged);
            btnRecording.onClick.AddListener(OnRecording);

            toggleAutoRecording.isOn = PlayerPrefManager.Instance.PlayerPrefData.autoRecording;
            if (toggleAutoRecording.isOn)
            {
                cameraRecorder.StartRecording();
            }

            UpdateStatusText();
        }

        private void OnAutoRecordingChanged(bool value)
        {
            PlayerPrefManager.Instance.PlayerPrefData.autoRecording = value;
        }

        private async void OnRecording()
        {
            if (cameraRecorder.IsRecording)
            {
                cameraRecorder.StopRecording();
            }
            else
            {
                cameraRecorder.StartRecording();
            }

            UpdateStatusText();
        }

        private void UpdateStatusText()
        {
            textStatus.text = cameraRecorder.IsRecording ? "Stop" : "Start";
        }
    }
}