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
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Vuplex.WebViewUpgrade {

    /// <summary>
    /// Detects if the Vuplex/WebView folder needs to be deleted
    /// because it contains subdirectories from old versions of
    /// 3D WebView:
    /// https://support.vuplex.com/articles/v4-changes#directory
    /// </summary>
    /// <remarks>
    /// This script is located in Plugins/Editor so that it's included in the assembly Assembly-CSharp-Editor-firstpass
    /// and can still function if there are C# compiler errors in other scripts:
    /// https://docs.unity3d.com/Manual/ScriptCompileOrderFolders.html
    /// </remarks>
    [InitializeOnLoad]
    class OldWebViewDirectoryDeleter {

        static OldWebViewDirectoryDeleter() => _askPermissionToDeleteWebViewDirectoryIfNeeded();

        static string[] _oldSubdirectoryNames = new string[] { "Editor", "Licenses", "Materials", "Plugins", "Prefabs", "Scripts", "Shaders" };

        static void _askPermissionToDeleteWebViewDirectoryIfNeeded() {

            var vuplexDirectoryPath = DirectoryDeleterUtils.FindDirectory(Path.Combine(Application.dataPath, "Vuplex"));
            if (vuplexDirectoryPath == null) {
                // The Vuplex directory couldn't be found. This can happen if the user renamed the Vuplex directory or
                // moved it to the Packages folder.
                return;
            }
            var webViewDirectoryPath = Path.Combine(vuplexDirectoryPath, "WebView");
            var oldSubdirectoriesExist = _oldSubdirectoryNames.Any(dirName => Directory.Exists(Path.Combine(webViewDirectoryPath, dirName)));
            if (!oldSubdirectoriesExist) {
                return;
            }
            var userApprovedDeletingWebViewDirectory = EditorUtility.DisplayDialog(
                "3D WebView v4 Upgrade",
                $"Thank you for upgrading to 3D WebView v4! In v4, 3D WebView changed its directory structure compared to past versions. To upgrade, we need to delete your project's existing Vuplex/WebView folder, and then you'll need to reimport this new version of 3D WebView. Do you want 3D WebView to go ahead and delete the existing Vuplex/WebView folder so that you can reimport the new version?\n\nDirectory that will be deleted: {webViewDirectoryPath}\n\nMore info: https://support.vuplex.com/articles/v4-changes#directory\n",
                "Yes, delete the old WebView folder, and then I'll reimport",
                "Cancel"
            );
            if (!userApprovedDeletingWebViewDirectory) {
                return;
            }
            try {
                Directory.Delete(webViewDirectoryPath, true);
                var metaFilePath = webViewDirectoryPath + ".meta";
                if (File.Exists(metaFilePath)) {
                    File.Delete(metaFilePath);
                }
                Debug.Log("[3D WebView] Successfully finished deleting the old Vuplex/WebView directory. To finish upgrading, please reimport the new 3D WebView package.");
            } catch (Exception ex) {
                Debug.LogError($"[3D WebView] An exception occurred while deleting the old Vuplex/WebView directory. Please manually delete the directory {webViewDirectoryPath} and then reimport the new 3D WebView package. Exception: {ex}");
            }
        }

        // Methods copied from EditorUtils.cs so that this script has no dependencies
        // on classes in the Assembly-CSharp-Editor assembly.
        static class DirectoryDeleterUtils {

            public static string FindDirectory(string expectedPath, string directoryToSearch = null, string[] ignorePaths = null) {

                if (Directory.Exists(expectedPath)) {
                    return expectedPath;
                }
                // The directory isn't in the expected location, so fall back to finding it.
                var directoryName = Path.GetFileName(expectedPath);
                if (directoryToSearch == null) {
                    directoryToSearch = Application.dataPath;
                }
                var directories = Directory.GetDirectories(directoryToSearch, directoryName, SearchOption.AllDirectories);
                if (ignorePaths != null) {
                    directories = directories.ToList().Where(d => !ignorePaths.Contains(d)).ToArray();
                }
                return _returnOnePath(directories, expectedPath, directoryToSearch, true);
            }

            static string _returnOnePath(string[] paths, string expectedPath, string directorySearched, bool isDirectory = false) {

                var itemName = isDirectory ? "directory" : "file";
                if (paths.Length == 1) {
                    return paths[0];
                }
                var targetFileOrDirectoryName = Path.GetFileName(expectedPath);
                if (paths.Length > 1) {
                    var joinedPaths = String.Join(", ", paths);
                    throw new Exception($"Unable to determine which version of the {itemName} {targetFileOrDirectoryName} to use because multiple instances ({paths.Length}) were unexpectedly found in the directory {directorySearched}. Please review the list of instances found and remove duplicates so that there is only one: {joinedPaths}");
                }
                return null;
            }
        }

    }
}
