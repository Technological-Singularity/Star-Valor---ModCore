using HarmonyLib;

namespace Charon.StarValor.ModCore {
    [HasPatches]
    public sealed class EffectEx : Effect, IIndexableInstance {
        #region Patches
        [HarmonyPatch(typeof(EquipmentDB), "GetEffectString")]
        [HarmonyPrefix]
        public static bool GetEffectString(ref string __result, Effect effect, int rarity, float rarityMod, int shipClass) {
            var context = new EffectContext();
            if (effect is EffectEx ex) {
                __result = ex.GetDescription(context);
                return false;
            }
            return true;
        }
        #endregion
        #region IIndexableInstance
        public IndexableTemplate Template => Data.Template;
        public IndexableInstanceData Data { get; }
        public EffectEx(int id) => (this.type, Data) = (id, new IndexableInstanceData(this));
        public int GetHashCode(HashContext context) => Data.GetHashCode(context);
        public int Id {
            get => type;
            set => type = value;
        }
        public void Allocate() => IndexSystem.Instance.Ref(this);
        public bool Release() => IndexSystem.Instance.Deref(this);
        public object GetSerialization() => Data.GetSerialization();
        public void Deserialize(object serialization) => Data.Deserialize(serialization);
        #endregion

        public string GetDescription(EffectContext context) => ((EffectExTemplate)Template).GetDescription(Data, context);
        public float GetValue(EffectContext context) => ((EffectExTemplate)Template).GetValue(Data, context);
    }
}
