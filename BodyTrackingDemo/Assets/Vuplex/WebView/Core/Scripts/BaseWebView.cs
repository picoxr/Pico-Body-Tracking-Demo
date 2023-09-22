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
// Only define BaseWebView.cs on supported platforms to avoid IL2CPP linking
// errors on unsupported platforms.
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_ANDROID || (UNITY_IOS && !VUPLEX_OMIT_IOS) || (UNITY_WEBGL && !VUPLEX_OMIT_WEBGL) || UNITY_WSA
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Vuplex.WebView.Internal;

namespace Vuplex.WebView {

    /// <summary>
    /// The base IWebView implementation, which is extended for each platform.
    /// </summary>
    public abstract class BaseWebView : MonoBehaviour {

        public event EventHandler CloseRequested;

        public event EventHandler<ConsoleMessageEventArgs> ConsoleMessageLogged {
            add {
                _consoleMessageLogged += value;
                if (_consoleMessageLogged != null && _consoleMessageLogged.GetInvocationList().Length == 1) {
                    _setConsoleMessageEventsEnabled(true);
                }
            }
            remove {
                _consoleMessageLogged -= value;
                if (_consoleMessageLogged == null) {
                    _setConsoleMessageEventsEnabled(false);
                }
            }
        }

        public event EventHandler<FocusedInputFieldChangedEventArgs> FocusedInputFieldChanged {
            add {
                _focusedInputFieldChanged += value;
                if (_focusedInputFieldChanged != null && _focusedInputFieldChanged.GetInvocationList().Length == 1) {
                    _setFocusedInputFieldEventsEnabled(true);
                }
            }
            remove {
                _focusedInputFieldChanged -= value;
                if (_focusedInputFieldChanged == null) {
                    _setFocusedInputFieldEventsEnabled(false);
                }
            }
        }

        public event EventHandler<ProgressChangedEventArgs> LoadProgressChanged;

        public event EventHandler<EventArgs<string>> MessageEmitted;

        public event EventHandler PageLoadFailed;

        public event EventHandler<EventArgs<string>> TitleChanged;

        public event EventHandler<UrlChangedEventArgs> UrlChanged;

        public bool IsDisposed { get; protected set; }

        public bool IsInitialized { get { return _initState == InitState.Initialized; }}

        public List<string> PageLoadScripts { get; } = new List<string>();

        public Vector2Int Size { get; private set; }

        public Texture2D Texture { get; protected set; }

        public string Title { get; private set; } = "";

        public string Url { get; private set; } = "";

        public virtual Task<bool> CanGoBack() {

            _assertValidState();
            var taskSource = new TaskCompletionSource<bool>();
            _pendingCanGoBackCallbacks.Add(taskSource.SetResult);
            WebView_canGoBack(_nativeWebViewPtr);
            return taskSource.Task;
        }

        public virtual Task<bool> CanGoForward() {

            _assertValidState();
            var taskSource = new TaskCompletionSource<bool>();
            _pendingCanGoForwardCallbacks.Add(taskSource.SetResult);
            WebView_canGoForward(_nativeWebViewPtr);
            return taskSource.Task;
        }

        public virtual Task<byte[]> CaptureScreenshot() {

            var texture = _getReadableTexture();
            var bytes = ImageConversion.EncodeToPNG(texture);
            Destroy(texture);
            return Task.FromResult(bytes);
        }

        public virtual void Click(int xInPixels, int yInPixels, bool preventStealingFocus = false) {

            _assertValidState();
            _assertPointIsWithinBounds(xInPixels, yInPixels);
            // On most platforms, the regular Click() method doesn't steal focus,
            // So, the default is to ignore preventStealingFocus.
            WebView_click(_nativeWebViewPtr, xInPixels, yInPixels);
        }

        public void Click(Vector2 normalizedPoint, bool preventStealingFocus = false) {

            _assertValidState();
            var pixelsPoint = _convertNormalizedToPixels(normalizedPoint);
            Click(pixelsPoint.x, pixelsPoint.y, preventStealingFocus);
        }

        public virtual async void Copy() {

            _assertValidState();
            GUIUtility.systemCopyBuffer = await _getSelectedText();
        }

        public virtual Material CreateMaterial() {

            var material = VXUtils.CreateDefaultMaterial();
            material.mainTexture = Texture;
            return material;
        }

