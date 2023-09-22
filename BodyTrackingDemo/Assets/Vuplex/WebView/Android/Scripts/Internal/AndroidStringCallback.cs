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
using UnityEngine;

namespace Vuplex.WebView.Internal {

    class AndroidStringCallback : AndroidJavaProxy {

        public AndroidStringCallback(Action<string> callback) : base("com.vuplex.webview.StringCallback") {
            _callback = callback;
        }

        public void callback(string result) {

            _callback(result);
            _callback = null;
        }

        Action<string> _callback;
    }
}
#endif
