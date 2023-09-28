using UnityEngine;

#if RECORDER
using NatSuite.Recorders;
using NatSuite.Recorders.Clocks;
using NatSuite.Recorders.Inputs;
#endif

namespace BodyTrackingDemo
{
    public class CameraRecorder : MonoBehaviour
    {
        [Header(@"Recording")]
        public int videoWidth = 1280;
        public int videoHeight = 720;
        public bool recordMicrophone;
        public Camera targetCamera;

        // private AudioSource _microphoneSource;
#if RECORDER
        private AudioInput _audioInputListener;
        private CameraInput _cameraInput;
        private MP4Recorder _recorder;
        private RealtimeClock _clock;
#endif

        public bool IsRecording { get; private set; }

        
        private void Awake()
        {
            if (targetCamera == null) targetCamera = GetComponent<Camera>();

            if (targetCamera == null) targetCamera = Camera.current;
        }

        // private IEnumerator Start()
        // {
        //     // Start microphone
        //     _microphoneSource = gameObject.AddComponent<AudioSource>();
        //     _microphoneSource.mute =
        //         _microphoneSource.loop = true;
        //     _microphoneSource.bypassEffects =
        //         _microphoneSource.bypassListenerEffects = false;
        //     _microphoneSource.clip = Microphone.Start(null, true, 1, AudioSettings.outputSampleRate);
        //     yield return new WaitUntil(() => Microphone.GetPosition(null) > 0);
        //     _microphoneSource.Play();
        // }

        private void OnDestroy()
        {
            // Stop microphone
            // _microphoneSource.Stop();
            // Microphone.End(null);
            StopRecording();
        }

        [ContextMenu("StartRecording")]
        public void StartRecording()
        {
            // Start recording
            var frameRate = 30;
            var sampleRate = recordMicrophone ? AudioSettings.outputSampleRate : 0;
            var channelCount = recordMicrophone ? (int) AudioSettings.speakerMode : 0;
#if RECORDER
            _clock = new RealtimeClock();
            _recorder = new MP4Recorder(videoWidth, videoHeight, frameRate, sampleRate, channelCount, audioBitRate: 96_000);
            _cameraInput = new CameraInput(_recorder, _clock, targetCamera);

            var audioListener = FindObjectOfType<AudioListener>();
           _audioInputListener = new AudioInput(_recorder, _clock, audioListener);
#endif

            IsRecording = true;
            
            Debug.Log($"CameraRecorder.StartRecording: resolution = {videoWidth}x{videoHeight}, camera = {targetCamera.name}");
        }

        [ContextMenu("StopRecording")]
        public async void StopRecording()
        {
            if (IsRecording)
            {
                IsRecording = false;
                // Mute microphone
                // _microphoneSource.mute = true;
                // Stop recording
#if RECORDER
                _audioInputListener?.Dispose();
                _cameraInput.Dispose();
                var path = await _recorder.FinishWriting();
                // Playback recording
                Debug.Log($"Saved recording to: {path}");
#endif
// #if !UNITY_STANDALONE
//                 Handheld.PlayFullScreenMovie($"file://{path}");
// #endif
                Debug.Log($"CameraRecorder.StopRecording: resolution = {videoWidth}x{videoHeight}, camera = {targetCamera.name}");
            }
        }
    }
}