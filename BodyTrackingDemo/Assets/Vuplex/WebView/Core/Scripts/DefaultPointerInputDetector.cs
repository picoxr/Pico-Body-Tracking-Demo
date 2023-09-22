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
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Vuplex.WebView.Internal;

#if VUPLEX_MRTK
    using Microsoft.MixedReality.Toolkit.Input;
#endif

namespace Vuplex.WebView {

    [HelpURL("https://developer.vuplex.com/webview/IPointerInputDetector")]
    public class DefaultPointerInputDetector : MonoBehaviour,
                                               IPointerInputDetector,
                                               IBeginDragHandler,
                                               IDragHandler,
                                               IPointerClickHandler,
                                               IPointerDownHandler,
                                               IPointerEnterHandler,
                                               IPointerExitHandler,
                                               IPointerUpHandler,
                                            #if VUPLEX_MRTK
                                               IMixedRealityPointerHandler,
                                            #endif
                                               IScrollHandler {

        public event EventHandler<EventArgs<Vector2>> BeganDrag;

        public event EventHandler<EventArgs<Vector2>> Dragged;

        public event EventHandler<PointerEventArgs> PointerDown;

        public event EventHandler PointerEntered;
        
        public event EventHandler PointerExited;

        public event EventHandler<EventArgs<Vector2>> PointerMoved;

        public event EventHandler<PointerEventArgs> PointerUp;

        public event EventHandler<ScrolledEventArgs> Scrolled;

        public bool PointerMovedEnabled { get; set; }

        /// <see cref="IBeginDragHandler"/>
        public void OnBeginDrag(PointerEventData eventData) {

            _raiseBeganDragEvent(_convertToEventArgs(eventData));
        }

        /// <see cref="IDragHandler"/>
        public void OnDrag(PointerEventData eventData) {

            // The point is Vector3.zero when the user drags off of the screen.
            if (!_positionIsZero(eventData)) {
                _raiseDraggedEvent(_convertToEventArgs(eventData));
            }
        }

        /// <summary>
        /// VRIF requires IPointerClickHandler to be implemented in order to detect the object
        /// and invoke its OnPointerDown() and OnPointerUp() methods.
        /// </summary>
        /// <see cref="IPointerClickHandler"/>
        public void OnPointerClick(PointerEventData eventData) {}

        /// <see cref="IPointerDownHandler"/>
        public virtual void OnPointerDown(PointerEventData eventData) {

            _raisePointerDownEvent(_convertToPointerEventArgs(eventData));
        }

        /// <see cref="IPointerEnterHandler"/>
        public void OnPointerEnter(PointerEventData eventData) {

            _isHovering = true;
            _raisePointerEnteredEvent(_convertToPointerEventArgs(eventData));
        }

        /// <see cref="IPointerExitHandler"/>
        public void OnPointerExit(PointerEventData eventData) {

            _isHovering = false;
            _raisePointerExitedEvent(EventArgs.Empty);
        }

        /// <see cref="IPointerUpHandler"/>
        public virtual void OnPointerUp(PointerEventData eventData) {

            _raisePointerUpEvent(_convertToPointerEventArgs(eventData));
        }

        /// <see cref="IScrollHandler"/>
        public void OnScroll(PointerEventData eventData) {

            var scrollDelta = new Vector2(
                -eventData.scrollDelta.x,
                -eventData.scrollDelta.y
            );
            _raiseScrolledEvent(new ScrolledEventArgs(scrollDelta, _convertToNormalizedPoint(eventData)));
        }

        bool _isHovering;

        EventArgs<Vector2> _convertToEventArgs(Vector3 worldPosition) {

            var screenPoint = _convertToNormalizedPoint(worldPosition);
            return new EventArgs<Vector2>(screenPoint);
        }

        EventArgs<Vector2> _convertToEventArgs(PointerEventData pointerEventData) {

            var screenPoint = _convertToNormalizedPoint(pointerEventData);
            return new EventArgs<Vector2>(screenPoint);
        }

        protected virtual Vector2 _convertToNormalizedPoint(PointerEventData pointerEventData) {

            return _convertToNormalizedPoint(pointerEventData.pointerCurrentRaycast.worldPosition);
        }

        protected virtual Vector2 _convertToNormalizedPoint(Vector3 worldPosition) {
            // Note: transform.parent is WebViewPrefabResizer
            var localPosition = transform.parent.InverseTransformPoint(worldPosition);
            return new Vector2(1 - localPosition.x, -1 * localPosition.y);
        }

        PointerEventArgs _convertToPointerEventArgs(PointerEventData eventData) {

            return new PointerEventArgs {
                Point = _convertToNormalizedPoint(eventData),
                Button = (MouseButton)eventData.button,
                // StandaloneInputModule incorrectly specifies a click count of 0
                // for PointerDown events, so set the minimum to 1 click.
                ClickCount = Math.Max(eventData.clickCount, 1)
            };
        }

        /// <summary>
        /// Unity's event system doesn't include a standard pointer event
        /// for hovering (i.e. there's no `IPointerHoverHandler` interface).
        /// So, this method implements the equivalent functionality by
        /// using the protected `PointerInputModule.GetLastPointerEventData()`
        /// method to detect where the pointer is hovering.
        /// </summary>
        PointerEventData _getLastPointerEventData() {

            var pointerInputModule = EventSystem.current?.currentInputModule as PointerInputModule;
            if (pointerInputModule == null) {
                return null;
            }
            // Use reflection to get access to the protected `GetPointerData()`
            // method. Unity isn't going to change this API because most input modules
            // extend PointerInputModule. Note that `GetPointerData()` is used instead
            // of `GetLastPointerEventData()` because the latter doesn't work with
            // the Oculus SDK's OVRInputModule.
            var args = new object[] { PointerInputModule.kMouseLeftId, null, false };
            pointerInputModule.GetType().InvokeMember(
                "GetPointerData",
                BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                pointerInputModule,
                args
            );
            // The second argument is an out param.
            var pointerEventData = args[1] as PointerEventData;
            return pointerEventData;
        }

        protected virtual bool _positionIsZero(PointerEventData eventData) {

            return eventData.pointerCurrentRaycast.worldPosition == Vector3.zero;
        }

        protected void _raiseBeganDragEvent(EventArgs<Vector2> eventArgs) {

            BeganDrag?.Invoke(this, eventArgs);
        }

        protected void _raiseDraggedEvent(EventArgs<Vector2> eventArgs) {

            Dragged?.Invoke(this, eventArgs);
        }

        protected void _raisePointerDownEvent(PointerEventArgs eventArgs) {

            PointerDown?.Invoke(this, eventArgs);
        }

        protected void _raisePointerEnteredEvent(EventArgs eventArgs) {

            PointerEntered?.Invoke(this, eventArgs);
        }
        
        protected void _raisePointerExitedEvent(EventArgs eventArgs) {

            PointerExited?.Invoke(this, eventArgs);
        }

        void _raisePointerMovedIfNeeded() {

            if (!(PointerMovedEnabled && _isHovering)) {
                return;
            }
            var pointerEventData = _getLastPointerEventData();
            if (pointerEventData == null) {
                return;
            }
            var screenPoint = _convertToNormalizedPoint(pointerEventData);
            if (!(screenPoint.x >= 0f && screenPoint.y >= 0f)) {
                // This can happen while the prefab is being resized.
                return;
            }
            _raisePointerMovedEvent(new EventArgs<Vector2>(screenPoint));
        }

        protected void _raisePointerMovedEvent(EventArgs<Vector2> eventArgs) {

            PointerMoved?.Invoke(this, eventArgs);
        }

        protected void _raisePointerUpEvent(PointerEventArgs eventArgs) {

            PointerUp?.Invoke(this, eventArgs);
        }

        protected void _raiseScrolledEvent(ScrolledEventArgs eventArgs) {

            Scrolled?.Invoke(this, eventArgs);
        }

        void Update() => _raisePointerMovedIfNeeded();

    // Code specific to Microsoft's Mixed Reality Toolkit.
    #if VUPLEX_MRTK

        bool _beganDragEmitted;

        /// <see cref="IMixedRealityPointerHandler"/>
        public void OnPointerClicked(MixedRealityPointerEventData eventData) {}

        /// <see cref="IMixedRealityPointerHandler"/>
        public void OnPointerDragged(MixedRealityPointerEventData eventData) {

            var eventArgs = _convertToEventArgs(eventData.Pointer.Result.Details.Point);
            if (_beganDragEmitted) {
                _raiseDraggedEvent(eventArgs);
            } else {
                _beganDragEmitted = true;
                _raiseBeganDragEvent(eventArgs);
            }
        }

        /// <see cref="IMixedRealityPointerHandler"/>
        public void OnPointerDown(MixedRealityPointerEventData eventData) {

            // Set IsTargetPositionLockedOnFocusLock to false, or else the Point
            // coordinates will be locked and won't change in OnPointerDragged or OnPointerUp.
            eventData.Pointer.IsTargetPositionLockedOnFocusLock = false;
            _beganDragEmitted = false;
            var screenPoint = _convertToNormalizedPoint(eventData.Pointer.Result.Details.Point);
            _raisePointerDownEvent(new PointerEventArgs { Point = screenPoint });
        }

        /// <see cref="IMixedRealityPointerHandler"/>
        public void OnPointerUp(MixedRealityPointerEventData eventData) {

            var screenPoint = _convertToNormalizedPoint(eventData.Pointer.Result.Details.Point);
            _raisePointerUpEvent(new PointerEventArgs { Point = screenPoint });
        }

        void Start() {

            WebViewLogger.Log("Just a heads-up: please ignore the warning 'BoxCollider is null...' warning from MRTK. WebViewPrefab doesn't use a BoxCollider, so it sets the bounds of NearInteractionTouchable manually, but MRTK doesn't provide a way to disable the warning.");
            // Add a NearInteractionTouchable script to allow touch interactions
            // to trigger the IMixedRealityPointerHandler methods.
            var touchable = gameObject.AddComponent<NearInteractionTouchable>();
            touchable.EventsToReceive = TouchableEventType.Pointer;
            touchable.SetBounds(Vector2.one);
            touchable.SetLocalForward(new Vector3(0, 0, -1));
        }
    #endif
    }
}
