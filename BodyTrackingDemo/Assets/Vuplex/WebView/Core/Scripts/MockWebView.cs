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
#pragma warning disable CS0067
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Vuplex.WebView.Internal;

namespace Vuplex.WebView {

    /// <summary>
    /// Mock IWebView implementation used for running in the Unity editor.
    /// </summary>
    /// <remarks>
    /// MockWebView logs messages to the console to indicate when its methods are
    /// called, but it doesn't actually load or render web content.
    /// </remarks>
    partial class MockWebView : MonoBehaviour, IWebView {

        public event EventHandler CloseRequested;

        public event EventHandler<ConsoleMessageEventArgs> ConsoleMessageLogged;

        public event EventHandler<FocusedInputFieldChangedEventArgs> FocusedInputFieldChanged;

        public event EventHandler<ProgressChangedEventArgs> LoadProgressChanged;

        public event EventHandler<EventArgs<string>> MessageEmitted;

        public event EventHandler PageLoadFailed;

        public event EventHandler<EventArgs<string>> TitleChanged;

        public event EventHandler<UrlChangedEventArgs> UrlChanged;

        public bool IsDisposed { get; private set; }

        public bool IsInitialized { get; private set; }

        public List<string> PageLoadScripts { get; } = new List<string>();

        public WebPluginType PluginType { get; } = WebPluginType.Mock;

        public Vector2Int Size { get; private set; }

        public Texture2D Texture { get; private set; }

        public string Title { get; private set; } = "";

        public string Url { get; private set; } = "";

        public Task<bool> CanGoBack() {

            _log("CanGoBack()");
            OnCanGoBack();
            return Task.FromResult(false);
        }

        public Task<bool> CanGoForward() {

            _log("CanGoForward()");
            OnCanGoForward();
            return Task.FromResult(false);
        }

        public Task<byte[]> CaptureScreenshot() {

            _log("CaptureScreenshot()");
            OnCaptureScreenshot();
            return Task.FromResult(new byte[0]);
        }

        public void Click(int xInPixels, int yInPixels, bool preventStealingFocus = false) {

            var pointIsValid = xInPixels >= 0 && xInPixels <= Size.x && yInPixels >= 0 && yInPixels <= Size.y;
            if (!pointIsValid) {
                throw new ArgumentException($"The point provided ({xInPixels}px, {yInPixels}px) is not within the bounds of the webview (width: {Size.x}px, height: {Size.y}px).");
            }
            _log($"Click({xInPixels}, {yInPixels}, {preventStealingFocus})");
        }

        public void Click(Vector2 point, bool preventStealingFocus = false) {

            _assertValidNormalizedPoint(point);
            _log($"Click({point.ToString("n4")}, {preventStealingFocus})");
        }

        public void Copy() {

            _log("Copy()");
            OnCopy();
        }

        public Material CreateMaterial() {
            #if UNITY_SERVER
                return null;
            #else
                var material = new Material(Resources.Load<Material>("MockViewportMaterial"));
                // Create a copy of the texture so that an Exception won't be thrown when the prefab destroys it.
                // Also, explicitly use RGBA32 here so that the texture will be converted to RGBA32 if the editor
                // imported it as a different format. For example, when Texture Compression is set to ASTC in Android build settings,
                // the editor automatically imports new textures as ASTC, even though the Windows editor doesn't support that format.
                var texture = new Texture2D(material.mainTexture.width, material.mainTexture.height, TextureFormat.RGBA32, true);
                texture.SetPixels((material.mainTexture as Texture2D).GetPixels());
                texture.Apply();
                material.mainTexture = texture;
                return material;
            #endif
        }

        public void Cut() {

            _log("Cut()");
            OnCut();
        }

        public static Task<bool> DeleteCookies(string url, string cookieName = null) {

            if (url == null) {
                throw new ArgumentException("The url cannot be null.");
            }
            _log($"DeleteCookies(\"{url}\", \"{cookieName}\")");
            return Task.FromResult(true);
        }

        public void Dispose() {

            IsDisposed = true;
            _log("Dispose()");
            if (this != null) {
                Destroy(gameObject);
            }
        }

        public Task<string> ExecuteJavaScript(string javaScript) {

            var taskSource = new TaskCompletionSource<string>();
            ExecuteJavaScript(javaScript, taskSource.SetResult);
            return taskSource.Task;
        }

        public void ExecuteJavaScript(string javaScript, Action<string> callback) {

            _log($"ExecuteJavaScript(\"{_truncateIfNeeded(javaScript)}\")");
            callback("");
            OnExecuteJavaScript();
        }

        public static Task<Cookie[]> GetCookies(string url, string cookieName = null) {

            if (url == null) {
                throw new ArgumentException("The url cannot be null.");
            }
            _log($"GetCookies(\"{url}\", \"{cookieName}\")");
            var taskSource = new TaskCompletionSource<Cookie[]>();
            return Task.FromResult(new Cookie[0]);
        }

        public Task<byte[]> GetRawTextureData() {

            _log("GetRawTextureData()");
            OnGetRawTextureData();
            return Task.FromResult(new byte[0]);
        }

        public void GoBack() {

            _log("GoBack()");
            OnGoBack();
        }

        public void GoForward() {

            _log("GoForward()");
            OnGoForward();
        }

        public Task Init(int width, int height) {

            Texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Size = new Vector2Int(width, height);
            IsInitialized = true;
            DontDestroyOnLoad(gameObject);
            _log($"Init() width: {width.ToString("n4")}, height: {height.ToString("n4")}");
            return Task.FromResult(true);
        }

