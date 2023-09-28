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
    /// An interface implemented by a webview if it supports file downloads.
    /// </summary>
    /// <remarks>
    /// When downloads are enabled enabled, files are downloaded to Application.temporaryCachePath, but you can move them to a different
    /// location after they finish downloading.
    /// </remarks>
    /// <example>
    /// <code>
    /// await webViewPrefab.WaitUntilInitialized();
    /// var webViewWithDownloads = webViewPrefab.WebView as IWithDownloads;
    /// if (webViewWithDownloads != null) {
    ///     webViewWithDownloads.SetDownloadsEnabled(true);
    ///     webViewWithDownloads.DownloadProgressChanged += (sender, eventArgs) => {
    ///         Debug.Log(
    ///             $@"DownloadProgressChanged:
    ///             Type: {eventArgs.Type},
    ///             Url: {eventArgs.Url},
    ///             Progress: {eventArgs.Progress},
    ///             Id: {eventArgs.Id},
    ///             FilePath: {eventArgs.FilePath},
    ///             ContentType: {eventArgs.ContentType}"
    ///         );
    ///         if (eventArgs.Type == ProgressChangeType.Finished) {
    ///             Debug.Log("Download finished");
    ///             File.Move(eventArgs.FilePath, someOtherLocation);
    ///         }
    ///     };
    /// }
    /// </code>
    /// </example>
    public interface IWithDownloads {

        /// <summary>
        /// Sets whether file downloads are enabled. The default is disabled.
        /// </summary>
        void SetDownloadsEnabled(bool enabled);

        /// <summary>
        /// Indicates that the progress of a file download changed.
        /// </summary>
        event EventHandler<DownloadChangedEventArgs> DownloadProgressChanged;
    }
}
