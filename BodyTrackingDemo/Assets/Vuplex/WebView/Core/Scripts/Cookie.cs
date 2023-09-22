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
using UnityEngine;
using Vuplex.WebView.Internal;

namespace Vuplex.WebView {

    /// <summary>
    /// An HTTP cookie.
    /// </summary>
    [Serializable]
    public class Cookie {

        /// <summary>
        /// The name of the cookie.
        /// </summary>
        public string Name;

        /// <summary>
        /// The value of the cookie.
        /// </summary>
        public string Value;

        /// <summary>
        /// The domain to which the cookie belongs (e.g. "www.vuplex.com", "example.com").
        /// </summary>
        public string Domain;

        /// <summary>
        /// The URL path of the cookie (e.g. "/", "/products/1234").
        /// The default is "/".
        /// </summary>
        public string Path = "/";

        /// <summary>
        /// A number representing the expiration date of the cookie as the number of seconds since the UNIX epoch, or 0
        /// if there is no expiration date. An expiration date is not provided for session cookies.
        /// </summary>
        public int ExpirationDate;

        /// <summary>
        /// Indicates whether the cookie is marked as HttpOnly (i.e. the cookie is inaccessible to client-side scripts).
        /// </summary>
        public bool HttpOnly;

        /// <summary>
        /// Indicates whether cookie is marked as secure (i.e. its scope is limited to secure channels, typically HTTPS).
        /// </summary>
        public bool Secure;

        /// <summary>
        /// Indicates whether the cookie is valid.
        /// </summary>
        public bool IsValid {
            get {
                var isValid = true;
                if (Name == null) {
                    WebViewLogger.LogWarning("Invalid value for Cookie.Name: " + Name);
                    isValid = false;
                }
                if (Value == null) {
                    WebViewLogger.LogWarning("Invalid value for Cookie.Value: " + Value);
                    isValid = false;
                }
                if (Domain == null || !Domain.Contains(".") || Domain.Contains("/")) {
                    WebViewLogger.LogWarning("Invalid value for Cookie.Domain: " + Domain);
                    isValid = false;
                }
                if (Path == null) {
                    WebViewLogger.LogWarning("Invalid value for Cookie.Path: " + Path);
                    isValid = false;
                }
                return isValid;
            }
        }

        /// <summary>
        /// Deserializes a Cookie array from JSON.
        /// </summary>
        public static Cookie[] ArrayFromJson(string serializedCookies) {

            if (serializedCookies == "null") {
                return new Cookie[0];
            }
            var cookiesWrapper = JsonUtility.FromJson<JsonArrayWrapper<Cookie>>(serializedCookies);
            var cookies = cookiesWrapper.Items ?? new Cookie[0];
            return cookies;
        }

        /// <summary>
        /// Deserializes a Cookie from JSON.
        /// </summary>
        public static Cookie FromJson(string serializedCookie) {

            if (serializedCookie == "null") {
                return null;
            }
            return JsonUtility.FromJson<Cookie>(serializedCookie);
        }

        /// <summary>
        /// Serializes the instance to JSON.
        /// </summary>
        public string ToJson() => JsonUtility.ToJson(this);

        public override string ToString() => ToJson();
    }
}