        public static MockWebView Instantiate() => new GameObject("MockWebView").AddComponent<MockWebView>();

        public virtual void LoadHtml(string html) {

            var truncatedHtml = _truncateIfNeeded(html);
            Url = truncatedHtml;
            _log($"LoadHtml(\"{truncatedHtml}...\")");
            OnLoadHtml();
            _handlePageLoad(truncatedHtml);
        }

        public virtual void LoadUrl(string url) => LoadUrl(url, null);

        public virtual void LoadUrl(string url, Dictionary<string, string> additionalHttpHeaders) {

            Url = url;
            _log($"LoadUrl(\"{url}\")");
            OnLoadUrl(url);
            _handlePageLoad(url);
        }

        public Vector2Int NormalizedToPoint(Vector2 normalizedPoint) {

            return new Vector2Int((int)(normalizedPoint.x * (float)Size.x), (int)(normalizedPoint.y * (float)Size.y));
        }

        public void Paste() {

            _log("Paste()");
            OnPaste();
        }

        public Vector2 PointToNormalized(int xInPixels, int yInPixels) {

            return new Vector2((float)xInPixels / Size.x, (float)yInPixels / Size.y);
        }

        public void PostMessage(string data) => _log($"PostMessage(\"{data}\")");

        public void Reload() => _log("Reload()");

        public void Resize(int width, int height) {

            Size = new Vector2Int(width, height);
            _log($"Resize({width.ToString("n4")}, {height.ToString("n4")})");
        }

        public void Scroll(int scrollDeltaX, int scrollDeltaY) => _log($"Scroll({scrollDeltaX}, {scrollDeltaY})");

        public void Scroll(Vector2 delta) => _log($"Scroll({delta.ToString("n4")})");

        public void Scroll(Vector2 delta, Vector2 point) {

            _assertValidNormalizedPoint(point);
            _log($"Scroll({delta.ToString("n4")}, {point.ToString("n4")})");
        }

        public void SelectAll() => _log("SelectAll()");

        public void SendKey(string input) => _log($"SendKey(\"{input}\")");

        public static Task<bool> SetCookie(Cookie cookie) {

            if (cookie == null) {
                throw new ArgumentException("Cookie cannot be null.");
            }
            if (!cookie.IsValid) {
                throw new ArgumentException("Cannot set invalid cookie: " + cookie);
            }
            _log($"SetCookie({cookie}");
            return Task.FromResult(true);
        }

        public void SetFocused(bool focused) => _log($"SetFocused({focused})");

        public void SetRenderingEnabled(bool enabled) => _log($"SetRenderingEnabled({enabled})");

        public void StopLoad() => _log("StopLoad()");

        public Task WaitForNextPageLoadToFinish() {

            if (_pageLoadFinishedTaskSource == null) {
                _pageLoadFinishedTaskSource = new TaskCompletionSource<bool>();
            }
            return _pageLoadFinishedTaskSource.Task;
        }

        public void ZoomIn() => _log("ZoomIn()");

        public void ZoomOut() => _log("ZoomOut()");

        TaskCompletionSource<bool> _pageLoadFinishedTaskSource;

        // Partial methods implemented by other 3D WebView packages
        // to provide platform-specific warnings in the editor.
        partial void OnCanGoBack();
        partial void OnCanGoForward();
        partial void OnCaptureScreenshot();
        partial void OnCopy();
        partial void OnCut();
        partial void OnExecuteJavaScript();
        partial void OnGetRawTextureData();
        partial void OnGoBack();
        partial void OnGoForward();
        partial void OnLoadHtml();
        partial void OnLoadUrl(string url);
        partial void OnPaste();

        void _assertValidNormalizedPoint(Vector2 normalizedPoint) {

            var isValid = normalizedPoint.x >= 0f && normalizedPoint.x <= 1f && normalizedPoint.y >= 0f && normalizedPoint.y <= 1f;
            if (!isValid) {
                throw new ArgumentException($"The normalized point provided is invalid. The x and y values of normalized points must be in the range of [0, 1], but the value provided was {normalizedPoint.ToString("n4")}. For more info, please see https://support.vuplex.com/articles/normalized-points");
            }
        }

        void _handlePageLoad(string url) {

            UrlChanged?.Invoke(this, new UrlChangedEventArgs(url, UrlActionType.Load));
            LoadProgressChanged?.Invoke(this, new ProgressChangedEventArgs(ProgressChangeType.Started, 0));
            LoadProgressChanged?.Invoke(this, new ProgressChangedEventArgs(ProgressChangeType.Finished, 1));
            _pageLoadFinishedTaskSource?.SetResult(true);
            _pageLoadFinishedTaskSource = null;
        }

        static void _log(string message) {

            #if !VUPLEX_DISABLE_MOCK_WEBVIEW_LOGGING
                WebViewLogger.Log("[MockWebView] " + message);
            #endif
        }

        string _truncateIfNeeded(string str) {

            var maxLength = 25;
            if (str.Length <= maxLength) {
                return str;
            }
            return str.Substring(0, maxLength) + "...";
        }

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

        [Obsolete(ObsoletionMessages.VideoRectChanged, true)]
        public event EventHandler<EventArgs<Rect>> VideoRectChanged;

        [Obsolete(ObsoletionMessages.VideoTexture, true)]
        public Texture2D VideoTexture { get; }
    }
}
