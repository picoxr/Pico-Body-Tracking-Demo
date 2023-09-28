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
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Vuplex.WebView.Internal;

namespace Vuplex.WebView.Demos {

    /// <summary>
    /// Script that detects keys pressed on the hardware
    /// (USB or Bluetooth) keyboard and emits the corresponding
    /// strings that can be passed to IWebView.SendKey()
    /// or IWithKeyDownAndUp.KeyDown() and KeyUp().
    /// </summary>
    public class HardwareKeyboardListener : MonoBehaviour {

        [Obsolete("The HardwareKeyboardListener.InputReceived event is now deprecated. Please use the HardwareKeyboardListener.KeyDownReceived event instead.")]
        public event EventHandler<KeyboardInputEventArgs> InputReceived {
            add {
                KeyDownReceived += value;
            }
            remove {
                KeyDownReceived -= value;
            }
        }

        public event EventHandler<KeyboardInputEventArgs> KeyDownReceived;

        public event EventHandler<KeyboardInputEventArgs> KeyUpReceived;

        public static HardwareKeyboardListener Instantiate() {

            return (HardwareKeyboardListener) new GameObject("HardwareKeyboardListener").AddComponent<HardwareKeyboardListener>();
        }

        Regex _alphanumericRegex = new Regex("[a-zA-Z0-9]");
        Func<string, bool> _hasValidUnityKeyName = _memoize<string, bool>(
            javaScriptKeyName => {
                try {
                    var unityKeyName = _getPotentialUnityKeyName(javaScriptKeyName);
                    Input.GetKey(unityKeyName);
                    return true;
                } catch {
                    return false;
                }
            }
        );
        List<string> _keysDown = new List<string>();
        // Keys that don't show up correctly in Input.inputString. Must be defined before _keyValues.
        static readonly string[] _keyValuesUndetectableThroughInputString = new string[] {
            // Note: "Backspace" is included here for the Hololens system TouchScreenKeyboard. In other scenarios, \b is detectable through Input.inputString.
            "Tab", "ArrowUp", "ArrowDown", "ArrowRight", "ArrowLeft", "Escape", "Delete", "Home", "End", "Insert", "PageUp", "PageDown", "Help", "Backspace"
        };
        static readonly string[] _keyValues = new string[] {
            "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "`", "-", "=", "[", "]", "\\", ";", "'", ",", ".", "/", " ", "Enter"
        }.Concat(_keyValuesUndetectableThroughInputString).ToArray();

        bool _areKeysUndetectableThroughInputStringPressed() {

            foreach (var key in _keyValuesUndetectableThroughInputString) {
                // Use GetKey instead of GetKeyDown because on macOS, Input.inputString
                // contains garbage when the arrow keys are held down.
                if (Input.GetKey(_getPotentialUnityKeyName(key))) {
                    return true;
                }
            }
            return false;
        }

        KeyModifier _getModifiers() {

            var modifiers = KeyModifier.None;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                modifiers |= KeyModifier.Shift;
            }
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
                modifiers |= KeyModifier.Control;
            }
            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) {
                modifiers |= KeyModifier.Alt;
            }
            if (Input.GetKey(KeyCode.LeftWindows) ||
                Input.GetKey(KeyCode.RightWindows)) {
                modifiers |= KeyModifier.Meta;
            }
            // Don't pay attention to the command keys on Windows because Unity has a bug
            // where it falsly reports the command keys are pressed after switching languages
            // with the windows+space shortcut.
            #if !(UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN)
                if (Input.GetKey(KeyCode.LeftCommand) ||
                    Input.GetKey(KeyCode.RightCommand)) {
                    modifiers |= KeyModifier.Meta;
                }
            #endif

            return modifiers;
        }

        // https://docs.unity3d.com/Manual/class-InputManager.html#:~:text=the%20Input%20Manager.-,Key%20names,-follow%20these%20naming
        static string _getPotentialUnityKeyName(string javaScriptKeyValue) {

            switch (javaScriptKeyValue) {
                case " ":
                    return "space";
                case "Alt":
                    return "left alt";
                case "ArrowUp":
                    return "up";
                case "ArrowDown":
                    return "down";
                case "ArrowRight":
                    return "right";
                case "ArrowLeft":
                    return "left";
                case "Control":
                    return "left ctrl";
                case "Enter":
                    return "return";
                case "Meta":
                    return "left cmd";
                case "PageUp":
                    return "page up";
                case "PageDown":
                    return "page down";
                case "Shift":
                    return "left shift";
            }
            return javaScriptKeyValue.ToLowerInvariant();
        }

        /// <summary>
        /// Returns a memoized version of the given function.
        /// </summary>
        static Func<TArg, TReturn> _memoize<TArg, TReturn>(Func<TArg, TReturn> function) {

            var cache = new Dictionary<TArg, TReturn>();
            return arg => {
                TReturn result;
                if (cache.TryGetValue(arg, out result)) {
                    return result;
                }
                result = function(arg);
                cache.Add(arg, result);
                return result;
            };
        }

        bool _processInputString(KeyModifier modifiers) {

            var keyDownHandler = KeyDownReceived;
            foreach (var character in Input.inputString) {
                string characterString;
                switch (character) {
                    case '\b':
                        characterString = "Backspace";
                        break;
                    case '\n':
                    case '\r':
                        characterString = "Enter";
                        break;
                    case (char)0xF728:
                        // 0xF728 = NSDeleteFunctionKey on macOS
                        characterString = "Delete";
                        break;
                    default:
                        characterString = character.ToString();
                        break;
                }
                // For some keyboard layouts like AZERTY (e.g. French), Input.inputString will contain
                // the correct character for a ctr+alt+{} key combination (e.g. ctrl+alt+0 makes Input.inputString equal "@"), but
                // Input.GetKeyUp() won't return true for that key when the key combination is released
                // (e.g. Input.GetKeyUp("@") always returns false). So, as a workaround, we emit
                // the KeyUpReceived event immediately in that scenario instead of adding it to _keysDown.
                var skipGetKeyUpBecauseUnityBug = modifiers != KeyModifier.None && characterString.Length == 1 && !_alphanumericRegex.IsMatch(characterString);
                // We also need to skip calling Input.GetKeyUp() if the character isn't compatible with GetKeyUp(). For example, on
                // Azerty keyboards, the 2 key (without modifiers) triggers "é", which can't be passed to GetKeyUp().
                var skipGetKeyUpBecauseIncompatibleCharacter = !_hasValidUnityKeyName(characterString);
                if (skipGetKeyUpBecauseUnityBug || skipGetKeyUpBecauseIncompatibleCharacter) {
                    if (keyDownHandler != null) {
                        keyDownHandler(this, new KeyboardInputEventArgs(characterString, KeyModifier.None));
                    }
                    var keyUpHandler = KeyUpReceived;
                    if (keyUpHandler != null) {
                        keyUpHandler(this, new KeyboardInputEventArgs(characterString, KeyModifier.None));
                    }
                } else {
                    if (keyDownHandler != null) {
                        keyDownHandler(this, new KeyboardInputEventArgs(characterString, modifiers));
                    }
                    // It's a character that works with Input.GetKeyUp(), so add it to _keysDown.
                    _keysDown.Add(characterString);
                }
            }
            return Input.inputString.Length > 0;
        }

        void _processKeysPressed(KeyModifier modifiers) {

            if (!(Input.anyKeyDown || Input.inputString.Length > 0)) {
                return;
            }
            var nonInputStringKeysDetected = _processKeysUndetectableThroughInputString(modifiers);
            if (nonInputStringKeysDetected) {
                return;
            }
            // Using Input.inputString when possible is preferable since it
            // handles different languages and characters that would be hard
            // to support using Input.GetKeyDown().
            var inputStringKeysDetected = _processInputString(modifiers);
            if (inputStringKeysDetected) {
                return;
            }
            // If we've made it to this point, then only modifier keys by themselves have been pressed.
            _processModifierKeysOnly(modifiers);
        }

        void _processKeysReleased(KeyModifier modifiers) {

            if (_keysDown.Count == 0) {
                return;
            }
            var keysDownCopy = new List<string>(_keysDown);
            foreach (var key in keysDownCopy) {
                bool keyUp = false;
                try {
                    keyUp = Input.GetKeyUp(_getPotentialUnityKeyName(key));
                } catch (ArgumentException ex) {
                    // This would only happen if an invalid key is added to _keyValuesUndetectableThroughInputString
                    // because other keys are verified via _hasValidUnityKeyName.
                    WebViewLogger.LogError("Invalid key value passed to Input.GetKeyUp: " + ex);
                    _keysDown.Remove(key);
                    return;
                }
                if (keyUp) {
                    var handler = KeyUpReceived;
                    if (handler != null) {
                        handler(this, new KeyboardInputEventArgs(key, modifiers));
                    }
                    _keysDown.Remove(key);
                }
            }
        }

        bool _processKeysUndetectableThroughInputString(KeyModifier modifiers) {

            var keyDownHandler = KeyDownReceived;
            var modifierKeysPressed = !(modifiers == KeyModifier.None || modifiers == KeyModifier.Shift);
            var keysUndetectableThroughInputStringArePressed = _areKeysUndetectableThroughInputStringPressed();
            var oneOrMoreKeysProcessed = false;
            // On Windows, when modifier keys are held down, Input.inputString is blank
            // even if other keys are pressed. So, use Input.GetKeyDown() in that scenario.
            if (keysUndetectableThroughInputStringArePressed || (Input.inputString.Length == 0 && modifierKeysPressed)) {
                foreach (var key in _keyValues) {
                    if (Input.GetKeyDown(_getPotentialUnityKeyName(key))) {
                        if (keyDownHandler != null) {
                            keyDownHandler(this, new KeyboardInputEventArgs(key, modifiers));
                        }
                        _keysDown.Add(key);
                        oneOrMoreKeysProcessed = true;
                    }
                }
            }
            return oneOrMoreKeysProcessed;
        }

        void _processModifierKeysOnly(KeyModifier modifiers) {

            var keyDownHandler = KeyDownReceived;
            foreach (var value in Enum.GetValues(typeof(KeyModifier))) {
                var modifierValue = (KeyModifier)value;
                if (modifierValue == KeyModifier.None) {
                    continue;
                }
                if ((modifiers & modifierValue) != 0) {
                    var key = modifierValue.ToString();
                    if (keyDownHandler != null) {
                        keyDownHandler(this, new KeyboardInputEventArgs(key, KeyModifier.None));
                    }
                    _keysDown.Add(key);
                }
            }
        }

        void Update() {

            var modifiers = _getModifiers();
            _processKeysPressed(modifiers);
            _processKeysReleased(modifiers);
        }
    }
}
