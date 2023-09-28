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

namespace Vuplex.WebView {

    /// <summary>
    /// Event args for IWebView.LoadProgressChanged.
    /// </summary>
    public class ProgressChangedEventArgs : EventArgs {

        public ProgressChangedEventArgs(ProgressChangeType type, float progress) {

            Type = type;
            Progress = progress;
        }

        /// <summary>
        /// The estimated load progress, normalized to a float between 0 and 1.
        /// </summary>
        public readonly float Progress;

        /// <summary>
        /// The load progress event type.
        /// </summary>
        public readonly ProgressChangeType Type;
    }
}

