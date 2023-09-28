/* 
*   NatCorder
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatSuite.Examples {

    using UnityEngine;
    using Recorders;
    using Recorders.Inputs;

    public class Giffy : MonoBehaviour {
        
        [Header("GIF Settings")]
        public int imageWidth = 640;
        public int imageHeight = 480;
        public float frameDuration = 0.1f; // seconds

        private GIFRecorder recorder;
        private CameraInput cameraInput;

        public void StartRecording () {
            // Start recording
            recorder = new GIFRecorder(imageWidth, imageHeight, frameDuration);
            cameraInput = new CameraInput(recorder, Camera.main);
            // Get a real GIF look by skipping frames
            cameraInput.frameSkip = 4;
        }

        public async void StopRecording () {
            // Stop the recording
            cameraInput.Dispose();
            var path = await recorder.FinishWriting();
            // Log path
            Debug.Log($"Saved animated GIF image to: {path}");
            Application.OpenURL($"file://{path}");
        }
    }
}