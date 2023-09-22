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
#if UNITY_ANDROID && !UNITY_EDITOR
using System.Threading.Tasks;

namespace Vuplex.WebView {

    /// <summary>
    /// The Android ICookieManager implementation.
    /// </summary>
    public class AndroidCookieManager : ICookieManager {

        public static AndroidCookieManager Instance {
            get {
                if (_instance == null) {
                    _instance = new AndroidCookieManager();
                }
                return _instance;
            }
        }

        public Task<bool> DeleteCookies(string url, string cookieName = null) => AndroidWebView.DeleteCookies(url, cookieName);

        public Task<Cookie[]> GetCookies(string url, string cookieName = null) => AndroidWebView.GetCookies(url, cookieName);

        public Task<bool> SetCookie(Cookie cookie) => AndroidWebView.SetCookie(cookie);

        static AndroidCookieManager _instance;
    }
}
#endif
