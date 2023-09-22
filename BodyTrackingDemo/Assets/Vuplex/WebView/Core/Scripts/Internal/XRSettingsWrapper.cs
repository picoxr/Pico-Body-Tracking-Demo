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
using UnityEngine;
#if UNITY_2017_2_OR_NEWER
    using UnityEngine.XR;
#else
    using XRSettings = UnityEngine.VR.VRSettings;
#endif

namespace Vuplex.WebView.Internal {

    /// <summary>
    /// Utility class to help manage the name change from VRSettings
    /// to XRSettings that occurred in Unity 2017.2.
    /// </summary>
    public class XRSettingsWrapper {

        public static XRSettingsWrapper Instance {
            get {
                if (_instance == null) {
                    _instance = new XRSettingsWrapper();
                }
                return _instance;
            }
        }

        public bool enabled { get { return XRSettings.enabled; }}

        public RenderTextureDescriptor eyeTextureDesc { get { return XRSettings.eyeTextureDesc; }}

        public string loadedDeviceName { get { return XRSettings.loadedDeviceName; }}

        public string[] supportedDevices { get { return XRSettings.supportedDevices; }}

        static XRSettingsWrapper _instance;
    }
}
