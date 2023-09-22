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
    /// Event args for `ScriptAlerted`. The `Continue` callback must
    /// be called to continue script execution.
    /// </summary>
    public class ScriptDialogEventArgs : EventArgs {

        public ScriptDialogEventArgs(string message, Action continueCallback) {

            Message = message;
            Continue = continueCallback;
        }

        /// <summary>
        /// The event's message.
        /// </summary>
        public readonly string Message;

        /// <summary>
        /// The callback to invoke to continue script execution.
        /// </summary>
        public readonly Action Continue;
    }

    /// <summary>
    /// Generic event args for script dialog events like
    /// `ScriptConfirmRequested`. The type parameter
    /// determines the data type that must be passed
    /// to the `Continue` callback to continue script execution.
    /// </summary>
    public class ScriptDialogEventArgs<T> : EventArgs {

        public ScriptDialogEventArgs(string message, Action<T> continueCallback) {

            Message = message;
            Continue = continueCallback;
        }

        /// <summary>
        /// The event's message.
        /// </summary>
        public readonly string Message;

        /// <summary>
        /// The callback to invoke to continue script execution.
        /// </summary>
        public readonly Action<T> Continue;
    }
}
