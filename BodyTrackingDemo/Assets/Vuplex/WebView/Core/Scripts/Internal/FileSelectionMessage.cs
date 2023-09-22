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
#pragma warning disable CS0649
using System;
using UnityEngine;

namespace Vuplex.WebView.Internal {

    /// <summary>
    /// A class used internally to pass messages about file selection
    /// from the native plugins to the webview scripts.
    /// </summary>
    [Serializable]
    public class FileSelectionMessage {

        public string[] AcceptFilters;

        public bool MultipleAllowed;

        public static FileSelectionMessage FromJson(string json) {

            return JsonUtility.FromJson<FileSelectionMessage>(json);
        }
    }
}

