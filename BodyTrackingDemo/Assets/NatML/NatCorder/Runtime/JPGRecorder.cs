/* 
*   NatCorder
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatSuite.Recorders {

    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Experimental.Rendering;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Internal;
    
    /// <summary>
    /// JPG image sequence recorder.
    /// </summary>
    public sealed class JPGRecorder : IMediaRecorder {

        #region --Client API--
        /// <summary>
        /// Image size.
        /// </summary>
        public (int width, int height) frameSize { get; private set; }

        /// <summary>
        /// Create a JPG recorder.
        /// </summary>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="quality">Encoding quality in range [1, 100].</param>
        public JPGRecorder (int width, int height, int quality = 80) {
            // Save state
            this.frameSize = (width, height);
            this.quality = Mathf.Clamp(quality, 1, 100);
            this.writeTasks = new List<Task>();
            // Create directory
            this.recordingPath = Utility.GetPath(string.Empty);
            Directory.CreateDirectory(recordingPath);
        }

        /// <summary>
        /// Commit a video pixel buffer for encoding.
        /// The pixel buffer MUST have an RGBA8888 pixel layout.
        /// </summary>
        /// <param name="pixelBuffer">Pixel buffer containing video frame to commit.</param>
        /// <param name="timestamp">Not used.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public unsafe void CommitFrame<T> (T[] pixelBuffer, long timestamp = default) where T : unmanaged {
            fixed (T* baseAddress = pixelBuffer)
                CommitFrame(baseAddress, timestamp);
        }

        /// <summary>
        /// Commit a video pixel buffer for encoding.
        /// The pixel buffer MUST have an RGBA8888 pixel layout.
        /// </summary>
        /// <param name="nativeBuffer">Pixel buffer in native memory to commit.</param>
        /// <param name="timestamp">Not used.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public unsafe void CommitFrame (void* nativeBuffer, long timestamp = default) {
            // Encode immediately
            var nativeArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(
                nativeBuffer,
                frameSize.width * frameSize.height * 4,
                Allocator.None
            );
            var buffer = ImageConversion.EncodeNativeArrayToJPG(
                nativeArray,
                GraphicsFormat.R8G8B8A8_UNorm,
                (uint)frameSize.width,
                (uint)frameSize.height,
                quality: quality
            ).ToArray();
            // Write on worker thread
            var frameIndex = ++frameCount;
            var task = Task.Run(() => File.WriteAllBytes(Path.Combine(recordingPath, $"{frameIndex}.jpg"), buffer));
            writeTasks.Add(task);
        }

        /// <summary>
        /// Finish writing.
        /// </summary>
        /// <returns>Path to directory containing image sequence.</returns>
        public async Task<string> FinishWriting () {
            await Task.WhenAll(writeTasks);
            return recordingPath;
        }
        #endregion


        #region --Operations--
        private readonly int quality;
        private readonly string recordingPath;
        private readonly List<Task> writeTasks;
        private int frameCount;

        unsafe void IMediaRecorder.CommitSamples (float[] sampleBuffer, long timestamp) {
            fixed (float* baseAddress = sampleBuffer)
                (this as IMediaRecorder).CommitSamples(baseAddress, sampleBuffer.Length, timestamp);
        }

        unsafe void IMediaRecorder.CommitSamples (float* sampleBuffer, int sampleCount, long timestamp) {
            Debug.LogError("NatCorder Error: JPGRecorder does not support committing audio samples");
        }
        #endregion
    }
}