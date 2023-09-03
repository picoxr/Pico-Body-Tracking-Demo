/* 
*   NatCorder
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatSuite.Recorders.Internal {

    using System;
    using System.IO;
    using UnityEngine;

    public static class Utility {

        private static string directory;

        public static string GetPath (string extension) {
            if (directory == null) {
                var editor = Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor;
                directory = editor ? Directory.GetCurrentDirectory() : Application.persistentDataPath;
            }
            var timestamp = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff");
            var name = $"recording_{timestamp}{extension}";
            var path = Path.Combine(directory, name);
            return path;
        }
    }
}