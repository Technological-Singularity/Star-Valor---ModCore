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
        Dictionary<Action, ActionTracker> onUpdate = null;
        List<Action> onFixedUpdate = null;

        void Update() {
            if (onUpdate != null)
                foreach (var action in onUpdate.Values)
                    action.Invoke?.Invoke();
        }
        void FixedUpdate() {
            if (onFixedUpdate != null)
                foreach (var action in onFixedUpdate)
                    action?.Invoke();
        }
        public void Register(Action onUpdate, float periodSeconds = 0) => _Register(onUpdate);
        ActionTracker _Register(Action onUpdate, float periodSeconds = 0) {
            if (this.onUpdate == null)
                this.onUpdate = new Dictionary<Action, ActionTracker>();
            var tracker = new ActionTracker(onUpdate, periodSeconds);
            this.onUpdate[onUpdate] = tracker;
            return tracker;
        }
        public void SetPeriod(Action onUpdate, float periodSeconds = 0) {
            if (!this.onUpdate.TryGetValue(onUpdate, out var tracker))
                tracker = _Register(onUpdate, periodSeconds);
            tracker.period = periodSeconds;
        }
        public void RegisterFixed(Action onFixedUpdate) {
            if (this.onFixedUpdate == null)
                this.onFixedUpdate = new List<Action>();
            this.onFixedUpdate.Add(onFixedUpdate);
        }
    }
}
