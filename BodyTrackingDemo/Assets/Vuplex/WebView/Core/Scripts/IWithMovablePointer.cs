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

namespace Vuplex.WebView {

    /// <summary>
    /// An interface implemented by a webview if it supports MovePointer(),
    /// which can be used to implement hover or drag interactions.
    /// </summary>
    /// <remarks>
    /// For information on the limitations of hover and drag interactions on iOS and UWP, please see
    /// https://support.vuplex.comarticles/hover-and-drag-limitations.
    /// </remarks>
    /// <example>
    /// <code>
    /// var webViewWithMovablePointer = webViewPrefab.WebView as IWithMovablePointer;
    /// if (webViewWithMovablePointer != null) {
    ///     // Move the pointer to (250px, 100px) in the web page.
    ///     var normalizedPoint = webViewPrefab.WebView.PointToNormalized(250, 100);
    ///     webViewWithMovablePointer.MovePointer(normalizedPoint)
    /// }
    /// </code>
    /// </example>
    public interface IWithMovablePointer {

        /// <summary>
        /// Moves the pointer to the given normalized point in the web page.
        /// </summary>
        /// <remarks>
        /// This can be used to trigger hover effects in the page or can be used
        /// in conjunction with IWithPointerDownAndUp to implement
        /// drag interactions.
        /// </remarks>
        void MovePointer(Vector2 normalizedPoint);
    }
}
