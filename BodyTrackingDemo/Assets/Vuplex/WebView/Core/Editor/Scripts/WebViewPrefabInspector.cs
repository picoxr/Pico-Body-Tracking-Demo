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
using UnityEditor;

namespace Vuplex.WebView.Editor {

    [CustomEditor(typeof(WebViewPrefab))]
    public class WebViewPrefabInspector : BaseWebViewPrefabInspector {

        protected override string _getDocumentationLink() {
            return "https://developer.vuplex.com/webview/WebViewPrefab";
        }
    }
}
