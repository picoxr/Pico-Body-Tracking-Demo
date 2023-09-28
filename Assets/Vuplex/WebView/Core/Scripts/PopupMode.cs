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
namespace Vuplex.WebView {

    /// <summary>
    /// Options for how a webview that implements `IWithPopups` handles popups.
    /// </summary>
    public enum PopupMode {

        /// <summary>
        /// The popup URL is automatically loaded into the original webview.
        /// This is the default behavior. In this mode, the `PopupRequested` event isn't raised.
        /// </summary>
        LoadInOriginalWebView = 0,

        /// <summary>
        /// The browser engine automatically creates a new webview for the popup
        /// and loads the popup URL into the new webview. The original webview then
        /// raises its `PopupRequested` event and provides the new popup webview as `PopupRequestedEventArgs.WebView`.
        /// </summary>
        /// <remarks>
        /// Some authentication flows require this mode in order to function correctly. For example,
        /// auth flows like "Sign in with Google" open the signin page in a special popup
        /// that can relay the auth result back to the original page after authorization is finished.
        /// `LoadInNewWebView` must be used for flows like this, and the flow can not be emulated with
        /// the other popup modes.
        /// </remarks>
        LoadInNewWebView = 1,

        /// <summary>
        /// The browser engine doesn't automatically create a new webview for the popup,
        /// but it still raises the `PopupRequested` event with a `PopupRequestedEventArgs.WebView` of `null`.
        /// </summary>
        NotifyWithoutLoading = 2
    }
}
