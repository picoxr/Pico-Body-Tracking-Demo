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
namespace Vuplex.WebView {

    /// <summary>
    /// Indicates the type of input field focused.
    /// </summary>
    public enum FocusedInputFieldType {

        /// <summary>
        /// Indicates that a text input field is focused. Examples of a text input field include
        /// an `input` element, a `textarea` element, and an element with a `contentEditable`
        /// attribute.
        /// </summary>
        Text,

        /// <summary>
        /// Indicates that no input field is focused.
        /// </summary>
        None
    }
}
