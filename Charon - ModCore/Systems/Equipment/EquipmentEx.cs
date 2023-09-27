using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace Charon.StarValor.ModCore {
    [HasPatches]
    public sealed class EquipmentEx : Equipment, IIndexableInstance, ISerializable {
        #region Patches
        static Dictionary<int, Equipment> equipmentDict = new Dictionary<int, Equipment>();
        static MethodInfo _EquipmentDB_ValidateDatabase = typeof(EquipmentDB).GetMethod("ValidateDatabase", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        static Action EquipmentDB_ValidateDatabase = () => _EquipmentDB_ValidateDatabase.Invoke(null, null);
        //static FieldInfo _EquipmentDB_Equipments = typeof(EquipmentDB).GetField("equipments", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        //static List<Equipment> EquipmentDB_Equipments => (List<Equipment>)_EquipmentDB_Equipments.GetValue(null);

        [HarmonyPatch(typeof(EquipmentDB), nameof(EquipmentDB.LoadDatabaseForce))]
        [HarmonyPostfix]
        public static void LoadDatabaseForce(List<Equipment> ___equipments) {
            if (!defaultEquipmentRegistered) {
                defaultEquipmentRegistered = true;
                foreach (var equipment in ___equipments) {
                    var template = DefaultEquipmentTemplate.Register(equipment);
                    var eq = template.CreateInstance();
                    equipmentDict.Add(eq.id, eq);
                }
            }
            ___equipments = new List<Equipment>();
            foreach (var eq in IndexSystem.Instance.GetAllTypeInstance<EquipmentEx>().Where(ex => ex.TemplateData.Template is DefaultEquipmentTemplate))
                ___equipments.Add(eq);
            EquipmentDB.SortList();
        }
        static bool defaultEquipmentRegistered = false;

        [HarmonyPatch(typeof(EquipmentDB), nameof(EquipmentDB.GetEquipment))]
        [HarmonyPrefix]
        public static bool GetEquipment(ref Equipment __result, int id) {
            EquipmentDB_ValidateDatabase();
            return !equipmentDict.TryGetValue(id, out __result);
        }

        [HarmonyPatch(typeof(EquipmentDB), nameof(EquipmentDB.ClearDatabase))]
        [HarmonyPostfix]
        public static void ClearDatabase(ref bool ___databaseLoaded, List<Equipment> ___equipments) {
            equipmentDict.Clear();
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

        internal static bool TryAddEquipment(Equipment eq) {
            if (equipmentDict.ContainsKey(eq.id))
                return false;
            equipmentDict.Add(eq.id, eq);
            return true;
        }
        internal static bool TryRemoveEquipment(Equipment eq) {
            return equipmentDict.Remove(eq.id);
        }

        #region IIndexableInstance
        [Serialize]
        public IndexableInstanceData TemplateData { get; set; }

        public EquipmentEx() {
            TemplateData = new IndexableInstanceData(this);
            effects = new List<Effect>() { EffectEx.Empty };
        }
        public int Id {
            get => id;
            set => id = value;
        }
        public QualifiedName QualifiedName { get; set; }
        int IIndexable.RefCount { get; set; } = 0;
        bool IIndexable.UseQualifiedName { get; } = true;
        bool IIndexable.UniqueType { get; } = false;

        object ISerializable.OnSerialize() => null;
        void ISerializable.OnDeserialize(object serialization) { }
        #endregion

        public ActiveEquipmentEx ActiveEquipment { get; set; } = null;

        Dictionary<Type, EffectEx> EffectsByType { get; } = new Dictionary<Type, EffectEx>();
        public EffectEx AddEffect(Type type) {
            if (EffectsByType.TryGetValue(type, out var effect))
                return effect;
            var template = (EffectExTemplate)IndexSystem.Instance.GetTypeInstance(type);
            effect = (EffectEx)template.CreateInstance(null, null);
            if (effects is null || (effects.Count > 0 && effects[0] == EffectEx.Empty))
                effects = new List<Effect>();
            if (effects.Count > 0)
                ((EffectEx)effects[effects.Count - 1]).IsLastInOrder = false;
            effect.IsLastInOrder = true;
            effects.Add(effect);
            EffectsByType.Add(type, effect);
            return effect;
        }
        public EffectEx AddEffect<T>() where T : EffectExTemplate => AddEffect(typeof(T));
        public EffectEx GetEffect(Type type) {
            if (EffectsByType.TryGetValue(type, out var effect))
                return effect;
            throw new ArgumentException("Effect not found", type.FullName);
        }
        public EffectEx GetEffect<T>() where T : EffectExTemplate => GetEffect(typeof(T));
        public bool HasEffect(Type type) => EffectsByType.ContainsKey(type);
        public bool HasEffect<T>() where T : EffectExTemplate => HasEffect(typeof(T));
        public bool RemoveEffect(Type type) {
            if (!EffectsByType.TryGetValue(type, out var ef))
                return false;
            EffectsByType.Remove(type);
            effects.Remove(ef);
            return true;            
        }
        public bool RemoveEffect<T>() where T : EffectExTemplate => RemoveEffect(typeof(T));

        public Dictionary<Type, EquipmentComponent> ComponentsByType { get; } = new Dictionary<Type, EquipmentComponent>();
    }
}
