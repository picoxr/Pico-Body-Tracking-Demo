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
    /// An interface implemented by a webview if it supports file selection.
    /// This interface doesn't cause a file selection UI to automatically be shown, but the application
    /// can use this interface's FileSelectionRequested event to detect when file selection is requested
    /// and then use a system API or third party asset to present the user with a file selection UI.
    /// </summary>
    /// <example>
    /// <code>
    /// await webViewPrefab.WaitUntilInitialized();
    /// var webViewWithFileSelection = webViewPrefab.WebView as IWithFileSelection;
    /// if (webViewWithFileSelection != null) {
    ///     webViewWithFileSelection.FileSelectionRequested += (sender, eventArgs) => {
    ///         // Note: Here's where the application could use a system API or third party
    ///         //       asset to show a file selection UI and then pass the selected file(s) to
    ///         //       the Continue() callback.
    ///         var filePaths = new string[] { "C:\\Users\\YourUser\\Desktop\\selected-file.txt" };
    ///         eventArgs.Continue(filePaths);
    ///     };
    /// }
    /// </code>
    /// </example>
    public interface IWithFileSelection {

        /// <summary>
        /// Indicates that the page requested a file selection dialog. This can happen, for example, when a file input
        /// is activated. Call the event args' Continue(filePaths) callback to provide a file selection or call
        /// Cancel() to cancel file selection.
        /// </summary>
        event EventHandler<FileSelectionEventArgs> FileSelectionRequested;
    }
}
