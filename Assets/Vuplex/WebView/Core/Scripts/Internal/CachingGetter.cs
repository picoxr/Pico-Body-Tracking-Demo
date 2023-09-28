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
using System.Collections;
using UnityEngine;

namespace Vuplex.WebView.Internal {

    /// <summary>
    /// Internal utility class to help cache values that may change but
    /// shouldn't be looked up every frame.
    /// </summary>
    class CachingGetter<TResult> {

        public CachingGetter(Func<TResult> getterFunction, int cacheInvalidationPeriodSeconds, MonoBehaviour monoBehaviourForCoroutine) {

            _getterFunction = getterFunction;
            _waitForSeconds = new WaitForSeconds(cacheInvalidationPeriodSeconds);
            monoBehaviourForCoroutine.StartCoroutine(_invalidateCachePeriodically());
        }

        public TResult GetValue() {

            if (_valueNeedsToBeUpdated) {
                _cachedValue = _getterFunction();
                _valueNeedsToBeUpdated = false;
            }
            return _cachedValue;
        }

        Func<TResult> _getterFunction;
        bool _valueNeedsToBeUpdated = true;
        TResult _cachedValue;
        WaitForSeconds _waitForSeconds;

        IEnumerator _invalidateCachePeriodically() {

            while (true) {
                yield return _waitForSeconds;
                _valueNeedsToBeUpdated = true;
            }
        }
    }
}
