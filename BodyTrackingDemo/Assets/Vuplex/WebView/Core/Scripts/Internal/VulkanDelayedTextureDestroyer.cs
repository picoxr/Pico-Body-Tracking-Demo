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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vuplex.WebView.Internal {

    /// <summary>
    /// Helper classed used to wait a short delay before deleting Vulkan textures.
    /// </summary>
    /// <remarks>
    /// Due to the workaround for the Unity bug where Texture2D.UpdateExternalTexture() doesn't work
    /// (see the comments in IWithChangingTexture.cs for details of that), new Texture2D instances
    /// are frequently created and used to replace the existing web texture. To prevent this from
    /// causing flickering, this class waits a short delay before deleting old textures.
    /// </remarks>
    class VulkanDelayedTextureDestroyer : MonoBehaviour {

        public static VulkanDelayedTextureDestroyer GetInstance(Action<IntPtr> destroyVulkanTextureFunction) {

            if (!_instance) {
                _instance = (VulkanDelayedTextureDestroyer)new GameObject("VulkanDelayedTextureDestroyer").AddComponent<VulkanDelayedTextureDestroyer>();
                _instance._destroyVulkanTextureFunction = destroyVulkanTextureFunction;
                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }

        public void DestroyTexture(IntPtr nativeTexture) {

            _textureLists[_indexOfTextureListActiveForDiscarding].Add(nativeTexture);
        }

        Action<IntPtr> _destroyVulkanTextureFunction;
        int _indexOfTextureListActiveForDiscarding = 0;
        static VulkanDelayedTextureDestroyer _instance;
        // To ensure a delay, toggle between two lists. Use one list for discarding
        // textures while the textures in the other list are being destroyed.
        List<IntPtr>[] _textureLists = new List<IntPtr>[] {
            new List<IntPtr>(),
            new List<IntPtr>()
        };
        WaitForSeconds _waitShortDelay = new WaitForSeconds(0.5f);

        void OnEnable() => StartCoroutine(_destroyTexturesPeriodically());

        IEnumerator _destroyTexturesPeriodically() {

            while (true) {
                yield return _waitShortDelay;
                var indexOfTextureListToDestroy = 1 - _indexOfTextureListActiveForDiscarding;
                var texturesToDestroy = _textureLists[indexOfTextureListToDestroy];
                foreach (var nativeTexture in texturesToDestroy) {
                    _destroyVulkanTextureFunction(nativeTexture);
                }
                texturesToDestroy.Clear();
                _indexOfTextureListActiveForDiscarding = indexOfTextureListToDestroy;
            }
        }
    }
}
