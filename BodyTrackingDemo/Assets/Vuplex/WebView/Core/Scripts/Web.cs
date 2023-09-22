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

namespace Vuplex.WebView {

    /// <summary>
    /// `Web` is the top-level static class for the 3D WebView plugin.
    /// It contains static methods for configuring the module and creating resources.
    /// </summary>
    /// <seealso cref="WebViewPrefab"/>
    /// <seealso cref="CanvasWebViewPrefab"/>
    /// <seealso cref="IWebView"/>
    public static class Web {

        /// <summary>
        /// Returns the ICookieManager for managing HTTP cookies, or `null` if ICookieManager
        /// isn't supported on the current platform.
        /// </summary>
        /// <remarks>
        /// ICookieManager is supported by all of the 3D WebView packages except for:
        /// - 3D WebView for Android with Gecko Engine
        /// - 2D WebView for WebGL
        /// </remarks>
        public static ICookieManager CookieManager {
            get { return _pluginFactory.GetPlugin().CookieManager; }
        }

        /// <summary>
        /// Gets the default 3D WebView plugin type among those
        /// installed for the current platform.
        /// </summary>
        public static WebPluginType DefaultPluginType {
            get { return _pluginFactory.GetPlugin().Type; }
        }

        /// <summary>
        /// Clears all data that persists between webview instances,
        /// including cookies, storage, and cached resources.
        /// </summary>
        /// <example>
        /// <code>
        /// void Awake() {
        ///     Web.ClearAllData();
        /// }
        /// </code>
        /// </example>
        /// <remarks>
        /// Important notes:
        /// <list type="bullet">
        ///   <item>On Windows and macOS, this method cannot be executed while the Chromium browser process is running. So, you will likely need to call it from Awake() to ensure that it's executed before Chromium is started. Alternatively, you can manually terminate Chromium prior to calling this method using StandaloneWebView.TerminateBrowserProcess().</item>
        ///   <item>On Universal Windows Platform, this method is unable to clear cookies due to a UWP limitation.</item>
        /// </list>
        /// </remarks>
        /// <seealso cref="StandaloneWebView.DeleteAllCookies"/>
        public static void ClearAllData() {

            _pluginFactory.GetPlugin().ClearAllData();
        }

        /// <summary>
        /// Creates a new webview in a platform-agnostic way. After a webview
        /// is created, it must be initialized by calling its Init() method.
        /// </summary>
        /// <remarks>
        /// Note that WebViewPrefab takes care of creating and managing
        /// an IWebView instance for you, so you only need to call this method directly
        /// if you need to create an IWebView instance outside of a prefab
        /// (for example, to connect it to your own custom GameObject).
        /// </remarks>
        /// <example>
        /// <code>
        /// var webView = Web.CreateWebView();
        /// // Initialize the webview to 600px x 300px.
        /// await webView.Init(600, 300);
        /// webView.LoadUrl("https://vuplex.com");
        /// // Set the Material attached to this GameObject to display the webview.
        /// GetComponent&lt;Renderer&gt;().material = webView.CreateMaterial();
        /// </code>
        /// </example>
        public static IWebView CreateWebView() {

            return _pluginFactory.GetPlugin().CreateWebView();
        }

        /// <summary>
        /// Like CreateWebView(), except an array of preferred plugin types can be
        /// provided to override which 3D WebView plugin is used in the case where
        /// multiple plugins are installed for the same build platform.
        /// </summary>
        /// <remarks>
        /// Currently, Android is the only platform that supports multiple 3D WebView
        /// plugins: WebPluginType.Android and WebPluginType.AndroidGecko. If both
        /// plugins are installed in the same project, WebPluginType.AndroidGecko will be used by default.
        /// However, you can override this to force WebPluginType.Android to be used instead by passing
        /// `new WebPluginType[] { WebPluginType.Android }`.
        /// </remarks>
        public static IWebView CreateWebView(WebPluginType[] preferredPlugins) {

            return _pluginFactory.GetPlugin(preferredPlugins).CreateWebView();
        }

        /// <summary>
        /// Enables [remote debugging](https://support.vuplex.com/articles/how-to-debug-web-content).
        /// </summary>
        /// <remarks>
        /// On Windows and macOS, this method can only be called prior to initializing any webviews.
        /// </remarks>
        /// <example>
        /// <code>
        /// void Awake() {
        ///     Web.EnableRemoteDebugging();
        /// }
        /// </code>
        /// </example>
        public static void EnableRemoteDebugging() {

            _pluginFactory.GetPlugin().EnableRemoteDebugging();
        }

