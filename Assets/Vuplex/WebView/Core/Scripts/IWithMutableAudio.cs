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
    /// An interface implemented by a webview if it supports muting audio.
    /// </summary>
    /// <example>
    /// <code>
    /// await webViewPrefab.WaitUntilInitialized();
    /// var mutableWebView = webViewPrefab.WebView as IWithMutableAudio;
    /// if (mutableWebView != null) {
    ///     mutableWebView.SetAudioMuted(true);
    /// }
    /// </code>
    /// </example>
    public interface IWithMutableAudio {

        /// <summary>
        /// Sets whether audio is muted. The default is `false`.
        /// </summary>
        void SetAudioMuted(bool muted);
    }
}
