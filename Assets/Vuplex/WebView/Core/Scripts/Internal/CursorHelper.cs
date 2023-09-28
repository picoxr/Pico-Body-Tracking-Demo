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
using System.Collections.Generic;
using UnityEngine;

namespace Vuplex.WebView.Internal {

    /// <summary>
    /// Class used internally to change the cursor icon on Windows and macOS.
    /// </summary>
    public static class CursorHelper {

        public static void SetCursorIcon(string cursorType) {

            CursorInfo cursorInfo;
            _supportedCursors.TryGetValue(cursorType, out cursorInfo);
            if (cursorInfo == null) {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                return;
            }
            if (cursorInfo.Texture == null) {
                cursorInfo.Texture = Resources.Load<Texture2D>(cursorType);
            }
            var hotspot = cursorInfo.Centered ? new Vector2(16, 16) : Vector2.zero;
            Cursor.SetCursor(cursorInfo.Texture, hotspot, CursorMode.Auto);
        }

        class CursorInfo {
            public CursorInfo(bool centered = false) {
                Centered = centered;
            }
            public bool Centered;
            public Texture2D Texture;
        }

        static Dictionary<string, CursorInfo> _supportedCursors = new Dictionary<string, CursorInfo> {
            ["pointer"] = new CursorInfo(),
            ["text"] = new CursorInfo(true)
        };
    }
}
