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
    /// Event args for FileSelectionRequested. To handle file selection, the application
    /// must either call Continue() to select files or call Cancel() to cancel file selection.
    /// </summary>
    public class FileSelectionEventArgs : EventArgs {

        public FileSelectionEventArgs(string[] acceptFilters, bool multipleAllowed, Action<string[]> continueCallback, Action cancelCallback) {

            AcceptFilters = acceptFilters;
            MultipleAllowed = multipleAllowed;
            Continue = continueCallback;
            Cancel = cancelCallback;
        }

        /// <summary>
        /// Filters provided by the page to specify the allowed file types. If the page didn't specify
        /// any file types, then this array is empty.
        /// </summary>
        /// <seealso href="https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input/file#accept">
        /// MDN's documentation for the file input `accept` attribute
        /// </seealso>
        public readonly string[] AcceptFilters;

        /// <summary>
        /// Indicates whether multiple files are permitted.
        /// </summary>
        /// <seealso href="https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input/file#multiple">
        /// MDN's documentation for the file input `multiple` attribute
        /// </seealso>
        public readonly bool MultipleAllowed;

        /// <summary>
        /// To select files, call this callback with an array of one or more absolute file paths.
        /// </summary>
        public readonly Action<string[]> Continue;

        /// <summary>
        /// Call this callback to cancel file selection.
        /// </summary>
        public readonly Action Cancel;
    }
}
