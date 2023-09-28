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
using UnityEngine.UI;

namespace Vuplex.WebView.Internal {

    public class CanvasViewportMaterialView : ViewportMaterialView {

        public override Material Material {
            get {
                return GetComponent<RawImage>().material;
            }
            set {
                GetComponent<RawImage>().material = value;
                if (value.mainTexture != null) {
                    GetComponent<RawImage>().texture = value.mainTexture;
                }
            }
        }

        public override Texture Texture {
            get {
                return GetComponent<RawImage>().material.mainTexture;
            }
            set {
                GetComponent<RawImage>().material.mainTexture = value;
                // Also set RawImage.texture because updating just RawImage.material.mainTexture
                // doesn't work for changing the texture for IWithChangingTexture.
                GetComponent<RawImage>().texture = value;
            }
        }

        public override bool Visible {
            get {
                return GetComponent<RawImage>().enabled;
            }
            set {
                GetComponent<RawImage>().enabled = value;
            }
        }

        public override void SetCropRect(Rect rect) => _setShaderProperty("_CropRect", _rectToVector(rect));

        public override void SetCutoutRect(Rect rect) {

            var rectVector = _rectToVector(rect);
            if (rect != new Rect(0, 0, 1, 1)) {
                // Make the actual cutout slightly smaller (2% shorter and 2% skinnier) so that
                // the gap between the video layer and the viewport isn't visible.
                // This is only done if the rect doesn't cover the entire view, because
                // the Keyboard component uses a rect cutout of the entire view for Android Gecko.
                var onePercentOfWidth = rect.width * 0.01f;
                var onePercentOfHeight = rect.height * 0.01f;
                rectVector = new Vector4(
                    rectVector.x + onePercentOfWidth,
                    rectVector.y + onePercentOfHeight,
                    rectVector.z - 2 * onePercentOfWidth,
                    rectVector.w - 2 * onePercentOfHeight
                );
            }
            _setShaderProperty("_VideoCutoutRect", rectVector);
        }

        Vector4 _rectToVector(Rect rect) => new Vector4(rect.x, rect.y, rect.width, rect.height);

        void _setShaderProperty(string propertyName, Vector4 value) {

            // Important: Use RawImage.materialForRendering instead of RawImage.material, or else
            // the property won't be set correctly when the webview is masked by a UI Mask.
            GetComponent<RawImage>().materialForRendering.SetVector(propertyName, value);
        }
    }
}

