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
    /// Enum for the different mouse buttons.
    /// </summary>
    /// <summary>
    /// This enum's values are compatible with those from
    /// the UnityEngine.EventSystem [`InputButton`](https://docs.unity3d.com/Packages/com.unity.ugui@1.0/api/UnityEngine.EventSystems.PointerEventData.InputButton.html) enum,
    /// so you can safely cast an `InputButton` to a `MouseButton`.
    /// </summary>
    public enum MouseButton {

        /// <summary>
        /// The left mouse button (i.e. left click).
        /// </summary>
        Left = 0,

        /// <summary>
        /// The right mouse button (i.e. right click).
        /// </summary>
        Right = 1,

        /// <summary>
        /// The center mouse button.
        /// </summary>
        Middle = 2
    }
}
