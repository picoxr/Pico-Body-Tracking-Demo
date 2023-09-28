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
#pragma warning disable CS0067
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using Vuplex.WebView.Internal;

namespace Vuplex.WebView {

    /// <summary>
    /// The IWebView implementation used by 3D WebView for Android.
    /// This class also includes extra methods for Android-specific functionality.
    /// </summary>
    public class AndroidWebView : BaseWebView,
                                  IWebView,
                                  IWithChangingTexture,
                                  IWithDownloads,
                                  IWithFileSelection,
                                  IWithFind,
                                  IWithMovablePointer,
                                  IWithNative2DMode,
                                  IWithNativeOnScreenKeyboard,
                                  IWithPointerDownAndUp,
                                  IWithPopups,
                                  IWithSettableUserAgent {

        /// <see cref="IWithDownloads"/>
        public event EventHandler<DownloadChangedEventArgs> DownloadProgressChanged;

        /// <see cref="IWithChangingTexture"/>
        public event EventHandler<EventArgs<Texture2D>> TextureChanged;

        /// <seealso cref="IWithNative2DMode"/>
        public bool Native2DModeEnabled { get; private set; }

        public WebPluginType PluginType { get; } = WebPluginType.Android;

        /// <seealso cref="IWithNative2DMode"/>
        public Rect Rect { get { return _rect; }}

        /// <seealso cref="IWithNative2DMode"/>
        public bool Visible { get; private set; }

        /// <see cref="IWithFileSelection"/>
        public event EventHandler<FileSelectionEventArgs> FileSelectionRequested {
            add {
                _assertSingletonEventHandlerUnset(_fileSelectionHandler, "FileSelectionRequested");
                _fileSelectionHandler = value;
                _webView.Call("setFileSelectionHandler", new AndroidFileSelectionCallback(_handleFileSelection));
            }
            remove {
                if (_fileSelectionHandler == value) {
                    _fileSelectionHandler = null;
                    _webView.Call("setFileSelectionHandler", null);
                }
            }
        }

        /// <see cref="IWithPopups"/>
        public event EventHandler<PopupRequestedEventArgs> PopupRequested;

        /// <summary>
        /// Indicates that the browser's render process terminated, either because it
        /// crashed or because the operating system killed it.
        /// Note that this event only works in Android API level 26 and newer.
        /// </summary>
        /// <remarks>
        /// 3D WebView for Android internally uses the android.webkit.WebView system
        /// package as its browser engine. Android's documentation indicates that
        /// the browser's render process can terminate in some rare circumstances.
        /// This RenderProcessGone event indicates when that occurs so that the application
        /// can recover be destroying the existing webviews and creating new webviews.
        /// </remarks>
        /// <example>
        /// <code>
        /// await webViewPrefab.WaitUntilInitialized();
        /// #if UNITY_ANDROID &amp;&amp; !UNITY_EDITOR
        ///     var androidWebView = webViewPrefab.WebView as AndroidWebView;
        ///     androidWebView.RenderProcessGone += (sender, eventArgs) => {
        ///         Debug.Log("The browser process was terminated");
        ///     };
        /// #endif
        /// </code>
        /// </example>
        /// <seealso href="https://developer.android.com/reference/android/webkit/WebViewClient#onRenderProcessGone(android.webkit.WebView,%20android.webkit.RenderProcessGoneDetail)">
        /// android.webkit.WebViewClient.onRenderProcessGone()
        /// </seealso>
        /// <seealso href="https://developer.android.com/guide/webapps/managing-webview#termination-handle">
        /// Termination Handling API (Android docs)
        /// </seealso>
        public event EventHandler RenderProcessGone;

        /// <summary>
        /// Event raised when a script in the page calls window.alert().
        /// </summary>
        /// <remarks>
        /// If no handler is attached to this event, then `window.alert()` will return
        /// immediately and the script will continue execution. If a handler is attached to
        /// this event, then script execution will be paused until the event args' Continue()
        /// callback is called.
        /// </remarks>
        /// <example>
        /// <code>
        /// await webViewPrefab.WaitUntilInitialized();
        /// #if UNITY_ANDROID &amp;&amp; !UNITY_EDITOR
        ///     var androidWebView = webViewPrefab.WebView as AndroidWebView;
        ///     androidWebView.ScriptAlerted += (sender, eventArgs) => {
        ///         Debug.Log("Script alerted: " + eventArgs.Message);
        ///         eventArgs.Continue();
        ///     };
        /// #endif
        /// </code>
        /// </example>
        public event EventHandler<ScriptDialogEventArgs> ScriptAlerted {
            add {
                _assertSingletonEventHandlerUnset(_scriptAlertHandler, "ScriptAlerted");
                _scriptAlertHandler = value;
                _webView.Call("setScriptAlertHandler", new AndroidStringAndBoolDelegateCallback(_handleScriptAlert));
            }
            remove {
                if (_scriptAlertHandler == value) {
                    _scriptAlertHandler = null;
                    _webView.Call("setScriptAlertHandler", null);
                }
            }
        }

        /// <summary>
        /// Event raised when a script in the page calls window.confirm().
        /// </summary>
        /// <remarks>
        /// If no handler is attached to this event, then `window.confirm()` will return
        /// `false` immediately and the script will continue execution. If a handler is attached to
        /// this event, then script execution will be paused until the event args' Continue() callback
        /// is called, and `window.confirm()` will return the value passed to `Continue()`.
        /// </remarks>
        /// <example>
        /// <code>
        /// await webViewPrefab.WaitUntilInitialized();
        /// #if UNITY_ANDROID &amp;&amp; !UNITY_EDITOR
        ///     var androidWebView = webViewPrefab.WebView as AndroidWebView;
        ///     androidWebView.ScriptConfirmRequested += (sender, eventArgs) => {
        ///         Debug.Log("Script confirm requested: " + eventArgs.Message);
        ///         eventArgs.Continue(true);
        ///     };
        /// #endif
        /// </code>
        /// </example>
        public event EventHandler<ScriptDialogEventArgs<bool>> ScriptConfirmRequested {
            add {
                _assertSingletonEventHandlerUnset(_scriptConfirmHandler, "ScriptConfirmRequested");
                _scriptConfirmHandler = value;
                _webView.Call("setScriptConfirmHandler", new AndroidStringAndBoolDelegateCallback(_handleScriptConfirm));
            }
            remove {
                if (_scriptConfirmHandler == value) {
                    _scriptConfirmHandler = null;
                    _webView.Call("setScriptConfirmHandler", null);
                }
            }
        }

        internal static void AssertWebViewIsAvailable() {

            if (!IsWebViewAvailable()) {
                throw new WebViewUnavailableException("The Android WebView package is currently unavailable. This is rare but can occur if it's not installed on the system or is currently being updated.");
            }
        }

        /// <seealso cref="IWithNative2DMode"/>
        public void BringToFront() {

            _assertNative2DModeEnabled();
            _callInstanceMethod("bringToFront");
        }

        public override Task<bool> CanGoBack() {

            var taskSource = new TaskCompletionSource<bool>();
            _callInstanceMethod("canGoBack", new AndroidBoolCallback(taskSource.SetResult));
            return taskSource.Task;
        }

        public override Task<bool> CanGoForward() {

            var taskSource = new TaskCompletionSource<bool>();
            _callInstanceMethod("canGoForward", new AndroidBoolCallback(taskSource.SetResult));
            return taskSource.Task;
        }

        // Override because BaseWebView.CaptureScreenshot() doesn't work with Android OES textures.
        public override Task<byte[]> CaptureScreenshot() {

            var taskSource = new TaskCompletionSource<byte[]>();
            _callInstanceMethod("captureScreenshot", new AndroidByteArrayCallback(taskSource.SetResult));
            return taskSource.Task;
        }

        public static void ClearAllData() => _class.CallStatic("clearAllData");

        /// <see cref="IWithFind"/>
        public void ClearFindMatches() => _callInstanceMethod("clearFindMatches");

        /// <summary>
        /// Clears the webview's back / forward navigation history.
        /// </summary>
        /// <example>
        /// <code>
        /// #if UNITY_ANDROID &amp;&amp; !UNITY_EDITOR
        ///     var androidWebView = webViewPrefab.WebView as AndroidWebView;
        ///     androidWebView.ClearHistory();
        /// #endif
        /// </code>
        /// </example>
        public void ClearHistory() => _callInstanceMethod("clearHistory");

        public override void Click(int xInPixels, int yInPixels, bool preventStealingFocus = false) {

            _assertPointIsWithinBounds(xInPixels, yInPixels);
            _callInstanceMethod("click", xInPixels, yInPixels, preventStealingFocus);
        }

        public override Material CreateMaterial() {

            var material = VXAndroidUtils.CreateAndroidMaterial();
            material.mainTexture = Texture;
            return material;
        }

        public static Task<bool> DeleteCookies(string url, string cookieName = null) {

            if (url == null) {
                throw new ArgumentException("The url cannot be null.");
            }
            _class.CallStatic("deleteCookies", url, cookieName);
            return Task.FromResult(true);
        }

        public override void Dispose() {

            _assertValidState();
            // Cancel the render if it has been scheduled via GL.IssuePluginEvent().
            WebView_removePointer(_webView.GetRawObject());
            IsDisposed = true;
            _webView.Call("destroy");
            _webView.Dispose();
            Destroy(gameObject);
        }

        public override void ExecuteJavaScript(string javaScript, Action<string> callback) {

            var nativeCallback = callback == null ? null : new AndroidStringCallback(callback);
            _callInstanceMethod("executeJavaScript", javaScript, nativeCallback);
        }

        /// <see cref="IWithFind"/>
        public Task<FindResult> Find(string text, bool forward) {

            _assertValidState();
            var taskSource = new TaskCompletionSource<FindResult>();
            _pendingFindCallbacks.Add(taskSource.SetResult);
            _webView.Call("find", text, forward);
            return taskSource.Task;
        }

        public static Task<Cookie[]> GetCookies(string url, string cookieName = null) {

            var cookiesJson = _class.CallStatic<string>("getCookies", url, cookieName);
            var cookies = Cookie.ArrayFromJson(cookiesJson);
            return Task.FromResult(cookies);
        }

        /// <summary>
        /// Returns the instance's native android.webkit.WebView.
        /// </summary>
        /// <remarks>
        /// Warning: Adding code that interacts with the native WebView directly
        /// may interfere with 3D WebView's functionality
        /// and vice versa. So, it's highly recommended to stick to 3D WebView's
        /// C# APIs whenever possible and only use GetNativeWebView() if
        /// truly necessary. If 3D WebView is missing an API that you need,
        /// feel free to [contact us](https://vuplex.com/contact).
        /// </remarks>
        /// <example>
        /// <code>
        /// #if UNITY_ANDROID &amp;&amp; !UNITY_EDITOR
        ///     var androidWebView = webViewPrefab.WebView as AndroidWebView;
        ///     var nativeWebView = androidWebView.GetNativeWebView();
        ///     // Call the android.webkit.WebView.findAll() method to search for the letter "a".
        ///     // https://developer.android.com/reference/android/webkit/WebView#findAll(java.lang.String)
        ///     // Most native WebView methods must be called on the Android UI thread, so do
        ///     // that using com.unity3d.player.UnityPlayer.currentActivity.runOnUiThread().
        ///     var UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        ///     var currentActivity = UnityPlayer.GetStatic&lt;AndroidJavaObject&gt;("currentActivity");
        ///     currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
        ///         nativeWebView.Call&lt;int&gt;("findAll", "a");
        ///     }));
        /// #endif
        /// </code>
        /// </example>
        public AndroidJavaObject GetNativeWebView() => _callInstanceMethod<AndroidJavaObject>("getNativeWebView");

        // Override because BaseWebView.GetRawTextureData() is slow on Android.
        public override Task<byte[]> GetRawTextureData() {

            var taskSource = new TaskCompletionSource<byte[]>();
            _callInstanceMethod("getRawTextureData", new AndroidByteArrayCallback(taskSource.SetResult));
            return taskSource.Task;
        }

        public static void GloballySetUserAgent(bool mobile) => _class.CallStatic("globallySetUserAgent", mobile);

        public static void GloballySetUserAgent(string userAgent) => _class.CallStatic("globallySetUserAgent", userAgent);

        public override void GoBack() => _callInstanceMethod("goBack");

        public override void GoForward() => _callInstanceMethod("goForward");

        public async Task Init(int width, int height) {

            AssertWebViewIsAvailable();
            await _initAndroid3D(width, height, null);
        }

        /// <seealso cref="IWithNative2DMode"/>
        public async Task InitInNative2DMode(Rect rect) {

            AssertWebViewIsAvailable();
            await _initAndroid2D(rect, null);
        }

        public static AndroidWebView Instantiate() => new GameObject().AddComponent<AndroidWebView>();

        /// <summary>
        /// Indicates whether the Android WebView package is installed on the system and available.
        /// </summary>
        /// <remarks>
        /// 3D WebView internally depends on Android's WebView package, which is normally installed
        /// as part of the operating system. In rare circumstances, the Android WebView package may be unavailable.
        /// For example, this can happen if the user used developer tools to delete the WebView package
        /// or if [updates to the WebView package are currently being installed](https://bugs.chromium.org/p/chromium/issues/detail?id=506369) .
        /// </remarks>
        /// <example>
        /// <code>
        /// #if UNITY_ANDROID &amp;&amp; !UNITY_EDITOR
        ///     Debug.Log("WebView is available: " + AndroidWebView.IsWebViewAvailable())
        /// #endif
        /// </code>
        /// </example>
        public static bool IsWebViewAvailable() {

            if (_webViewPackageIsAvailable == null) {
                _webViewPackageIsAvailable = _class.CallStatic<bool>("isWebViewAvailable");
            }
            return (bool)_webViewPackageIsAvailable;
        }

        public override void LoadHtml(string html) => _callInstanceMethod("loadHtml", html);

        /// <summary>
        /// Like IWebView.LoadHtml(), but also allows a virtual base URL
        /// to be specified. Setting a base URL allows, for example, for
        /// additional resources like CSS and JavaScript files to be referenced
        /// via a relative path.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// // Load some HTML that references a javascript.js file
        /// // located in Application.persistentDataPath.
        /// await webViewPrefab.WaitUntilInitialized();
        /// #if UNITY_ANDROID && !UNITY_EDITOR
        ///     var androidWebView = webViewPrefab.WebView as AndroidWebView;
        ///     var persistentDataPathFileUrl = "file://" + Application.persistentDataPath;
        ///     androidWebView.LoadHtml(
        ///         @"<div>
        ///             <script src='javascript.js'></script>
        ///             <h1>Hello!</h1>
        ///         </div>",
        ///         persistentDataPathFileUrl
        ///     );
        /// #endif
        /// ]]>
        /// </code>
        /// </example>
        public void LoadHtml(string html, string baseUrl) => _callInstanceMethod("loadHtml", html, baseUrl);

        public override void LoadUrl(string url) {

            _callInstanceMethod("loadUrl", _transformUrlIfNeeded(url));
        }

        public override void LoadUrl(string url, Dictionary<string, string> additionalHttpHeaders) {

            if (additionalHttpHeaders == null) {
                LoadUrl(url);
            } else {
                var map = VXAndroidUtils.ToJavaMap(additionalHttpHeaders);
                _callInstanceMethod("loadUrl", url, map);
            }
        }

        /// <see cref="IWithMovablePointer"/>
        public void MovePointer(Vector2 normalizedPoint) {

            var pixelsPoint = _convertNormalizedToPixels(normalizedPoint);
            _callInstanceMethod("movePointer", pixelsPoint.x, pixelsPoint.y);
        }

        /// <summary>
        /// Pauses processing, media, and rendering for this webview instance
        /// until Resume() is called.
        /// </summary>
        /// <example>
        /// <code>
        /// #if UNITY_ANDROID &amp;&amp; !UNITY_EDITOR
        ///     var androidWebView = webViewPrefab.WebView as AndroidWebView;
        ///     androidWebView.Pause();
        /// #endif
        /// </code>
        /// </example>
        public void Pause() => _callInstanceMethod("pause");

        /// <summary>
        /// Pauses processing, media, and rendering for all webview instances.
        /// By default, 3D WebView automatically calls this method when the application
        /// is paused.
        /// </summary>
        /// <remarks>
        /// This method internally calls android.webkit.WebView.pauseTimers(), which globally affects all
        /// native webview instances. So, if your project contains other plugins that use
        /// the System WebView (for example, ad SDKs), they can be affected by this method.
        /// If you find that 3D WebView is interfering with an ad SDK or other plugin in your project that
        /// uses the System WebView, please add the scripting symbol `VUPLEX_ANDROID_DISABLE_AUTOMATIC_PAUSING`
        /// to your project to prevent 3D WebView from automatically calling this method.
        /// </remarks>
        /// <example>
        /// <code>
        /// #if UNITY_ANDROID &amp;&amp; !UNITY_EDITOR
        ///     AndroidWebView.PauseAll();
        /// #endif
        /// </code>
        /// </example>
        public static void PauseAll() => _class.CallStatic("pauseAll");

        /// <see cref="IWithPointerDownAndUp"/>
        public void PointerDown(Vector2 point) => _pointerDown(point, MouseButton.Left, 1);

        /// <see cref="IWithPointerDownAndUp"/>
        public void PointerDown(Vector2 point, PointerOptions options) {

            if (options == null) {
                options = new PointerOptions();
            }
            _pointerDown(point, options.Button, options.ClickCount);
        }

        /// <see cref="IWithPointerDownAndUp"/>
        public void PointerUp(Vector2 point) => _pointerUp(point, MouseButton.Left, 1);

        /// <see cref="IWithPointerDownAndUp"/>
        public void PointerUp(Vector2 point, PointerOptions options) {

            if (options == null) {
                options = new PointerOptions();
            }
            _pointerUp(point, options.Button, options.ClickCount);
        }

        /// <summary>
        /// Loads the given URL using an HTTP POST request and the given
        /// application/x-www-form-urlencoded data.
        /// </summary>
        /// <example>
        /// <code>
        /// await webViewPrefab.WaitUntilInitialized();
        /// #if UNITY_ANDROID &amp;&amp; !UNITY_EDITOR
        ///     var androidWebView = webViewPrefab.WebView as AndroidWebView;
        ///     androidWebView.PostUrl("https://postman-echo.com/post", Encoding.Unicode.GetBytes("foo=bar"));
        /// #endif
        /// </code>
        /// </example>
        public void PostUrl(string url, byte[] data) => _callInstanceMethod("postUrl", url, data);

        public override void Reload() => _callInstanceMethod("reload");

        /// <summary>
        /// Resumes processing and rendering for all webview instances
        /// after a previous call to Pause().
        /// </summary>
        /// <example>
        /// <code>
        /// #if UNITY_ANDROID &amp;&amp; !UNITY_EDITOR
        ///     var androidWebView = webViewPrefab.WebView as AndroidWebView;
        ///     androidWebView.Resume();
        /// #endif
        /// </code>
        /// </example>
        public void Resume() => _callInstanceMethod("resume");

        /// <summary>
        /// Resumes processing and rendering for all webview instances
        /// after a previous call to PauseAll(). This method
        /// is automatically called by the plugin when the application resumes after
        /// being paused.
        /// </summary>
        /// <example>
        /// <code>
        /// #if UNITY_ANDROID &amp;&amp; !UNITY_EDITOR
        ///     AndroidWebView.ResumeAll();
        /// #endif
        /// </code>
        /// </example>
        public static void ResumeAll() => _class.CallStatic("resumeAll");

        public override void Scroll(int x, int y) => _callInstanceMethod("scroll", x, y);

        public override void Scroll(Vector2 normalizedScrollDelta, Vector2 normalizedPoint) {

            var scrollDeltaInPixels = _convertNormalizedToPixels(normalizedScrollDelta, false);
            var pointInPixels = _convertNormalizedToPixels(normalizedPoint);
            _callInstanceMethod("scroll", scrollDeltaInPixels.x, scrollDeltaInPixels.y, pointInPixels.x, pointInPixels.y);
        }

        public override void SendKey(string key) => _callInstanceMethod("sendKey", key);

        public static void SetAlternativeKeyboardInputSystemEnabled(bool enabled) {

            _class.CallStatic("setAlternativeKeyboardInputSystemEnabled", enabled);
        }

        /// <summary>
        /// This method is automatically called by AndroidWebPlugin.cs on Oculus Quest
        /// in order to activate a workaround for an issue where the Android System WebView
        /// dispatches pointer events to the wrong coordinates.
        /// https://support.vuplex.com/articles/android-comparison
        /// </summary>
        public static void SetAlternativePointerInputSystemEnabled(bool enabled) {

            _class.CallStatic("setAlternativePointerInputSystemEnabled", enabled);
        }

        public static new void SetCameraAndMicrophoneEnabled(bool enabled) => _class.CallStatic("setCameraAndMicrophoneEnabled", enabled);

        public static void SetAutoplayEnabled(bool enabled) => _class.CallStatic("setAutoplayEnabled", enabled);

        public static void SetClickCorrectionEnabled(bool enabled) => _class.CallStatic("setClickCorrectionEnabled", enabled);

        public static Task<bool> SetCookie(Cookie cookie) {

            if (!cookie.IsValid) {
                throw new ArgumentException("Unable to set invalid cookie: " + cookie);
            }
            _class.CallStatic("setCookie", cookie.ToJson());
            return Task.FromResult(true);
        }

        /// <summary>
        /// Sets whether deep links are enabled. The default is `false`.
        /// </summary>
        /// <example>
        /// <code>
        /// async void Awake() {
        ///     #if UNITY_ANDROID &amp;&amp; !UNITY_EDITOR
        ///         AndroidWebView.SetDeepLinksEnabled(true);
        ///     #endif
        ///     await webViewPrefab.WaitUntilInitialized();
        ///     // Load a page with a link that opens the YouTube app.
        ///     webViewPrefab.WebView.LoadHtml("&lt;a href='vnd.youtube://grP0iDrSjso'&gt;Click to launch YouTube&lt;/a&gt;");
        /// }
        /// </code>
        /// </example>
        public static void SetDeepLinksEnabled(bool enabled) => _class.CallStatic("setDeepLinksEnabled", enabled);

        /// <see cref="IWithDownloads"/>
        public void SetDownloadsEnabled(bool enabled) => _callInstanceMethod("setDownloadsEnabled", enabled);

        /// <summary>
        /// Enables WideVine DRM. DRM is disabled by default because it
        /// could potentially be used for tracking.
        /// </summary>
        /// <remarks>
        /// You can verify that DRM is enabled by using the DRM Stream Test
        /// on [this page](https://bitmovin.com/demos/drm).
        /// </remarks>
        /// <example>
        /// <code>
        /// #if UNITY_ANDROID &amp;&amp; !UNITY_EDITOR
        ///     AndroidWebView.SetDrmEnabled(true);
        /// #endif
        /// </code>
        /// </example>
        public static void SetDrmEnabled(bool enabled) => _class.CallStatic("setDrmEnabled", enabled);

        public override void SetFocused(bool focused) => _callInstanceMethod("setFocused", focused);

        /// <summary>
        /// Sets the force dark mode for this WebView. Note that this API is only supported on Android API level >= 29
        /// and is ignored in older versions of Android.
        /// </summary>
        /// <example>
        /// <code>
        /// await webViewPrefab.WaitUntilInitialized();
        /// #if UNITY_ANDROID &amp;&amp; !UNITY_EDITOR
        ///     var androidWebView = webViewPrefab.WebView as AndroidWebView;
        ///     androidWebView.SetForceDark(ForceDark.On);
        /// #endif
        /// </code>
        /// </example>
        /// <seealso href="https://developer.android.com/reference/android/webkit/WebSettings#setForceDark(int)">android.webkit.WebSettings.setForceDark()</seealso>
        public void SetForceDark(ForceDark forceDark) => _callInstanceMethod("setForceDark", (int)forceDark);

        /// <summary>
        /// By default, web pages can use the JavaScript Fullscreen API to make the browser occupy the entire
        /// device screen when running in Native 2D Mode, but this method can be used to disable that. This method
        /// only has an effect in Native 2D Mode and has no effect when Native 2D Mode is disabled.
        /// </summary>
        /// <example>
        /// <code>
        /// #if UNITY_ANDROID &amp;&amp; !UNITY_EDITOR
        ///     await canvasWebViewPrefab.WaitUntilInitialized();
        ///     var androidWebView = canvasWebViewPrefab.WebView as AndroidWebView;
        ///     androidWebView.SetFullscreenEnabled(false);
        /// #endif
        /// </code>
        /// </example>
        public void SetFullscreenEnabled(bool enabled) => _callInstanceMethod("setFullscreenEnabled", enabled);

        /// <summary>
        /// By default, web pages cannot access the device's
        /// geolocation via JavaScript, even if the user has granted
        /// the app permission to access location. Invoking `SetGeolocationPermissionEnabled(true)` allows
        /// **all web pages** to access the geolocation if the user has
        /// granted the app location permissions via the standard Android permission dialogs.
        /// </summary>
        /// <remarks>
        /// The following Android permissions must be included in the app's AndroidManifest.xml
        /// and also requested by the application at runtime:
        /// - android.permission.ACCESS_COARSE_LOCATION
        /// - android.permission.ACCESS_FINE_LOCATION
        /// </remarks>
        /// <example>
        /// <code>
        /// #if UNITY_ANDROID &amp;&amp; !UNITY_EDITOR
        ///     AndroidWebView.SetGeolocationPermissionEnabled(true);
        /// #endif
        /// </code>
        /// </example>
        public static void SetGeolocationPermissionEnabled(bool enabled) {

            _class.CallStatic("setGeolocationPermissionEnabled", enabled);
        }

        public static void SetIgnoreCertificateErrors(bool ignore) {

            _class.CallStatic("setIgnoreCertificateErrors", ignore);
        }

        /// <summary>
        /// Sets the initial scale for web content, where 1.0 is the default scale.
        /// </summary>
        /// <example>
        /// <code>
        /// await webViewPrefab.WaitUntilInitialized();
        /// #if UNITY_ANDROID &amp;&amp; !UNITY_EDITOR
        ///     var androidWebView = webViewPrefab.WebView as AndroidWebView;
        ///     androidWebView.SetInitialScale(1.75f);
        /// #endif
        /// </code>
        /// </example>
        public void SetInitialScale(float scale) => _callInstanceMethod("setInitialScale", scale);

        /// <summary>
        /// By default, a native file picker is shown for file inputs,
        /// but this method can be used to disable it. Note that the screen orientation
        /// of the native file picker UI is determined by the "Auto-rotate screen" preference
        /// in the device's Settings app.
        /// </summary>
        /// <example>
        /// <code>
        /// await webViewPrefab.WaitUntilInitialized();
        /// #if UNITY_ANDROID &amp;&amp; !UNITY_EDITOR
        ///     var androidWebView = webViewPrefab.WebView as AndroidWebView;
        ///     androidWebView.SetNativeFileSelectionEnabled(false);
        /// #endif
        /// </code>
        /// </example>
        public void SetNativeFileSelectionEnabled(bool enabled) => _callInstanceMethod("setNativeFileSelectionEnabled", enabled);

        /// <summary>
        /// 3D WebView for Android works by using native android.webkit.WebView instances, and it must
        /// add those instances to the native Android view hierarchy in order for them to work correctly.
        /// By default, 3D WebView adds the native WebView instances as children of
        /// the Unity game's ViewGroup, which is obtained using this approach in Java:
        /// <code>
        /// ViewGroup parentViewGroup = (ViewGroup)UnityPlayer.currentActivity.getWindow().getDecorView().getRootView();
        /// </code>
        /// However, you can call this method at the start of the app to override the ViewGroup to which
        /// 3D WebView adds the native WebView instances. For example, you may need to do that if you're
        /// embedding Unity as a library, which may cause `UnityPlayer.currentActivity` to return a different
        /// activity than expected.
        /// </summary>
        /// <example>
        /// <code>
        /// void Awake() {
        ///     #if UNITY_ANDROID &amp;&amp; !UNITY_EDITOR
        ///         AndroidWebView.SetNativeViewGroup(viewGroup);
        ///     #endif
        /// }
        /// </code>
        /// </example>
        public static void SetNativeViewGroup(AndroidJavaObject viewGroup) => _class.CallStatic("setNativeViewGroup", viewGroup);

        /// <summary>
        /// Configures the webview's behavior when a secure origin attempts to load a resource from an insecure origin.
        /// The default mode is MixedContentMode.CompatibilityMode.
        /// </summary>
        /// <example>
        /// <code>
        /// await webViewPrefab.WaitUntilInitialized();
        /// #if UNITY_ANDROID &amp;&amp; !UNITY_EDITOR
        ///     var androidWebView = webViewPrefab.WebView as AndroidWebView;
        ///     androidWebView.SetMixedContentMode(MixedContentMode.AlwaysAllow);
        /// #endif
        /// </code>
        /// </example>
        public void SetMixedContentMode(MixedContentMode mode) => _callInstanceMethod("setMixedContentMode", (int)mode);

        /// <see cref="IWithNativeOnScreenKeyboard"/>
        public void SetNativeOnScreenKeyboardEnabled(bool enabled) => _callInstanceMethod("setNativeOnScreenKeyboardEnabled", enabled);

        /// <see cref="IWithNative2DMode"/>
        public void SetNativeZoomEnabled(bool enabled) {

            _assertNative2DModeEnabled();
            _callInstanceMethod("setNativeZoomEnabled", enabled);
        }

        /// <see cref="IWithPopups"/>
        public void SetPopupMode(PopupMode popupMode) => _callInstanceMethod("setPopupMode", (int)popupMode);

        /// <see cref="IWithNative2DMode"/>
        public void SetRect(Rect rect) {

            _assertNative2DModeEnabled();
            _callInstanceMethod("setRect", (int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
            _rect = rect;
        }

        /// <summary>
        /// When using 3D rendering mode (i.e. when Native 2D Mode is disabled), remote debugging is enabled by default
        /// on Android because some APIs like MovePointer() and SendKey() work better with remote debugging enabled.
        /// However, this method can be used to explicitly disable remote debugging. When running in Native 2D Mode, remote
        /// debugging is disabled by default.
        /// </summary>
        /// <seealso cref="Web.EnableRemoteDebugging"/>
        /// <example>
        /// <code>
        /// void Awake() {
        ///     #if UNITY_ANDROID &amp;&amp; !UNITY_EDITOR
        ///         AndroidWebView.SetRemoteDebuggingEnabled(false);
        ///     #endif
        /// }
        /// </code>
        /// </example>
        public static void SetRemoteDebuggingEnabled(bool enabled) => _class.CallStatic("setRemoteDebuggingEnabled", enabled);

        public override void SetRenderingEnabled(bool enabled) {

            if (Native2DModeEnabled) {
                VXUtils.LogNative2DModeWarning("SetRenderingEnabled");
                return;
            }
            _callInstanceMethod("setRenderingEnabled", enabled);;
            _renderingEnabled = enabled;
        }

        /// <summary>
        /// When Native 2D Mode is enabled, this method sets whether scrollbars
        /// are enabled. The default is `true`. When Native 2D Mode is
        /// not enabled, this method has no effect.
        /// </summary>
        /// <example>
        /// <code>
        /// await webViewPrefab.WaitUntilInitialized();
        /// #if UNITY_ANDROID &amp;&amp; !UNITY_EDITOR
        ///     var androidWebView = webViewPrefab.WebView as AndroidWebView;
        ///     androidWebView.SetScrollbarsEnabled(false);
        /// #endif
        /// </code>
        /// </example>
        public void SetScrollbarsEnabled(bool enabled) => _callInstanceMethod("setScrollbarsEnabled", enabled);

        public static void SetStorageEnabled(bool enabled) => _class.CallStatic("setStorageEnabled", enabled);

        /// <summary>
        /// Sets the android.view.Surface to which the webview renders.
        /// This can be used, for example, to render to an Oculus
        /// [OVROverlay](https://developer.oculus.com/reference/unity/1.34/class_o_v_r_overlay).
        /// After this method is called, the webview no longer renders
        /// to its original texture and instead renders to the given surface.
        /// </summary>
        /// <example>
        /// <code>
        /// await webViewPrefab.WaitUntilInitialized();
        /// var surface = ovrOverlay.externalSurfaceObject;
        /// webViewPrefab.Resize(ovrOverlay.externalSurfaceWidth, ovrOverlay.externalSurfaceHeight);
        /// #if UNITY_ANDROID &amp;&amp; !UNITY_EDITOR
        ///     var androidWebView = webViewPrefab.WebView as AndroidWebView;
        ///     androidWebView.SetSurface(surface);
        /// #endif
        /// </code>
        /// </example>
        public void SetSurface(IntPtr surface) {

            var surfaceObject = VXAndroidUtils.ToJavaObject(surface);
            _callInstanceMethod("setSurface", surfaceObject);
        }

        /// <summary>
        /// Sets the text zoom of the page in percent. For example, the browser engine automatically
        /// adjusts the size of web pages' text by default based on the "Font size" preference
        /// in the device's Settings app, but you can use `SetTextZoom(100)` to force that
        /// system font size preference to be ignored.
        /// </summary>
        /// <example>
        /// <code>
        /// await webViewPrefab.WaitUntilInitialized();
        /// #if UNITY_ANDROID &amp;&amp; !UNITY_EDITOR
        ///     var androidWebView = webViewPrefab.WebView as AndroidWebView;
        ///     androidWebView.SetTextZoom(100);
        /// #endif
        /// </code>
        /// </example>
        /// <seealso href="https://developer.android.com/reference/android/webkit/WebSettings#setTextZoom(int)">android.webkit.WebSettings.setTextZoom()</seealso>v
        public void SetTextZoom(int textZoom) => _callInstanceMethod("setTextZoom", textZoom);

        /// <see cref="IWithSettableUserAgent"/>
        public void SetUserAgent(bool mobile) => _callInstanceMethod("setUserAgent", mobile);

        /// <see cref="IWithSettableUserAgent"/>
        public void SetUserAgent(string userAgent) => _callInstanceMethod("setUserAgent", userAgent);

        /// <see cref="IWithNative2DMode"/>
        public void SetVisible(bool visible) {

            _assertNative2DModeEnabled();
            _callInstanceMethod("setVisible", visible);
            Visible = visible;
        }

        public override void StopLoad() => _callInstanceMethod("stopLoad");

        /// <summary>
        /// Zooms in or out by the given factor, which is multiplied by the current zoom level
        /// to reach the new zoom level.
        /// </summary>
        /// <remarks>
        /// Note that the zoom level gets reset when a new page is loaded.
        /// </remarks>
        /// <param name="zoomFactor">
        /// The zoom factor to apply in the range from 0.01 to 100.0.
        /// </param>
        /// <example>
        /// <code>
        /// // Zoom by 1.75 after the page finishes loading.
        /// await webViewPrefab.WaitUntilInitialized();
        /// webViewPrefab.WebView.LoadProgressChanged += (sender, eventArgs) => {
        ///     if (eventArgs.Type == ProgressChangeType.Finished) {
        ///         #if UNITY_ANDROID &amp;&amp; !UNITY_EDITOR
        ///             var androidWebView = webViewPrefab.WebView as AndroidWebView;
        ///             androidWebView.ZoomBy(1.75f);
        ///         #endif
        ///     }
        /// };
        /// </code>
        /// </example>
        public void ZoomBy(float zoomFactor) => _callInstanceMethod("zoomBy", zoomFactor);

        public override void ZoomIn() {

            if (Native2DModeEnabled) {
                VXUtils.LogNative2DModeWarning("ZoomIn");
                return;
            }
            _callInstanceMethod("zoomIn");
        }

        public override void ZoomOut() {

            if (Native2DModeEnabled) {
                VXUtils.LogNative2DModeWarning("ZoomOut");
                return;
            }
            _callInstanceMethod("zoomOut");
        }

    #region Non-public members
        const string _2dWebViewClassName = "com.vuplex.webview.WebView";
        const string _3dWebViewClassName = "com.vuplex.webview.WebView3D";
        static AndroidJavaClass _class = new AndroidJavaClass(_3dWebViewClassName);
        internal const string DllName = "VuplexWebViewAndroid";
        EventHandler<FileSelectionEventArgs> _fileSelectionHandler;
        List<Action<FindResult>> _pendingFindCallbacks = new List<Action<FindResult>>();
        EventHandler<ScriptDialogEventArgs> _scriptAlertHandler;
        EventHandler<ScriptDialogEventArgs<bool>> _scriptConfirmHandler;
        readonly WaitForEndOfFrame _waitForEndOfFrame = new WaitForEndOfFrame();
        AndroidJavaObject _webView;
        static bool? _webViewPackageIsAvailable = null;

        void _assertNative2DModeEnabled() {

            if (!Native2DModeEnabled) {
                throw new InvalidOperationException("IWithNative2DMode methods can only be called on a webview with Native 2D Mode enabled.");
            }
        }

        void _callInstanceMethod(string methodName, params object[] args) {

            _assertValidState();
            _webView.Call(methodName, args);
        }

        TReturn _callInstanceMethod<TReturn>(string methodName, params object[] args) {

            _assertValidState();
            return _webView.Call<TReturn>(methodName, args);
        }

        protected override Task<Texture2D> _createTexture(int width, int height) => AndroidTextureCreator.Instance.CreateTexture(width, height);

        protected override void _destroyNativeTexture(IntPtr nativeTexture) => VulkanDelayedTextureDestroyer.GetInstance(WebView_destroyVulkanTexture).DestroyTexture(nativeTexture);

        // Invoked by the native plugin.
        void HandleDownloadProgressChanged(string serializedMessage) {

            DownloadProgressChanged?.Invoke(this, DownloadMessage.FromJson(serializedMessage).ToEventArgs());
        }

        void _handleFileSelection(string serializedMessage, Action<string[]> continueCallback, Action cancelCallback) {

            var message = FileSelectionMessage.FromJson(serializedMessage);
            var eventArgs = new FileSelectionEventArgs(
                message.AcceptFilters,
                message.MultipleAllowed,
                continueCallback,
                cancelCallback
            );
            _fileSelectionHandler(this, eventArgs);
        }

        // Invoked by the native plugin.
        void HandleFindResult(string serializedResult) {

            var segments = serializedResult.Split(new char[]{','});
            var matchCount = int.Parse(segments[0]);
            var currentMatchIndex = int.Parse(segments[1]);
            var callbacks = new List<Action<FindResult>>(_pendingFindCallbacks);
            _pendingFindCallbacks.Clear();
            foreach (var callback in callbacks) {
                callback(new FindResult {
                    MatchCount = matchCount,
                    CurrentMatchIndex = currentMatchIndex
                });
            }
        }

        void _handlePopup(string url, AndroidJavaObject popupResultMessage) {

            if (PopupRequested == null) {
                return;
            }
            if (popupResultMessage == null) {
                PopupRequested?.Invoke(this, new PopupRequestedEventArgs(url, null));
                return;
            }
            Dispatcher.RunOnMainThread(async () => {
                var popupWebView = Instantiate();
                if (Native2DModeEnabled) {
                    await popupWebView._initAndroid2D(Rect, popupResultMessage);
                } else {
                    await popupWebView._initAndroid3D(Size.x, Size.y, popupResultMessage);
                }
                PopupRequested?.Invoke(this, new PopupRequestedEventArgs(url, popupWebView));
            });
        }

        // Invoked by the native plugin.
        void HandleRenderProcessGone() => RenderProcessGone?.Invoke(this, EventArgs.Empty);

        void _handleScriptAlert(string message, Action<bool> continueCallback) {

            _scriptAlertHandler(this, new ScriptDialogEventArgs(message, () => continueCallback(true)));
        }

        void _handleScriptConfirm(string message, Action<bool> continueCallback) {

            _scriptConfirmHandler(this, new ScriptDialogEventArgs<bool>(message, continueCallback));
        }

        // Invoked by the native plugin.
        protected override void HandleTextureChanged(string textureString) {

            base.HandleTextureChanged(textureString);
            #if !UNITY_2022_1_OR_NEWER
                // On Android, HandleTextureChanged() is only used for Vulkan.
                // See the comments in IWithChangingTexture.cs for details.
                Texture = Texture2D.CreateExternalTexture(
                    Size.x,
                    Size.y,
                    TextureFormat.RGBA32,
                    false,
                    false,
                    _currentNativeTexture
                );
                TextureChanged?.Invoke(this, new EventArgs<Texture2D>(Texture));
            #endif
        }

        async Task _initAndroid2D(Rect rect, AndroidJavaObject popupResultMessage) {

            Native2DModeEnabled = true;
            _rect = rect;
            Visible = true;
            await _initBase((int)rect.width, (int)rect.height, createTexture: false, asyncInit: true);
            _webView = new AndroidJavaObject(
                _2dWebViewClassName,
                gameObject.name,
                (int)rect.x,
                (int)rect.y,
                (int)rect.width,
                (int)rect.height,
                new AndroidStringAndObjectCallback(_handlePopup),
                popupResultMessage
            );
            await _initTaskSource.Task;
        }

        async Task _initAndroid3D(int width, int height, AndroidJavaObject popupResultMessage) {

            var vulkanEnabled = SystemInfo.graphicsDeviceType == GraphicsDeviceType.Vulkan;
            if (vulkanEnabled && !WebView_deviceHasRequiredVulkanExtension()) {
                VXAndroidUtils.ThrowVulkanExtensionException();
            }
            await _initBase(width, height, asyncInit: true);
            _webView = new AndroidJavaObject(
                _3dWebViewClassName,
                gameObject.name,
                vulkanEnabled ? 0 : Texture.GetNativeTexturePtr().ToInt32(),
                width,
                height,
                SystemInfo.graphicsMultiThreaded,
                vulkanEnabled,
                new AndroidStringAndObjectCallback(_handlePopup),
                popupResultMessage
            );
            await _initTaskSource.Task;
        }

        // Start the coroutine from OnEnable so that the coroutine
        // is restarted if the object is deactivated and then reactivated.
        void OnEnable() => StartCoroutine(_renderPluginOncePerFrame());

        void _pointerDown(Vector2 normalizedPoint, MouseButton mouseButton, int clickCount) {

            var pixelsPoint = _convertNormalizedToPixels(normalizedPoint);
            _callInstanceMethod("pointerDown", pixelsPoint.x, pixelsPoint.y, (int)mouseButton, clickCount, false);
        }

        void _pointerUp(Vector2 normalizedPoint, MouseButton mouseButton, int clickCount) {

            var pixelsPoint = _convertNormalizedToPixels(normalizedPoint);
            _callInstanceMethod("pointerUp", pixelsPoint.x, pixelsPoint.y, (int)mouseButton, clickCount);
        }

        IEnumerator _renderPluginOncePerFrame() {

            while (true) {
                yield return _waitForEndOfFrame;
                if (Native2DModeEnabled) {
                    break;
                }
                if (!_renderingEnabled || IsDisposed || _webView == null) {
                    continue;
                }
                var nativeWebViewPtr = _webView.GetRawObject();
                if (nativeWebViewPtr != IntPtr.Zero) {
                    int pointerId = WebView_depositPointer(nativeWebViewPtr);
                    GL.IssuePluginEvent(WebView_getRenderFunction(), pointerId);
                }
            }
        }

        protected override void _resize() => _webView.Call("resize", Size.x, Size.y);

        protected override void _setConsoleMessageEventsEnabled(bool enabled) => _callInstanceMethod("setConsoleMessageEventsEnabled", enabled);

        protected override void _setFocusedInputFieldEventsEnabled(bool enabled) => _callInstanceMethod("setFocusedInputFieldEventsEnabled", enabled);

        [DllImport(DllName)]
        static extern void WebView_destroyVulkanTexture(IntPtr texture);

        [DllImport(DllName)]
        static extern IntPtr WebView_getRenderFunction();

        [DllImport(DllName)]
        static extern int WebView_depositPointer(IntPtr pointer);

        [DllImport(DllName)]
        static extern bool WebView_deviceHasRequiredVulkanExtension();

        [DllImport(DllName)]
        static extern void WebView_removePointer(IntPtr pointer);
    #endregion

    #region Obsolete APIs
        // Removed in v3.14.
        [Obsolete("AndroidWebView.IsUsingNativeVideoRendering() has been removed because native video is now supported on all devices.", true)]
        public static bool IsUsingNativeVideoRendering() { return true; }

        // Added in v3.15, deprecated in v4.0.
        [Obsolete("AndroidWebView.GetCookie() is now deprecated. Please use Web.CookieManager.GetCookies() instead: https://developer.vuplex.com/webview/CookieManager#GetCookies")]
        public static async Task<Cookie> GetCookie(string url, string cookieName) {
            var cookies = await GetCookies(url, cookieName);
            return cookies.Length > 0 ? cookies[0] : null;
        }

        // Added in v3.15, deprecated in v4.0.
        [Obsolete("AndroidWebView.GetCookie() is now deprecated. Please use Web.CookieManager.GetCookies() instead: https://developer.vuplex.com/webview/CookieManager#GetCookies")]
        public static async void GetCookie(string url, string cookieName, Action<Cookie> callback) {
            var cookie = await GetCookie(url, cookieName);
            callback(cookie);
        }

        // Removed in v3.10.
        [Obsolete("AndroidWebView.GloballyUseAlternativeInputEventSystem() has been removed. Please switch to AndroidWebView.SetAlternativePointerInputSystemEnabled() and/or SetAlternativeKeyboardInputSystemEnabled().", true)]
        public static void GloballyUseAlternativeInputEventSystem(bool useAlternativeInputEventSystem) {}

        // Added in v3.3, removed in v3.9.
        [Obsolete("The ScriptAlert event has been renamed to ScriptAlerted. Please switch to ScriptAlerted: https://developer.vuplex.com/webview/AndroidWebView#ScriptAlerted", true)]
        public event EventHandler<ScriptDialogEventArgs> ScriptAlert;

        // Added in v2.6.3, deprecated in v4.0.
        [Obsolete("AndroidWebView.SetAudioAndVideoCaptureEnabled() is now deprecated. Please switch to Web.SetCameraAndMicrophoneEnabled(): https://developer.vuplex.com/webview/Web#SetCameraAndMicrophoneEnabled")]
        public static void SetAudioAndVideoCaptureEnabled(bool enabled) => SetCameraAndMicrophoneEnabled(enabled);

        // Added in v3.1, removed in v3.11.
        [Obsolete("AndroidWebView.SetCustomUriSchemesEnabled() has been removed. Now when a page redirects to a URI with a custom scheme, 3D WebView will automatically emit the UrlChanged and LoadProgressChanged events for the navigation, but a deep link (i.e. to an external application) won't occur.", true)]
        public static void SetCustomUriSchemesEnabled(bool enabled) {}

        // Added in v3.10, removed in v3.12.
        [Obsolete("AndroidWebView.SetForceDrawEnabled() has been removed because it is no longer needed.", true)]
        public static void SetForceDrawEnabled(bool enabled) {}

        // Added in v3.3, deprecated in v3.10, removed in v4.0.
        [Obsolete("AndroidWebView.SetIgnoreSslErrors() has been removed. Please use Web.SetIgnoreCertificateErrors() instead: https://developer.vuplex.com/webview/Web#SetIgnoreCertificateErrors", true)]
        public static void SetIgnoreSslErrors(bool ignore) {}

        // Added in v2.1.0, deprecated in v3.14, removed in v4.0.
        [Obsolete("AndroidWebView.SetMediaPlaybackRequiresUserGesture() has been removed. Please call Web.SetAutoplayEnabled(true) instead: https://developer.vuplex.com/webview/Web#SetAutoplayEnabled", true)]
        public void SetMediaPlaybackRequiresUserGesture(bool mediaPlaybackRequiresUserGesture) {}

        // Added in v2.6.3, deprecated in v3.10, removed in v4.0.
        [Obsolete("AndroidWebView.SetNativeKeyboardEnabled() has been removed. Please use the NativeOnScreenKeyboardEnabled property of WebViewPrefab / CanvasWebViewPrefab or the IWithNativeOnScreenKeyboard interface instead: https://developer.vuplex.com/webview/WebViewPrefab#NativeOnScreenKeyboardEnabled", true)]
        public static void SetNativeKeyboardEnabled(bool enabled) {}

        // Removed in v3.14.
        [Obsolete("AndroidWebView.SetNativeVideoRenderingEnabled() has been removed because native video is now supported on all devices.", true)]
        public static void SetNativeVideoRenderingEnabled(bool enabled) {}

        // Removed in v3.10.
        [Obsolete("AndroidWebView.UseAlternativeInputEventSystem() has been removed. Please use AndroidWebView.SetAlternativePointerInputSystemEnabled() and/or SetAlternativeKeyboardInputSystemEnabled() instead.", true)]
        public void UseAlternativeInputEventSystem(bool useAlternativeInputEventSystem) {}
    #endregion
    }
}
#endif
