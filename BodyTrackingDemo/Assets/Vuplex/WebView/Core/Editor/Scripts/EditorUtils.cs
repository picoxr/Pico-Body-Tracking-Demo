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
#pragma warning disable CS0618
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine.Rendering;
using Vuplex.WebView.Internal;

namespace Vuplex.WebView.Editor {

    public static class EditorUtils {

        public static void AssertThatOculusLowOverheadModeIsDisabled() {

            if (!EditorUtils.XRSdkIsEnabled("oculus")) {
                return;
            }
            var lowOverheadModeEnabled = false;
            #if VUPLEX_OCULUS
                // The Oculus XR plugin is installed
                Unity.XR.Oculus.OculusLoader oculusLoader = Unity.XR.Oculus.OculusSettings.CreateInstance<Unity.XR.Oculus.OculusLoader>();
                Unity.XR.Oculus.OculusSettings oculusSettings = oculusLoader.GetSettings();
                lowOverheadModeEnabled = oculusSettings.LowOverheadMode;
            #elif UNITY_2019_2_OR_NEWER && !UNITY_2020_1_OR_NEWER
                // VROculus.lowOverheadMode is only supported from Unity 2019.2 - 2019.4
                lowOverheadModeEnabled = PlayerSettings.VROculus.lowOverheadMode;
            #endif
            if (lowOverheadModeEnabled) {
                throw new BuildFailedException("XR settings error: Vuplex 3D WebView requires that \"Low Overhead Mode\" be disabled in Oculus XR settings. Please disable Low Overhead Mode in Oculus XR settings.");
            }
        }

        public static void AssertThatSrpBatcherIsDisabled() {

            #if UNITY_2018_2_OR_NEWER && !VUPLEX_DISABLE_SRP_WARNING
                if (UnityEngine.Rendering.GraphicsSettings.useScriptableRenderPipelineBatching) {
                    throw new BuildFailedException("URP settings error: \"SRP Batcher\" is enabled in Universal Render Pipeline (URP) settings, but URP for Android has an issue that prevents 3D WebView's textures from showing up outside of a Canvas. If the project uses a WebViewPrefab, please go to \"UniversalRenderPipelineAsset\" -> \"Advanced\" and disable SRP Batcher. If the project only uses CanvasWebViewPrefab and not WebViewPrefab, you can instead add the scripting symbol VUPLEX_DISABLE_SRP_WARNING to the project to ignore this warning.");
                }
            #endif
        }

        public static void CopyAndReplaceDirectory(string srcPath, string dstPath, bool ignoreMetaFiles = true) {

            if (Directory.Exists(dstPath)) {
                Directory.Delete(dstPath, true);
            }
            if (File.Exists(dstPath)) {
                File.Delete(dstPath);
            }
            Directory.CreateDirectory(dstPath);

            foreach (var file in Directory.GetFiles(srcPath)) {
                if (!ignoreMetaFiles || Path.GetExtension(file) != ".meta") {
                    File.Copy(file, Path.Combine(dstPath, Path.GetFileName(file)));
                }
            }
            foreach (var dir in Directory.GetDirectories(srcPath)) {
                CopyAndReplaceDirectory(dir, Path.Combine(dstPath, Path.GetFileName(dir)), ignoreMetaFiles);
            }
        }

        public static void DrawLink(string linkText, string url, int underlineLength) {

            var linkStyle = new GUIStyle {
                richText = true,
                padding = new RectOffset {
                    top = 2,
                    bottom = 2
                }
            };
            var linkClicked = GUILayout.Button(
                EditorUtils.TextWithColor(linkText, EditorUtils.GetLinkColor()),
                linkStyle
            );
            var linkRect = GUILayoutUtility.GetLastRect();
            EditorGUIUtility.AddCursorRect(linkRect, MouseCursor.Link);

            // Unity's editor GUI doesn't support underlines, so fake it.
            var underscores = new string[underlineLength];
            for (var i = 0; i < underlineLength; i++) {
                underscores[i] = "_";
            }
            var underline = String.Join("", underscores);

            GUI.Label(
                linkRect,
                EditorUtils.TextWithColor(underline, EditorUtils.GetLinkColor()),
                new GUIStyle {
                    richText = true,
                    padding = new RectOffset {
                        top = 4,
                        bottom = 2
                }
            });
            if (linkClicked) {
                Application.OpenURL(url);
            }
        }

        /// <summary>
        /// Returns the path to a given directory, searching for it if needed.
        /// If `directoryToSearch` isn't provided, `Application.dataPath` is used.
        /// </summary>
        public static string FindDirectory(string expectedPath, string directoryToSearch = null, string[] ignorePaths = null) {

            if (Directory.Exists(expectedPath)) {
                return expectedPath;
            }
            // The directory isn't in the expected location, so fall back to finding it.
            var directoryName = Path.GetFileName(expectedPath);
            if (directoryToSearch == null) {
                directoryToSearch = Application.dataPath;
            }
            var directories = Directory.GetDirectories(directoryToSearch, directoryName, SearchOption.AllDirectories);
            if (ignorePaths != null) {
                directories = directories.ToList().Where(d => !ignorePaths.Contains(d)).ToArray();
            }
            return _returnOnePathOrThrow(directories, expectedPath, directoryToSearch, true);
        }

