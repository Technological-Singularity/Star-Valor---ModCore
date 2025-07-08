using Charon.StarValor.ModCore;
using UnityEngine;

namespace Charon.StarValor.MasterTinker {
    public partial class Equipment_DeflectorArray {
        public static class Effects {
            public class Targets : EffectExTemplate {
                public override void OnApplying(IIndexableInstance instance) {
                    base.OnApplying(instance);
                    var ef = (EffectEx)instance;
                    ef.uniqueLevel = (int)EffectUniqueLevel.Exclusive;
                }
            }
            public abstract class Magnitudes : EffectExTemplate {
                public class Dispersion : Magnitudes { }
                public class Repulsion : Magnitudes { }
                public class Vectoring : Magnitudes { }

                public override object GetValue(IndexableInstanceData data, EffectContext context) => data.Data;
                public override void SetValue(IndexableInstanceData data, EffectContext context, object value) => data.Data = value;
            }
            public class Force : EffectExTemplate {
                public override string GetDescription(IndexableInstanceData data, EffectContext context) => 
                    GetDefaultDescription(data, context);
                public override object GetValue(IndexableInstanceData data, EffectContext context) => 
                    Mathf.Floor(GetDefaultScaledValue(data, context));
            }
            public class Range : EffectExTemplate {
                public override string GetDescription(IndexableInstanceData data, EffectContext context) => 
                    GetDefaultDescription(data, context);
                public override object GetValue(IndexableInstanceData data, EffectContext context) => 
                    Mathf.Floor(GetDefaultScaledValue(data, context));
            }
            public class Hardness : EffectExTemplate {
                public override string GetDescription(IndexableInstanceData data, EffectContext context) => 
                    GetDefaultDescription(data, context);
                public override object GetValue(IndexableInstanceData data, EffectContext context) => 
                    Mathf.Floor(GetDefaultScaledValue(data, context));
            }
            public class Emitters : EffectExTemplate {
                public override string GetDescription(IndexableInstanceData data, EffectContext context) => 
                    GetDefaultDescription(data, context);
                public override object GetValue(IndexableInstanceData data, EffectContext context) => 
                    Mathf.Ceil(GetDefaultScaledValue(data, context));
            }
        }
    }
}
