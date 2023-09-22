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
using UnityEngine;
using UnityEngine.Serialization;
using Vuplex.WebView.Internal;

namespace Vuplex.WebView {

    /// <summary>
    /// CanvasWebViewPrefab is a prefab that makes it easy to view and interact with an IWebView in a 2D Canvas.
    /// It takes care of creating an IWebView, displaying its texture, and handling pointer interactions
    /// from the user, like clicking, dragging, and scrolling. So, all you need to do is specify a URL or HTML to load,
    /// and then the user can view and interact with it. For use outside of a Canvas, see WebViewPrefab instead.
    /// </summary>
    /// <remarks>
    /// There are two ways to create a CanvasWebViewPrefab:
    /// <list type="number">
    ///   <item>
    ///     By dragging the CanvasWebViewPrefab.prefab file into your scene via the editor and setting its "Initial URL" property.</item>
    ///   <item>
    ///     Or by creating an instance programmatically with CanvasWebViewPrefab.Instantiate(), waiting for
    ///     it to initialize, and then calling methods on its WebView property, like LoadUrl().
    ///   </item>
    /// </list>
    /// <para>
    /// If your use case requires a high degree of customization, you can instead create an IWebView
    /// outside of the prefab with Web.CreateWebView().
    /// </para>
    /// See also:
    /// <list type="bullet">
    ///   <item>WebViewPrefab: https://developer.vuplex.com/webview/WebViewPrefab</item>
    ///   <item>How clicking and scrolling works: https://support.vuplex.com/articles/clicking</item>
    ///   <item>IWebView: https://developer.vuplex.com/webview/IWebView</item>
    ///   <item>Web (static methods): https://developer.vuplex.com/webview/Web</item>
    /// </list>
    /// </remarks>
    [HelpURL("https://developer.vuplex.com/webview/CanvasWebViewPrefab")]
    public partial class CanvasWebViewPrefab : BaseWebViewPrefab {

        public override event EventHandler<ClickedEventArgs> Clicked {
            add {
                if (_native2DModeActive) {
                    _logNative2DModeWarning("The CanvasWebViewPrefab.Clicked event is not supported in Native 2D Mode.");
                }
                base.Clicked += value;
            }
            remove {
                base.Clicked -= value;
            }
        }

        public override event EventHandler<ScrolledEventArgs> Scrolled {
            add {
                if (_native2DModeActive) {
                    _logNative2DModeWarning("The CanvasWebViewPrefab.Scrolled event is not supported in Native 2D Mode.");
                }
                base.Scrolled += value;
            }
            remove {
                base.Scrolled -= value;
            }
        }

        /// <summary>
        /// Enables or disables [Native 2D Mode](https://support.vuplex.com/articles/native-2d-mode/),
        /// which makes it so that 3D WebView positions a native 2D webview in front of the Unity game view
        /// instead of displaying web content as a texture in the Unity scene. The default is `false`. If set to `true` and the 3D WebView package
        /// in use doesn't support Native 2D Mode, then the default rendering mode is used instead.
        /// </summary>
        /// <remarks>
        /// Important notes:
        /// <list type="bullet">
        ///   <item>
        ///     Native 2D Mode is only supported for 3D WebView for Android (non-Gecko) and 3D WebView for iOS.
        ///     For other packages, the default render mode is used instead.
        ///   </item>
        ///   <item>Native 2D Mode requires that the canvas's render mode be set to "Screen Space - Overlay".</item>
        /// </list>
        /// </remarks>
        [Label("Native 2D Mode (Android, iOS, WebGL, and UWP only)")]
        [Tooltip("Native 2D Mode positions a native 2D webview in front of the Unity game view instead of rendering web content as a texture in the Unity scene. Native 2D Mode provides better performance on iOS and UWP, because the default mode of rendering web content to a texture is slower. \n\nImportant notes:\n• Native 2D Mode is only supported for Android (non-Gecko), iOS, WebGL, and UWP. For the other 3D WebView packages, the default render mode is used instead.\n• Native 2D Mode requires that the canvas's render mode be set to \"Screen Space - Overlay\".")]
        [HideInInspector]
        [Header("Platform-specific")]
        public bool Native2DModeEnabled;

