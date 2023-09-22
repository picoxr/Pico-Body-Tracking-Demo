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
    /// Unity added support for using Texture2D.CreateExternalTexture() with Vulkan in 2020.2,
    /// but support for Texture2D.UpdateExternalTexture() wasn't added until 2022.1.
    /// So, this interface is used by 3D WebView on Android to update the texture
    /// when Vulkan is used with versions of Unity prior to 2022.1.
    /// https://issuetracker.unity3d.com/issues/android-vulkan-texture2d-dot-updateexternaltexture-does-not-respond-when-a-project-is-built-on-android-with-vulkan-graphics-api
    /// </summary>
    public interface IWithChangingTexture {

        event EventHandler<EventArgs<Texture2D>> TextureChanged;
    }
}
