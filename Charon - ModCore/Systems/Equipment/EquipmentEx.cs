using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace Charon.StarValor.ModCore {
    [HasPatches]
    public sealed class EquipmentEx : Equipment, IIndexableInstance, ISerializable {
        #region Patches
        static MethodInfo _EquipmentDB_ValidateDatabase = typeof(EquipmentDB).GetMethod("ValidateDatabase", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        static Action EquipmentDB_ValidateDatabase = () => _EquipmentDB_ValidateDatabase.Invoke(null, null);

        [HarmonyPatch(typeof(EquipmentDB), nameof(EquipmentDB.LoadDatabaseForce))]
        [HarmonyPostfix]
        public static void LoadDatabaseForce(List<Equipment> ___equipments) {
            if (!defaultEquipmentRegistered) {
                defaultEquipmentRegistered = true;
                ___equipments.Sort((x, y) => x.id.CompareTo(y.id));
                foreach (var equipment in ___equipments) {
                    //ModCore.Instance.Log.LogWarning($"Loading {equipment.name} [{equipment.id}]");
                    var template = DefaultEquipmentTemplate.Register(equipment);
                    var eq = (EquipmentEx)template.CreateInstance(new QualifiedName(typeof(Equipment), equipment.name), template.Guid);
                    //if (eq.activated)
                    //    ModCore.Instance.Log.LogWarning("Adding AE for " + eq.name);
                }
                //ModCore.Instance.Log.LogWarning("Done loading default equipment");
            }
            ___equipments.Clear();
            foreach (var eq in IndexSystem.Instance.GetAllTypeInstance<EquipmentEx>().Where(ex => ex.TemplateData.Template is DefaultEquipmentTemplate))
                ___equipments.Add(eq);
            EquipmentDB.SortList();
        }
        static bool defaultEquipmentRegistered = false;

        [HarmonyPatch(typeof(EquipmentDB), nameof(EquipmentDB.GetEquipment))]
        [HarmonyPrefix]
        public static bool GetEquipment(ref Equipment __result, int id) {
            EquipmentDB_ValidateDatabase();
            if (IndexSystem.Instance.TryGetTypeInstance<EquipmentEx>(Utilities.Int_to_Guid(id), out var wr)) {
                __result = wr;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(Equipment), nameof(Equipment.IsDrone), MethodType.Getter)]
        [HarmonyPrefix]
        public static bool GetEquipment_IsDrone(Equipment __instance, ref bool __result) {
            if (__instance.effects.Count == 0) {
                __result = false;
                return false;
            }
            return true;
        }
        #endregion

        [Serialize]
        public IndexableInstanceData TemplateData { get; set; }

        public EquipmentEx() {
            TemplateData = new IndexableInstanceData(this);
            effects = new List<Effect>() { EffectEx.Empty };
        }
        public Guid Guid { get; set; }
        public QualifiedName QualifiedName { get; set; }
        int IIndexable.RefCount { get; set; } = 0;
        bool IIndexable.UseQualifiedName { get; } = true;
        bool IIndexable.UniqueType { get; } = false;

        object ISerializable.OnSerialize() => null;
        void ISerializable.OnDeserialize(object serialization) { }

        public ActiveEquipmentEx ActiveEquipment { get; set; } = null;

        Dictionary<Type, EffectEx> EffectsByType { get; } = new Dictionary<Type, EffectEx>();
        public EffectEx AddEffect(EffectExTemplate template) {
            var effect = (EffectEx)template.CreateInstance(null, null);
            if (effects is null || (effects.Count > 0 && effects[0] == EffectEx.Empty))
                effects = new List<Effect>();
            if (effects.Count > 0)
                ((EffectEx)effects[effects.Count - 1]).IsLastInOrder = false;
            effect.IsLastInOrder = true;
            effects.Add(effect);
            return effect;
        }
        public EffectEx AddEffect(System.Type type) {
            if (EffectsByType.TryGetValue(type, out var effect))
                return effect;
            var template = (EffectExTemplate)IndexSystem.Instance.GetTypeInstance(type);
            effect = AddEffect(template);
            EffectsByType.Add(type, effect);
            return effect;
        }
        public EffectEx AddEffect<T>() where T : EffectExTemplate {
            return AddEffect(typeof(T));
        }
        public EffectEx GetEffect(Type type) {
            if (EffectsByType.TryGetValue(type, out var effect))
                return effect;
            throw new ArgumentException("Effect not found", type.FullName);
        }
        public EffectEx GetEffect<T>() where T : EffectExTemplate => GetEffect(typeof(T));
        public bool HasEffect(Type type) => EffectsByType.ContainsKey(type);
        public bool HasEffect<T>() where T : EffectExTemplate => HasEffect(typeof(T));
        public bool RemoveEffect(EffectEx effect) {
            return effects.Remove(effect);
        }
        public bool RemoveEffect(System.Type effectType) {
            if (!EffectsByType.TryGetValue(effectType, out var effect))
                return false;
            EffectsByType.Remove(effectType);
            RemoveEffect(effect);
            return true;
        }
        public bool RemoveEffect<T>() where T : EffectExTemplate => RemoveEffect(typeof(T));

        public Dictionary<Type, EquipmentComponent> ComponentsByType { get; } = new Dictionary<Type, EquipmentComponent>();
    }
}