        /// <summary>
        /// Determines whether the operating system's native on-screen keyboard is
        /// automatically shown when a text input in the webview is focused. The default for
        /// CanvasWebViewPrefab is `true`.
        /// </summary>
        /// <remarks>
        /// The native on-screen keyboard is only supported for the following packages:
        /// <list type="bullet">
        ///   <item>3D WebView for Android (non-Gecko)</item>
        ///   <item>3D WebView for iOS</item>
        /// </list>
        /// </remarks>
        /// <remarks>
        /// On iOS, disabling the keyboard for one webview disables it for all webviews.
        /// </remarks>
        [Label("Native On-Screen Keyboard (Android and iOS only)")]
        [Tooltip("Determines whether the operating system's native on-screen keyboard is automatically shown when a text input in the webview is focused. The native on-screen keyboard is only supported for the following packages:\n• 3D WebView for Android (non-Gecko)\n• 3D WebView for iOS")]
        public bool NativeOnScreenKeyboardEnabled = true;

        /// <summary>
        /// Gets or sets the prefab's resolution in pixels per Unity unit.
        /// You can change the resolution to make web content appear larger or smaller.
        /// The default resolution for CanvasWebViewPrefab is `1`.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Setting a lower resolution decreases the pixel density, but has the effect
        /// of making web content appear larger. Setting a higher resolution increases
        /// the pixel density, but has the effect of making content appear smaller.
        /// For more information on scaling web content, see
        /// [this support article](https://support.vuplex.com/articles/how-to-scale-web-content).
        /// </para>
        /// <para>
        /// Changing the Resolution has no effect when running in [Native 2D Mode](https://support.vuplex.com/articles/native-2d-mode)
        /// because it uses the device's native resolution.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Set the resolution to 2.5px per Unity unit.
        /// webViewPrefab.Resolution = 2.5f;
        /// </code>
        /// </example>
        [Label("Resolution (px / Unity unit)")]
        [Tooltip("You can change this to make web content appear larger or smaller. Note that This property is ignored when running in Native 2D Mode.")]
        [HideInInspector]
        [FormerlySerializedAs("InitialResolution")]
        public float Resolution = 1;

        /// <summary>
        /// Determines the scroll sensitivity. The default sensitivity for CanvasWebViewPrefab is `15`.
        /// </summary>
        /// <remarks>
        /// This property is ignored when running in [Native 2D Mode](https://support.vuplex.com/articles/native-2d-mode).
        /// </remarks>
        [HideInInspector]
        [Tooltip("Determines the scroll sensitivity. Note that This property is ignored when running in Native 2D Mode.")]
        public float ScrollingSensitivity = 15;

