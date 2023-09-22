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
    /// Mock ICookieManager implementation used for running in the Unity editor.
    /// </summary>
    class MockCookieManager : ICookieManager {

        public static MockCookieManager Instance {
            get {
                if (_instance == null) {
                    _instance = new MockCookieManager();
                }
                return _instance;
            }
        }

        public Task<bool> DeleteCookies(string url, string cookieName = null) => MockWebView.DeleteCookies(url, cookieName);

        public Task<Cookie[]> GetCookies(string url, string cookieName = null) => MockWebView.GetCookies(url, cookieName);

        public Task<bool> SetCookie(Cookie cookie) => MockWebView.SetCookie(cookie);

        static MockCookieManager _instance;
    }
}
