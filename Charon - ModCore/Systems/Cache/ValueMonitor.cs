using System;
using System.Collections.Generic;

namespace Charon.StarValor.ModCore {
    public class ValueMonitor {
        protected string FieldName { get; } = null;
        protected Type FieldType = null;

        public object UID { get; }
        protected CachedValue cachedValue { get; private set; }
        object lastTarget;

        LinkedListNode<ValueMonitor> linkNode;

        EventHandler<CachedValue.CachedValueUpdateArgs> _onChanged;
        public event EventHandler<CachedValue.CachedValueUpdateArgs> OnChanged {
            add {
                lock (this) {
                    _onChanged += value;
                    if (cachedValue != null && linkNode == null)
                        LinkMonitor(lastTarget);
                }
            }
            remove {
                lock (this) {
                    _onChanged -= value;
                    if (_onChanged == null && linkNode != null)
                        UnlinkMonitor();
                }
            }
        }
        public void Notify(object sender, CachedValue.CachedValueUpdateArgs e) => _onChanged?.Invoke(sender, e);

        public static ValueMonitor FromBaseField<T>(string fieldName) => FromBaseField(typeof(T), fieldName);
        static ValueMonitor FromBaseField(Type type, string fieldName) => new ValueMonitor(type, fieldName);
        protected ValueMonitor(Type type, string fieldName) : this(CachedValue.GetDefaultUID(type, fieldName)) {
            FieldType = type;
            FieldName = fieldName;
        }
        public ValueMonitor(object uid) {
            UID = uid;
        }

        public virtual bool Enabled { get; set; }
        public float? Value => cachedValue.Value;
        //public static implicit operator float?(ValueMonitor c) => (float?)c.Value;
        public static implicit operator float(ValueMonitor c) => (float)(c.Value ?? 0);
        //public static implicit operator int?(ValueMonitor c) => (int?)c.Value;
        //public static implicit operator int(ValueMonitor c) => (int)(c.Value ?? 0);
        //public static implicit operator sbyte?(ValueMonitor c) => (sbyte?)c.Value;
        //public static implicit operator sbyte(ValueMonitor c) => (sbyte)(c.Value ?? 0);

        //Link is a unique shared key
        public void Link(object target) {
            Unlink();
            lastTarget = target;
            cachedValue = LinkOne(target);
        }
        protected virtual CachedValue LinkOne(object target) {
            if (_onChanged != null)
                return LinkMonitor(target);
            return null;
        }
        CachedValue LinkMonitor(object target) {
            if (linkNode != null)
                return this.cachedValue;

            CachedValue cachedValue;
            if (FieldType is null)
                (cachedValue, linkNode) = CachedValue.RegisterMonitor(this, UID, target);
            else
                (cachedValue, linkNode) = CachedValue.RegisterBaseFieldMonitor(this, FieldType, FieldName, target);
            return cachedValue;

        }
        public virtual void Unlink() {
            if (cachedValue == null)
                return;
            UnlinkOne();
            if (linkNode != null)
                UnlinkMonitor();
            cachedValue = null;
        }
        void UnlinkMonitor() {
            cachedValue.UnregisterMonitor(linkNode);
            linkNode = null;
        }
        protected virtual void UnlinkOne() { }
        public void Relink() {
            if (cachedValue != null)
                return;
            cachedValue = LinkOne(lastTarget);
        }
    }
}
