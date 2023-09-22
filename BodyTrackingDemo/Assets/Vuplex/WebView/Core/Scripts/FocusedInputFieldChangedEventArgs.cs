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
using Vuplex.WebView.Internal;

namespace Vuplex.WebView {

    /// <summary>
    /// Event args for FocusedInputFieldChanged.
    /// </summary>
    public class FocusedInputFieldChangedEventArgs : EventArgs {

        public FocusedInputFieldChangedEventArgs(FocusedInputFieldType type) => Type = type;

        /// <summary>
        /// The type of input field focused.
        /// </summary>
        public readonly FocusedInputFieldType Type;

        public static FocusedInputFieldType ParseType(string typeString) {

            switch (typeString) {
                case "TEXT":
                    return FocusedInputFieldType.Text;
                case "NONE":
                    return FocusedInputFieldType.None;
                default:
                    WebViewLogger.LogWarning("Unrecognized FocusedInputFieldType string: " + typeString);
                    return FocusedInputFieldType.None;
            }
        }
    }
}
