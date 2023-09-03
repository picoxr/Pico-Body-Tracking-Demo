/* 
*   NatCorder
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatSuite.Examples {

    using System.Collections;
    using UnityEngine;
    using Recorders;
    using Recorders.Inputs;

    public class Karaoke : MonoBehaviour {

        private WAVRecorder recorder;
        private AudioInput audioInput;
        private AudioSource microphoneSource;

        private IEnumerator Start () {
            // Start microphone
            microphoneSource = gameObject.AddComponent<AudioSource>();
            microphoneSource.mute =
            microphoneSource.loop = true;
            microphoneSource.bypassEffects =
            microphoneSource.bypassListenerEffects = false;
            microphoneSource.clip = Microphone.Start(null, true, 1, AudioSettings.outputSampleRate);
            yield return new WaitUntil(() => Microphone.GetPosition(null) > 0);
            microphoneSource.Play();
        }

        private void OnDestroy () {
            // Stop microphone
            microphoneSource.Stop();
            Microphone.End(null);
        }

        public void StartRecording () {
            // Start recording
            recorder = new WAVRecorder(AudioSettings.outputSampleRate, (int)AudioSettings.speakerMode);
            audioInput = new AudioInput(recorder, microphoneSource, true);
            // Unmute microphone
            microphoneSource.mute = false;
        }

        public async void StopRecording () {
            // Mute microphone
            microphoneSource.mute = true;
            // Stop recording
            audioInput.Dispose();
            var path = await recorder.FinishWriting();
            // Playback recording
            Debug.Log($"Saved recording to: {path}");
            #if !UNITY_STANDALONE
            Handheld.PlayFullScreenMovie($"file://{path}");
            #endif
        }
    }
}