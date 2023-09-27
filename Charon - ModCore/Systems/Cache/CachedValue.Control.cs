using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

namespace Charon.StarValor.ModCore {
    public partial class CachedValue {
        partial class Control : MonoBehaviour {
            readonly Dictionary<object, CachedValue> cachedValues = new Dictionary<object, CachedValue>();
            readonly Dictionary<CachedValue, RegisteredField> registeredFields = new Dictionary<CachedValue, RegisteredField>();
            object source;

            public void Initialize(object source) => this.source = source;

            public (CachedValue value, LinkedListNode<ValueMonitor> node) RegisterMonitor(ValueMonitor cacheable, object uid) {
                if (!cachedValues.TryGetValue(uid, out var value)) {
                    value = new CachedValue(this, uid);
                    cachedValues[uid] = value;
                }
                return (value, value.AssignMonitor(cacheable));
            }
            public (CachedValue value, LinkedListNode<ValueMonitor> node) RegisterBaseFieldMonitor(ValueMonitor cacheable, Type fieldType, string fieldName, object instance) {
                var uid = GetDefaultUID(fieldType, fieldName);
                if (!cachedValues.TryGetValue(uid, out var cached)) {
                    cached = new CachedValue(this, uid);
                    var field = new RegisteredField(instance, fieldType, fieldName);
                    cachedValues[uid] = cached;
                    registeredFields.Add(cached, field);
                }
                return (cached, cached.AssignMonitor(cacheable));
            }
            public void UnregisterMonitor(CachedValue value) {
                cachedValues.Remove(value.uid);
                if (registeredFields.TryGetValue(value, out var field)) {
                    field.Reset();
                    registeredFields.Remove(value);
                }
                if (cachedValues.Count == 0)
                    Destroy(this);
            }

            public (CachedValue value, LinkedListNode<(ValueModifier, int)> node) RegisterModifier(ValueModifier cacheable, object uid, int priority) {
                if (!cachedValues.TryGetValue(uid, out var value)) {
                    value = new CachedValue(this, uid);
                    cachedValues[uid] = value;
                }
                return (value, value.AssignModifier(cacheable, priority));
            }
            public (CachedValue value, LinkedListNode<(ValueModifier, int)> node) RegisterBaseFieldModifier(ValueModifier cacheable, Type fieldType, string fieldName, object instance, int priority) {
                var uid = GetDefaultUID(fieldType, fieldName);
                if (!cachedValues.TryGetValue(uid, out var cached)) {
                    cached = new CachedValue(this, uid);
                    var field = new RegisteredField(instance, fieldType, fieldName);
                    cachedValues[uid] = cached;
                    registeredFields.Add(cached, field);
                }
                return (cached, cached.AssignModifier(cacheable, priority));
            }
            public void UnregisterModifier(CachedValue value) {
                cachedValues.Remove(value.uid);
                if (registeredFields.TryGetValue(value, out var field)) {
                    field.Reset();
                    registeredFields.Remove(value);
                }
                if (cachedValues.Count == 0)
                    Destroy(this);
            }
            public List<string> Dump(string prefix = "") {
                List<string> wr = new List<string>();
                foreach (var kvp in cachedValues)
                    wr.Add($"{prefix}{kvp.Value.uid.ToString()} [{kvp.Value.registeredMonitors.Count}|{kvp.Value.registeredModifiers.Count}]] : " + kvp.Value.Value);
                return wr;
            }
            void Awake() => this.enabled = true;
            void LateUpdate() {
                if (source == null) {
                    Destroy(this);
                    return;
                }
                foreach (var kvp in registeredFields) {
                    if (kvp.Value.Pull(out var value) || !kvp.Key.valid) {
                        kvp.Key.defaultValue = value;
                        kvp.Key.Invalidate();
                    }
                }
                foreach (var kvp in registeredFields)
                    kvp.Value.Commit(kvp.Key.Value ?? 0); //this also performs validation
                foreach (var kvp in registeredFields)
                    kvp.Value.Push();
            }
            void OnDestroy() {
                foreach (var kvp in registeredFields)
                    kvp.Value.Reset();
                cachedValues.Clear();
                registeredFields.Clear();
                RemoveControl(source);
            }
        }
    }
}
