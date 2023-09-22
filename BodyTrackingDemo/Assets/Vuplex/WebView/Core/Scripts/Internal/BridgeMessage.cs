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

namespace Vuplex.WebView.Internal {

    /// <summary>
    /// A simple base class used for sending messages from
    /// C# to a WebView's JavaScript and vice versa as serialized JSON.
    /// </summary>
    [Serializable]
    public class BridgeMessage {

        public string type;

        public static string ParseType(string serializedMessage) {
            try {
                var message = JsonUtility.FromJson<BridgeMessage>(serializedMessage);
                return message.type;
            } catch (Exception) {
                return null;
            }
        }
    }
}

