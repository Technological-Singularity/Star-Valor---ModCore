using System;
using System.Collections.Generic;

namespace Charon.StarValor.ModCore {
    public partial class ValueModifier : ValueMonitor {
        public delegate void ModifierFunc(float? thisValue, ref float? incomingValue);

        public static new ValueModifier FromBaseField<T>(string fieldName) => FromBaseField<T>(fieldName, StandardFunction.Add);
        public static ValueModifier FromBaseField<T>(string fieldName, StandardFunction standardFunction) => FromBaseField<T>(fieldName, standardFunction.Priority, standardFunction.Function);
        public static ValueModifier FromBaseField<T>(string fieldName, int priority, ModifierFunc function) => FromBaseField(typeof(T), fieldName, priority, function);
        static ValueModifier FromBaseField(Type type, string fieldName, int priority, ModifierFunc function) => new ValueModifier(type, fieldName, priority, function);
        ValueModifier(Type type, string fieldName, int priority, ModifierFunc function) : base(type, fieldName) => (this.function, this.priority) = (function, priority);

        public ValueModifier(QualifiedName qualifiedName) : this(qualifiedName, StandardFunction.Add) { }
        public ValueModifier(QualifiedName qualifiedName, StandardFunction standardFunction) : this(qualifiedName, standardFunction.Priority, standardFunction.Function) { }
        public ValueModifier(QualifiedName qualifiedName, int priority, ModifierFunc function) : base(qualifiedName) => (this.priority, this.function) = (priority, function);

        public ValueModifier(string guid, string name) : this(guid, name, StandardFunction.Add) { }
        public ValueModifier(string guid, string name, StandardFunction standardFunction) : this(guid, name, standardFunction.Priority, standardFunction.Function) { }
        public ValueModifier(string guid, string name, int priority, ModifierFunc function) : base(guid, name) => (this.function, this.priority) = (function, priority);

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
        LinkedListNode<(ValueModifier, int)> modifierChainNode;

        public void Update(ref float? result) {
            if (Enabled)
                function(Modifier, ref result);
        }
        protected override CachedValue LinkOne(object target) {
            CachedValue cachedValue;
            if (fieldType == null)
                (cachedValue, modifierChainNode) = CachedValue.RegisterModifier(this, QualifiedName, target, priority);
            else
                (cachedValue, modifierChainNode) = CachedValue.RegisterBaseFieldModifier(this, fieldType, Name, target, priority);
            return cachedValue;
        }
        protected override void UnlinkOne() {
            cachedValue.UnregisterModifier(modifierChainNode);
            modifierChainNode = null;
        }
    }
}
