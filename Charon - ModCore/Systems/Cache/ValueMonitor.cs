using System;
using System.Collections.Generic;

namespace Charon.StarValor.ModCore {
    public class ValueMonitor {
        public string Name => QualifiedName.Name;
        public QualifiedName QualifiedName { get; }

        protected CachedValue cachedValue { get; private set; }
        object lastTarget;
        protected Type fieldType;
        LinkedListNode<ValueMonitor> monitorLinkNode;

        EventHandler<CachedValue.CachedValueUpdateArgs> _onChanged;
        public event EventHandler<CachedValue.CachedValueUpdateArgs> OnChanged {
            add {
                lock (this) {
                    _onChanged += value;
                    if (cachedValue != null && monitorLinkNode == null)
                        LinkMonitor(lastTarget);
                }
            }
            remove {
                lock (this) {
                    _onChanged -= value;
                    if (_onChanged == null && monitorLinkNode != null)
                        UnlinkMonitor();
                }
            }
        }
        public void Notify(object sender, CachedValue.CachedValueUpdateArgs e) => _onChanged?.Invoke(sender, e);

        public static ValueMonitor FromBaseField<T>(string fieldName) => FromBaseField(typeof(T), fieldName);
        static ValueMonitor FromBaseField(Type type, string fieldName) => new ValueMonitor(type, fieldName);
        protected ValueMonitor(Type type, string fieldName) {
            fieldType = type;
            QualifiedName = CachedValue.GetBaseFieldQualifiedName(type, fieldName);
        }

        public ValueMonitor(ModCorePlugin context, string name) : this(context.Guid, name) { }
        public ValueMonitor(string guid, string name) : this(ModCorePlugin.Qualify(guid, name)) { }
        public ValueMonitor(QualifiedName qualifiedName) {
            fieldType = null;
            QualifiedName = qualifiedName;
        }

        public virtual bool Enabled { get; set; }
        public float? Cached => cachedValue.Value;
        public static implicit operator float?(ValueMonitor c) => (float?)c.Cached;
        public static implicit operator float(ValueMonitor c) => (float)(c.Cached ?? 0);
        public static implicit operator int?(ValueMonitor c) => (int?)c.Cached;
        public static implicit operator int(ValueMonitor c) => (int)(c.Cached ?? 0);
        public static implicit operator sbyte?(ValueMonitor c) => (sbyte?)c.Cached;
        public static implicit operator sbyte(ValueMonitor c) => (sbyte)(c.Cached ?? 0);

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
            if (monitorLinkNode != null)
                return this.cachedValue;

            CachedValue cachedValue;
            if (fieldType == null)
                (cachedValue, monitorLinkNode) = CachedValue.RegisterMonitor(this, QualifiedName, target);
            else
                (cachedValue, monitorLinkNode) = CachedValue.RegisterBaseFieldMonitor(this, fieldType, Name, target);
            return cachedValue;

        }
        public virtual void Unlink() {
            if (cachedValue == null)
                return;
            UnlinkOne();
            cachedValue = null;
            if (monitorLinkNode != null)
                UnlinkMonitor();
        }
        void UnlinkMonitor() {
            cachedValue.UnregisterMonitor(monitorLinkNode);
            monitorLinkNode = null;
        }
        protected virtual void UnlinkOne() { }
        public void Relink() {
            if (cachedValue != null)
                return;
            LinkOne(lastTarget);
        }
    }
}
