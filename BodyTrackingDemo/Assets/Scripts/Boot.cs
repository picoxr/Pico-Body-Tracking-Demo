using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.XR;

namespace PicoPorting.Runtime
{
    public static class Boot
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)] 
        public static void InitializeBeforeSplashScreen()
        {
            XRSettings.eyeTextureResolutionScale = 2f;
            Debug.Log($"Boot.InitializeBeforeSplashScreen: eyeTextureResolutionScale = {XRSettings.eyeTextureResolutionScale}, DeviceName = {XRSettings.loadedDeviceName}");
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] 
        public static void InitializeBeforeSceneLoad()
        {
            float resolutionScale = 1;
            var curDeviceType = PXR_Input.GetControllerDeviceType();
            switch (curDeviceType)
            {
                case PXR_Input.ControllerDevice.Neo3:
                    resolutionScale = 1.21f;
                    break;
                case PXR_Input.ControllerDevice.PICO_4:
                    resolutionScale = 1.436f;
                    break;
                case PXR_Input.ControllerDevice.NewController:
                    resolutionScale = 1.44f;
                    break;
            }

            XRSettings.eyeTextureResolutionScale = resolutionScale;
            Debug.Log($"Boot.InitializeBeforeSceneLoad: eyeTextureResolutionScale = {XRSettings.eyeTextureResolutionScale}");
        }
    }
}