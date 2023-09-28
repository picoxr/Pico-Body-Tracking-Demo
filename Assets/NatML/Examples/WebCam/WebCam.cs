/* 
*   NatCorder
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatSuite.Examples {

    using System.Collections;
    using UnityEngine;
    using UnityEngine.UI;
    using Recorders;
    using Recorders.Clocks;

    public class WebCam : MonoBehaviour {

        [Header(@"UI")]
        public RawImage rawImage;
        public AspectRatioFitter aspectFitter;

        private WebCamTexture webCamTexture;
        private MP4Recorder recorder;
        private IClock clock;
        private bool recording;
        private Color32[] pixelBuffer;


        #region --Recording State--

        public void StartRecording () {
            // Start recording
            clock = new RealtimeClock();
            recorder = new MP4Recorder(webCamTexture.width, webCamTexture.height, 30);
            pixelBuffer = webCamTexture.GetPixels32();
            recording = true;
        }

        public async void StopRecording () {
            // Stop recording
            recording = false;
            var path = await recorder.FinishWriting();
            // Playback recording
            Debug.Log($"Saved recording to: {path}");
            #if !UNITY_STANDALONE
            Handheld.PlayFullScreenMovie($"file://{path}");
            #endif
        }
        #endregion


        #region --Operations--

        IEnumerator Start () {
            // Request camera permission
            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
            if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
                yield break;
            // Start the WebCamTexture
            webCamTexture = new WebCamTexture(1280, 720, 30);
            webCamTexture.Play();
            // Display webcam
            yield return new WaitUntil(() => webCamTexture.width != 16 && webCamTexture.height != 16); // Workaround for weird bug on macOS
            rawImage.texture = webCamTexture;
            aspectFitter.aspectRatio = (float)webCamTexture.width / webCamTexture.height;
        }

        void Update () {
            // Record frames from the webcam
            if (recording && webCamTexture.didUpdateThisFrame) {
                webCamTexture.GetPixels32(pixelBuffer);
                recorder.CommitFrame(pixelBuffer, clock.timestamp);
            }
        }
        #endregion
    }
}