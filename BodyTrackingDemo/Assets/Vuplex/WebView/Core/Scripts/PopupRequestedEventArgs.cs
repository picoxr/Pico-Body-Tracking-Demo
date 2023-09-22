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

namespace Vuplex.WebView {

    /// <summary>
    /// Event args for `PopupRequested`.
    /// </summary>
    public class PopupRequestedEventArgs : EventArgs {

        public PopupRequestedEventArgs(string url, IWebView webView) {
            Url = url;
            WebView = webView;
        }

        /// <summary>
        /// The URL for the popup.
        /// </summary>
        /// <summary>
        /// Note that with 3D WebView for Android, the `Url` field may be empty in some
        /// cases when using `PopupMode.LoadInNewWebView`.
        /// </summary>
        public readonly string Url;

        /// <summary>
        /// The new webview that was created for the popup if the webview's popup
        //// mode is set to `PopupMode.LoadInNewWebView`, or null if the popup mode
        /// is set to `PopupMode.NotifyWithoutLoading`.
        /// </summary>
        public readonly IWebView WebView;
    }
}

