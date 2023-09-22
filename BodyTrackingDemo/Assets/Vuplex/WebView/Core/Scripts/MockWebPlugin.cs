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
using System;
using System.Threading.Tasks;
using UnityEngine;
using Vuplex.WebView.Internal;

namespace Vuplex.WebView {

    /// <summary>
    /// Mock IWebPlugin implementation used for running in the Unity editor.
    /// </summary>
    class MockWebPlugin : IWebPlugin {

        public ICookieManager CookieManager { get; } = MockCookieManager.Instance;

        public static MockWebPlugin Instance {
            get {
                if (_instance == null) {
                    _instance = new MockWebPlugin();
                }
                return _instance;
            }
        }

        public WebPluginType Type { get; } = WebPluginType.Mock;

        public void ClearAllData() {}

        // Deprecated
        public void CreateMaterial(Action<Material> callback) {

            var material = new Material(Resources.Load<Material>("MockViewportMaterial"));
            // Create a copy of the texture so that an Exception won't be thrown when the prefab destroys it.
            // Also, explicitly use RGBA32 here so that the texture will be converted to RGBA32 if the editor
            // imported it as a different format. For example, when Texture Compression is set to ASTC in Android build settings,
            // the editor automatically imports new textures as ASTC, even though the Windows editor doesn't support that format.
            var texture = new Texture2D(material.mainTexture.width, material.mainTexture.height, TextureFormat.RGBA32, true);
            texture.SetPixels((material.mainTexture as Texture2D).GetPixels());
            texture.Apply();

            material.mainTexture = texture;
            Dispatcher.RunOnMainThread(() => callback(material));
        }

        public virtual IWebView CreateWebView() => MockWebView.Instantiate();

        public void EnableRemoteDebugging() {}

        public void SetAutoplayEnabled(bool enabled) {}

        public void SetCameraAndMicrophoneEnabled(bool enabled) {}

        public void SetIgnoreCertificateErrors(bool ignore) {}

        public void SetStorageEnabled(bool enabled) {}

        public void SetUserAgent(bool mobile) {}

        public void SetUserAgent(string userAgent) {}

        static MockWebPlugin _instance;
    }
}
