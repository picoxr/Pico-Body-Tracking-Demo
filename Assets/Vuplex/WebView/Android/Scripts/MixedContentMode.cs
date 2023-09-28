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
    /// Options for the webview's behavior when a secure origin attempts to load a resource from an insecure origin.
    /// </summary>
    public enum MixedContentMode {

        /// <summary>
        /// In this mode, the webview will allow a secure origin to load content from any other origin, even if that origin is insecure.
        /// </summary>
        AlwaysAllow = 0,

        /// <summary>
        /// In this mode, the webview will not allow a secure origin to load content from an insecure origin.
        /// </summary>
        NeverAllow = 1,

        /// <summary>
        /// In this mode, the webview will attempt to be compatible with the approach of a modern web browser with regard to mixed content.
        /// Some insecure content may be allowed to be loaded by a secure origin and other types of content will be blocked.
        /// The types of content are allowed or blocked may change release to release and are not explicitly defined.
        /// This mode is intended to be used by apps that are not in control of the content that they render but desire to operate in a reasonably secure environment.
        /// </summary>
        CompatibilityMode = 2
    }
}
