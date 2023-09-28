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
using System.Threading.Tasks;

namespace Vuplex.WebView {

    /// <summary>
    /// Provides methods for getting, setting, and deleting HTTP cookies.
    /// You can access the ICookieManager via Web.CookieManager.
    /// </summary>
    /// <remarks>
    /// When developing code that interacts with
    /// cookies, it may also be helpful to view a webview's cookies using
    /// [remote debugging](https://support.vuplex.com/articles/how-to-debug-web-content).
    /// </remarks>
    public interface ICookieManager {

        /// <summary>
        /// Deletes all of the cookies that match the given URL and returns a
        /// Task&lt;bool&gt; indicating whether the deletion succeeded. A `cookieName`
        /// can be optionally passed as a second parameter to further filter
        /// to a specific cookie.
        /// If a deletion fails, it could be because the URL was invalid.
        /// For more details regarding a failure, check the Unity logs.
        /// </summary>
        /// <remarks>
        /// Important notes:
        /// <list type="bullet">
        ///   <item>On Windows and macOS, if this method is called without a `cookieName` it only deletes cookies that were set without an explicit Domain attribute.</item>
        ///   <item>On versions of iOS older than iOS 11, session cookies are excluded because WKHTTPCookieStore is only supported in iOS 11 and newer.</item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <code>
        /// if (Web.CookieManager == null) {
        ///     Debug.Log("Web.CookieManager isn't supported on this platform.");
        ///     return;
        /// }
        /// // Delete all the cookies for this cookie test page, which will reset the test.
        /// var succeeded = await Web.CookieManager.DeleteCookies("http://www.whatarecookies.com/cookietest.asp");
        /// Debug.Log("Cookie deletion succeeded: " + succeeded);
        /// </code>
        /// </example>
        Task<bool> DeleteCookies(string url, string cookieName = null);

        /// <summary>
        /// Gets all of the cookies that match the given URL. A `cookieName`
        /// can be optionally passed as a second parameter to further filter
        /// results to a specific cookie.
        /// </summary>
        /// <remarks>
        /// Important notes:
        /// <list type="bullet">
        ///   <item>On Android, the cookies returned only have their Name and Value fields set, and the other fields are set to their default values.</item>
        ///   <item>On versions of iOS older than iOS 11, session cookies are excluded because WKHTTPCookieStore is only supported in iOS 11 and newer.</item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <code>
        /// if (Web.CookieManager == null) {
        ///     Debug.Log("Web.CookieManager isn't supported on this platform.");
        ///     return;
        /// }
        /// // Get the cookie named "NID" set by google.com.
        /// var cookies = await Web.CookieManager.GetCookies("https://www.google.com", "NID");
        /// if (cookies.Length > 0) {
        ///     Debug.Log("Cookie: " + cookies[0]);
        /// } else {
        ///     Debug.Log("Cookie not found.");
        /// }
        /// </code>
        /// </example>
        Task<Cookie[]> GetCookies(string url, string cookieName = null);

        /// <summary>
        /// Sets the given cookie and returns a Task&lt;bool&gt; indicating
        /// whether the cookie was set successfully.
        /// If setting the cookie fails, it could be because the data in the provided Cookie
        /// was malformed. For more details regarding a failure, please check the Unity logs.
        /// </summary>
        /// <example>
        /// <code>
        /// if (Web.CookieManager == null) {
        ///     Debug.Log("Web.CookieManager isn't supported on this platform.");
        ///     return;
        /// }
        /// var success = await Web.CookieManager.SetCookie(new Cookie {
        ///     Domain = "vuplex.com",
        ///     Path = "/",
        ///     Name = "example_name",
        ///     Value = "example_value",
        ///     Secure = true,
        ///     // Expire one day from now
        ///     ExpirationDate = (int)DateTimeOffset.Now.ToUnixTimeSeconds() + 60 * 60 * 24
        /// });
        /// </code>
        /// </example>
        Task<bool> SetCookie(Cookie cookie);
    }
}
