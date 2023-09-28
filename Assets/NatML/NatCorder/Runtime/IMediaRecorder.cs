/* 
*   NatCorder
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatSuite.Recorders {

    using System.Threading.Tasks;

    /// <summary>
    /// A recorder capable of recording video frames, and optionally audio frames, to a media output.
    /// All recorder methods are thread safe, and as such can be called from any thread.
    /// </summary>
    public interface IMediaRecorder {

        /// <summary>
        /// Recording frame size.
        /// </summary>
        (int width, int height) frameSize { get; }

        /// <summary>
        /// Commit a video pixel buffer for encoding.
        /// The pixel buffer MUST have an RGBA8888 pixel layout.
        /// </summary>
        /// <param name="pixelBuffer">Pixel buffer to commit.</param>
        /// <param name="timestamp">Pixel buffer timestamp in nanoseconds.</param>
        void CommitFrame<T> (T[] pixelBuffer, long timestamp) where T : unmanaged;

        /// <summary>
        /// Commit a video pixel buffer for encoding.
        /// The pixel buffer MUST have an RGBA8888 pixel layout.
        /// </summary>
        /// <param name="nativeBuffer">Pixel buffer in native memory to commit.</param>
        /// <param name="timestamp">Pixel buffer timestamp in nanoseconds.</param>
        unsafe void CommitFrame (void* nativeBuffer, long timestamp);
        
        /// <summary>
        /// Commit an audio sample buffer for encoding.
        /// The sample buffer MUST be a linear PCM buffer interleaved by channel.
        /// </summary>
        /// <param name="sampleBuffer">Sample buffer to commit.</param>
        /// <param name="timestamp">Sample buffer timestamp in nanoseconds.</param>
        void CommitSamples (float[] sampleBuffer, long timestamp);

        /// <summary>
        /// Commit an audio sample buffer for encoding.
        /// The sample buffer MUST be a linear PCM buffer interleaved by channel.
        /// </summary>
        /// <param name="nativeBuffer">Sample buffer in native memory to commit.</param>
        /// <param name="sampleCount">Total number of samples in the buffer.</param>
        /// <param name="timestamp">Sample buffer timestamp in nanoseconds.</param>
        unsafe void CommitSamples (float* nativeBuffer, int sampleCount, long timestamp);

        /// <summary>
        /// Finish writing.
        /// </summary>
        /// <returns>Path to recorded media file.</returns>
        Task<string> FinishWriting ();
    }
}