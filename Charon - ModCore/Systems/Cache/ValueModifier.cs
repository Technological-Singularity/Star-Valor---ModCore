using System;
using System.Collections.Generic;

namespace Charon.StarValor.ModCore {
    public partial class ValueModifier : ValueMonitor {
        public delegate void ModifierFunc(float? thisValue, ref float? incomingValue);

        ValueModifier(Type type, string fieldName, int priority, ModifierFunc function) : base(type, fieldName) => (this.function, this.priority) = (function, priority);

        public ValueModifier(object uniqueIdentifier) : this(uniqueIdentifier, StandardFunction.Add) { }
        public ValueModifier(object uniqueIdentifier, StandardFunction standardFunction) : this(uniqueIdentifier, standardFunction.Priority, standardFunction.Function) { }
        public ValueModifier(object uniqueIdentifier, int priority, ModifierFunc function) : base(uniqueIdentifier) => (this.priority, this.function) = (priority, function);

        public static new ValueModifier FromBaseField<T>(string fieldName) => FromBaseField<T>(fieldName, StandardFunction.Add);
        public static ValueModifier FromBaseField<T>(string fieldName, StandardFunction standardFunction) => FromBaseField<T>(fieldName, standardFunction.Priority, standardFunction.Function);
        public static ValueModifier FromBaseField<T>(string fieldName, int priority, ModifierFunc function) => FromBaseField(typeof(T), fieldName, priority, function);
        static ValueModifier FromBaseField(Type type, string fieldName, int priority, ModifierFunc function) => new ValueModifier(type, fieldName, priority, function);

        public static ValueModifier FromType<T>() => new ValueModifier(typeof(T));
        public static ValueModifier FromType<T>(StandardFunction standardFunction) => new ValueModifier(typeof(T), standardFunction);
        public static ValueModifier FromType<T>(int priority, ModifierFunc function) => new ValueModifier(typeof(T), priority, function);
        public static ValueModifier FromType(Type type) => new ValueModifier(type);
        public static ValueModifier FromType(Type type, StandardFunction standardFunction) => new ValueModifier(type, standardFunction);
        public static ValueModifier FromType(Type type, int priority, ModifierFunc function) => new ValueModifier(type, priority, function);

        public override bool Enabled {
            get => _enabled;
            set {
                if (_enabled == value)
                    return;
                _enabled = value;
                cachedValue?.Invalidate();
            }
        }
        public float? Modifier {
            get => _modifier;
            set {
                if (_modifier == value)
                    return;
                _modifier = value;
                cachedValue?.Invalidate();
            }
        }

        int priority;
        bool _enabled;
        float? _modifier;
        ModifierFunc function;
        LinkedListNode<(ValueModifier, int)> chainNode;

        public void Update(ref float? result) {
            if (Enabled)
                function(Modifier, ref result);
        }
        protected override CachedValue LinkOne(object target) {
            CachedValue cachedValue;
            if (FieldType is null)
                (cachedValue, chainNode) = CachedValue.RegisterModifier(this, UID, target, priority);
            else
                (cachedValue, chainNode) = CachedValue.RegisterBaseFieldModifier(this, FieldType, FieldName, target, priority);
            return cachedValue;
        }
        protected override void UnlinkOne() {
            cachedValue.UnregisterModifier(chainNode);
            chainNode = null;
        }
    }
}
