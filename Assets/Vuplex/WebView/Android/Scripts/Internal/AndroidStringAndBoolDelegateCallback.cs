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

    class AndroidStringAndBoolDelegateCallback : AndroidJavaProxy {

        public AndroidStringAndBoolDelegateCallback(Action<string, Action<bool>> callback) : base("com.vuplex.webview.StringAndBooleanDelegateCallback") {
            _callback = callback;
        }

        public void callback(string stringParam, AndroidJavaObject delegateParam) {

            _callback(stringParam, boolParam => delegateParam.Call("callback", boolParam));
            // Don't set _callback = null because this callback is called multiple times.
        }

        Action<string, Action<bool>> _callback;
    }
}
#endif
