# NatCorder
NatCorder is a lightweight, easy-to-use, native video recording API for iOS, Android, macOS, and Windows. NatCorder comes with a rich featureset including:
+ Record any texture, anything that can be rendered into a texture, or any pixel data.
+ Record to MP4 videos and animated GIF images.
+ Control recording quality and file size with bitrate and keyframe interval.
+ Record at any resolution. You can specify what resolution recording you want.
+ Get the path to recorded video in device storage.
+ Record game audio with video.
+ Support for recording HEVC videos.

## Fundamentals of Recording
NatCorder provides a simple recording API with instances of the `IMediaRecorder` interface. **NatCorder works by encoding video and audio frames on demand**. To start recording, simply create a recorder corresponding to the media type you want to record:
```csharp
var gifRecorder = new GIFRecorder(...);
var mp4Recorder = new MP4Recorder(...);
var hevcRecorder = new HEVCRecorder(...);
var wavRecorder = new WAVRecorder(...);
var jpegRecorder = new JPGRecorder(...);
```

Once you create a recorder, you then commit frames to it. You can commit video and audio frames to these recorders. These committed frames are then encoded into a media file. When committing frames, you must provide the frame data with a corresponding timestamp. The spacing between timestamps determine the final frame rate of the recording.

### Committing Video Frames
NatCorder records video from raw pixel buffers. The pixel buffers must be 32-bit per pixel, RGBA encoding (`TextureFormat.RGBA32`). The managed type of the pixel buffer is entirely flexible. As a result, you can commit a `Color32[]`, a `byte[]`, an `int[]`, or any struct array that can be interpreted as an `RGBA32` pixel buffer.

When committing a pixel buffer for encoding, you will typically need to provide a corresponding timestamp. For this purpose, you can use implementations of the `IClock` interface. Here is an example illustrating recording a `WebCamTexture`:
```csharp
async void Start () {
    // Start camera
    var cameraTexture = new WebCamTexture();
    cameraTexture.Play();
    // Create a clock for generating recording timestamps
    var clock = new RealtimeClock();
    // Create a recorder
    var recorder = new MP4Recorder(...);
    // Record 150 frames
    for (int i = 0; i < 150; i++) {
        // Commit the frame to NatCorder for encoding
        recorder.CommitFrame(cameraTexture.GetPixels32(), clock.timestamp);
        // Wait for some milliseconds
        await Task.Delay(10);
    }
    // Finish writing
    var recordingPath = await recorder.FinishWriting();
}
```

### Committing Audio Frames
NatCorder records audio provided as floating-point linear PCM sample buffers, interleaved by channel (`float[]`). Similar to recording video frames, you will call the `IMediaRecorder.CommitSamples` method, passing in a sample buffer and a corresponding timestamp. It is important that the timestamps synchronize with those of video, so it is recommended to use the same `IClock` for generating video and audio timestamps. Below is an example illustrating recording game audio using Unity's `OnAudioFilterRead` callback:
```csharp
void OnAudioFilterRead (float[] sampleBuffer, int channels) {
    // Commit the audio frame
    recorder.CommitSamples(sampleBuffer, clock.timestamp);
}
```

## Easier Recording with Recorder Inputs
In most cases, you will likely just want to record a game camera optionally with game audio. To do so, you can use NatCorder's *recorder inputs*. A recorder input is a lightweight utility class that eases out the process of recording some aspect of a Unity application. NatCorder comes with two recorder inputs: `CameraInput` and `AudioInput`. You can create your own recorder inputs to do more interesting things like add a watermark to the video, or retime the video. Here is a simple example showing recording a game camera:
```csharp
IClock clock;
IMediaRecorder recorder;
CameraInput cameraInput;
AudioInput audioInput;

void StartRecording () {
    // Create a recording clock
    clock = new RealtimeClock();
    // Start recording
    mediaRecorder = new ...;
    // Create a camera input to record the main camera
    cameraInput = new CameraInput(recorder, clock, Camera.main);
    // Create an audio input to record the scene's AudioListener
    audioInput = new AudioInput(recorder, clock, audioListener);
}

async void StopRecording () {
    // Destroy the recording inputs
    cameraInput.Dispose();
    audioInput.Dispose();
    // Stop recording
    var recordingPath = await recorder.FinishWriting();
}
```

___

## Requirements
- Unity 2020.3+
- Android API Level 24+
- iOS 13+
- macOS 10.15+
- Windows 10+, 64-bit only

## Resources
- [NatCorder Documentation](https://docs.natml.ai/natcorder/)
- [Video Recording Made Easy](https://blog.natml.ai/natcorder-unity-recording-made-easy-f0fdee0b5055)
- [NatCorder on Unity Forums](https://forum.unity.com/threads/natcorder-video-recording-api.505146/)
- [NatML on GitHub](https://github.com/natmlx)
- [Email Support](mailto:hi@natml.ai)

Thank you very much!