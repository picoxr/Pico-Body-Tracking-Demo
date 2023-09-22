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

    public abstract class BaseWebViewPrefabInspector : UnityEditor.Editor {

        public virtual void OnEnable() {

            _clickingEnabled = serializedObject.FindProperty("ClickingEnabled");
            _cursorIconsEnabled = serializedObject.FindProperty("CursorIconsEnabled");
            _dragMode = serializedObject.FindProperty("DragMode");
            _dragThreshold = serializedObject.FindProperty("DragThreshold");
            _hoveringEnabled = serializedObject.FindProperty("HoveringEnabled");
            _logConsoleMessages = serializedObject.FindProperty("LogConsoleMessages");
            _resolution = serializedObject.FindProperty("Resolution");
            _initialUrl = serializedObject.FindProperty("InitialUrl");
            _nativeOnScreenKeyboardEnabled = serializedObject.FindProperty("NativeOnScreenKeyboardEnabled");
            _pixelDensity = serializedObject.FindProperty("PixelDensity");
            _remoteDebuggingEnabled = serializedObject.FindProperty("RemoteDebuggingEnabled");
            _scrollingEnabled = serializedObject.FindProperty("ScrollingEnabled");
            _scrollingSensitivity = serializedObject.FindProperty("ScrollingSensitivity");
        }

        public override void OnInspectorGUI() {

            DocumentationLinkDrawer.DrawDocumentationLink(_getDocumentationLink());
            EditorGUILayout.Space();
            serializedObject.Update();
            EditorGUILayout.PropertyField(_initialUrl);
            EditorGUILayout.PropertyField(_resolution);
            EditorGUILayout.PropertyField(_dragMode);

            EditorGUILayout.Space();
            _renderCustomPlatformSpecificSettings();
            EditorGUILayout.PropertyField(_nativeOnScreenKeyboardEnabled);
            EditorGUILayout.PropertyField(_pixelDensity);
            EditorGUILayout.PropertyField(_cursorIconsEnabled);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_remoteDebuggingEnabled);
            EditorGUILayout.PropertyField(_logConsoleMessages);
            EditorUtils.DrawLink("Troubleshooting help", "https://developer.vuplex.com/webview/troubleshooting", 24);
            EditorUtils.DrawLink("Remote debugging help", "https://support.vuplex.com/articles/how-to-debug-web-content", 27);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            #if UNITY_2019_1_OR_NEWER
                // BeginFoldoutHeaderGroup and EndFoldoutHeaderGroup only exist in Unity 2019.1 and newer.
                _showOtherSettings = EditorGUILayout.BeginFoldoutHeaderGroup(_showOtherSettings, "Other settings");
            #else
                // Fallback to creating a label for the section.
                _showOtherSettings = true;
                var style = new UnityEngine.GUIStyle { fontStyle = UnityEngine.FontStyle.Bold };
                style.normal.textColor = EditorGUIUtility.isProSkin ? new UnityEngine.Color(0.77f, 0.77f, 0.77f) : UnityEngine.Color.black;
                EditorGUILayout.LabelField("Other settings", style);
            #endif
            if (_showOtherSettings) {
                _renderCustomOtherSettings();
                EditorGUILayout.PropertyField(_clickingEnabled);
                EditorGUILayout.PropertyField(_hoveringEnabled);
                EditorGUILayout.PropertyField(_scrollingEnabled);
                EditorGUILayout.PropertyField(_scrollingSensitivity);
                EditorGUILayout.PropertyField(_dragThreshold);
            }
            #if UNITY_2019_1_OR_NEWER
                EditorGUILayout.EndFoldoutHeaderGroup();
            #endif
            EditorGUILayout.Space();
            serializedObject.ApplyModifiedProperties();
        }

        SerializedProperty _clickingEnabled;
        SerializedProperty _cursorIconsEnabled;
        SerializedProperty _dragMode;
        SerializedProperty _dragThreshold;
        SerializedProperty _hoveringEnabled;
        SerializedProperty _logConsoleMessages;
        SerializedProperty _resolution;
        SerializedProperty _initialUrl;
        SerializedProperty _nativeOnScreenKeyboardEnabled;
        SerializedProperty _pixelDensity;
        SerializedProperty _remoteDebuggingEnabled;
        SerializedProperty _scrollingEnabled;
        SerializedProperty _scrollingSensitivity;
        bool _showOtherSettings;

        protected abstract string _getDocumentationLink();

        protected virtual void _renderCustomOtherSettings() {}

        protected virtual void _renderCustomPlatformSpecificSettings() {}
    }
}
