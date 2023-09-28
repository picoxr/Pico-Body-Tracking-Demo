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
    /// Event args for `ConsoleMessageLogged`.
    /// </summary>
    public class ConsoleMessageEventArgs : EventArgs {

        public ConsoleMessageEventArgs(ConsoleMessageLevel level, string message, string source, int line) {

            Level = level;
            Message = message;
            Source = source;
            Line = line;
        }

        /// <summary>
        /// The message's log level.
        /// </summary>
        public readonly ConsoleMessageLevel Level;

        /// <summary>
        /// The message logged to the JavaScript console.
        /// </summary>
        public readonly string Message;

        /// <summary>
        /// The name of the file from which the message was logged,
        /// or `null` if the source is unknown.
        /// </summary>
        public readonly string Source;

        /// <summary>
        /// The line number of the file from which the message was logged,
        /// or `0` if the source is unknown.
        /// </summary>
        public readonly int Line;
    }
}
