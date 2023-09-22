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
using System.Linq;
using UnityEngine;
using Vuplex.WebView.Internal;

namespace Vuplex.WebView {

    public class WebPluginFactory {

        public virtual IWebPlugin GetPlugin() => GetPlugin(null);

        public virtual IWebPlugin GetPlugin(WebPluginType[] preferredPlugins) {

            #if UNITY_SERVER
                _logMockWarningOnce("3D WebView for Windows doesn't support the \"Server Build\" option because it uses a null graphics device (GraphicsDeviceType.Null)");
                return _mockPlugin;
            #elif UNITY_EDITOR
                #if UNITY_EDITOR_WIN
                    if (_windowsPlugin != null) {
                        return _windowsPlugin;
                    }
                    _logMockWarningOnce("The 3D WebView for Windows plugin is not currently installed");
                    return _mockPlugin;
                #elif UNITY_EDITOR_OSX
                    if (_macPlugin != null) {
                        return _macPlugin;
                    }
                    _logMockWarningOnce("The 3D WebView for macOS plugin is not currently installed");
                    return _mockPlugin;
                #else
                    _logMockWarningOnce("There is not currently a 3D WebView plugin for the current editor platform");
                    return _mockPlugin;
                #endif
            #elif UNITY_STANDALONE_WIN
                if (_windowsPlugin != null) {
                    return _windowsPlugin;
                }
                throw new WebViewUnavailableException("The 3D WebView for Windows plugin is not currently installed." + MORE_INFO_TEXT);
            #elif UNITY_STANDALONE_OSX
                if (_macPlugin != null) {
                    return _macPlugin;
                }
                throw new WebViewUnavailableException("The 3D WebView for macOS plugin is not currently installed." + MORE_INFO_TEXT);
            #elif UNITY_IOS
                if (_iosPlugin != null) {
                    return _iosPlugin;
                }
                throw new WebViewUnavailableException("The 3D WebView for iOS plugin is not currently installed." + MORE_INFO_TEXT);
            #elif UNITY_ANDROID
                var preferChromiumAndroidPlugin = preferredPlugins != null && preferredPlugins.Contains(WebPluginType.Android);
                if (_androidPlugin != null && (_androidGeckoPlugin == null || preferChromiumAndroidPlugin)) {
                    return _androidPlugin;
                }
                if (_androidGeckoPlugin != null) {
                    return _androidGeckoPlugin;
                }
                throw new WebViewUnavailableException("The 3D WebView for Android plugin is not currently installed." + MORE_INFO_TEXT);
            #elif UNITY_WSA
                if (_uwpPlugin != null) {
                    return _uwpPlugin;
                }
                throw new WebViewUnavailableException("The 3D WebView for UWP plugin is not currently installed." + MORE_INFO_TEXT);
            #elif UNITY_WEBGL
                if (_webGLPlugin != null) {
                    return _webGLPlugin;
                }
                throw new WebViewUnavailableException("The 2D WebView for WebGL plugin is not currently installed." + MORE_INFO_TEXT);
            #else
                throw new WebViewUnavailableException("This version of 3D WebView does not support the current build platform." + MORE_INFO_TEXT);
            #endif
        }

        public static void RegisterAndroidPlugin(IWebPlugin plugin) {

            _androidPlugin = plugin;
        }

        public static void RegisterAndroidGeckoPlugin(IWebPlugin plugin) {

            _androidGeckoPlugin = plugin;
        }

        public static void RegisterIOSPlugin(IWebPlugin plugin) {

            _iosPlugin = plugin;
        }

        public static void RegisterMacPlugin(IWebPlugin plugin) {

            _macPlugin = plugin;
        }

        public static void RegisterMockPlugin(IWebPlugin plugin) {

            _mockPlugin = plugin;
        }

        public static void RegisterUwpPlugin(IWebPlugin plugin) {

            _uwpPlugin = plugin;
        }

        public static void RegisterWebGLPlugin(IWebPlugin plugin) {

            _webGLPlugin = plugin;
        }

        public static void RegisterWindowsPlugin(IWebPlugin plugin) {

            _windowsPlugin = plugin;
        }

        protected static IWebPlugin _androidPlugin;
        protected static IWebPlugin _androidGeckoPlugin;
        protected static IWebPlugin _iosPlugin;
        protected static IWebPlugin _macPlugin;
        protected static IWebPlugin _mockPlugin = MockWebPlugin.Instance;
        bool _mockWarningLogged;
        const string MORE_INFO_TEXT = " For more info, please visit https://developer.vuplex.com.";
        protected static IWebPlugin _uwpPlugin;
        protected static IWebPlugin _webGLPlugin;
        protected static IWebPlugin _windowsPlugin;

        /// <summary>
        /// Logs the warning once so that it doesn't spam the console.
        /// </summary>
        void _logMockWarningOnce(string reason) {

            if (!_mockWarningLogged) {
                _mockWarningLogged = true;
                WebViewLogger.LogWarning($"{reason}, so the mock webview will be used{(Application.isEditor ? " while running in the editor" : "")}. For more info, please see <em>https://support.vuplex.com/articles/mock-webview</em>.");
            }
        }
    }
}
