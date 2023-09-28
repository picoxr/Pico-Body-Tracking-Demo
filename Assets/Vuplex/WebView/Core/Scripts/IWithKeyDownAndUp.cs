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
    /// An interface implemented by a webview if it supports separate
    /// `KeyDown()` and `KeyUp()` methods.
    /// </summary>
    /// <example>
    /// <code>
    /// // Dispatch ctrl + shift + right arrow
    /// await webViewPrefab.WaitUntilInitialized();
    /// var webViewWithKeyDownAndUp = webViewPrefab.WebView as IWithKeyDownAndUp;
    /// if (webViewWithKeyDownAndUp != null) {
    ///     webViewWithKeyDownAndUp.KeyDown("ArrowRight", KeyModifier.Control | KeyModifier.Shift);
    ///     webViewWithKeyDownAndUp.KeyUp("ArrowRight", KeyModifier.Control | KeyModifier.Shift);
    /// }
    /// </code>
    /// </example>
    public interface IWithKeyDownAndUp {

        /// <summary>
        /// Dispatches a key down event to the webview.
        /// </summary>
        /// <param name="key">
        /// A key can either be a single character representing
        /// a unicode character (e.g. "A", "b", "?") or a [JavaScript Key value](https://developer.mozilla.org/en-US/docs/Web/API/KeyboardEvent/key/Key_Values)
        /// (e.g. "ArrowUp", "Enter").
        /// </param>
        /// <param name="modifiers">
        /// Modifier flags that can be used to trigger keyboard shortcuts.
        /// </param>
        void KeyDown(string key, KeyModifier modifiers);

        /// <summary>
        /// Dispatches a key up event to the webview.
        /// </summary>
        /// <param name="key">
        /// A key can either be a single character representing
        /// a unicode character (e.g. "A", "b", "?") or a [JavaScript Key value](https://developer.mozilla.org/en-US/docs/Web/API/KeyboardEvent/key/Key_Values)
        /// (e.g. "ArrowUp", "Enter").
        /// </param>
        /// <param name="modifiers">
        /// Modifier flags that can be used to trigger keyboard shortcuts.
        /// </param>
        void KeyUp(string key, KeyModifier modifiers);
    }
}
