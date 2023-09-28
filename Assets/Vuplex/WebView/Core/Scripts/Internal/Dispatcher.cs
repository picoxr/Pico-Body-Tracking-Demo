using System.Collections.Generic;
using System.Threading;
using System;
using UnityEngine;

namespace Vuplex.WebView.Internal {

    /// <summary>
    /// Utility class for running code on the main Unity thread.
    /// </summary>
    /// <remarks>
    /// From [this Unity forum post](https://answers.unity.com/questions/305882/how-do-i-invoke-functions-on-the-main-thread.html#answer-1417505).
    /// </remarks>
    public class Dispatcher : MonoBehaviour {

        public static void RunAsync(Action action) {
            ThreadPool.QueueUserWorkItem(o => action());
        }

        public static void RunAsync(Action<object> action, object state) {
            ThreadPool.QueueUserWorkItem(o => action(o), state);
        }

        public static void RunOnMainThread(Action action) {
            lock(_backlog) {
                _backlog.Add(action);
                _queued = true;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize() {
            if (_instance == null) {
                _instance = new GameObject("Dispatcher").AddComponent<Dispatcher>();
                DontDestroyOnLoad(_instance.gameObject);
            }
        }

        private void Update() {
            if (_queued) {
                lock(_backlog) {
                    var tmp = _actions;
                    _actions = _backlog;
                    _backlog = tmp;
                    _queued = false;
                }

                foreach (var action in _actions) {
                    try {
                        action();
                    } catch (Exception e) {
                        WebViewLogger.LogError("An exception occurred while dispatching an action on the main thread: " + e);
                    }
                }
                _actions.Clear();
            }
        }

        static Dispatcher _instance;
        static volatile bool _queued = false;
        static List<Action> _backlog = new List<Action>(8);
        static List<Action> _actions = new List<Action>(8);
    }
}
