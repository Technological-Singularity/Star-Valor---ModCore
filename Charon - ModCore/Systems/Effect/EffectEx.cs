using HarmonyLib;

namespace Charon.StarValor.ModCore {
    [HasPatches]
    public class EffectEx : Effect, IIndexableInstance, ISerializable {
        #region Patches
        [HarmonyPatch(typeof(Equipment), nameof(Equipment.GetEffectString))]
        [HarmonyPrefix]
        public static bool GetEffectString(ref string __result, Effect effect, int rarity, float rarityMod, int shipClass) {
            var context = new EffectContext() { Rarity = rarity, EquipmentRarityMod = rarityMod };
            if (effect is EffectEx ex) {
                __result = ex.GetDescription(context);
                return false;
            }
            return true;
        }
        #endregion

        class EffectExEmpty : EffectEx {
            public override string GetDescription(EffectContext context) => null;
            public override float GetValue(EffectContext context) => 0;
            public override void SetValue(EffectContext context, float value) { }
        }
        public static EffectEx Empty { get; } = new EffectExEmpty();

        public QualifiedName QualifiedName { get; set; }
        int IIndexable.RefCount { get; set; } = 0;
        public bool UseQualifiedName { get; } = true;
        public bool UniqueType { get; } = false;
        public IndexableTemplate Template => TemplateData.Template;
        [Serialize]
        public IndexableInstanceData TemplateData { get; private set; }
        public EffectEx() => TemplateData = new IndexableInstanceData(this);
        public int Id {
            get => type;
            set => type = value;
        }
        public bool IsLastInOrder { get; set; } = false;
        object ISerializable.OnSerialize() => null;
        void ISerializable.OnDeserialize(object data) { }

        public virtual string GetDescription(EffectContext context) => ((EffectExTemplate)Template).GetDescription(TemplateData, context) + (IsLastInOrder ? "" : " ");
        public virtual float GetValue(EffectContext context) => (float)((EffectExTemplate)Template).GetValue(TemplateData, context);
        public virtual void SetValue(EffectContext context, float value) => ((EffectExTemplate)Template).SetValue(TemplateData, context, value);
    }
}
