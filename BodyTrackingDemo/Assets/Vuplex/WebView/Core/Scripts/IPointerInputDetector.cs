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
    /// An interface that can be passed to WebViewPrefab.SetPointerInputDetector()
    /// CanvasWebViewPrefab.SetPointerInputDetector() to override how the prefab detects pointer input.
    /// For example implementations of this interface, please see 3D WebView's DefaultPointerInputDetector.cs
    /// and CanvasPointerInputDetector.cs scripts.
    /// </summary>
    public interface IPointerInputDetector {

        /// <summary>
        /// Indicates the normalized point for the beginning of a drag interaction.
        /// </summary>
        event EventHandler<EventArgs<Vector2>> BeganDrag;

        /// <summary>
        /// Indicates the normalized point for the continuation of a drag interaction.
        /// </summary>
        event EventHandler<EventArgs<Vector2>> Dragged;

        /// <summary>
        /// Indicates a pointer down interaction occurred.
        /// </summary>
        event EventHandler<PointerEventArgs> PointerDown;

        /// <summary>
        /// Indicates that the pointer exited.
        /// </summary>
        event EventHandler PointerExited;

        /// <summary>
        /// Indicates the normalized point where the pointer moved.
        /// </summary>
        event EventHandler<EventArgs<Vector2>> PointerMoved;

        /// <summary>
        /// Indicates a pointer up interaction occurred.
        /// </summary>
        event EventHandler<PointerEventArgs> PointerUp;

        /// <summary>
        /// Indicates a scroll interaction occurred.
        /// </summary>
        event EventHandler<ScrolledEventArgs> Scrolled;

        /// <summary>
        /// The prefab sets this property to indicate whether
        /// the PointerMoved event should be enabled.
        /// </summary>
        bool PointerMovedEnabled { get; set; }
    }
}
