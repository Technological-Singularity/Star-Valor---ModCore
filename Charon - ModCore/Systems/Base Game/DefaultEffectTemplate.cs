using System;
using System.Collections.Generic;
using System.Reflection;

namespace Charon.StarValor.ModCore {
    public class DefaultEffectTemplate : EffectExTemplate {
        readonly static List<(PropertyInfo, FieldInfo)> binds = Utilities.GetBindsPropertyField<DefaultComponent, Effect>();
        static Dictionary<int, DefaultEffectTemplate> registered = new Dictionary<int, DefaultEffectTemplate>();

        public override bool UniqueType { get; } = false;
        public override bool UseQualifiedName { get; } = true;

        Effect DefaultEffect = null;

        public static EffectExTemplate Register(Effect effect) {
            if (registered.TryGetValue(effect.type, out var wr))
                return wr;
            var name = "effect_" + effect.type;
            wr = new DefaultEffectTemplate();
            IndexSystem.Instance.AllocateTypeInstance(wr, effect.type);
            wr.DefaultEffect = effect;
            wr.QualifiedName = new QualifiedName(wr, name);
            return wr;
        }

        class DefaultComponent : ComponentEx {
            public string Description { get; set; }
            public float Value { get; set; }
            public float Mod { get; set; } = 1;
            public int UniqueLevel { get; set; }

            public override void OnApplying(IIndexableInstance instance) {
                if (instance == null) ModCore.Instance.Log.LogWarning("DEFAULTCOMPONENT_0");
                if (binds == null) ModCore.Instance.Log.LogWarning("DEFAULTCOMPONENT_1");

                Utilities.BindSet(instance, binds, this, instance);
            }
        }

        public override void OnApplying(IIndexableInstance instance) {
            if (instance == null) ModCore.Instance.Log.LogWarning("DEFAULTCOMPONENT_2");
            if (binds == null) ModCore.Instance.Log.LogWarning("DEFAULTCOMPONENT_3");

            var component = instance.TemplateData.AddComponent<DefaultComponent>(exclusive: true);
            Utilities.BindSet(component, binds, component, DefaultEffect);
            component.OnApplying(instance);
        }
        public override void OnRemoving(IIndexableInstance instance) {
            base.OnRemoving(instance);
            UnityEngine.Object.Destroy(instance.TemplateData.GetComponent<DefaultComponent>());
        }
    }
}
