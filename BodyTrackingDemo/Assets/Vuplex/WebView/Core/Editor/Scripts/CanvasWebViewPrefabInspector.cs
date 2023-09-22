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

    [CustomEditor(typeof(CanvasWebViewPrefab))]
    public class CanvasWebViewPrefabInspector : BaseWebViewPrefabInspector {

        public override void OnEnable() {

            base.OnEnable();
            serializedObject.Update();
            _native2DModeEnabled = serializedObject.FindProperty("Native2DModeEnabled");
        }

        SerializedProperty _native2DModeEnabled;

        protected override string _getDocumentationLink() {
            return "https://developer.vuplex.com/webview/CanvasWebViewPrefab";
        }

        protected override void _renderCustomOtherSettings() {

            EditorGUILayout.HelpBox("These settings are used with the default rendering mode but are ignored when running in native 2D mode.", MessageType.Info);
        }

        protected override void _renderCustomPlatformSpecificSettings() {

            EditorGUILayout.PropertyField(_native2DModeEnabled);
        }
    }
}
