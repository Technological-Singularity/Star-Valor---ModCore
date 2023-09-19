using System;
using System.Collections.Generic;
using System.Reflection;

namespace Charon.StarValor.ModCore {
    public class DefaultEffectTemplate : EffectExTemplate {
        readonly static List<(PropertyInfo, FieldInfo)> binds = Utilities.GetBindsPropertyField<DefaultComponent, Effect>();

        class DefaultComponent : ComponentEx {
            #region ComponentEx
            public override int GetHashCode(HashContext context) => Utilities.GetHashCode(GetSerialization().GetHashCode());
            public override object GetSerialization() => (Description, Value, RarityMod);
            public override void Deserialize(object serialization) => (Description, Value, RarityMod) = (Tuple<string, float, float>)serialization;
            #endregion

            public string Description { get; set; }
            public float Value { get; set; }
            public float RarityMod { get; set; } = 1;
            public int UniqueLevel { get; set; }

            public override void OnApplying(IIndexableInstance instance) {
                if (instance == null) ModCorePlugin.Log.LogWarning("0");
                if (binds == null) ModCorePlugin.Log.LogWarning("1");

                Utilities.BindSet(instance, binds, this, instance);
            }
        }

        public override void OnApplying(IIndexableInstance instance, object data) {
            if (instance == null) ModCorePlugin.Log.LogWarning("0");
            if (binds == null) ModCorePlugin.Log.LogWarning("1");

            var component = instance.Data.AddComponent<DefaultComponent>(exclusive: true);

            var effect = (Effect)data;
            Utilities.BindSet(component, binds, component, effect);

            component.OnApplying(instance);
        }
        public override void OnRemoving(IIndexableInstance instance, object data) {
            base.OnRemoving(instance, data);
            UnityEngine.Object.Destroy(instance.Data.GetComponent<DefaultComponent>());
        }
    }
}