        /// <summary>
        /// By default, browsers block sites from autoplaying video with audio,
        /// but this method can be used to enable autoplay.
        /// </summary>
        /// <example>
        /// <code>
        /// void Awake() {
        ///     Web.SetAutoplayEnabled(true);
        /// }
        /// </code>
        /// </example>
        /// <remarks>
        /// Important notes:
        /// <list type="bullet">
        ///   <item>
        ///     On Windows and macOS, this method cannot be executed while the Chromium browser process is running. So, you will likely need to call it from Awake() to ensure that it's executed before Chromium is started. Alternatively, you can manually terminate Chromium prior to calling this method using StandaloneWebView.TerminateBrowserProcess().
        ///   </item>
        ///   <item>
        ///     This method works for every package except for 3D WebView for UWP,
        ///     because the underlying UWP WebView control doesn't allow autoplaying
        ///     video with audio.
        ///   </item>
        /// </list>
        /// </remarks>
        public static void SetAutoplayEnabled(bool enabled) {

            _pluginFactory.GetPlugin().SetAutoplayEnabled(enabled);
        }

        /// <summary>
        /// By default, web pages cannot access the device's
        /// camera or microphone via JavaScript, but this method can
        /// be used to grant **all web pages** access to the camera and microphone.
        /// This is useful, for example, to enable WebRTC support. Note that on
        /// Android, iOS, and UWP, [additional project configuration is needed in order to enable
        /// permission for the camera and microphone](https://support.vuplex.com/articles/webrtc).
        /// </summary>
        /// <remarks>
        /// Important notes:
        /// <list type="bullet">
        ///   <item>
        ///     On Windows and macOS, this method cannot be executed while the Chromium browser process is running. So, you will likely need to call it from Awake() to ensure that it's executed before Chromium is started. Alternatively, you can manually terminate Chromium prior to calling this method using StandaloneWebView.TerminateBrowserProcess().
        ///   </item>
        ///   <item>
        ///     On iOS, enabling the camera and microphone is only supported in iOS 14.3 or newer
        ///     and is only supported in Native 2D Mode.
        ///   </item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <code>
        /// void Awake() {
        ///     Web.SetCameraAndMicrophoneEnabled(true);
        /// }
        /// </code>
        /// </example>
        public static void SetCameraAndMicrophoneEnabled(bool enabled) {

            _pluginFactory.GetPlugin().SetCameraAndMicrophoneEnabled(enabled);
        }

        /// <summary>
        /// By default, browsers block https URLs with invalid SSL certificates
        /// from being loaded, but this method can be used to disable those
        /// certificate errors.
        /// </summary>
        /// <example>
        /// <code>
        /// void Awake() {
        ///     Web.SetIgnoreCertificateErrors(true);
        /// }
        /// </code>
        /// </example>
        /// <remarks>
        /// Important notes:
        /// <list type="bullet">
        ///   <item>
        ///     On Windows and macOS, this method cannot be executed while the Chromium browser process is running. So, you will likely need to call it from Awake() to ensure that it's executed before Chromium is started. Alternatively, you can manually terminate Chromium prior to calling this method using StandaloneWebView.TerminateBrowserProcess().
        ///   </item>
        ///   <item>
        ///     This method works for every package except for 3D WebView for UWP.
        ///     For UWP, certificates must be [whitelisted in the Package.appxmanifest file](https://www.suchan.cz/2015/10/displaying-https-page-with-invalid-certificate-in-uwp-webview/).
        ///   </item>
        /// </list>
        /// </remarks>
        public static void SetIgnoreCertificateErrors(bool ignore) {

            _pluginFactory.GetPlugin().SetIgnoreCertificateErrors(ignore);
        }

        /// <summary>
        /// Controls whether data like cookies, localStorage, and cached resources
        /// is persisted between webview instances. The default is `true`, but this
        /// can be set to `false` to achieve an "incognito mode".
        /// </summary>
        /// <remarks>
        /// On Windows and macOS, this method cannot be executed while the Chromium browser process is running. So, you will likely need to call it from Awake() to ensure that it's executed before Chromium is started. Alternatively, you can manually terminate Chromium prior to calling this method using StandaloneWebView.TerminateBrowserProcess().
        /// </remarks>
        /// <example>
        /// <code>
        /// void Awake() {
        ///     Web.SetStorageEnabled(false);
        /// }
        /// </code>
        /// </example>
        public static void SetStorageEnabled(bool enabled) {

            _pluginFactory.GetPlugin().SetStorageEnabled(enabled);
        }

