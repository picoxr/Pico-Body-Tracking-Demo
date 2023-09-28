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

namespace Vuplex.WebView.Internal {

    /// <summary>
    /// Script that helps with setting the video-related shader properties on mobile.
    /// </summary>
    public class ViewportMaterialView : MonoBehaviour {

        public virtual Material Material {
            get {
                return GetComponent<Renderer>().sharedMaterial;
            }
            set {
                // Use sharedMaterial instead of material, because the latter creates copies
                // that are hard to destroy properly.
                GetComponent<Renderer>().sharedMaterial = value;
            }
        }

        /// <summary>
        /// The view's texture, which is `null` until the material has been set.
        /// </summary>
        public virtual Texture Texture {
            get {
                return GetComponent<Renderer>().sharedMaterial.mainTexture;
            }
            set {
                GetComponent<Renderer>().sharedMaterial.mainTexture = value;
            }
        }

        public virtual bool Visible {
            get {
                return GetComponent<Renderer>().enabled;
            }
            set {
                GetComponent<Renderer>().enabled = value;
            }
        }

        public virtual void SetCropRect(Rect rect) {

            GetComponent<Renderer>().sharedMaterial.SetVector("_CropRect", _rectToVector(rect));
        }

        public virtual void SetCutoutRect(Rect rect) {

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
            GetComponent<Renderer>().sharedMaterial.SetVector("_VideoCutoutRect", rectVector);
        }

        Vector4 _rectToVector(Rect rect) => new Vector4(rect.x, rect.y, rect.width, rect.height);
    }
}

