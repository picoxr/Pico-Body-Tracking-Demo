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

namespace Vuplex.WebView {

    /// <summary>
    /// Values for AndroidWebView.SetForceDark().
    /// </summary>
    public enum ForceDark {
        /// <summary>
        /// Disable force dark, irrespective of the force dark mode of the WebView parent.
        /// In this mode, WebView content will always be rendered as-is, regardless of whether
        /// native views are being automatically darkened.
        /// </summary>
        /// <seealso href="https://developer.android.com/reference/android/webkit/WebSettings#FORCE_DARK_OFF">android.webkit.WebSettings.FORCE_DARK_OFF</seealso>
        Off = 0,
        /// <summary>
        /// Enable force dark dependent on the state of the WebView parent view.
        /// If the WebView parent view is being automatically force darkened
        /// (see: View.setForceDarkAllowed(boolean)), then WebView content will be
        /// rendered so as to emulate a dark theme. WebViews that are not attached
        /// to the view hierarchy will not be inverted.
        /// </summary>
        /// <seealso href="https://developer.android.com/reference/android/webkit/WebSettings#FORCE_DARK_AUTO">android.webkit.WebSettings.FORCE_DARK_AUTO</seealso>
        Auto = 1,
        /// <summary>
        /// Unconditionally enable force dark. In this mode WebView content will always be rendered so as to emulate a dark theme.
        /// </summary>
        /// <seealso href="https://developer.android.com/reference/android/webkit/WebSettings#FORCE_DARK_ON">android.webkit.WebSettings.FORCE_DARK_ON</seealso>
        On = 2
    }
}
#endif
