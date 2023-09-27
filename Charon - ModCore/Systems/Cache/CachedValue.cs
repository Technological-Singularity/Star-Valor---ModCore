using System;
using System.Collections.Generic;
using UnityEngine;

namespace Charon.StarValor.ModCore {
    public partial class CachedValue {
        public static object GetDefaultUID(Type parentType, string fieldName) => parentType.FullName + "++" + fieldName;
        static Stack<List<CachedValue>> deferredContexts = new Stack<List<CachedValue>>();
        static List<CachedValue> deferredCurrent = null;

        /// <summary>
        /// Allows multiple invalidations to be performed before notifying any invalidated values of any changes. FIXME
        /// </summary>
        /// <param name="action"></param>
        public static void Defer(Action action) {
            if (deferredCurrent != null)
                deferredContexts.Push(deferredCurrent);
            deferredCurrent = new List<CachedValue>();
            action?.Invoke();
            var outgoing = deferredCurrent;
            deferredCurrent = deferredContexts.Count == 0 ? null : deferredContexts.Pop();
            foreach (var o in outgoing)
                o.Undefer();
        }

        int deferCount = 0;
        void Defer() {
            ++deferCount;
        }
        void Undefer() {
            if (--deferCount <= 0) {
                deferCount = 0;
                Validate(true);
            }
        }

        public struct CachedValueUpdateArgs {
            public float? OldValue;
            public float? NewValue;
            public CachedValueUpdateArgs(float? oldValue, float? newValue) => (OldValue, NewValue) = (oldValue, newValue);
        }
        public delegate void CachedValueUpdateHandler(object sender, CachedValueUpdateArgs e);

        static GameObject _anchorGO;
        static GameObject anchorGO {
            get {
                if (_anchorGO == null)
                    _anchorGO = new GameObject();
                return _anchorGO;
            }
        }
        static Dictionary<object, Control> controls = new Dictionary<object, Control>();
        static bool clearing = false;
        static void RemoveControl(object source) {
            if (clearing)
                return;
            if (controls.TryGetValue(source, out var control)) {
                controls.Remove(source);
                if (control != null)
                    UnityEngine.Object.Destroy(control);
            }
        }
        public static void ClearControls() {
            clearing = true;
            foreach (var control in controls.Values)
                UnityEngine.Object.Destroy(control);
            controls.Clear();
            clearing = false;
        }
        static bool TryGetControl(object source, out Control control) => controls.TryGetValue(source, out control);
        static Control GetCreateControl(object source) {
            if (controls.TryGetValue(source, out var wr))
                return wr;
            wr = anchorGO.AddComponent<Control>();
            wr.Initialize(source);
            controls[source] = wr;
            return wr;
        }

        LinkedList<(ValueModifier value, int priority)> registeredModifiers = new LinkedList<(ValueModifier, int)>();
        LinkedList<ValueMonitor> registeredMonitors = new LinkedList<ValueMonitor>();

        bool valid;
        Control control;
        object uid;
        float? defaultValue = null;
        float? _value;

        public float? Value {
            get {
                if (!valid)
                    Validate(false);
                return _value;
            }
        }
        public void Invalidate() {
            if (deferredCurrent != null) {
                Defer();
                deferredCurrent.Add(this);
            }
            else {
                valid = false;
                if (registeredModifiers.Count > 0)
                    Validate(true);
            }
        }
        void Validate(bool notify) {
            var oldValue = _value;
            float? newValue = defaultValue;
            foreach ((var value, _) in registeredModifiers)
                value.Update(ref newValue);
            _value = newValue;
            valid = true;
            if (notify) {
                if (oldValue != newValue && (registeredMonitors.Count > 0 || registeredModifiers.Count > 0)) {
                    var args = new CachedValueUpdateArgs(oldValue, newValue);
                    foreach (var m in registeredMonitors)
                        m.Notify(this, args);
                }
            }
        }

        CachedValue(Control control, object uid) => (this.control, this.uid) = (control, uid);
        LinkedListNode<ValueMonitor> AssignMonitor(ValueMonitor cacheable) {
            LinkedListNode<ValueMonitor> newNode = new LinkedListNode<ValueMonitor>(cacheable);
            registeredMonitors.AddLast(newNode);
            return newNode;
        }
        LinkedListNode<(ValueModifier, int)> AssignModifier(ValueModifier cacheable, int priority) {
            LinkedListNode<(ValueModifier value, int priority)> newNode = new LinkedListNode<(ValueModifier value, int priority)>((cacheable, priority));
            if (registeredModifiers.Count == 0) {
                registeredModifiers.AddFirst(newNode);
            }
            else {
                var lowerNode = registeredModifiers.First;
                while (lowerNode != null && lowerNode.Value.priority < priority)
                    lowerNode = lowerNode.Next;
                registeredModifiers.AddAfter(lowerNode, newNode);
            }
            Invalidate();
            return newNode;
        }

        public static (CachedValue value, LinkedListNode<ValueMonitor> node) RegisterMonitor(ValueMonitor cacheable, object uid, object parent) {
            var control = GetCreateControl(parent);
            return control.RegisterMonitor(cacheable, uid);
        }
        public static (CachedValue value, LinkedListNode<ValueMonitor> node) RegisterBaseFieldMonitor(ValueMonitor cacheable, Type fieldType, string fieldName, object instance) {
            var control = GetCreateControl(instance);
            return control.RegisterBaseFieldMonitor(cacheable, fieldType, fieldName, instance);
        }
        public void UnregisterMonitor(LinkedListNode<ValueMonitor> node) {
            registeredMonitors.Remove(node);
            if (registeredMonitors.Count == 0)
                control.UnregisterMonitor(this);
            Invalidate();
        }

        public static (CachedValue value, LinkedListNode<(ValueModifier, int)> node) RegisterModifier(ValueModifier cacheable, object uid, object parent, int priority) {
            var control = GetCreateControl(parent);
            return control.RegisterModifier(cacheable, uid, priority);
        }
        public static (CachedValue value, LinkedListNode<(ValueModifier, int)> node) RegisterBaseFieldModifier(ValueModifier cacheable, Type fieldType, string fieldName, object instance, int priority) {
            var control = GetCreateControl(instance);
            return control.RegisterBaseFieldModifier(cacheable, fieldType, fieldName, instance, priority);
        }
        public void UnregisterModifier(LinkedListNode<(ValueModifier cacheable, int)> node) {
            registeredModifiers.Remove(node);
            if (registeredModifiers.Count == 0)
                control.UnregisterModifier(this);
            Invalidate();
        }
    }
}
