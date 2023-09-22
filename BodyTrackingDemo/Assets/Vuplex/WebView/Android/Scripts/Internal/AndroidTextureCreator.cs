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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace Vuplex.WebView.Internal {

    public class AndroidTextureCreator : MonoBehaviour {

        public static AndroidTextureCreator Instance {
            get {
                if (!_instance) {
                    _instance = (AndroidTextureCreator) new GameObject("AndroidTextureCreator").AddComponent<AndroidTextureCreator>();
                    DontDestroyOnLoad(_instance.gameObject);
                }
                return _instance;
            }
        }

        public Task<Texture2D> CreateTexture(int width, int height) {

            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Vulkan) {
                return Task.FromResult(VXUtils.CreateDefaultTexture(width, height));
            }
            var error = VXUtils.GetGraphicsApiErrorMessage(SystemInfo.graphicsDeviceType, new GraphicsDeviceType[] { GraphicsDeviceType.OpenGLES3, GraphicsDeviceType.OpenGLES2 });
            if (error != null) {
                throw new InvalidOperationException(error);
            }
            var taskSource = new TaskCompletionSource<Texture2D>();
            // Textures must be created on the render thread, so we send the arguments to the
            // native code, which queues the invocation so that the texture can be created on
            // the next render pass.
            var invocation = new TextureCreatorInvocation {
                ID = _nextInvocationID++,
                Width = width,
                Height = height,
                Callback = taskSource.SetResult
            };
            _pendingInvocations.Add(invocation);
            _invocationIDsToTrigger.Add(invocation.ID);
            return taskSource.Task;
        }

        struct TextureCreatorInvocation {
            public int ID;
            public int Width;
            public int Height;
            public Action<Texture2D> Callback;
        }

        static AndroidTextureCreator _instance;
        List<int> _invocationIDsToTrigger = new List<int>();
        int _nextInvocationID;
        List<TextureCreatorInvocation> _pendingInvocations = new List<TextureCreatorInvocation>();
        readonly WaitForEndOfFrame _waitForEndOfFrame = new WaitForEndOfFrame();

        IEnumerator _callPluginOncePerFrame() {

            while (true) {
                yield return _waitForEndOfFrame;

                if (_invocationIDsToTrigger.Count > 0) {
                    foreach (var invocationID in _invocationIDsToTrigger) {
                        GL.IssuePluginEvent(WebView_getCreateTextureFunction(), invocationID);
                    }
                    _invocationIDsToTrigger.Clear();
                }
            }
        }

        // Invoked by the native plugin.
        void HandleTextureCreated(string parameterString) {

            var parameters = parameterString.Split(new char[]{';'});
            var invocationID = int.Parse(parameters[0]);
            var nativeTexture = new IntPtr(int.Parse(parameters[1]));
            var invocation = _pendingInvocations.Find(i => i.ID == invocationID);
            _pendingInvocations.Remove(invocation);
            Texture2D texture = Texture2D.CreateExternalTexture(
                invocation.Width,
                invocation.Height,
                TextureFormat.RGBA32,
                false,
                false,
                nativeTexture
            );
            invocation.Callback(texture);
        }

        void Start() => StartCoroutine(_callPluginOncePerFrame());

        [DllImport(AndroidWebView.DllName)]
        static extern IntPtr WebView_getCreateTextureFunction();
    }
}
#endif
