using System;
using System.Collections.Generic;
using System.Reflection;

namespace Charon.StarValor.ModCore {
    [RegisterManual]
    public partial class DefaultEffectTemplate : EffectExTemplate {
        static Dictionary<int, DefaultEffectTemplate> registered = new Dictionary<int, DefaultEffectTemplate>();

        public override bool UniqueType { get; } = false;
        public override bool UseQualifiedName { get; } = true;

        #region Binds
        static List<(PropertyInfo, FieldInfo)> binds { get; } = Utilities.GetBindsPropertyField<BaseValues, Effect>();
        class BaseValues {
            public int Type { get; set; }
            public string Description { get; set; }
            public float Value { get; set; }
            public float Mod { get; set; } = 1;
            public int UniqueLevel { get; set; }
        }
        BaseValues bindValues = new BaseValues();
        #endregion

        public static EffectExTemplate Register(Effect effect) {
            if (registered.TryGetValue(effect.type, out var wr))
                return wr;
            var name = "effect_" + effect.type;
            wr = new DefaultEffectTemplate(effect);
            IndexSystem.Instance.AllocateTypeInstance(wr, Utilities.Int_to_Guid(effect.type));
            registered.Add(effect.type, wr);
            wr.QualifiedName = new QualifiedName(wr, name);
            return wr;
        }
        DefaultEffectTemplate(Effect effect) {
            Utilities.BindSet(bindValues, binds, bindValues, effect);
        }
        public override void OnApplying(IIndexableInstance instance) {
            base.OnApplying(instance);
            Utilities.BindSet(instance, binds, bindValues, instance);
        }
    }
}