        public virtual async void Cut() {

            _assertValidState();
            GUIUtility.systemCopyBuffer = await _getSelectedText();
            SendKey("Backspace");
        }

        public virtual void Dispose() {

            _assertValidState();
            IsDisposed = true;
            WebView_destroy(_nativeWebViewPtr);
            _nativeWebViewPtr = IntPtr.Zero;
            // To avoid a MissingReferenceException, verify that this script
            // hasn't already been destroyed prior to accessing gameObject.
            if (this != null) {
                Destroy(gameObject);
            }
        }

        public Task<string> ExecuteJavaScript(string javaScript) {

            var taskSource = new TaskCompletionSource<string>();
            ExecuteJavaScript(javaScript, taskSource.SetResult);
            return taskSource.Task;
        }

        public virtual void ExecuteJavaScript(string javaScript, Action<string> callback) {

            _assertValidState();
            string resultCallbackId = null;
            if (callback != null) {
                resultCallbackId = Guid.NewGuid().ToString();
                _pendingJavaScriptResultCallbacks[resultCallbackId] = callback;
            }
            WebView_executeJavaScript(_nativeWebViewPtr, javaScript, resultCallbackId);
        }

        public virtual Task<byte[]> GetRawTextureData() {

            var texture = _getReadableTexture();
            var bytes = texture.GetRawTextureData();
            Destroy(texture);
            return Task.FromResult(bytes);
        }

        public virtual void GoBack() {

            _assertValidState();
            WebView_goBack(_nativeWebViewPtr);
        }

        public virtual void GoForward() {

            _assertValidState();
            WebView_goForward(_nativeWebViewPtr);
        }

        public virtual void LoadHtml(string html) {

            _assertValidState();
            WebView_loadHtml(_nativeWebViewPtr, html);
        }

        public virtual void LoadUrl(string url) {

            _assertValidState();
            WebView_loadUrl(_nativeWebViewPtr, _transformUrlIfNeeded(url));
        }

        public virtual void LoadUrl(string url, Dictionary<string, string> additionalHttpHeaders) {

            _assertValidState();
            if (additionalHttpHeaders == null) {
                LoadUrl(url);
            } else {
                var headerStrings = additionalHttpHeaders.Keys.Select(key => $"{key}: {additionalHttpHeaders[key]}").ToArray();
                var newlineDelimitedHttpHeaders = String.Join("\n", headerStrings);
                WebView_loadUrlWithHeaders(_nativeWebViewPtr, url, newlineDelimitedHttpHeaders);
            }
        }

        public Vector2Int NormalizedToPoint(Vector2 normalizedPoint) {

            return new Vector2Int((int)(normalizedPoint.x * (float)Size.x), (int)(normalizedPoint.y * (float)Size.y));
        }

        public virtual void Paste() {

            _assertValidState();
            var text = GUIUtility.systemCopyBuffer;
            foreach (var character in text) {
                SendKey(char.ToString(character));
            }
        }

        public Vector2 PointToNormalized(int xInPixels, int yInPixels) {

            return new Vector2((float)xInPixels / (float)Size.x, (float)yInPixels / (float)Size.y);
        }

        public virtual void PostMessage(string message) {

            var escapedString = message.Replace("\\", "\\\\")
                                       .Replace("'", "\\\\'")
                                       .Replace("\n", "\\\\n");
            ExecuteJavaScript($"vuplex._emit('message', {{ data: \'{escapedString}\' }})", null);
        }

        public virtual void Reload() {

            _assertValidState();
            WebView_reload(_nativeWebViewPtr);
        }

        public virtual void Resize(int width, int height) {

            if (width == Size.x && height == Size.y) {
                return;
            }
            _assertValidState();
            _assertValidSize(width, height);
            VXUtils.ThrowExceptionIfAbnormallyLarge(width, height);
            Size = new Vector2Int(width, height);
            _resize();
        }

        public virtual void Scroll(int scrollDeltaXInPixels, int scrollDeltaYInPixels) {

            _assertValidState();
            WebView_scroll(_nativeWebViewPtr, scrollDeltaXInPixels, scrollDeltaYInPixels);
        }

