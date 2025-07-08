using System;
using System.Collections.Generic;
using UnityEngine;

namespace Charon.StarValor.ModCore {
    public class Updater : MonoBehaviour {
        class ActionTracker {
            Action action;
            public Action Invoke { get; private set; }
            public float period {
                get => _period;
                set {
                    _period = value;
                    if (value < 0)
                        Invoke = null;
                    else if (value == 0)
                        Invoke = action;
                    else
                        Invoke = OnUpdatePeriod;
                }
            }
            float _period;
            float current = 0;
            public ActionTracker(Action action, float period) {
                this.action = action;
                this.period = period;
            }
            void OnUpdatePeriod() {
                current += Time.deltaTime;
                if (current >= period) {
                    current %= period;
                    action.Invoke();
                }
            }
        }
        Dictionary<Action, ActionTracker> OnUpdate = null;
        List<Action> OnFixedUpdate = null;

        void Update() {
            if (OnUpdate != null)
                foreach (var action in OnUpdate.Values)
                    action.Invoke?.Invoke();
        }
        void FixedUpdate() {
            if (OnFixedUpdate != null)
                foreach (var action in OnFixedUpdate)
                    action.Invoke();
        }
        public void SetOnUpdate(Action action, float? period) {
            if (period is null) {
                OnUpdate?.Remove(action);
            }
            else {
                if (OnUpdate == null)
                    OnUpdate = new Dictionary<Action, ActionTracker>();
                if (!OnUpdate.TryGetValue(action, out var tracker)) {
                    tracker = new ActionTracker(action, period.Value);
                    OnUpdate.Add(action, tracker);
                }
            }
        }
        public void SetOnFixedUpdate(Action action, bool enabled) {
            if (enabled) {
                if (OnFixedUpdate == null)
                    OnFixedUpdate = new List<Action>();
                if (!OnFixedUpdate.Contains(action))
                    OnFixedUpdate.Add(action);
            }
            else {
                OnFixedUpdate?.Remove(action);
            }

        }
    }
}
