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
#pragma warning disable CS0649
using System;
using UnityEngine;

namespace Vuplex.WebView.Internal {

    /// <summary>
    /// Used internally for implementing `IWebView.ConsoleMessageLogged`.
    /// </summary>
    [Serializable]
    class ConsoleBridgeMessage : BridgeMessage {

        public string message;

        public string level;

        public string source;

        public int line;

        public ConsoleMessageEventArgs ToEventArgs() {

            return new ConsoleMessageEventArgs(_parseMessageLevel(level), message, source, line);
        }

        ConsoleMessageLevel _parseMessageLevel(string levelString) {
            switch (levelString) {
                case "LOG":
                    return ConsoleMessageLevel.Log;
                case "DEBUG":
                    return ConsoleMessageLevel.Debug;
                case "WARNING":
                    return ConsoleMessageLevel.Warning;
                case "ERROR":
                    return ConsoleMessageLevel.Error;
                default:
                    WebViewLogger.LogError("Unrecognized ConsoleMessageLevel string: " + levelString);
                    return ConsoleMessageLevel.Log;
            }
        }
    }
}

