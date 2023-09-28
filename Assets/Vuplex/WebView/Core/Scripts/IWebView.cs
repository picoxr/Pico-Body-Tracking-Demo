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
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Vuplex.WebView {

    /// <summary>
    /// IWebView is the primary interface for loading and interacting with web content.
    /// It contains methods and properties for common browser-related functionality,
    /// like LoadUrl(), GoBack(), Reload(), and ExecuteJavaScript().
    /// </summary>
    /// <remarks>
    /// <para>
    /// To create an IWebView, instantiate a WebViewPrefab or CanvasWebViewPrefab. After
    /// the prefab is initialized, you can access its IWebView via the WebViewPrefab.WebView property.
    /// If your use case requires a high degree of customization, you can instead create
    /// an IWebView outside of a prefab (to connect to your own custom GameObject) by
    /// using Web.CreateWebView().
    /// </para>
    /// <para>
    /// For additional functionality, you can cast an IWebView to an interface for a specific
    /// feature, like IWithDownloads or IWithPopups. For a list of additional feature interfaces and
    /// information about how to use them, see this page: https://developer.vuplex.com/webview/additional-interfaces
    /// </para>
    /// See also:
    /// <list type="bullet">
    ///   <item>WebViewPrefab: https://developer.vuplex.com/webview/WebViewPrefab</item>
    ///   <item>CanvasWebViewPrefab: https://developer.vuplex.com/webview/CanvasWebViewPrefab</item>
    ///   <item>Web (static methods): https://developer.vuplex.com/webview/Web</item>
    /// </list>
    /// </remarks>
    public interface IWebView {

        /// <summary>
        /// Indicates that the page has requested to close (i.e. via window.close()).
        /// </summary>
        /// <example>
        /// <code>
        /// await webViewPrefab.WaitUntilInitialized();
        /// webViewPrefab.WebView.CloseRequested += (sender, eventArgs) => {
        ///     Debug.Log("Close requested");
        /// };
        /// </code>
        /// </example>
        event EventHandler CloseRequested;

        /// <summary>
        /// Indicates that a message was logged to the JavaScript console.
        /// </summary>
        /// <remarks>
        /// The 3D WebView packages for Android with Gecko, iOS, and UWP have the following limitations:
        /// <list type="bullet">
        ///   <item>
        ///     Only messages explicitly passed to a console method like console.log() are included,
        ///     and other messages like uncaught errors or network errors aren't automatically included.
        ///   </item>
        ///   <item>Messages from iframes aren't included.</item>
        ///   <item>Messages logged early when the page starts loading may be missed.</item>
        /// </list>
        /// For Android Gecko, an alternative that avoids these limitations is to call
        /// AndroidGeckoWebView.SetConsoleOutputEnabled().
        /// </remarks>
        /// <example>
        /// <code>
        /// await webViewPrefab.WaitUntilInitialized();
        /// webViewPrefab.WebView.ConsoleMessageLogged += (sender, eventArgs) => {
        ///     Debug.Log($"Console message logged: [{eventArgs.Level}] {eventArgs.Message}");
        /// };
        /// </code>
        /// </example>
        /// <seealso cref="WebViewPrefab.LogConsoleMessages"/>
        /// <seealso href="https://support.vuplex.com/articles/how-to-debug-web-content">Remote debugging</seealso>
        event EventHandler<ConsoleMessageEventArgs> ConsoleMessageLogged;

        /// <summary>
        /// Indicates when an input field has been focused or unfocused. This can be used,
        /// for example, to determine when to show or hide an on-screen keyboard.
        /// This event is also raised when a focused input field is clicked subsequent times.
        /// Note that this event is currently only fired for input fields focused in the main frame
        /// and is not fired for input fields in iframes.
        /// </summary>
        /// <example>
        /// <code>
        /// await webViewPrefab.WaitUntilInitialized();
        /// webViewPrefab.WebView.FocusedInputFieldChanged += (sender, eventArgs) => {
        ///     Debug.Log("Focused input field changed. Text input is focused: " + eventArgs.Type == FocusedInputFieldType.Text);
        /// };
        /// </code>
        /// </example>
        event EventHandler<FocusedInputFieldChangedEventArgs> FocusedInputFieldChanged;

        /// <summary>
        /// Indicates that the page load progress changed.
        /// </summary>
        /// <remarks>
        /// For 2D WebView for WebGL, LoadProgressChanged only indicates the ProgressChangeType.Started and Finished events,
        /// and it's unable to indicate the Failed or Updated events.
        /// </remarks>
        /// <example>
        /// <code>
        /// await webViewPrefab.WaitUntilInitialized();
        /// webViewPrefab.WebView.LoadProgressChanged += (sender, eventArgs) => {
        ///     Debug.Log($"Load progress changed: {eventArgs.Type}, {eventArgs.Progress}");
        ///     if (eventArgs.Type == ProgressChangeType.Finished) {
        ///         Debug.Log("The page finished loading");
        ///     }
        /// };
        /// </code>
        /// </example>
        /// <seealso cref="WaitForNextPageLoadToFinish"/>
        event EventHandler<ProgressChangedEventArgs> LoadProgressChanged;

        /// <summary>
        /// Indicates that JavaScript running in the page used the `window.vuplex.postMessage`
        /// JavaScript API to emit a message to the Unity application. For more details, please see
        /// [this support article](https://support.vuplex.com/articles/how-to-send-messages-from-javascript-to-c-sharp).
        /// </summary>
        /// <example>
        /// <code>
        /// await webViewPrefab.WaitUntilInitialized();
        /// // Add JavaScript to the page that sends a message.
        /// webViewPrefab.WebView.PageLoadScripts.Add(@"
        ///     window.vuplex.postMessage('Hello from JavaScript!');
        /// ");
        /// webViewPrefab.WebView.MessageEmitted += (sender, eventArgs) => {
        ///     Debug.Log("Message received from JavaScript: " + eventArgs.Value);
        /// };
        /// </code>
        /// </example>
        /// <seealso cref="ExecuteJavaScript"/>
        /// <seealso cref="PageLoadScripts"/>
        event EventHandler<EventArgs<string>> MessageEmitted;

        /// <summary>
        /// Indicates that the page failed to load. This can happen, for instance,
        /// if DNS is unable to resolve the hostname.
        /// </summary>
        /// <example>
        /// <code>
        /// await webViewPrefab.WaitUntilInitialized();
        /// webViewPrefab.WebView.PageLoadFailed += (sender, eventArgs) => {
        ///     Debug.Log("Page load failed");
        /// };
        /// </code>
        /// </example>
        event EventHandler PageLoadFailed;

        /// <summary>
        /// Indicates that the page's title changed.
        /// </summary>
        /// <example>
        /// <code>
        /// await webViewPrefab.WaitUntilInitialized();
        /// webViewPrefab.WebView.TitleChanged += (sender, eventArgs) => {
        ///     Debug.Log("Page title changed: " + eventArgs.Value);
        /// };
        /// </code>
        /// </example>
        event EventHandler<EventArgs<string>> TitleChanged;

        /// <summary>
        /// Indicates that the URL of the webview changed, either
        /// due to user interaction or JavaScript.
        /// </summary>
        /// <example>
        /// <code>
        /// await webViewPrefab.WaitUntilInitialized();
        /// webViewPrefab.WebView.UrlChanged += (sender, eventArgs) => {
        ///     Debug.Log("URL changed: " + eventArgs.Url);
        /// };
        /// </code>
        /// </example>
        /// <seealso cref="Url"/>
        event EventHandler<UrlChangedEventArgs> UrlChanged;

        /// <summary>
        /// Gets a value indicating whether the instance has been disposed via Dispose().
        /// </summary>
        bool IsDisposed { get; }

        /// <summary>
        /// Gets a value indicating whether the instance has been initialized via Init().
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Gets a list of JavaScript scripts that are automatically executed in every new page that is loaded.
        /// </summary>
        /// <remarks>
        /// This list is empty by default, but the application can add scripts. When used in conjunction
        /// with 3D WebView's [message passing API](https://support.vuplex.com/articles/how-to-send-messages-from-javascript-to-c-sharp),
        /// it's possible to modify the browser's behavior in significant ways, similar to creating browser extensions.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Add a script that automatically hides all scrollbars.
        /// await webViewPrefab.WaitUntilInitialized();
        /// webViewPrefab.WebView.PageLoadScripts.Add(@"
        ///     var styleElement = document.createElement('style');
        ///     styleElement.innerText = 'body::-webkit-scrollbar { display: none; }';
        ///     document.head.appendChild(styleElement);
        /// ");
        /// </code>
        /// </example>
        /// <seealso cref="ExecuteJavaScript"/>
        /// <seealso href="https://support.vuplex.com/articles/how-to-send-messages-from-javascript-to-c-sharp">JS-to-C# message passing</seealso>
        List<string> PageLoadScripts { get; }

        /// <summary>
        /// Gets the instance's plugin type.
        /// </summary>
        /// <example>
        /// <code>
        /// await webViewPrefab.WaitUntilInitialized();
        /// Debug.Log("Plugin type: " + webViewPrefab.WebView.PluginType);
        /// </code>
        /// </example>
        WebPluginType PluginType { get; }

        /// <summary>
        /// Gets the webview's size in pixels.
        /// </summary>
        /// <example>
        /// <code>
        /// await webViewPrefab.WaitUntilInitialized();
        /// Debug.Log("Size: " + webViewPrefab.WebView.Size);
        /// </code>
        /// </example>
        /// <seealso cref="Resize"/>
        /// <seealso cref="WebViewPrefab.Resolution"/>
        Vector2Int Size { get; }

        /// <summary>
        /// Gets the texture for the webview's web content, or `null` if running in
        /// Native 2D Mode. In order to render the texture, the application must use
        /// a Material created with CreateMaterial().
        /// </summary>
        /// <remarks>
        /// <para>
        /// This texture is an "external texture" created with
        /// Texture2D.CreateExternalTexture(). An undocumented characteristic
        /// of external textures in Unity is that not all Texture2D methods work for them.
        /// For example, Texture2D.GetRawTextureData() and ImageConversion.EncodeToPNG()
        /// fail for external textures. To compensate, the IWebView interface includes
        /// its own GetRawTextureData() and CaptureScreenshot() methods to replace them.
        /// </para>
        /// <para>
        /// Another quirk of this texture is that Unity always reports its size as
        /// 1300px × 1300px in the editor. In reality, 3D WebView resizes the
        /// texture in native code to match the dimensions of the webview, but
        /// Unity doesn't provide an API to notify the engine that an external texture's size
        /// has changed. So, Unity always reports its size as the initial size that was
        /// passed to Texture2D.CreateExternalTexture(), which in 3D WebView's case is
        /// 1300px × 1300px.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// await webViewPrefab.WaitUntilInitialized();
        /// var material = webViewPrefab.WebView.CreateMaterial();
        /// // Note: the material returned by CreateMaterial() already
        /// // has its mainTexture set to IWebView.Texture, so setting
        /// // it explicitly like this is really only needed if you are
        /// // switching a material from one webview's texture to another.
        /// material.mainTexture = webViewPrefab.WebView.Texture;
        /// </code>
        /// </example>
        Texture2D Texture { get; }

        /// <summary>
        /// Gets the current web page title.
        /// </summary>
        /// <example>
        /// <code>
        /// // Get the page's title after it finishes loading.
        /// await webViewPrefab.WaitUntilInitialized();
        /// await webViewPrefab.WebView.WaitForNextPageLoadToFinish();
        /// Debug.Log("Page title: " + webViewPrefab.WebView.Title);
        /// </code>
        /// </example>
        /// <seealso cref="TitleChanged"/>
        string Title { get; }

        /// <summary>
        /// Gets the current URL.
        /// </summary>
        /// <example>
        /// <code>
        /// // Get the page's URL after it finishes loading.
        /// await webViewPrefab.WaitUntilInitialized();
        /// await webViewPrefab.WebView.WaitForNextPageLoadToFinish();
        /// Debug.Log("Page URL: " + webViewPrefab.WebView.Url);
        /// </code>
        /// </example>
        /// <seealso cref="UrlChanged"/>
        string Url { get; }

        /// <summary>
        /// Checks whether the webview can go back with a call to GoBack().
        /// </summary>
        /// <example>
        /// <c>var canGoBack = await webViewPrefab.CanGoBack();</c>
        /// </example>
        /// <seealso cref="GoBack"/>
        Task<bool> CanGoBack();

        /// <summary>
        /// Checks whether the webview can go forward with a call to GoForward().
        /// </summary>
        /// <example>
        /// <c>var canGoForward = await webViewPrefab.CanGoForward();</c>
        /// </example>
        /// <seealso cref="GoForward"/>
        Task<bool> CanGoForward();

        /// <summary>
        /// Returns a PNG image of the content visible in the webview.
        /// </summary>
        /// <remarks>
        /// On iOS, screenshots do not include video content, which appears black.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Get a screenshot and write it to a file.
        /// var screenshotBytes = await webViewPrefab.WebView.CaptureScreenshot();
        /// var filePath = Path.Combine(Application.peristentDataPath, "screenshot.png");
        /// File.WriteAllBytes(filePath, screenshotBytes);
        /// </code>
        /// </example>
        /// <seealso href="https://docs.unity3d.com/ScriptReference/ImageConversion.LoadImage.html">ImageConversion.LoadImage()</seealso>
        /// <seealso cref="GetRawTextureData"/>
        Task<byte[]> CaptureScreenshot();

        /// <summary>
        /// Clicks at the given coordinates in pixels in the web page, dispatching both a mouse
        /// down and a mouse up event.
        /// </summary>
        /// <example>
        /// <code>
        /// // Click at (250px, 100px).
        /// webViewPrefab.WebView.Click(250, 100);
        ///
        /// // Click at (50px, 150px) and prevent stealing focus from another webview.
        /// webViewPrefab.WebView.Click(50, 150, true);
        /// </code>
        /// </example>
        void Click(int xInPixels, int yInPixels, bool preventStealingFocus = false);

        /// <summary>
        /// Like Click(int, int, bool?), except it takes a normalized point instead of
        /// pixel coordinates.
        /// </summary>
        /// <example>
        /// <code>
        /// // Click in the exact center of the page.
        /// webViewPrefab.WebView.Click(new Vector2(0.5f, 0.5f));
        ///
        /// // Click in the upper right quadrant of the page
        /// // and prevent stealing focus from another webview.
        /// webViewPrefab.WebView.Click(new Vector2(0.75f, 0.25f), true);
        /// </code>
        /// </example>
        void Click(Vector2 normalizedPoint, bool preventStealingFocus = false);

        /// <summary>
        /// Copies the selected text to the clipboard.
        /// </summary>
        /// <example>
        /// <c>webViewPrefab.WebView.Copy();</c>
        /// </example>
        /// <seealso cref="Cut"/>
        /// <seealso cref="Paste"/>
        /// <seealso cref="SelectAll"/>
        void Copy();

        /// <summary>
        /// Creates a Material that can be used to display the webview.
        /// The returned material already has the webview's Texture set as its mainTexture.
        /// </summary>
        /// <remarks>
        /// Note that WebViewPrefab and CanvasWebViewPrefab take care of material creation for you, so you only need
        /// to call this method directly if you need to create an IWebView instance outside of a prefab with
        /// Web.CreateWebView().
        /// </remarks>
        /// <example>
        /// <code>
        /// GetComponent&lt;Renderer&gt;().material = webView.CreateMaterial();
        /// </code>
        /// </example>
        Material CreateMaterial();

        /// <summary>
        /// Copies the selected text to the clipboard and removes it.
        /// </summary>
        /// <example>
        /// <c>webViewPrefab.WebView.Cut();</c>
        /// </example>
        /// <seealso cref="Copy"/>
        /// <seealso cref="Paste"/>
        /// <seealso cref="SelectAll"/>
        void Cut();

        /// <summary>
        /// Destroys the webview, releasing all of its resources.
        /// </summary>
        /// <remarks>
        /// If you're using a WebViewPrefab or CanvasWebViewPrefab, please call Destroy() on the prefab instead
        /// of calling IWebView.Dispose(). Calling IWebView.Dispose() while the prefab is still using the webview
        /// can cause issues.
        /// </remarks>
        void Dispose();

        /// <summary>
        /// Executes the given JavaScript in the context of the page and returns the result.
        /// </summary>
        /// <remarks>
        /// In order to run JavaScript, a web page must first be loaded. You can use WaitForNextPageLoadToFinish() or the
        /// LoadProgressChanged event to run JavaScript after a page loads.
        /// </remarks>
        /// <example>
        /// <code>
        /// await webViewPrefab.WaitUntilInitialized();
        /// await webViewPrefab.WebView.WaitForNextPageLoadToFinish();
        /// var headerText = await webViewPrefab.WebView.ExecuteJavaScript("document.getElementsByTagName('h1')[0].innerText");
        /// Debug.Log("H1 text: " + headerText);
        /// </code>
        /// </example>
        /// <seealso cref="PageLoadScripts"/>
        /// <seealso href="https://support.vuplex.com/articles/how-to-send-messages-from-javascript-to-c-sharp">JS-to-C# message passing</seealso>
        Task<string> ExecuteJavaScript(string javaScript);

        /// <summary>
        /// Like the other version of ExecuteJavaScript(), except it uses a callback instead
        /// of a Task to return the result. If you don't need the result from executing the JavaScript, you can
        /// improve the method's efficiency by passing `null` as the callback argument.
        /// </summary>
        void ExecuteJavaScript(string javaScript, Action<string> callback);

        /// <summary>
        /// A replacement for [Texture2D.GetRawTextureData()](https://docs.unity3d.com/ScriptReference/Texture2D.GetRawTextureData.html)
        /// for IWebView.Texture.
        /// </summary>
        /// <remarks>
        /// Unity's Texture2D.GetRawTextureData() method currently does not work for textures created with
        /// Texture2D.CreateExternalTexture(). So, this method serves as a replacement by providing
        /// the equivalent functionality. You can load the bytes returned by this method into another
        /// texture using [Texture2D.LoadRawTextureData()](https://docs.unity3d.com/ScriptReference/Texture2D.LoadRawTextureData.html).
        /// Note that on iOS, the texture data excludes video content, which appears black.
        /// </remarks>
        /// <example>
        /// <code>
        /// var textureData = await webViewPrefab.WebView.GetRawTextureData();
        /// var texture = new Texture2D(
        ///     webView.Size.x,
        ///     webView.Size.y,
        ///     TextureFormat.RGBA32,
        ///     false,
        ///     false
        /// );
        /// texture.LoadRawTextureData(textureData);
        /// texture.Apply();
        /// </code>
        /// </example>
        /// <seealso href="https://docs.unity3d.com/ScriptReference/Texture2D.LoadRawTextureData.html">Texture2D.GetRawTextureData()</seealso>
        /// <seealso cref="CaptureScreenshot"/>
        Task<byte[]> GetRawTextureData();

        /// <summary>
        /// Navigates back to the previous page in the webview's history.
        /// </summary>
        /// <example>
        /// <c>webViewPrefab.WebView.GoBack();</c>
        /// </example>
        /// <seealso cref="CanGoBack"/>
        void GoBack();

        /// <summary>
        /// Navigates forward to the next page in the webview's history.
        /// </summary>
        /// <example>
        /// <c>webViewPrefab.WebView.GoForward();</c>
        /// </example>
        /// <seealso cref="CanGoForward"/>
        void GoForward();

        /// <summary>
        /// Asynchronously initializes the webview with the given the dimensions in pixels.
        /// </summary>
        /// <remarks>
        /// Note that you don't need to call this method if you're using one of the prefabs, like WebViewPrefab.
        /// You only need to call Init() if you create an IWebView directly with Web.CreateWebView()].
        /// Also, this method's signature was [updated in 3D WebView v4](https://support.vuplex.com/articles/v4-changes#init).
        /// </remarks>
        Task Init(int width, int height);

        /// <summary>
        /// Loads the web page contained in the given HTML string.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// await webViewPrefab.WaitUntilInitialized();
        /// webViewPrefab.WebView.LoadHtml(@"
        ///     <!DOCTYPE html>
        ///     <html>
        ///         <head>
        ///             <title>Test Page</title>
        ///             <style>
        ///                 h1 {
        ///                     font-family: Helvetica, Arial, Sans-Serif;
        ///                 }
        ///             </style>
        ///         </head>
        ///         <body>
        ///             <h1>LoadHtml Example</h1>
        ///             <script>
        ///                 console.log('This page was loaded!');
        ///             </script>
        ///         </body>
        ///     </html>"
        /// );
        /// ]]>
        /// </code>
        /// </example>
        /// <seealso cref="LoadUrl"/>
        void LoadHtml(string html);

        /// <summary>
        /// Loads the given URL. Supported URL schemes:
        /// - `http://`, `https://` - loads a remote page over HTTP
        /// - `streaming-assets://` - loads a local page from StreamingAssets
        ///     (equivalent to `"file://" + Application.streamingAssetsPath + path`)
        /// - `file://` - some platforms support loading arbitrary file URLs
        /// </summary>
        /// <example>
        /// <code>
        /// await webViewPrefab.WaitUntilInitialized();
        /// webViewPrefab.WebView.LoadUrl("https://vuplex.com");
        /// </code>
        /// </example>
        /// <seealso href="https://support.vuplex.com/articles/how-to-load-local-files">How to load local files</seealso>
        /// <seealso cref="LoadHtml"/>
        /// <seealso cref="WebViewPrefab.InitialUrl"/>
        void LoadUrl(string url);

        /// <summary>
        /// Like LoadUrl(string url), but also sends the given additional HTTP request headers
        /// when loading the URL. The headers are sent for the initial page load request but are not sent
        /// for requests for subsequent resources, like linked JavaScript or CSS files.
        /// </summary>
        /// <remarks>
        /// On Windows and macOS, this method cannot be used to set the Accept-Language header.
        /// For more info, please see [this article](https://support.vuplex.com/articles/how-to-change-accept-language-header).
        /// </remarks>
        /// <example>
        /// <code>
        /// await webViewPrefab.WaitUntilInitialized();
        /// webViewPrefab.WebView.LoadUrl("https://vuplex.com", new Dictionary&lt;string, string&gt; {
        ///     ["Authorization"] = "Basic YWxhZGRpbjpvcGVuc2VzYW1l",
        ///     ["Cookie"] = "foo=bar"
        /// });
        /// </code>
        /// </example>
        void LoadUrl(string url, Dictionary<string, string> additionalHttpHeaders);

        /// <summary>
        /// Returns a point in pixels for the given normalized point.
        /// </summary>
        /// <seealso cref="PointToNormalized"/>
        Vector2Int NormalizedToPoint(Vector2 normalizedPoint);

        /// <summary>
        /// Pastes text from the clipboard.
        /// </summary>
        /// <example>
        /// <c>webViewPrefab.WebView.Paste();</c>
        /// </example>
        /// <seealso cref="Copy"/>
        /// <seealso cref="Cut"/>
        void Paste();

        /// <summary>
        /// Returns a normalized point for the given x and y coordinates in pixels.
        /// This can be used to create normalized points for APIs that accept them, like MovePointer().
        /// </summary>
        /// <example>
        /// <code>
        /// var webView = webViewPrefab.WebView;
        /// // Scroll to the right by 50 pixels at (100px, 1300px).
        /// webView.Scroll(
        ///     webView.PointToNormalized(50, 0),
        ///     webView.PointToNormalized(100, 1300)
        /// );
        /// </code>
        /// </example>
        /// <seealso cref="NormalizedToPoint"/>
        Vector2 PointToNormalized(int xInPixels, int yInPixels);

        /// <summary>
        /// Posts a message that JavaScript within the webview can listen for
        /// using `window.vuplex.addEventListener('message', function(message) {})`.
        /// The provided data string is passed as the data property of the message object.
        /// For more details, please see [this support article](https://support.vuplex.com/articles/how-to-send-messages-from-javascript-to-c-sharp).
        /// </summary>
        /// <example>
        /// <code>
        /// await webViewPrefab.WebView.WaitUntilInitialized();
        /// // Add some JavaScript to the page to receive the message.
        /// webViewPrefab.WebView.PageLoadScripts(@"
        ///     window.vuplex.addEventListener('message', function(event) {
        ///         console.log('Message received from C#: ' + event.data);
        ///     });
        /// ");
        /// // When the page finishes loading, send a message from C# to JavaScript.
        /// await webViewPrefab.WebView.WaitForNextPageLoadToFinish();
        /// webViewPrefab.WebView.PostMessage("Hello from C#");
        /// </code>
        /// </example>
        void PostMessage(string data);

        /// <summary>
        /// Reloads the current page.
        /// </summary>
        /// <example>
        /// <c>webViewPrefab.WebView.Reload();</c>
        /// </example>
        void Reload();

        /// <summary>
        /// Resizes the webview to the given dimensions in pixels.
        /// </summary>
        /// <remarks>
        /// If you're using WebViewPrefab, you should call WebViewPrefab.Resize() instead.
        /// </remarks>
        /// <seealso cref="Size"/>
        /// <seealso cref="WebViewPrefab.Resolution"/>
        void Resize(int width, int height);

        /// <summary>
        /// Scrolls the top-level document by the given delta in pixels.
        /// This method works by calling window.scrollBy(), which works for simple web
        /// pages but not for all pages. An alternative is to instead use Scroll(Vector2, Vector2)
        /// to scroll at a specific location in the page.
        /// </summary>
        /// <example>
        /// <code>
        /// // Scroll down by 50 pixels.
        /// webViewPrefab.WebView.Scroll(0, 50);
        ///
        /// // Scroll to the left by 20 pixels.
        /// webViewPrefab.WebView.Scroll(-20, 0);
        /// </code>
        /// </example>
        void Scroll(int scrollDeltaXInPixels, int scrollDeltaYInPixels);

        /// <summary>
        /// Like Scroll(int, int), but accepts a normalized scroll delta instead of
        /// values in pixels.
        /// </summary>
        /// <example>
        /// <code>
        /// // Scroll down one quarter of the page.
        /// webViewPrefab.WebView.Scroll(new Vector2(0, 0.25f));
        /// </code>
        /// </example>
        void Scroll(Vector2 normalizedScrollDelta);

        /// <summary>
        /// Scrolls by the given normalized scroll delta at the given normalized pointer position.
        /// </summary>
        /// <example>
        /// <code>
        /// var webView = webViewPrefab.WebView;
        /// // Scroll down by a quarter of the page in the center of the page
        /// webView.Scroll(new Vector2(0, 0.25f), new Vector2(0.5f, 0.5f));
        ///
        /// // Scroll to the right by 50 pixels at (100px, 1300px).
        /// webView.Scroll(
        ///     webView.PointToNormalized(50, 0),
        ///     webView.PointToNormalized(100, 1300)
        /// );
        /// </code>
        /// </example>
        void Scroll(Vector2 normalizedScrollDelta, Vector2 normalizedPoint);

        /// <summary>
        /// Selects all text, depending on the page's focused element.
        /// </summary>
        /// <example>
        /// <c>webViewPrefab.WebView.SelectAll();</c>
        /// </example>
        /// <seealso cref="Copy"/>
        void SelectAll();

        /// <summary>
        /// Dispatches the given keyboard key to the webview.
        /// </summary>
        /// <param name="key">
        /// A key can either be a single character representing
        /// a unicode character (e.g. "A", "b", "?") or a [JavaScript key value](https://developer.mozilla.org/en-US/docs/Web/API/KeyboardEvent/key/Key_Values)
        /// (e.g. "ArrowUp", "Enter", "Backspace", "Delete").
        /// </param>
        /// <example>
        /// <code>
        /// // Type "Hi!" and then submit the Enter key.
        /// webViewPrefab.WebView.SendKey("H");
        /// webViewPrefab.WebView.SendKey("i");
        /// webViewPrefab.WebView.SendKey("!");
        /// webViewPrefab.WebView.SendKey("Enter");
        /// </code>
        /// </example>
        /// <seealso cref="IWithKeyDownAndUp"/>
        void SendKey(string key);

        /// <summary>
        /// Makes the webview take or relinquish focus.
        /// </summary>
        /// <example>
        /// <c>webViewPrefab.WebView.SetFocused(true);</c>
        /// </example>
        void SetFocused(bool focused);

        /// <summary>
        /// Enables or disables the webview's ability to render to its texture.
        /// By default, a webview renders web content to its texture, but you can
        /// use this method to disable or re-enable rendering.
        /// </summary>
        /// <remarks>
        /// This method is ignored when running in [Native 2D Mode](https://support.vuplex.com/articles/native-2d-mode).
        /// </remarks>
        /// <example>
        /// <c>webViewPrefab.WebView.SetRenderingEnabled(false);</c>
        /// </example>
        void SetRenderingEnabled(bool enabled);

        /// <summary>
        /// Stops the current page load if one is in progress.
        /// </summary>
        /// <example>
        /// <c>webViewPrefab.WebView.StopLoad();</c>
        /// </example>
        void StopLoad();

        /// <summary>
        /// Returns a task that completes when the next page load finishes loading, or
        /// throws a PageLoadFailedException if the page load fails.
        /// </summary>
        /// <example>
        /// <code>
        /// await webViewPrefab.WaitUntilInitialized();
        /// await webViewPrefab.WebView.WaitForNextPageLoadToFinish();
        /// Debug.Log("The web page finished loading.");
        /// </code>
        /// </example>
        /// <seealso cref="LoadProgressChanged"/>
        Task WaitForNextPageLoadToFinish();

        /// <summary>
        /// Zooms into the currently loaded web content. Note that the zoom level gets reset when a new page is loaded.
        /// </summary>
        /// <remarks>
        /// On Windows and macOS, adjusting the zoom also affects other webviews viewing the same site,
        /// similar to how tabs behave in a desktop browser.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Zoom in after the page finishes loading.
        /// await webViewPrefab.WaitUntilInitialized();
        /// await webViewPrefab.WebView.WaitForNextPageLoadToFinish();
        /// webViewPrefab.WebView.ZoomIn();
        /// </code>
        /// </example>
        void ZoomIn();

        /// <summary>
        /// Zooms back out after a previous call to ZoomIn(). Note that the zoom level gets reset when a new page is loaded.
        /// </summary>
        /// <remarks>
        /// On Windows and macOS, adjusting the zoom also affects other webviews viewing the same site,
        /// similar to how tabs behave in a desktop browser.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Zoom out after the page finishes loading.
        /// await webViewPrefab.WaitUntilInitialized();
        /// await webViewPrefab.WebView.WaitForNextPageLoadToFinish();
        /// webViewPrefab.WebView.ZoomOut();
        /// </code>
        /// </example>
        void ZoomOut();

    #region Obsolete APIs
        // Added in v1.0, deprecated in v3.13, removed in v4.0.
        [Obsolete(ObsoletionMessages.Blur, true)]
        void Blur();

        // Added in v1.0, deprecated in v3.16, removed in v4.0.
        [Obsolete(ObsoletionMessages.CanGoBack, true)]
        void CanGoBack(Action<bool> callback);

        // Added in v1.0, deprecated in v3.16, removed in v4.0.
        [Obsolete(ObsoletionMessages.CanGoForward, true)]
        void CanGoForward(Action<bool> callback);

        // Added in v2.4, deprecated in v3.16, removed in v4.0.
        [Obsolete(ObsoletionMessages.CaptureScreenshot, true)]
        void CaptureScreenshot(Action<byte[]> callback);

        [Obsolete(ObsoletionMessages.DisableViewUpdates, true)]
        void DisableViewUpdates();

        [Obsolete(ObsoletionMessages.EnableViewUpdates, true)]
        void EnableViewUpdates();

        // Added in v1.0, deprecated in v3.13, removed in v4.0.
        [Obsolete(ObsoletionMessages.Focus, true)]
        void Focus();

        // Added in v2.6, deprecated in v3.16, removed in v4.0.
        [Obsolete(ObsoletionMessages.GetRawTextureData, true)]
        void GetRawTextureData(Action<byte[]> callback);

        // Added in v1.0, deprecated in v4.0.
        [Obsolete(ObsoletionMessages.HandleKeyboardInput)]
        void HandleKeyboardInput(string key);

        // Added in v1.0, removed in v4.0.
        [Obsolete(ObsoletionMessages.Init, true)]
        void Init(Texture2D texture, float width, float height);

        // Added in v1.0, removed in v4.0.
        [Obsolete(ObsoletionMessages.Init2, true)]
        void Init(Texture2D texture, float width, float height, Texture2D videoTexture);

        // Added in v1.0, removed in v4.0.
        [Obsolete(ObsoletionMessages.Resolution, true)]
        float Resolution { get; }

        // Added in v1.0, removed in v4.0.
        [Obsolete(ObsoletionMessages.SetResolution, true)]
        void SetResolution(float pixelsPerUnityUnit);

        // Deprecated in v4.0
        [Obsolete(ObsoletionMessages.SizeInPixels)]
        Vector2 SizeInPixels { get; }

        // Added in v1.0, removed in v4.0.
        [Obsolete(ObsoletionMessages.VideoRectChanged, true)]
        event EventHandler<EventArgs<Rect>> VideoRectChanged;

        // Added in v1.0, removed in v4.0.
        [Obsolete(ObsoletionMessages.VideoTexture, true)]
        Texture2D VideoTexture { get; }
    #endregion
    }

    static class ObsoletionMessages {
        public const string Blur = "IWebView.Blur() has been removed. Please use SetFocused(false) instead: https://developer.vuplex.com/webview/IWebView#SetFocused";
        public const string CanGoBack = "The callback-based CanGoBack(Action) version of this method has been removed. Please switch to the Task-based CanGoBack() version instead. If you prefer using a callback instead of awaiting the Task, you can still use a callback like this: CanGoBack().ContinueWith(result => {...})";
        public const string CanGoForward = "The callback-based CanGoForward(Action) version of this method has been removed. Please switch to the Task-based CanGoForward() version instead. If you prefer using a callback instead of awaiting the Task, you can still use a callback like this: CanGoForward().ContinueWith(result => {...})";
        public const string CaptureScreenshot = "The callback-based CaptureScreenshot(Action) version of this method has been removed. Please switch to the Task-based CaptureScreenshot() version instead. If you prefer using a callback instead of awaiting the Task, you can still use a callback like this: CaptureScreenshot().ContinueWith(result => {...})";
        public const string DisableViewUpdates = "DisableViewUpdates() has been removed. Please use SetRenderingEnabled(false) instead: https://developer.vuplex.com/webview/IWebView#SetRenderingEnabled";
        public const string EnableViewUpdates = "EnableViewUpdates() has been removed. Please use SetRenderingEnabled(true) instead: https://developer.vuplex.com/webview/IWebView#SetRenderingEnabled";
        public const string Focus = "IWebView.Focus() has been removed. Please use SetFocused(false) instead: https://developer.vuplex.com/webview/IWebView#SetFocused";
        public const string GetRawTextureData = "The callback-based GetRawTextureData(Action) version of this method has been removed. Please switch to the Task-based GetRawTextureData() version instead. If you prefer using a callback instead of awaiting the Task, you can still use a callback like this: GetRawTextureData().ContinueWith(result => {...})";
        public const string HandleKeyboardInput = "IWebView.HandleKeyboardInput() has been renamed to IWebView.SendKey(). Please switch to SendKey().";
        public const string Init = "IWebView.Init(Texture2D, float, float) has been removed in v4. Please switch to IWebView.Init(int, int) and await the Task it returns. For more details, please see this article: https://support.vuplex.com/articles/v4-changes#init";
        public const string Init2 = "IWebView.Init(Texture2D, float, float, Texture2D) has been removed in v4. Please switch to IWebView.Init(int, int) and await the Task it returns. For more details, please see this article: https://support.vuplex.com/articles/v4-changes#init";
        public const string Resolution = "IWebView.Resolution has been removed in v4. Please use WebViewPrefab.Resolution or CanvasWebViewPrefab.Resolution instead. For more details, please see this article: https://support.vuplex.com/articles/v4-changes#resolution";
        public const string SetResolution = "IWebView.SetResolution() has been removed in v4. Please set the WebViewPrefab.Resolution or CanvasWebViewPrefab.Resolution property instead. For more details, please see this article: https://support.vuplex.com/articles/v4-changes#resolution";
        public const string SizeInPixels = "IWebView.SizeInPixels is now deprecated. Please use IWebView.Size instead: https://developer.vuplex.com/webview/IWebView#Size";
        public const string VideoRectChanged = "IWebView.VideoRectChanged has been removed. Please use IWithFallbackVideo.VideoRectChanged instead: https://developer.vuplex.com/webview/IWithFallbackVideo#VideoRectChanged";
        public const string VideoTexture = "IWebView.VideoTexture has been removed. Please use IWithFallbackVideo.VideoTexture instead: https://developer.vuplex.com/webview/IWithFallbackVideo#VideoTexture";
    }
}
