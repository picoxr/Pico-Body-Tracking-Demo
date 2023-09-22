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
using System.Threading.Tasks;
using UnityEngine;

namespace Vuplex.WebView {

    /// <summary>
    /// An interface implemented by a webview if it supports Native 2D Mode, which makes it
    /// so that 3D WebView positions a native 2D webview in front of the Unity game view instead
    /// of displaying web content as a texture in the Unity scene.
    /// For more info, please see [this article](https://support.vuplex.com/articles/native-2d-mode).
    /// </summary>
    /// <seealso cref="CanvasWebViewPrefab.Native2DModeEnabled"/>
    public interface IWithNative2DMode {

        /// <summary>
        /// Gets a value indicating whether the webview is running in Native 2D Mode.
        /// </summary>
        /// <seealso cref="CanvasWebViewPrefab.Native2DModeEnabled"/>
        bool Native2DModeEnabled { get; }

        /// <summary>
        /// Gets the native 2D webview's rect on the screen, in pixels.
        /// </summary>
        /// <seealso cref="IWithNative2DMode.SetRect"/>
        Rect Rect { get; }

        /// <summary>
        /// Gets a value indicating whether the native 2D webview is visible.
        /// The default is `true`.
        /// </summary>
        /// <seealso cref="CanvasWebViewPrefab.Visible"/>
        /// <seealso cref="IWithNative2DMode.SetVisible"/>
        bool Visible { get; }

        /// <summary>
        /// Brings the native webview to the front of the view hierarchy.
        /// A webview is automatically placed in the front when it's created,
        /// but this method can be used to control which webview is in front
        /// if your scene contains multiple 2D webviews.
        /// </summary>
        /// <remarks>
        /// This method is currently not supported on UWP.
        /// </remarks>
        void BringToFront();

        /// <summary>
        /// Initializes the webview in Native 2D Mode. This method is to be used instead
        /// of IWebView.Init() for initialization.
        /// </summary>
        Task InitInNative2DMode(Rect rect);

        /// <summary>
        /// Sets whether the native 2D webview's pinch-to-zoom behavior
        /// is enabled. The default is `true`.
        /// </summary>
        void SetNativeZoomEnabled(bool enabled);

        /// <summary>
        /// Sets the native 2D webview's rect on the screen, in pixels.
        /// </summary>
        /// <seealso cref="IWithNative2DMode.Rect"/>
        void SetRect(Rect rect);

        /// <summary>
        /// Sets whether the native 2D webview is visible.
        /// </summary>
        /// <seealso cref="CanvasWebViewPrefab.Visible"/>
        /// <seealso cref="IWithNative2DMode.Visible"/>
        void SetVisible(bool visible);
    }
}
