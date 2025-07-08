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
        protected float GetDefaultScaledValue(IndexableInstanceData data, EffectContext context) => ((EffectEx)data.Instance).value * context.Qnt * Utilities.GetRarityMod(context.Rarity, context.EquipmentRarityMod, ((EffectEx)data.Instance).mod);

        protected override Type InstanceType { get; } = typeof(EffectEx);

        protected override QualifiedName GetInstanceQualifiedName(IIndexableInstance instance) => new QualifiedName(instance.GetType(), $"{this.GetType()}++{instance.GetType()}.{instance.Guid}");

        public override void OnApplying(IIndexableInstance instance) {
            base.OnApplying(instance);
            var ef = (EffectEx)instance;
            ef.type = Utilities.Guid_to_Int(this.Guid);
        }
        public override void OnRemoving(IIndexableInstance instance) {
            base.OnRemoving(instance);
            var ef = (EffectEx)instance;
            ef.type = -1;
        }

        public override object OnSerialize() => throw new NotImplementedException();
        public override void OnDeserialize(object data) { throw new NotImplementedException(); }
        public override bool UseQualifiedName => false;
        public override bool UniqueType => true;
    }
}
