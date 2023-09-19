using System;
using UnityEngine;

namespace Charon.StarValor.ModCore {
    public abstract class EffectExTemplate : IndexableTemplate {
        protected abstract class EffectExData : ComponentEx {
            #region ComponentEx
            public override int GetHashCode(HashContext context) => UniqueLevel.GetHashCode();
            public override object GetSerialization() => UniqueLevel;
            public override void Deserialize(object serialization) => UniqueLevel = (EffectUniqueLevel)serialization;
            #endregion

            public EffectUniqueLevel UniqueLevel { get; set; } = EffectUniqueLevel.None;

            public override void OnApplying(IIndexableInstance instance) {
                var effect = (EffectEx)instance;
                effect.uniqueLevel = (int)UniqueLevel;
            }
        }

        public virtual string GetDescription(IndexableInstanceData data, EffectContext context) => null;
        protected virtual string GetFormatter(IndexableInstanceData data, string s, EffectContext context) => Utilities.Text.Format(s, color: Color.gray);
        public virtual object GetValue(IndexableInstanceData data, EffectContext context) => null;
        public virtual void SetValue(IndexableInstanceData data, EffectContext context, object value) { }

        //public Func<EffectEx, EffectContext, string> DescriptionGetter { get; set; } = null;
        //public Func<EffectEx, string, EffectContext, string> FormatGetter { get; set; } = null;
        //public Func<EffectEx, EffectContext, float> ValueGetter { get; set; } = null;

        public override IIndexableInstance GenerateInstance(int id, object data) {
            IIndexableInstance instance = new EffectEx(id);
            Apply(instance, data);
            return instance;
        }
    }
}
