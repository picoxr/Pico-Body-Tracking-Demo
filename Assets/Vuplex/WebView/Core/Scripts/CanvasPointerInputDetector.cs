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
    public class CanvasPointerInputDetector : DefaultPointerInputDetector {

        RectTransform _cachedRectTransform;
        CachingGetter<Canvas> _canvasGetter;

        protected override Vector2 _convertToNormalizedPoint(PointerEventData pointerEventData) {

            if (_canvasGetter == null) {
                 _canvasGetter = new CachingGetter<Canvas>(GetComponentInParent<Canvas>, 1, this);
            }
            var canvas = _canvasGetter.GetValue();
            var camera = canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            Vector2 localPoint;
            var mousePosition = pointerEventData.position;
            #if (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX) && !UNITY_EDITOR
                // To handle multiple displays on Windows and macOS, Display.RelativeMouseAt() must be used
                // to translate the mouse position. However, Unity's UI system still has a limitation where
                // this may not work when the monitors have different sizes / resolutions.
                // - https://issuetracker.unity3d.com/issues/buttons-hitbox-is-offset-when-building-standalone-project-for-two-screens
                var positionForDisplay = Display.RelativeMouseAt(new Vector3(mousePosition.x, mousePosition.y));
                // RelativeMouseAt() returns Vector3.zero when multiple displays aren't supported.
                if (positionForDisplay != Vector3.zero) {
                    mousePosition = new Vector2(positionForDisplay.x, positionForDisplay.y);
                }
            #endif
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_getRectTransform(), mousePosition, camera, out localPoint);
            return _convertToNormalizedPoint(localPoint);
        }

        protected override Vector2 _convertToNormalizedPoint(Vector3 worldPosition) {

            var localPoint = _getRectTransform().InverseTransformPoint(worldPosition);
            return _convertToNormalizedPoint(localPoint);
        }

        Vector2 _convertToNormalizedPoint(Vector2 localPoint) {

            var normalizedPoint = Rect.PointToNormalized(_getRectTransform().rect, localPoint);
            normalizedPoint.y = 1 - normalizedPoint.y;
            return normalizedPoint;
        }

        RectTransform _getRectTransform() {

            if (_cachedRectTransform == null) {
                _cachedRectTransform = GetComponent<RectTransform>();
            }
            return _cachedRectTransform;
        }

        protected override bool _positionIsZero(PointerEventData eventData) => eventData.position == Vector2.zero;

    // Code specific to Microsoft's Mixed Reality Toolkit.
    #if VUPLEX_MRTK
        void Start() {
            // Add a NearInteractionTouchable script to allow touch interactions
            // to trigger the IMixedRealityPointerHandler methods.
            var touchable = gameObject.AddComponent<NearInteractionTouchableUnityUI>();
            touchable.EventsToReceive = TouchableEventType.Pointer;
        }
    #endif
    }
}
