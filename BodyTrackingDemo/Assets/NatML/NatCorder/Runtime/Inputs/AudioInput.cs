/* 
*   NatCorder
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatSuite.Recorders.Inputs {

    using System;
    using UnityEngine;
    using Clocks;

    /// <summary>
    /// Recorder input for recording audio frames from an `AudioListener` or `AudioSource`.
    /// </summary>
    public sealed class AudioInput : IDisposable {

        #region --Client API--
        /// <summary>
        /// Create an audio recording input from a scene's AudioListener.
        /// </summary>
        /// <param name="recorder">Media recorder to receive audio frames.</param>
        /// <param name="audioListener">Audio listener for the current scene.</param>
        public AudioInput (IMediaRecorder recorder, AudioListener audioListener) : this(recorder, default, audioListener) {}

        /// <summary>
        /// Create an audio recording input from a scene's AudioListener.
        /// </summary>
        /// <param name="recorder">Media recorder to receive audio frames.</param>
        /// <param name="clock">Clock for generating timestamps. Can be `null` if recorder does not require timestamps.</param>
        /// <param name="audioListener">Audio listener for the current scene.</param>
        public AudioInput (IMediaRecorder recorder, IClock clock, AudioListener audioListener) : this(recorder, clock, audioListener.gameObject) {}

        /// <summary>
        /// Create an audio recording input from an AudioSource.
        /// </summary>
        /// <param name="recorder">Media recorder to receive audio frames.</param>
        /// <param name="audioSource">Audio source to record.</param>
        /// <param name="mute">Optional. Mute audio source while recording so that it is not heard in scene.</param>
        public AudioInput (IMediaRecorder recorder, AudioSource audioSource, bool mute = false) : this(recorder, default, audioSource, mute) {}

        /// <summary>
        /// Create an audio recording input from an AudioSource.
        /// </summary>
        /// <param name="recorder">Media recorder to receive audio frames.</param>
        /// <param name="clock">Clock for generating timestamps. Can be `null` if recorder does not require timestamps.</param>
        /// <param name="audioSource">Audio source to record.</param>
        /// <param name="mute">Optional. Mute audio source while recording so that it is not heard in scene.</param>
        public AudioInput (IMediaRecorder recorder, IClock clock, AudioSource audioSource, bool mute = false) : this(recorder, clock, audioSource.gameObject, mute) {}

        /// <summary>
        /// Stop recorder input and release resources.
        /// </summary>
        public void Dispose () => AudioInputAttachment.Destroy(attachment);
        #endregion


        #region --Operations--

        private readonly IMediaRecorder recorder;
        private readonly IClock clock;
        private readonly AudioInputAttachment attachment;
        private readonly bool mute;

        private AudioInput (IMediaRecorder recorder, IClock clock, GameObject gameObject, bool mute = false) {
            this.recorder = recorder;
            this.clock = clock;
            this.attachment = gameObject.AddComponent<AudioInputAttachment>();
            this.attachment.sampleBufferDelegate = OnSampleBuffer;
            this.mute = mute;
        }

        private void OnSampleBuffer (float[] data) {
            AndroidJNI.AttachCurrentThread();
            recorder.CommitSamples(data, clock?.timestamp ?? 0L);
            if (mute)
                Array.Clear(data, 0, data.Length);
        }

        private class AudioInputAttachment : MonoBehaviour {
            public Action<float[]> sampleBufferDelegate;
            private void OnAudioFilterRead (float[] data, int channels) => sampleBufferDelegate?.Invoke(data);
        }
        #endregion
    }
}