using System;
using System.Collections.Generic;
using System.Reflection;

namespace Charon.StarValor.ModCore {
    [RegisterManual]
    public partial class DefaultEffectTemplate : EffectExTemplate {
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
            var name = "e_" + effect.type;
            //ModCore.Instance.Log.LogMessage($"    Registering {name} as DefaultEffectTemplate [{effect.description}]");
            var wr = new DefaultEffectTemplate(effect);
            wr.QualifiedName = new QualifiedName(wr, name);
            return wr;
        }
        DefaultEffectTemplate(Effect effect) {
            Utilities.BindSet(bindValues, binds, bindValues, effect);
            //ModCore.Instance.Log.LogWarning("---- Bind SET for " + this.bindValues.Type);
            //foreach (var (name, val) in Utilities.BindDump(bindValues, binds))
            //    ModCore.Instance.Log.LogWarning("    " + name + " : " + val);
            //ModCore.Instance.Log.LogWarning("----");
        }
        public override void OnApplying(IIndexableInstance instance) {
            base.OnApplying(instance);
            Utilities.BindSet(instance, binds, bindValues, instance);
            //ModCore.Instance.Log.LogWarning("---- Bind LOAD for " + this.bindValues.Type);
            //foreach (var (name, val) in Utilities.BindDump(instance, binds))
            //    ModCore.Instance.Log.LogWarning("    " + name + " : " + val);
            //ModCore.Instance.Log.LogWarning("----");
        }
    }
}