        public void Scroll(Vector2 normalizedScrollDelta) {

            _assertValidState();
            var scrollDeltaInPixels = _convertNormalizedToPixels(normalizedScrollDelta, false);
            Scroll(scrollDeltaInPixels.x, scrollDeltaInPixels.y);
        }

        public virtual void Scroll(Vector2 normalizedScrollDelta, Vector2 normalizedPoint) {

            _assertValidState();
            var scrollDeltaInPixels = _convertNormalizedToPixels(normalizedScrollDelta, false);
            var pointInPixels = _convertNormalizedToPixels(normalizedPoint);
            WebView_scrollAtPoint(_nativeWebViewPtr, scrollDeltaInPixels.x, scrollDeltaInPixels.y, pointInPixels.x, pointInPixels.y);
        }

        public virtual void SelectAll() {

            _assertValidState();
            // If the focused element is an input with a select() method, then use that.
            // Otherwise, travel up the DOM until we get to the body or a contenteditable
            // element, and then select its contents.
            ExecuteJavaScript(
                @"(function() {
                    var element = document.activeElement || document.body;
                    while (!(element === document.body || element.getAttribute('contenteditable') === 'true')) {
                        if (typeof element.select === 'function') {
                            element.select();
                            return;
                        }
                        element = element.parentElement;
                    }
                    var range = document.createRange();
                    range.selectNodeContents(element);
                    var selection = window.getSelection();
                    selection.removeAllRanges();
                    selection.addRange(range);
                })();",
                null
            );
        }

        public virtual void SendKey(string key) {

            _assertValidState();
            WebView_sendKey(_nativeWebViewPtr, key);
        }

        public static void SetCameraAndMicrophoneEnabled(bool enabled) => WebView_setCameraAndMicrophoneEnabled(enabled);

        public virtual void SetFocused(bool focused) {

            _assertValidState();
            WebView_setFocused(_nativeWebViewPtr, focused);
        }

        public virtual void SetRenderingEnabled(bool enabled) {

            _assertValidState();
            WebView_setRenderingEnabled(_nativeWebViewPtr, enabled);
            _renderingEnabled = enabled;
            if (enabled && _currentNativeTexture != IntPtr.Zero) {
                Texture.UpdateExternalTexture(_currentNativeTexture);
            }
        }

        public virtual void StopLoad() {

            _assertValidState();
            WebView_stopLoad(_nativeWebViewPtr);
        }

        public Task WaitForNextPageLoadToFinish() {

            if (_pageLoadFinishedTaskSource == null) {
                _pageLoadFinishedTaskSource = new TaskCompletionSource<bool>();
            }
            return _pageLoadFinishedTaskSource.Task;
        }

        public virtual void ZoomIn() {

            _assertValidState();
            WebView_zoomIn(_nativeWebViewPtr);
        }

        public virtual void ZoomOut() {

            _assertValidState();
            WebView_zoomOut(_nativeWebViewPtr);
        }

    #region Non-public members
        protected enum InitState {
            Uninitialized,
            InProgress,
            Initialized
        }

        EventHandler<ConsoleMessageEventArgs> _consoleMessageLogged;
        protected IntPtr _currentNativeTexture;

    #if (UNITY_STANDALONE_WIN && !UNITY_EDITOR) || UNITY_EDITOR_WIN
        protected const string _dllName = "VuplexWebViewWindows";
    #elif (UNITY_STANDALONE_OSX && !UNITY_EDITOR) || UNITY_EDITOR_OSX
        protected const string _dllName = "VuplexWebViewMac";
    #elif UNITY_WSA
        protected const string _dllName = "VuplexWebViewUwp";
    #elif UNITY_ANDROID
        protected const string _dllName = "VuplexWebViewAndroid";
    #else
        protected const string _dllName = "__Internal";
    #endif

        EventHandler<FocusedInputFieldChangedEventArgs> _focusedInputFieldChanged;
        protected InitState _initState = InitState.Uninitialized;
        protected TaskCompletionSource<bool> _initTaskSource;
        Material _materialForBlitting;
        protected Vector2Int _native2DPosition; // Used for Native 2D Mode.
        protected IntPtr _nativeWebViewPtr;
        TaskCompletionSource<bool> _pageLoadFinishedTaskSource;
        List<Action<bool>> _pendingCanGoBackCallbacks = new List<Action<bool>>();
        List<Action<bool>> _pendingCanGoForwardCallbacks = new List<Action<bool>>();
        protected Dictionary<string, Action<string>> _pendingJavaScriptResultCallbacks = new Dictionary<string, Action<string>>();
        protected bool _renderingEnabled = true;
        // Used for Native 2D Mode. Use Size as the single source of truth for the size
        // to ensure that both Size and Rect stay in sync when Resize() or SetRect() is called.
        protected Rect _rect {
            get { return new Rect(_native2DPosition, Size); }
            set {
                Size = new Vector2Int((int)value.width, (int)value.height);
                _native2DPosition = new Vector2Int((int)value.x, (int)value.y);
            }
        }
        static readonly Regex _streamingAssetsUrlRegex = new Regex(@"^streaming-assets:(//)?(.*)$", RegexOptions.IgnoreCase);

        protected void _assertPointIsWithinBounds(int xInPixels, int yInPixels) {

            var isValid = xInPixels >= 0 && xInPixels <= Size.x && yInPixels >= 0 && yInPixels <= Size.y;
            if (!isValid) {
                throw new ArgumentException($"The point provided ({xInPixels}px, {yInPixels}px) is not within the bounds of the webview (width: {Size.x}px, height: {Size.y}px).");
            }
        }

        protected void _assertSingletonEventHandlerUnset(object handler, string eventName) {

            if (handler != null) {
                throw new InvalidOperationException(eventName + " supports only one event handler. Please remove the existing handler before adding a new one.");
            }
        }

        void _assertValidSize(int width, int height) {

            if (!(width > 0 && height > 0)) {
                throw new ArgumentException($"Invalid size: ({width}, {height}). The width and height must both be greater than 0.");
            }
        }

        protected void _assertValidState() {

            if (!IsInitialized) {
                throw new InvalidOperationException("Methods cannot be called on an uninitialized webview. Prior to calling the webview's methods, please initialize it first by calling IWebView.Init() and awaiting the Task it returns.");
            }
            if (IsDisposed) {
                throw new InvalidOperationException("Methods cannot be called on a disposed webview.");
            }
        }

        protected Vector2Int _convertNormalizedToPixels(Vector2 normalizedPoint, bool assertBetweenZeroAndOne = true) {

            if (assertBetweenZeroAndOne) {
                var isValid = normalizedPoint.x >= 0f && normalizedPoint.x <= 1f && normalizedPoint.y >= 0f && normalizedPoint.y <= 1f;
                if (!isValid) {
                    throw new ArgumentException($"The normalized point provided is invalid. The x and y values of normalized points must be in the range of [0, 1], but the value provided was {normalizedPoint.ToString("n4")}. For more info, please see https://support.vuplex.com/articles/normalized-points");
                }
            }
            return new Vector2Int((int)(normalizedPoint.x * Size.x), (int)(normalizedPoint.y * Size.y));
        }

        protected virtual Task<Texture2D> _createTexture(int width, int height) {

            VXUtils.ThrowExceptionIfAbnormallyLarge(width, height);
            var texture = new Texture2D(
                width,
                height,
                TextureFormat.RGBA32,
                false,
                false
            );
            #if UNITY_2020_2_OR_NEWER
                // In Unity 2020.2, Unity's internal TexturesD3D11.cpp class on Windows logs an error if
                // UpdateExternalTexture() is called on a Texture2D created from the constructor
                // rather than from Texture2D.CreateExternalTexture(). So, rather than returning
                // the original Texture2D created via the constructor, we return a copy created
                // via CreateExternalTexture(). This approach is only used for 2020.2 and newer because
                // it doesn't work in 2018.4 and instead causes a crash.
                texture = Texture2D.CreateExternalTexture(
                    width,
                    height,
                    TextureFormat.RGBA32,
                    false,
                    false,
                    texture.GetNativeTexturePtr()
                );
            #endif
            return Task.FromResult(texture);
        }

        protected virtual void _destroyNativeTexture(IntPtr nativeTexture) {

            WebView_destroyTexture(nativeTexture, SystemInfo.graphicsDeviceType.ToString());
        }

        Texture2D _getReadableTexture() {

            // https://support.unity3d.com/hc/en-us/articles/206486626-How-can-I-get-pixels-from-unreadable-textures-
            RenderTexture tempRenderTexture = RenderTexture.GetTemporary(
                Size.x,
                Size.y,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear
            );
            RenderTexture previousRenderTexture = RenderTexture.active;
            RenderTexture.active = tempRenderTexture;
            // Explicitly clear the temporary render texture, otherwise it can contain
            // existing content that won't be overwritten by transparent pixels.
            GL.Clear(true, true, Color.clear);
            // Use the version of Graphics.Blit() that accepts a material
            // so that any transformations needed are performed with the shader.
            if (_materialForBlitting == null) {
                _materialForBlitting = VXUtils.CreateDefaultMaterial();
            }
            Graphics.Blit(Texture, tempRenderTexture, _materialForBlitting);
            Texture2D readableTexture = new Texture2D(Size.x, Size.y);
            readableTexture.ReadPixels(new Rect(0, 0, tempRenderTexture.width, tempRenderTexture.height), 0, 0);
            readableTexture.Apply();
            RenderTexture.active = previousRenderTexture;
            RenderTexture.ReleaseTemporary(tempRenderTexture);
            return readableTexture;
        }

        Task<string> _getSelectedText() {

            // window.getSelection() doesn't work on the content of <textarea> and <input> elements in
            // Gecko and legacy Edge.
            // https://developer.mozilla.org/en-US/docs/Web/API/Window/getSelection#Related_objects
            return ExecuteJavaScript(
                @"var element = document.activeElement;
                if (element instanceof HTMLInputElement || element instanceof HTMLTextAreaElement) {
                    element.value.substring(element.selectionStart, element.selectionEnd);
                } else {
                    window.getSelection().toString();
                }"
            );
        }

        // Invoked by the native plugin.
        void HandleCanGoBackResult(string message) {

            var result = Boolean.Parse(message);
            var callbacks = new List<Action<bool>>(_pendingCanGoBackCallbacks);
            _pendingCanGoBackCallbacks.Clear();
            foreach (var callback in callbacks) {
                try {
                    callback(result);
                } catch (Exception e) {
                    WebViewLogger.LogError("An exception occurred while calling the callback for CanGoBack: " + e);
                }
            }
        }

        // Invoked by the native plugin.
        void HandleCanGoForwardResult(string message) {

            var result = Boolean.Parse(message);
            var callBacks = new List<Action<bool>>(_pendingCanGoForwardCallbacks);
            _pendingCanGoForwardCallbacks.Clear();
            foreach (var callBack in callBacks) {
                try {
                    callBack(result);
                } catch (Exception e) {
                    WebViewLogger.LogError("An exception occurred while calling the callForward for CanGoForward: " + e);
                }
            }
        }

        // Invoked by the native plugin.
        void HandleCloseRequested(string message) => CloseRequested?.Invoke(this, EventArgs.Empty);

        // Invoked by the native plugin.
        void HandleInitFinished(string unusedParam) {

            _initState = InitState.Initialized;
            _initTaskSource.SetResult(true);
            _initTaskSource = null;
        }

        // Invoked by the native plugin.
        void HandleJavaScriptResult(string message) {

            var components = message.Split(new char[] { ',' }, 2);
            var resultCallbackId = components[0];
            var result = components[1];
            _handleJavaScriptResult(resultCallbackId, result);
        }

        void _handleJavaScriptResult(string resultCallbackId, string result) {

            var callback = _pendingJavaScriptResultCallbacks[resultCallbackId];
            _pendingJavaScriptResultCallbacks.Remove(resultCallbackId);
            callback(result);
        }

        // Invoked by the native plugin.
        void HandleLoadFailed(string unusedParam) {

            PageLoadFailed?.Invoke(this, EventArgs.Empty);
            OnLoadProgressChanged(new ProgressChangedEventArgs(ProgressChangeType.Failed, 1.0f));
            _pageLoadFinishedTaskSource?.SetException(new PageLoadFailedException("The current web page failed to load."));
            _pageLoadFinishedTaskSource = null;
            if (PageLoadFailed == null && LoadProgressChanged == null) {
                // No handlers are attached to PageLoadFailed or LoadProgressChanged,
                // so log a warning about the page load failure.
                WebViewLogger.LogWarning("A web page failed to load. This can happen if the URL loaded is invalid or if the device has no network connection. To detect and handle page load failures like this, applications can use the IWebView.LoadProgressChanged event or the IWebView.PageLoadFailed event.");
            }
        }

        // Invoked by the native plugin.
        void HandleLoadFinished(string unusedParam) {

            OnLoadProgressChanged(new ProgressChangedEventArgs(ProgressChangeType.Finished, 1.0f));
            _pageLoadFinishedTaskSource?.SetResult(true);
            _pageLoadFinishedTaskSource = null;
            foreach (var script in PageLoadScripts) {
                ExecuteJavaScript(script, null);
            }
        }

        // Invoked by the native plugin.
        void HandleLoadStarted(string unusedParam) {

            OnLoadProgressChanged(new ProgressChangedEventArgs(ProgressChangeType.Started, 0.0f));
        }

        // Invoked by the native plugin.
        void HandleLoadProgressUpdate(string progressString) {

            var progress = float.Parse(progressString, CultureInfo.InvariantCulture);
            OnLoadProgressChanged(new ProgressChangedEventArgs(ProgressChangeType.Updated, progress));
        }

        // Invoked by the native plugin.
        protected virtual void HandleMessageEmitted(string serializedMessage) {

            // For performance, only try to deserialize the message if it's one we're listening for.
            var messageType = serializedMessage.Contains("vuplex.webview") ? BridgeMessage.ParseType(serializedMessage) : null;
            switch (messageType) {
                case "vuplex.webview.consoleMessageLogged": {
                    var consoleMessage = JsonUtility.FromJson<ConsoleBridgeMessage>(serializedMessage);
                    _consoleMessageLogged?.Invoke(this, consoleMessage.ToEventArgs());
                    break;
                }
                case "vuplex.webview.focusedInputFieldChanged": {
                    var typeString = StringBridgeMessage.ParseValue(serializedMessage);
                    var type = FocusedInputFieldChangedEventArgs.ParseType(typeString);
                    _focusedInputFieldChanged?.Invoke(this, new FocusedInputFieldChangedEventArgs(type));
                    break;
                }
                case "vuplex.webview.javaScriptResult": {
                    var message = JsonUtility.FromJson<StringWithIdBridgeMessage>(serializedMessage);
                    _handleJavaScriptResult(message.id, message.value);
                    break;
                }
                case "vuplex.webview.titleChanged": {
                    Title = StringBridgeMessage.ParseValue(serializedMessage);
                    TitleChanged?.Invoke(this, new EventArgs<string>(Title));
                    break;
                }
                case "vuplex.webview.urlChanged": {
                    var action = JsonUtility.FromJson<UrlChangedMessage>(serializedMessage).urlAction;
                    if (Url == action.Url) {
                        return;
                    }
                    Url = action.Url;
                    UrlChanged?.Invoke(this, new UrlChangedEventArgs(action.Url, action.Type));
                    break;
                }
                default: {
                    MessageEmitted?.Invoke(this, new EventArgs<string>(serializedMessage));
                    break;
                }
            }
        }

        // Invoked by the native plugin.
        virtual protected void HandleTextureChanged(string textureString) {

            // Use UInt64.Parse() because Int64.Parse() can result in an OverflowException.
            var nativeTexture = new IntPtr((Int64)UInt64.Parse(textureString));
            if (nativeTexture == _currentNativeTexture) {
                return;
            }
            var previousNativeTexture = _currentNativeTexture;
            _currentNativeTexture = nativeTexture;
            if (_renderingEnabled) {
                Texture.UpdateExternalTexture(nativeTexture);
            }
            if (previousNativeTexture != IntPtr.Zero) {
                _destroyNativeTexture(previousNativeTexture);
            }
        }

        protected async Task _initBase(int width, int height, bool createTexture = true, bool asyncInit = false) {

            if (_initState != InitState.Uninitialized) {
                var message = _initState == InitState.Initialized ? "Init() cannot be called on a webview that has already been initialized."
                                                                  : "Init() cannot be called on a webview that is already in the process of initialization.";
                throw new InvalidOperationException(message);
            }
            _assertValidSize(width, height);
            // Assign the game object a unique name so that the native view can send it messages.
            gameObject.name = "WebView-" + Guid.NewGuid().ToString();
            Size = new Vector2Int(width, height);
            VXUtils.ThrowExceptionIfAbnormallyLarge(width, height);
            // Prevent the script from automatically being destroyed when a new scene is loaded.
            DontDestroyOnLoad(gameObject);
            if (createTexture) {
                Texture = await _createTexture(width, height);
            }
            if (asyncInit) {
                _initState = InitState.InProgress;
                _initTaskSource = new TaskCompletionSource<bool>();
            } else {
                _initState = InitState.Initialized;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void _logDeprecationErrorIfNeeded() {

            #if !(NET_4_6 || NET_STANDARD_2_0)
                WebViewLogger.LogError("Support for the legacy .NET 3.5 runtime was removed in 3D WebView v4.0. Please switch to the .NET 4.x runtime.");
            #endif
        }

        protected virtual void OnLoadProgressChanged(ProgressChangedEventArgs eventArgs) => LoadProgressChanged?.Invoke(this, eventArgs);

        protected ConsoleMessageLevel _parseConsoleMessageLevel(string levelString) {

            switch (levelString) {
                case "DEBUG":
                    return ConsoleMessageLevel.Debug;
                case "ERROR":
                    return ConsoleMessageLevel.Error;
                case "LOG":
                    return ConsoleMessageLevel.Log;
                case "WARNING":
                    return ConsoleMessageLevel.Warning;
                default:
                    WebViewLogger.LogWarning("Unrecognized console message level: " + levelString);
                    return ConsoleMessageLevel.Log;
            }
        }

        protected virtual void _resize() => WebView_resize(_nativeWebViewPtr, Size.x, Size.y);

        protected virtual void _setConsoleMessageEventsEnabled(bool enabled) {

            _assertValidState();
            WebView_setConsoleMessageEventsEnabled(_nativeWebViewPtr, enabled);
        }

        protected virtual void _setFocusedInputFieldEventsEnabled(bool enabled) {

            _assertValidState();
            WebView_setFocusedInputFieldEventsEnabled(_nativeWebViewPtr, enabled);
        }

        protected string _transformUrlIfNeeded(string originalUrl) {

            if (originalUrl == null) {
                throw new ArgumentException("URL cannot be null.");
            }
            // Default to https:// if no protocol is specified.
            if (!originalUrl.Contains(":")) {
                if (!originalUrl.Contains(".")) {
                    // The URL doesn't include a valid domain, so throw instead of defaulting to https://.
                    throw new ArgumentException("Invalid URL: " + originalUrl);
                }
                var updatedUrl = "https://" + originalUrl;
                WebViewLogger.LogWarning($"The provided URL is missing a protocol (e.g. http://, https://), so it will default to https://. Original URL: {originalUrl}, Updated URL: {updatedUrl}");
                return updatedUrl;
            }
            // If a streaming-assets:// URL was specified, so transform it to a URL that the browser can load.
            var streamingAssetsRegexMatch = _streamingAssetsUrlRegex.Match(originalUrl);
            if (streamingAssetsRegexMatch.Success) {
                var urlPath = streamingAssetsRegexMatch.Groups[2].Captures[0].Value;
                // If Application.streamingAssetsPath doesn't already contain a URL protocol, then add
                // the file:// protocol. It already has a protocol in the case of WebGL (http(s)://)
                // and Android (jar:file://).
                var urlProtocolToAdd = Application.streamingAssetsPath.Contains("://") ? "" : "file://";
                // Spaces in URLs must be escaped
                var streamingAssetsUrl = urlProtocolToAdd + Path.Combine(Application.streamingAssetsPath, urlPath).Replace(" ", "%20");
                return streamingAssetsUrl;
            }
            return originalUrl;
        }

        [DllImport(_dllName)]
        static extern void WebView_canGoBack(IntPtr webViewPtr);

        [DllImport(_dllName)]
        static extern void WebView_canGoForward(IntPtr webViewPtr);

        [DllImport(_dllName)]
        protected static extern void WebView_click(IntPtr webViewPtr, int x, int y);

        [DllImport(_dllName)]
        protected static extern void WebView_destroyTexture(IntPtr texture, string graphicsApi);

        [DllImport(_dllName)]
        static extern void WebView_destroy(IntPtr webViewPtr);

        [DllImport(_dllName)]
        static extern void WebView_executeJavaScript(IntPtr webViewPtr, string javaScript, string resultCallbackId);

        [DllImport(_dllName)]
        static extern void WebView_goBack(IntPtr webViewPtr);

        [DllImport(_dllName)]
        static extern void WebView_goForward(IntPtr webViewPtr);

        [DllImport(_dllName)]
        static extern void WebView_sendKey(IntPtr webViewPtr, string input);

        [DllImport(_dllName)]
        static extern void WebView_loadHtml(IntPtr webViewPtr, string html);

        [DllImport(_dllName)]
        static extern void WebView_loadUrl(IntPtr webViewPtr, string url);

        [DllImport(_dllName)]
        static extern void WebView_loadUrlWithHeaders(IntPtr webViewPtr, string url, string newlineDelimitedHttpHeaders);

        [DllImport(_dllName)]
        static extern void WebView_reload(IntPtr webViewPtr);

        [DllImport(_dllName)]
        protected static extern void WebView_resize(IntPtr webViewPtr, int width, int height);

        [DllImport(_dllName)]
        static extern void WebView_scroll(IntPtr webViewPtr, int deltaX, int deltaY);

        [DllImport(_dllName)]
        static extern void WebView_scrollAtPoint(IntPtr webViewPtr, int deltaX, int deltaY, int pointerX, int pointerY);

        [DllImport(_dllName)]
        static extern void WebView_setCameraAndMicrophoneEnabled(bool enabled);

        [DllImport(_dllName)]
        static extern void WebView_setConsoleMessageEventsEnabled(IntPtr webViewPtr, bool enabled);

        [DllImport(_dllName)]
        static extern void WebView_setFocused(IntPtr webViewPtr, bool focused);

        [DllImport(_dllName)]
        static extern void WebView_setFocusedInputFieldEventsEnabled(IntPtr webViewPtr, bool enabled);

        [DllImport(_dllName)]
        static extern void WebView_setRenderingEnabled(IntPtr webViewPtr, bool enabled);

        [DllImport(_dllName)]
        static extern void WebView_stopLoad(IntPtr webViewPtr);

        [DllImport(_dllName)]
        static extern void WebView_zoomIn(IntPtr webViewPtr);

        [DllImport(_dllName)]
        static extern void WebView_zoomOut(IntPtr webViewPtr);
    #endregion

    #region Obsolete APIs
        [Obsolete(ObsoletionMessages.Blur, true)]
        public void Blur() {}

        [Obsolete(ObsoletionMessages.CanGoBack, true)]
        public void CanGoBack(Action<bool> callback) {}

        [Obsolete(ObsoletionMessages.CanGoForward, true)]
        public void CanGoForward(Action<bool> callback) {}

        [Obsolete(ObsoletionMessages.CaptureScreenshot, true)]
        public void CaptureScreenshot(Action<byte[]> callback) {}

        [Obsolete(ObsoletionMessages.DisableViewUpdates, true)]
        public void DisableViewUpdates() {}

        [Obsolete(ObsoletionMessages.EnableViewUpdates, true)]
        public void EnableViewUpdates() {}

        [Obsolete(ObsoletionMessages.Focus, true)]
        public void Focus() {}

        [Obsolete(ObsoletionMessages.GetRawTextureData, true)]
        public void GetRawTextureData(Action<byte[]> callback) {}

        [Obsolete(ObsoletionMessages.HandleKeyboardInput)]
        public void HandleKeyboardInput(string key) => SendKey(key);

        [Obsolete(ObsoletionMessages.Init, true)]
        public void Init(Texture2D texture, float width, float height) {}

        [Obsolete(ObsoletionMessages.Init2, true)]
        public void Init(Texture2D texture, float width, float height, Texture2D videoTexture) {}

        [Obsolete(ObsoletionMessages.Resolution, true)]
        public float Resolution { get; }

        [Obsolete(ObsoletionMessages.SetResolution, true)]
        public void SetResolution(float pixelsPerUnityUnit) {}

        [Obsolete(ObsoletionMessages.SizeInPixels)]
        public Vector2 SizeInPixels { get { return (Vector2)Size; }}

        #pragma warning disable CS0067
        [Obsolete(ObsoletionMessages.VideoRectChanged, true)]
        public event EventHandler<EventArgs<Rect>> VideoRectChanged;

        [Obsolete(ObsoletionMessages.VideoTexture, true)]
        public Texture2D VideoTexture { get; }
    #endregion
    }
}
#endif