        /// <summary>
        /// Globally configures all webviews to use a mobile or desktop
        /// [User-Agent](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/User-Agent).
        /// By default, webviews use the browser engine's default User-Agent, but you
        /// can force them to use a mobile User-Agent by calling `Web.SetUserAgent(true)` or a
        /// desktop User-Agent with `Web.SetUserAgent(false)`.
        /// </summary>
        /// <remarks>
        /// On Windows and macOS, this method cannot be executed while the Chromium browser process is running. So, you will likely need to call it from Awake() to ensure that it's executed before Chromium is started. Alternatively, you can manually terminate Chromium prior to calling this method using StandaloneWebView.TerminateBrowserProcess().
        /// </remarks>
        /// <example>
        /// <code>
        /// void Awake() {
        ///     // Use a desktop User-Agent.
        ///     Web.SetUserAgent(false);
        /// }
        /// </code>
        /// </example>
        /// <seealso cref="IWithSettableUserAgent"/>
        public static void SetUserAgent(bool mobile) {

            _pluginFactory.GetPlugin().SetUserAgent(mobile);
        }

        /// <summary>
        /// Globally configures all webviews to use a custom
        /// [User-Agent](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/User-Agent).
        /// </summary>
        /// <remarks>
        /// On Windows and macOS, this method cannot be executed while the Chromium browser process is running. So, you will likely need to call it from Awake() to ensure that it's executed before Chromium is started. Alternatively, you can manually terminate Chromium prior to calling this method using StandaloneWebView.TerminateBrowserProcess().
        /// </remarks>
        /// <example>
        /// <code>
        /// void Awake() {
        ///     // Use FireFox's User-Agent.
        ///     Web.SetUserAgent("Mozilla/5.0 (Macintosh; Intel Mac OS X 10.15; rv:91.0) Gecko/20100101 Firefox/91.0");
        /// }
        /// </code>
        /// </example>
        /// <seealso cref="IWithSettableUserAgent"/>
        public static void SetUserAgent(string userAgent) {

            _pluginFactory.GetPlugin().SetUserAgent(userAgent);
        }

        static internal void SetPluginFactory(WebPluginFactory pluginFactory) => _pluginFactory = pluginFactory;

        static WebPluginFactory _pluginFactory = new WebPluginFactory();

    #region Obsolete APIs
        const string CreateMaterialMessage = "Web.CreateMaterial() is now deprecated in v4. Please use IWebView.CreateMaterial() instead: https://developer.vuplex.com/webview/IWebView#CreateMaterial";
        const string CreateTextureMessage = "Web.CreateTexture() has been removed in v4 because IWebView instances now automatically create their own textures. For more details, please see this article: https://support.vuplex.com/articles/v4-changes#init";

        // Added in v1.0, deprecated in v3.16.
        [Obsolete(CreateMaterialMessage)]
        public static Task<Material> CreateMaterial() {

            var taskSource = new TaskCompletionSource<Material>();
            _pluginFactory.GetPlugin().CreateMaterial(taskSource.SetResult);
            return taskSource.Task;
        }

        // Added in v3.8, deprecated in v4.0.
        [Obsolete(CreateMaterialMessage)]
        public static void CreateMaterial(Action<Material> callback) => _pluginFactory.GetPlugin().CreateMaterial(callback);

        // Added in v3.10, removed in v4.0.
        [Obsolete(CreateTextureMessage, true)]
        public static Task<Texture2D> CreateTexture(int width, int height) { return null; }

        // Added in v1.0, deprecated in v3.16, removed in v4.0.
        [Obsolete(CreateTextureMessage, true)]
        public static void CreateTexture(float width, float height, Action<Texture2D> callback) {}

        // Added in v1.0, removed in v4.0.
        [Obsolete("Web.CreateVideoMaterial() has been removed. Please use IWithFallbackVideo.CreateVideoMaterial() instead: https://developer.vuplex.com/webview/IWithFallbackVideo#CreateVideoMaterial", true)]
        public static void CreateVideoMaterial(Action<Material> callback) {}

        // Added in v3.10, deprecated in v3.14, removed in v4.0.
        [Obsolete("Web.SetTouchScreenKeyboardEnabled() has been removed. Please use the NativeOnScreenKeyboardEnabled property of WebViewPrefab / CanvasWebViewPrefab or the IWithNativeOnScreenKeyboard interface instead: https://developer.vuplex.com/webview/WebViewPrefab#NativeOnScreenKeyboardEnabled", true)]
        public static void SetTouchScreenKeyboardEnabled(bool enabled) {}
    #endregion
    }
}
