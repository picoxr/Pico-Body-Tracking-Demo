// Copyright (c) 2022 Vuplex Inc. All rights reserved.
//
// Licensed under the Vuplex Commercial Software Library License, you may
// not use this file except in compliance with the License. You may obtain
// a copy of the License at
//
//     https://vuplex.com/commercial-library-license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

namespace Vuplex.WebView.Internal {

    /// <summary>
    /// Static utility methods used internally by 3D WebView on Android.
    /// </summary>
    public static class VXAndroidUtils {

        public static Material CreateAndroidMaterial() {

            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Vulkan) {
                return VXUtils.CreateDefaultMaterial();
            }
            var singlePassStereoRenderingEnabled = false;
            // For some headsets like HTC Vive Focus, XRSettings.eyeTextureDesc.vrUsage returns a value
            // other than VRTextureUsage.TwoEyes, so the VUPLEX_FORCE_SINGLE_PASS scripting symbol can be
            // used to force single pass in that scenario.
            #if VUPLEX_FORCE_SINGLE_PASS
                singlePassStereoRenderingEnabled = VXUtils.XRSettings.enabled;
            #elif UNITY_2017_2_OR_NEWER
                singlePassStereoRenderingEnabled = VXUtils.XRSettings.enabled && VXUtils.XRSettings.eyeTextureDesc.vrUsage == VRTextureUsage.TwoEyes;
            #endif
            var materialName = singlePassStereoRenderingEnabled ? "AndroidSinglePassViewportMaterial"
                                                                : "AndroidViewportMaterial";
            // Construct a new material because Resources.Load<T>() returns a singleton.
            return new Material(Resources.Load<Material>(materialName));
        }

        public static byte[] ConvertFromJavaByteArray(AndroidJavaObject arrayObject) {

            // Unity 2019.1 and newer logs a warning that converting from byte[] is obsolete
            // but older versions are incapable of converting from sbyte[].
            #if UNITY_2019_1_OR_NEWER
                return (byte[])(Array)AndroidJNIHelper.ConvertFromJNIArray<sbyte[]>(arrayObject.GetRawObject());
            #else
                return AndroidJNIHelper.ConvertFromJNIArray<byte[]>(arrayObject.GetRawObject());
            #endif
        }

        public static void ThrowVulkanExtensionException() {

            throw new InvalidOperationException("The Vulkan Graphics API is in use, but this device does not support the VK_ANDROID_external_memory_android_hardware_buffer Vulkan API required by 3D WebView. Please switch to the OpenGLES Graphics API in Player Settings. For more info, see this page: https://support.vuplex.com/articles/vulkan");
        }

        public static AndroidJavaObject ToJavaMap(Dictionary<string, string> dictionary) {

            AndroidJavaObject map = new AndroidJavaObject("java.util.HashMap");
            IntPtr putMethod = AndroidJNIHelper.GetMethodID(map.GetRawClass(), "put", "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;");
            foreach (var entry in dictionary) {
                AndroidJNI.CallObjectMethod(
                    map.GetRawObject(),
                    putMethod,
                    AndroidJNIHelper.CreateJNIArgArray(new object[] { entry.Key, entry.Value })
                );
            }
            return map;
        }

        public static AndroidJavaObject ToJavaObject(IntPtr jobject) {

            if (jobject == IntPtr.Zero) {
                return null;
            }
            return (AndroidJavaObject)_androidJavaObjectIntPtrConstructor.Invoke(new object[] { jobject });
        }

        // Get a reference to AndroidJavaObject's hidden constructor that takes
        // the IntPtr for a jobject as a parameter.
        readonly static ConstructorInfo _androidJavaObjectIntPtrConstructor = typeof(AndroidJavaObject).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            new []{ typeof(IntPtr) },
            null
        );
    }
}
#endif
