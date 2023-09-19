using System;
using Charon.StarValor.ModCore;
using UnityEngine;

namespace Charon.StarValor.MasterTinker {
    public partial class Equipment_DeflectorShield {
        public static class Effects {
            public class Targets : EffectExTemplate {
                public override IIndexableInstance GenerateInstance(int id, object data) {
                    var instance = (EffectEx)base.GenerateInstance(id, data);
                    instance.uniqueLevel = (int)EffectUniqueLevel.Exclusive;
                    return instance;
                }
            }
            public class Magnitudes : EffectExTemplate {
                public override object GetValue(IndexableInstanceData data, EffectContext context) =>
                    (ValueTuple<float, float, float>)data.Data;
                public override void SetValue(IndexableInstanceData data, EffectContext context, object value) =>
                    data.Data = value;
            }
            public class Force : EffectExTemplate {
                public override string GetDescription(IndexableInstanceData data, EffectContext context) =>
                    $"Force: {(int)GetValue(data, context)}";
                public override object GetValue(IndexableInstanceData data, EffectContext context) =>
                    Mathf.Floor(((EffectEx)data.Instance).value * Utilities.GetRarityMod(context.Rarity, context.Equipment.rarityMod, 0.4f));
            }
            public class Range : EffectExTemplate {
                public override string GetDescription(IndexableInstanceData data, EffectContext context) =>
                    $"Range: {(int)GetValue(data, context)}";
                public override object GetValue(IndexableInstanceData data, EffectContext context) =>
                    Mathf.Floor(((EffectEx)data.Instance).value * Utilities.GetRarityMod(context.Rarity, context.Equipment.rarityMod, 0.2f));
            }
            public class Hardness : EffectExTemplate {
                public override string GetDescription(IndexableInstanceData data, EffectContext context) =>
                    $"Hardness: {(int)GetValue(data, context)}";
                public override object GetValue(IndexableInstanceData data, EffectContext context) =>
                    Mathf.Floor(((EffectEx)data.Instance).value * Utilities.GetRarityMod(context.Rarity, context.Equipment.rarityMod, 0.3f));
            }
            public class Emitters : EffectExTemplate {
                public override string GetDescription(IndexableInstanceData data, EffectContext context) =>
                    $"Emitters: {(int)(GetValue(data, context))}";
                public override object GetValue(IndexableInstanceData data, EffectContext context) =>
                    Mathf.Ceil(((EffectEx)data.Instance).value * Utilities.GetRarityMod(context.Rarity, context.Equipment.rarityMod, 0.4f));
            }
        }
    }
}
