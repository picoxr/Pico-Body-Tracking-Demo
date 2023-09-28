/* 
*   NatCorder
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatSuite.Recorders.Internal {

    using System;
    using System.Runtime.InteropServices;

    public static class NatCorder { // NatCorder.h

        private const string Assembly =
        #if UNITY_IOS && !UNITY_EDITOR
        @"__Internal";
        #else
        @"NatCorder";
        #endif

        public delegate void RecordingHandler (IntPtr context, IntPtr path);


        #region --IMediaRecorder--
        [DllImport(Assembly, EntryPoint = @"NCMediaRecorderFrameSize")]
        public static extern void FrameSize (
            this IntPtr recorder,
            out int width,
            out int height
        );

        [DllImport(Assembly, EntryPoint = @"NCMediaRecorderCommitFrame")]
        public static extern unsafe void CommitFrame (
            this IntPtr recorder,
            void* pixelBuffer,
            long timestamp
        );

        [DllImport(Assembly, EntryPoint = @"NCMediaRecorderCommitSamples")]
        public static extern unsafe void CommitSamples (
            this IntPtr recorder,
            float* sampleBuffer,
            int sampleCount,
            long timestamp
        );

        [DllImport(Assembly, EntryPoint = @"NCMediaRecorderFinishWriting")]
        public static extern void FinishWriting (
            this IntPtr recorder,
            RecordingHandler handler,
            IntPtr context
        );
        #endregion


        #region --Constructors--
        [DllImport(Assembly, EntryPoint = @"NCCreateMP4Recorder")]
        public static extern void CreateMP4Recorder (
            [MarshalAs(UnmanagedType.LPStr)] string path,
            int width,
            int height,
            float frameRate,
            int sampleRate,
            int channelCount,
            int videoBitrate,
            int keyframeInterval,
            int audioBitRate,
            out IntPtr recorder
        );
        
        [DllImport(Assembly, EntryPoint = @"NCCreateHEVCRecorder")]
        public static extern void CreateHEVCRecorder (
            [MarshalAs(UnmanagedType.LPStr)] string path,
            int width,
            int height,
            float frameRate,
            int sampleRate,
            int channelCount,
            int videoBitRate,
            int keyframeInterval,
            int audioBitRate,
            out IntPtr recorder
        );
        
        [DllImport(Assembly, EntryPoint = @"NCCreateGIFRecorder")]
        public static extern void CreateGIFRecorder (
            [MarshalAs(UnmanagedType.LPStr)] string path,
            int width,
            int height,
            float frameDuration,
            out IntPtr recorder
        );
        #endregion
    }
}