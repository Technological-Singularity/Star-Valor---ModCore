using System;
using UnityEngine;

namespace Charon.StarValor.ModCore {
    public abstract class EffectExTemplate : IndexableTemplate {
        public virtual string Name => GetType().Name;

        public virtual string GetDescription(IndexableInstanceData data, EffectContext context) => null;
        protected virtual string GetFormatter(IndexableInstanceData data, string s, EffectContext context) => Utilities.Text.Format(s, color: Color.gray);
        public virtual object GetValue(IndexableInstanceData data, EffectContext context) => null;
        public virtual void SetValue(IndexableInstanceData data, EffectContext context, object value) { }

        protected string GetDefaultDescription(IndexableInstanceData data, EffectContext context) => $"{Name}: {GetValue(data, context)}";
        protected float GetDefaultScaledValue(IndexableInstanceData data, EffectContext context) => ((EffectEx)data.Instance).value * Utilities.GetRarityMod(context.Rarity, context.EquipmentRarityMod, ((EffectEx)data.Instance).mod);

        protected override Type InstanceType { get; } = typeof(EffectEx);

        protected override QualifiedName GetInstanceQualifiedName(IIndexableInstance instance) => new QualifiedName(instance.GetType(), $"{this.GetType()}++{instance.GetType()}.{instance.Id}");

        public override object OnSerialize() => throw new NotImplementedException();
        public override void OnDeserialize(object data) { throw new NotImplementedException(); }
        public override bool UseQualifiedName => false;
        public override bool UniqueType => true;
    }
}
