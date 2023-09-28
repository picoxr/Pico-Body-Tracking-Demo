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
using System.Threading.Tasks;

namespace Vuplex.WebView {

    /// <summary>
    /// An interface implemented by a webview if it supports finding text in the page.
    /// </summary>
    /// <example>
    /// <code>
    /// await webViewPrefab.WaitUntilInitialized;
    /// // Search for the word "and" in the page.
    /// var webViewWithFind = webViewPrefab.WebView as IWithFind;
    /// if (webViewWithFind == null) {
    ///     Debug.Log("This 3D WebView plugin doesn't yet support IWithFind: " + webViewPrefab.WebView.PluginType);
    ///     return;
    /// }
    /// var result = await webViewWithFind.Find("and", true);
    /// Debug.Log($"Number of matches: {result.MatchCount}. Index of current match: {result.CurrentMatchIndex}");
    /// if (result.MatchCount > 1) {
    ///     // Later, scroll to the next instance of "and" in the page.
    ///     await webViewWithFind.Find("and", true);
    ///     // Later, go back to the first match.
    ///     await webViewWithFind.Find("and", false);
    /// }
    /// // Later, clear all matches.
    /// webViewWithFind.ClearFindMatches();
    /// </code>
    /// </example>
    public interface IWithFind {

        /// <summary>
        /// Clears the visual indicator of matches triggered by a previous call to Find().
        /// </summary>
        void ClearFindMatches();

        /// <summary>
        /// Finds the given text in the page. If the page contains a match for the given text,
        /// then the browser scrolls to and highlights that match. If the returned FindResult
        /// indicates that there are matches, then the application can call Find() again
        /// with the same text to scroll to the next or previous match, as determined by
        /// the `forward` parameter. Highlighted matches can be cleared by calling
        /// ClearFindMatches().
        /// </summary>
        Task<FindResult> Find(string text, bool forward);
    }

    /// <summary>
    /// The result of a call to IWithFind.Find().
    /// </summary>
    public struct FindResult {

        /// <summary>
        /// The index of the current highlighted match.
        /// </summary>
        public int CurrentMatchIndex;

        /// <summary>
        /// The total number of matches for the given text.
        /// </summary>
        public int MatchCount;

        public override string ToString() => $"MatchCount: {MatchCount}, CurrentMatchIndex: {CurrentMatchIndex}";
    }
}
