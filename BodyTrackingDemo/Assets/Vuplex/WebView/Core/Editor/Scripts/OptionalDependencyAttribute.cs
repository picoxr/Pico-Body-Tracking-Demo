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
#if UNITY_EDITOR
using System;
using System.Diagnostics;
using Vuplex.WebView.ConditionalCompilation;

namespace Vuplex.WebView.Editor {

    /// <summary>
    /// From the example of how to use Unity's ConditionalCompilationUtility:
    /// https://github.com/Unity-Technologies/ConditionalCompilationUtility/tree/f364090bbda3728e1662074c969c2b7c3c34199b
    /// </summary>
    [Conditional("VUPLEX_CCU")]
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class OptionalDependencyAttribute : Attribute {
        public string dependentClass;
        public string define;

        public OptionalDependencyAttribute(string dependentClass, string define) {
            this.dependentClass = dependentClass;
            this.define = define;
        }
    }
}
#endif
