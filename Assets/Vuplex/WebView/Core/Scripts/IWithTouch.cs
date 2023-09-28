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
    /// An interface implemented by a webview if it supports dispatching touch events.
    /// </summary>
    public interface IWithTouch {

        /// <summary>
        /// Dispatches a touch event (i.e. touchstart, touchend, touchmove, touchcancel) in
        /// the webview. This can be used, for example, to implement multitouch interactions.
        /// </summary>
        /// <example>
        /// <code>
        /// var webViewWithTouch = webViewPrefab.WebView as IWithTouch;
        /// if (webViewWithTouch == null) {
        ///     Debug.Log("This 3D WebView plugin doesn't yet support IWithTouch: " + webViewPrefab.WebView.PluginType);
        ///     return;
        /// }
        /// // Start and end a first touch at (250px, 100px) in the web page.
        /// var normalizedPoint1 = webViewPrefab.WebView.PointToNormalized(250, 100);
        /// webViewWithTouch.SendTouchEvent(new TouchEvent {
        ///     TouchID = 1,
        ///     Point = normalizedPoint1,
        ///     Type = TouchEventType.Start
        /// });
        ///
        /// webViewWithTouch.SendTouchEvent(new TouchEvent {
        ///     TouchID = 1,
        ///     Point = normalizedPoint1,
        ///     Type = TouchEventType.End
        /// });
        ///
        /// // Start a second touch at (400px, 100px), move it to (410px, 100), and release it.
        /// var normalizedPoint2 = webViewPrefab.WebView.PointToNormalized(400, 100);
        /// webViewWithTouch.SendTouchEvent(new TouchEvent {
        ///     TouchID = 2,
        ///     Point = normalizedPoint2,
        ///     Type = TouchEventType.Start
        /// });
        ///
        /// var normalizedPoint3 = webViewPrefab.WebView.PointToNormalized(410, 100);
        /// webViewWithTouch.SendTouchEvent(new TouchEvent {
        ///     TouchID = 2,
        ///     Point = normalizedPoint3,
        ///     Type = TouchEventType.Move
        /// });
        ///
        /// webViewWithTouch.SendTouchEvent(new TouchEvent {
        ///     TouchID = 2,
        ///     Point = normalizedPoint3,
        ///     Type = TouchEventType.End
        /// });
        /// </code>
        /// </example>
        void SendTouchEvent(TouchEvent touchEvent);
    }
}