        public override bool Visible {
            get {
                var native2DWebView = _getNative2DWebViewIfActive();
                if (native2DWebView != null) {
                    return native2DWebView.Visible;
                }
                return base.Visible;
            }
            set {
                var native2DWebView = _getNative2DWebViewIfActive();
                if (native2DWebView != null) {
                    native2DWebView.SetVisible(value);
                    return;
                }
                base.Visible = value;
            }
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <remarks>
        /// The WebView property is available after initialization completes,
        /// which is indicated by WaitUntilInitialized().
        /// </remarks>
        /// <example>
        /// <code>
        /// // Create a CanvasWebViewPrefab
        /// var canvasWebViewPrefab = CanvasWebViewPrefab.Instantiate();
        /// // Position the prefab how we want it
        /// var canvas = GameObject.Find("Canvas");
        /// canvasWebViewPrefab.transform.parent = canvas.transform;
        /// var rectTransform = canvasWebViewPrefab.transform as RectTransform;
        /// rectTransform.anchoredPosition3D = Vector3.zero;
        /// rectTransform.offsetMin = Vector2.zero;
        /// rectTransform.offsetMax = Vector2.zero;
        /// canvasWebViewPrefab.transform.localScale = Vector3.one;
        /// // Load a URL once the prefab finishes initializing
        /// await canvasWebViewPrefab.WaitUntilInitialized();
        /// canvasWebViewPrefab.WebView.LoadUrl("https://vuplex.com");
        /// </code>
        /// </example>
        public static CanvasWebViewPrefab Instantiate() {

            return Instantiate(new WebViewOptions());
        }

        /// <summary>
        /// Like Instantiate(), except it also accepts an object
        /// of options flags that can be used to alter the generated webview's behavior.
        /// </summary>
        public static CanvasWebViewPrefab Instantiate(WebViewOptions options) {

            var prefabPrototype = (GameObject) Resources.Load("CanvasWebViewPrefab");
            var gameObject = (GameObject) Instantiate(prefabPrototype);
            var canvasWebViewPrefab = gameObject.GetComponent<CanvasWebViewPrefab>();
            canvasWebViewPrefab._options = options;
            return canvasWebViewPrefab;
        }

        /// <summary>
        /// Like Instantiate(), except it initializes the instance with an existing, initialized
        /// IWebView instance. This causes the CanvasWebViewPrefab to use the existing
        /// IWebView instance instead of creating a new one.
        /// </summary>
        public static CanvasWebViewPrefab Instantiate(IWebView webView) {

            var prefabPrototype = (GameObject) Resources.Load("CanvasWebViewPrefab");
            var gameObject = (GameObject) Instantiate(prefabPrototype);
            var canvasWebViewPrefab = gameObject.GetComponent<CanvasWebViewPrefab>();
            canvasWebViewPrefab.SetWebViewForInitialization(webView);
            return canvasWebViewPrefab;
        }

    #region Non-public members
        RectTransform _cachedRectTransform;
        Canvas _canvas {
            get {
                if (_canvasGetter == null) {
                    _canvasGetter = new CachingGetter<Canvas>(GetComponentInParent<Canvas>, 1, this);
                }
                return _canvasGetter.GetValue();
            }
        }
        CachingGetter<Canvas> _canvasGetter;
        bool _native2DModeActive {
            get {
                var webViewWith2DMode = WebView as IWithNative2DMode;
                return webViewWith2DMode != null && webViewWith2DMode.Native2DModeEnabled;
            }
        }
        static Resolution _originalScreenResolution;
        RectTransform _rectTransform {
            get {
                if (_cachedRectTransform == null) {
                    _cachedRectTransform = GetComponent<RectTransform>();
                }
                return _cachedRectTransform;
            }
        }
        bool _setCustomPointerInputDetector;

        // Partial method implemented by various 3D WebView packages
        // to provide platform-specific warnings.
        partial void OnInit();

        bool _canNative2DModeBeEnabled(bool logWarnings = false) {

            if (_canvas?.renderMode == RenderMode.WorldSpace) {
                if (logWarnings) {
                    _logNative2DModeWarning("CanvasWebViewPrefab.Native2DModeEnabled is enabled but the canvas's render mode is set to World Space, so Native 2D Mode will not be enabled. In order to use Native 2D Mode, please switch the canvas's render mode to \"Screen Space - Overlay\" or \"Screen Space - Camera\".");
                }
                return false;
            }
            if (VXUtils.XRSettings.enabled) {
                if (logWarnings) {
                    _logNative2DModeWarning("CanvasWebViewPrefab.Native2DModeEnabled is enabled but XR is enabled, so Native 2D Mode will not be enabled.");
                }
                return false;
            }
            return true;
        }

        protected override float _getResolution() {

            if (Resolution > 0f) {
                return Resolution;
            }
            WebViewLogger.LogError("Invalid value set for CanvasWebViewPrefab.Resolution: " + Resolution);
            return 1;
        }

        IWithNative2DMode _getNative2DWebViewIfActive() {

            var webViewWith2DMode = WebView as IWithNative2DMode;
            if (webViewWith2DMode != null && webViewWith2DMode.Native2DModeEnabled) {
                return webViewWith2DMode;
            }
            return null;
        }

        protected override bool _getNativeOnScreenKeyboardEnabled() => NativeOnScreenKeyboardEnabled;

        protected override float _getScrollingSensitivity() => ScrollingSensitivity;

        Rect _getScreenSpaceRect() {

            var canvas = _canvas;
            if (canvas == null) {
                WebViewLogger.LogError("Unable to determine the screen space rect for Native 2D Mode because the CanvasWebViewPrefab is not placed in a Canvas. Please place the CanvasWebViewPrefab as the child of a Unity UI Canvas.");
                return Rect.zero;
            }
            var worldCorners = new Vector3[4];
            _rectTransform.GetWorldCorners(worldCorners);
            var topLeftCorner = worldCorners[1];
            var bottomRightCorner = worldCorners[3];

            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay) {
                var camera = canvas.worldCamera;
                if (camera == null) {
                    WebViewLogger.LogError("Unable to determine the screen space rect for Native 2D Mode because the Canvas's render camera is not set. Please set the Canvas's \"Render Camera\" setting or change its render mode to \"Screen Space - Overlay\".");
                } else {
                    topLeftCorner = camera.WorldToScreenPoint(topLeftCorner);
                    bottomRightCorner = camera.WorldToScreenPoint(bottomRightCorner);
                }
            }
            var x = topLeftCorner.x;
            var y = Screen.height - topLeftCorner.y;
            var width = bottomRightCorner.x - topLeftCorner.x;
            var height = topLeftCorner.y - bottomRightCorner.y;

            var scaleFactor = _getScreenSpaceScaleFactor();
            if (scaleFactor != 1f) {
                x *= scaleFactor;
                y *= scaleFactor;
                width *= scaleFactor;
                height *= scaleFactor;
            }
            return new Rect(x, y, width, height);
        }

