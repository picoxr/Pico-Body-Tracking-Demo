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

    class AndroidStringAndObjectCallback : AndroidJavaProxy {

        public AndroidStringAndObjectCallback(Action<string, AndroidJavaObject> callback) : base("com.vuplex.webview.StringAndObjectCallback") {
            _callback = callback;
        }

        public void callback(string param1, AndroidJavaObject param2) {

            _callback(param1, param2);
            // Don't set _callback = null because this callback is called multiple times.
        }


        Action<string, AndroidJavaObject> _callback;
    }
}
#endif
