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

namespace Vuplex.WebView.Internal {

    public interface IWebPlugin {

        ICookieManager CookieManager { get; }

        WebPluginType Type { get; }

        void ClearAllData();

        // Deprecated
        void CreateMaterial(Action<Material> callback);

        IWebView CreateWebView();

        void EnableRemoteDebugging();

        void SetAutoplayEnabled(bool enabled);

        void SetCameraAndMicrophoneEnabled(bool enabled);

        void SetIgnoreCertificateErrors(bool ignore);

        void SetStorageEnabled(bool enabled);

        void SetUserAgent(bool mobile);

        void SetUserAgent(string userAgent);
    }
}