        /// <summary>
        /// Returns the path to a given file, searching for it if needed.
        /// If `directoryToSearch` isn't provided, `Application.dataPath` is used.
        /// </summary>
        public static string FindFile(string expectedPath, string directoryToSearch = null) {

            if (File.Exists(expectedPath)) {
                return expectedPath;
            }
            // The file isn't in the expected location, so fall back to finding it.
            var fileName = Path.GetFileName(expectedPath);
            if (directoryToSearch == null) {
                directoryToSearch = Application.dataPath;
            }
            var files = Directory.GetFiles(directoryToSearch, fileName, SearchOption.AllDirectories);
            return _returnOnePathOrThrow(files, expectedPath, directoryToSearch);
        }

        public static void ForceAndroidInternetPermission() {

            #if !VUPLEX_ANDROID_DISABLE_REQUIRE_INTERNET
                if (!PlayerSettings.Android.forceInternetPermission) {
                    PlayerSettings.Android.forceInternetPermission = true;
                    WebViewLogger.LogWarning("Just a heads-up: 3D WebView changed the Android player setting \"Internet Access\" to \"Require\" to ensure that it can fetch web pages from the internet. (This message will only be logged once.)");
                }
            #endif
        }

        public static string GetLinkColor() {

            return EditorGUIUtility.isProSkin ? "#7faef0ff" : "#11468aff";
        }

        public static string TextWithColor(string text, string color) {

            return $"<color={color}>{text}</color>";
        }

        public static void ValidateAndroidGraphicsApi(bool native2DSupported = false) {

            #if !VUPLEX_DISABLE_GRAPHICS_API_WARNING
                var autoGraphicsApiEnabled = PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.Android);
                var selectedGraphicsApi = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android)[0];
                var vulkanEnabled = selectedGraphicsApi == GraphicsDeviceType.Vulkan;
                if (!(vulkanEnabled || autoGraphicsApiEnabled)) {
                    // OpenGLES is selected, so nothing to warn about.
                    return;
                }
                var warningPrefix = autoGraphicsApiEnabled ? "Auto Graphics API is enabled in Player Settings, which means that the Vulkan Graphics API may be used."
                                                           : "The Vulkan Graphics API is enabled in Player Settings.";
                #if UNITY_2020_2_OR_NEWER
                    // At build time, XRSettings.enabled is always false, so to check if XR is enabled,
                    // we must instead check whether XRSettings.supportedDevices[0] != "None".
                    var xrDevices = VXUtils.XRSettings.supportedDevices;
                    var xrIsEnabled = xrDevices.Length > 0 && xrDevices[0] != "None";
                    if (!xrIsEnabled) {
                        WebViewLogger.LogWarning($"{warningPrefix} 3D WebView for Android supports Vulkan, but{(native2DSupported ? " unless the application only uses webviews in Native 2D Mode, then" : "")} the target Android devices must support the Vulkan extension VK_ANDROID_external_memory_android_hardware_buffer. That extension is supported on newer devices like Oculus Quest but isn't supported on all Android phones that support Vulkan. If your application is intended for general Android phones, it's recommended to{(native2DSupported ? " either only use Native 2D Mode or to" : "")} change the project's selected Graphics API to OpenGLES in Player Settings.{(native2DSupported ? " If your application is already only using Native 2D Mode, then please ignore this message." : "")} For more details, see this page: <em>https://support.vuplex.com/articles/vulkan#android</em>");
                    }
                #else
                    throw new BuildFailedException(warningPrefix + " 3D WebView for Android requires Unity 2020.2 or newer in order to support Vulkan. So, please either upgrade to a newer version of Unity or change the selected Graphics API to OpenGLES in Player Settings.");
                #endif
            #endif
        }

        public static bool XRSdkIsEnabled(string sdkNameFragment) {

            // This approach is taken because the legacy Oculus XR plugin identifies itself as "Oculus", but
            // the new XR plugin shows up as two devices named "oculus input" and "oculus display". Similarly,
            // the MockHMD plugin used to identify itself as "MockHMD" but now it shows up as "MockHMD Head Tracking"
            // and "MockHMD Display".
            foreach (var sdkName in VXUtils.XRSettings.supportedDevices) {
                if (sdkName.ToLowerInvariant().Contains(sdkNameFragment.ToLowerInvariant())) {
                    return true;
                }
            }
            return false;
        }

        static string _returnOnePathOrThrow(string[] paths, string expectedPath, string directorySearched, bool isDirectory = false) {

            var itemName = isDirectory ? "directory" : "file";
            if (paths.Length == 1) {
                return paths[0];
            }
            var targetFileOrDirectoryName = Path.GetFileName(expectedPath);
            if (paths.Length > 1) {
                var joinedPaths = String.Join(", ", paths);
                throw new Exception($"Unable to determine which version of the {itemName} {targetFileOrDirectoryName} to use because multiple instances ({paths.Length}) were unexpectedly found in the directory {directorySearched}. Please review the list of instances found and remove duplicates so that there is only one: {joinedPaths}");
            }
            throw new Exception($"Unable to locate the {itemName} {targetFileOrDirectoryName}. It's not in the expected location ({expectedPath}), and no instances were found in the directory {directorySearched}. To resolve this issue, please try deleting your existing Assets/Vuplex directory and reinstalling 3D WebView.");
        }
    }
}
