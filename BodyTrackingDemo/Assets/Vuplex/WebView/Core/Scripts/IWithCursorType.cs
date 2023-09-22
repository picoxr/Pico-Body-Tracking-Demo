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
    /// An interface implemented by a webview if it supports indicating when the
    /// mouse cursor icon should change.
    /// </summary>
    public interface IWithCursorType {

        /// <summary>
        /// Indicates that the mouse cursor icon should change. The EventArgs.Value
        /// is a string indicating the [CSS cursor type value](https://developer.mozilla.org/en-US/docs/Web/CSS/cursor)
        /// (e.g. `"default"`, `"pointer"`, `"wait"`, etc.).
        /// </summary>
        /// <example>
        /// <code>
        /// await webViewPrefab.WaitUntilInitialized();
        /// var webViewWithCursorType = webViewPrefab.WebView as IWithCursorType;
        /// webViewWithCursorType.CursorTypeChanged += (sender, eventArgs) => {
        ///     // Note: Here's where the application could use Unity's Cursor.SetCursor() API to set
        ///     //       a custom cursor icon depending on the cursor type indicated by eventArgs.Value.
        ///     Debug.Log("Cursor type changed: " + eventArgs.Value);
        /// };
        /// </code>
        /// </example>
        /// <seealso cref="WebViewPrefab.CursorIconsEnabled"/>
        event EventHandler<EventArgs<string>> CursorTypeChanged;
    }
}