        float _getScreenSpaceScaleFactor() {

            // GetWorldCorners() has an issue where it's incorrect if the screen resolution
            // is changed at runtime using Screen.SetResolution(). So, detect if the screen
            // resolution has been changed and scale the rect's values accordingly.
            var screenResolutionHasChanged = !_resolutionsAreEqual(Screen.currentResolution, _originalScreenResolution);
            if (screenResolutionHasChanged) {
                float scaleFactor = (float)_originalScreenResolution.width / (float)Screen.currentResolution.width;
                return scaleFactor;
            }
            // On Android and iOS, GetWorldCorners() is also incorrect if the "Resolution Scaling Mode"
            // is set to "Fixed DPI" in Player Settings -> Resolution and Presentation.
            #if UNITY_ANDROID || UNITY_IOS
                var display = Display.main;
                var resolutionScalingModeIsFixedDpi = display.renderingWidth != display.systemWidth;
                if (resolutionScalingModeIsFixedDpi) {
                    float scaleFactor = (float)display.systemWidth / (float)display.renderingWidth;
                    return scaleFactor;
                }
            #endif
            return 1f;
        }

        protected override ViewportMaterialView _getVideoLayer() {

            return transform.Find("VideoLayer").GetComponent<ViewportMaterialView>();
        }

        protected override ViewportMaterialView _getView() {

            return transform.Find("CanvasWebViewPrefabView").GetComponent<ViewportMaterialView>();
        }

        void _initCanvasPrefab() {

            OnInit();
            Initialized += _logNative2DRecommendationIfNeeded;
            var preferNative2DMode = Native2DModeEnabled && _canNative2DModeBeEnabled(true);
            var rect = preferNative2DMode ? _getScreenSpaceRect() : _rectTransform.rect;
            if (_logErrorIfSizeIsInvalid(rect.size)) {
                return;
            }
            _initBase(rect, preferNative2DMode);
        }

        bool _logErrorIfSizeIsInvalid(Vector2 size) {

            if (!(size.x > 0f && size.y > 0f)) {
                WebViewLogger.LogError($"CanvasWebViewPrefab dimensions are invalid! Width: {size.x.ToString("f4")}, Height: {size.y.ToString("f4")}. To correct this, please adjust the CanvasWebViewPrefab's RectTransform to make it so that its width and height are both greater than zero. https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/class-RectTransform.html");
                return true;
            }
            return false;
        }

