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
    /// Utility class used internally for logging.
    /// </summary>
    public static class WebViewLogger {

        public static void Log(string message, bool enableFormatting = true) {

            if (enableFormatting) {
                Debug.Log(_format(message));
            } else {
                Debug.Log(PREFIX + message);
            }
        }

        public static void LogError(string message, bool enableFormatting = true) {

            if (enableFormatting) {
                Debug.LogError(_format(message));
            } else {
                Debug.LogError(PREFIX + message);
            }
        }

        public static void LogErrorWithoutFormatting(string message) {

            Debug.LogError(PREFIX + message);
        }

        public static void LogTip(string message) {

            #if !VUPLEX_DISABLE_TIPS
                Log("Tip: " + message);
            #endif
        }

        public static void LogWarning(string message, bool enableFormatting = true) {

            if (enableFormatting) {
                Debug.LogWarning(_format(message));
            } else {
                Debug.LogWarning(PREFIX + message);
            }
        }

        public static void LogWarningWithoutFormatting(string message) {

            Debug.LogWarning(PREFIX + message);
        }

    // Markup in log messages is only useful in the editor,
    // so omit the markup when not in the editor to prevent clutter.
    #if UNITY_EDITOR
        const string PREFIX = "<color=#12bae9ff>[3D WebView]</color> ";
        const string EM_OPENING_REPLACEMENT = "<color=#12bae9ff>";
        const string EM_CLOSING_REPLACEMENT = "</color>";
    #else
        const string PREFIX = "[3D WebView] ";
        const string EM_OPENING_REPLACEMENT = "";
        const string EM_CLOSING_REPLACEMENT = "";
    #endif

        static string _format(string originalMessage) {

            // Implement the <em> tag for highlighting.
            var formattedMessage = originalMessage;
            if (formattedMessage.Contains("<em>")) {
                formattedMessage = formattedMessage.Replace("<em>", EM_OPENING_REPLACEMENT)
                                                   .Replace("</em>", EM_CLOSING_REPLACEMENT);
            }
            return PREFIX + formattedMessage;
        }
    }
}
