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
        public GameObject browser;
        public GameObject browserMirror;

        private void Start()
        {
            toggleAutoRecording.onValueChanged.AddListener(OnAutoRecordingChanged);
            btnRecording.onClick.AddListener(OnRecording);
            cameraRecorder.gameObject.SetActive(false);

            toggleAutoRecording.isOn = PlayerPrefManager.Instance.PlayerPrefData.autoRecording;
            if (toggleAutoRecording.isOn)
            {
                cameraRecorder.gameObject.SetActive(true);
                cameraRecorder.StartRecording();
            }

            browserMirror.SetActive(false);
            
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
                cameraRecorder.gameObject.SetActive(false);
                cameraRecorder.StopRecording();
                if (browser.activeSelf)
                {
                    browserMirror.SetActive(false);
                }
            }
            else
            {
                cameraRecorder.gameObject.SetActive(true);
                cameraRecorder.StartRecording();
                if (browser.activeSelf)
                {
                    browserMirror.SetActive(true);
                }
            }

            UpdateStatusText();
        }

        private void UpdateStatusText()
        {
            textStatus.text = cameraRecorder.IsRecording ? "Stop" : "Start";
        }
    }
}