        void _logNative2DModeWarning(string message) {

            WebViewLogger.LogWarning(message + " For more info, please see this article: <em>https://support.vuplex.com/articles/native-2d-mode</em>");
        }

        void _logNative2DRecommendationIfNeeded(object sender, EventArgs eventArgs) {

            var webViewWith2DMode = WebView as IWithNative2DMode;
            if (_canNative2DModeBeEnabled() && webViewWith2DMode != null && !webViewWith2DMode.Native2DModeEnabled) {
                WebViewLogger.LogTip("This platform supports Native 2D Mode, so consider enabling CanvasWebViewPrefab.Native2DModeEnabled for best results. For more info, see https://support.vuplex.com/articles/native-2d-mode .");
            }
        }

        void OnDisable() {

            // When in Native 2D Mode, hide the webview when the CanvasWebViewPrefab is deactivated.
            var webView = _getNative2DWebViewIfActive();
            if (webView != null) {
                webView.SetVisible(false);
            }
        }

        void OnEnable() {

            // When in Native 2D Mode, show the webview when the CanvasWebViewPrefab is activated.
            var webView = _getNative2DWebViewIfActive();
            if (webView != null) {
                webView.SetVisible(true);
            }
        }

        bool _resolutionsAreEqual(Resolution res1, Resolution res2) {

            if (res1.width == res2.width && res1.height == res2.height) {
                return true;
            }
            // On mobile, the width and height may be switched due to screen rotation.
            if (res1.width == res2.height && res1.height == res2.width) {
                return true;
            }
            return false;
        }

        protected override void _setVideoLayerPosition(Rect videoRect) {

            var videoRectTransform = _videoLayer.transform as RectTransform;
            // Use Vector2.Scale() because Vector2 * Vector2 isn't supported in Unity 2017.
            videoRectTransform.anchoredPosition = Vector2.Scale(Vector2.Scale(videoRect.position, _rectTransform.rect.size), new Vector2(1, -1));
            videoRectTransform.sizeDelta = Vector2.Scale(videoRect.size, _rectTransform.rect.size);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void _saveOriginalScreenResolution() => _originalScreenResolution = Screen.currentResolution;

        void Start() => _initCanvasPrefab();

        protected override void Update() {

            base.Update();
            if (WebView == null) {
                return;
            }
            _sizeInUnityUnits = _rectTransform.rect.size;
            if (_logErrorIfSizeIsInvalid(_sizeInUnityUnits)) {
                return;
            }
            // Handle updating the rect for a native 2D webview.
            var native2DWebView = _getNative2DWebViewIfActive();
            if (native2DWebView != null) {
                var screenSpaceRect = _getScreenSpaceRect();
                if (native2DWebView.Rect != screenSpaceRect) {
                    native2DWebView.SetRect(screenSpaceRect);
                }
                return;
            }
            // Handle resizing a regular webview.
            _resizeWebViewIfNeeded();
        }
    #endregion

    #region Obsolete APIs
        // Added in v3.2, removed in v3.12.
        [Obsolete("CanvasWebViewPrefab.Init() has been removed. The CanvasWebViewPrefab script now initializes itself automatically, so Init() no longer needs to be called.", true)]
        public void Init() {}

        // Added in v3.2, removed in v3.12.
        [Obsolete("CanvasWebViewPrefab.Init() has been removed. The CanvasWebViewPrefab script now initializes itself automatically, so Init() no longer needs to be called.", true)]
        public void Init(WebViewOptions options) {}

        // Added in v3.10, removed in v3.12.
        [Obsolete("CanvasWebViewPrefab.Init() has been removed. The CanvasWebViewPrefab script now initializes itself automatically, so Init() no longer needs to be called. Please use CanvasWebViewPrefab.SetWebViewForInitialization(IWebView) instead.", true)]
        public void Init(IWebView webView) {}

        // Deprecated in v4.0.
        [Obsolete("CanvasWebViewPrefab.InitialResolution is now deprecated. Please use CanvasWebViewPrefab.Resolution instead.")]
        public float InitialResolution {
            get { return Resolution; }
            set { Resolution = value; }
        }
    #endregion
    }
}
