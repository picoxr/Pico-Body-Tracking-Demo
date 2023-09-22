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
    /// A touch event that can be dispatched with IWithTouch.SendTouchEvent().
    /// </summary>
    public struct TouchEvent {

        /// <summary>
        /// ID of the touch. This value must be unique per touch but is shared by all
        /// events for the same touch (i.e. the Start, Move, and End events for a
        /// single touch share the same TouchID). This can be any number except -1, and
        /// a maximum of 16 concurrent touches are tracked.
        /// </summary>
        public int TouchID;

        /// <summary>
        /// The touch event type. Touches begin with one TouchEventType.Start event,
        /// followed by zero or more TouchEventType.Move events, and finally one TouchEventType.End
        /// or TouchEventType.Cancel event.
        /// </summary>
        public TouchEventType Type;

        /// <summary>
        /// The normalized point of the touch event.
        /// </summary>
        public Vector2 Point;

        /// <summary>
        /// (optional) The X radius of the touch in pixels. If not set, the default
        /// value of 25px will be used.
        /// </summary>
        public float RadiusX;

        /// <summary>
        /// (optional) The Y radius of the touch in pixels. If not set, the default
        /// value of 25px will be used.
        /// </summary>
        public float RadiusY;

        /// <summary>
        /// (optional) The rotation angle in radians.
        /// </summary>
        public float RotationAngle;

        /// <summary>
        /// (optional) The normalized pressure of the touch in the range of [0, 1].
        /// </summary>
        public float Pressure;
    }
}
