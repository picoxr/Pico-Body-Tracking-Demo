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

namespace Vuplex.WebView {

    /// <summary>
    /// Event args for WebViewPrefab.Scrolled.
    /// </summary>
    public class ScrolledEventArgs : EventArgs {

        public ScrolledEventArgs(Vector2 scrollDelta, Vector2 point) {

            ScrollDelta = scrollDelta;
            Point = point;
        }

        /// <summary>
        /// The normalized scroll delta that was passed to IWebView.Scroll().
        /// </summary>
        public readonly Vector2 ScrollDelta;

        /// <summary>
        /// The normalized pointer position that was passed to IWebView.Scroll().
        /// </summary>
        public readonly Vector2 Point;
    }
}

