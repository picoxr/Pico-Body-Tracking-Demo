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
using UnityEngine;

namespace Vuplex.WebView.Demos {

    /// <summary>
    /// Sets up the SimpleWebViewDemo scene, which displays a WebViewPrefab
    /// with an on-screen keyboard in world space.
    /// </summary>
    /// <remarks>
    /// Links: <br/>
    /// - WebViewPrefab docs: https://developer.vuplex.com/webview/WebViewPrefab <br/>
    /// - How clicking works: https://support.vuplex.com/articles/clicking <br/>
    /// - Other examples: https://developer.vuplex.com/webview/overview#examples <br/>
    /// </remarks>
    class SimpleWebViewDemo : MonoBehaviour {

        HardwareKeyboardListener _hardwareKeyboardListener;
        WebViewPrefab _webViewPrefab;

        void Awake() {

            // Use a desktop User-Agent to request the desktop versions of websites.
            // https://developer.vuplex.com/webview/Web#SetUserAgent
            // Call this from Awake() to ensure it's called before the webview initializes.
            Web.SetUserAgent(false);
        }

        async void Start() {

            // The WebViewPrefab's InitialUrl property is set via the editor, so it
            // automatically loads that URL when it initializes.
            _webViewPrefab = GameObject.Find("WebViewPrefab").GetComponent<WebViewPrefab>();
            _setUpKeyboards();

            // Wait for the WebViewPrefab to initialize, because the WebViewPrefab.WebView property
            // is null until the prefab has initialized.
            await _webViewPrefab.WaitUntilInitialized();

            // The WebViewPrefab has initialized, so now we can use the IWebView APIs
            // using its WebViewPrefab.WebView property.
            // https://developer.vuplex.com/webview/IWebView
            _webViewPrefab.WebView.UrlChanged += (sender, eventArgs) => {
                Debug.Log("[SimpleWebViewDemo] URL changed: " + eventArgs.Url);
            };
        }

        void _setUpKeyboards() {

            // Send keys from the hardware (USB or Bluetooth) keyboard to the webview.
            // Use separate KeyDown() and KeyUp() methods if the webview supports
            // it, otherwise just use IWebView.SendKey().
            // https://developer.vuplex.com/webview/IWithKeyDownAndUp
            _hardwareKeyboardListener = HardwareKeyboardListener.Instantiate();
            _hardwareKeyboardListener.KeyDownReceived += (sender, eventArgs) => {
                var webViewWithKeyDown = _webViewPrefab.WebView as IWithKeyDownAndUp;
                if (webViewWithKeyDown != null) {
                    webViewWithKeyDown.KeyDown(eventArgs.Value, eventArgs.Modifiers);
                } else {
                    _webViewPrefab.WebView.SendKey(eventArgs.Value);
                }
            };
            _hardwareKeyboardListener.KeyUpReceived += (sender, eventArgs) => {
                var webViewWithKeyUp = _webViewPrefab.WebView as IWithKeyDownAndUp;
                webViewWithKeyUp?.KeyUp(eventArgs.Value, eventArgs.Modifiers);
            };

            // Also hook up the on-screen keyboard.
            var keyboard = GameObject.FindObjectOfType<Keyboard>();
            keyboard.InputReceived += (sender, eventArgs) => {
                _webViewPrefab.WebView.SendKey(eventArgs.Value);
            };
        }
    }
}
