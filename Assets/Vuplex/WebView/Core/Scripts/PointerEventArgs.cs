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
    /// Event args to indicate a pointer event (i.e. mouse event)
    /// for IPointerInputDetector.
    /// </summary>
    public class PointerEventArgs : EventArgs {

        /// <summary>
        /// The button for the event. The default is `MouseButton.Left`.
        /// </summary>
        public MouseButton Button = MouseButton.Left;

        /// <summary>
        /// The number of clicks for the event. For example, for a double click,
        /// set this value to `2`. The default is `1`.
        /// </summary>
        public int ClickCount = 1;

        /// <summary>
        /// The normalized point passed to IWebView.Click().
        /// </summary>
        public Vector2 Point;

        /// <summary>
        /// Converts the event args into PointerOptions
        /// that can be passed to methods like PointerUp() and PointerDown().
        /// </summary>
        public PointerOptions ToPointerOptions() {

            return new PointerOptions {
                Button = Button,
                ClickCount = ClickCount
            };
        }
    }
}
