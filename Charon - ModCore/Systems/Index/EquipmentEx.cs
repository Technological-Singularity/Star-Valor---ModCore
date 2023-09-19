using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace Charon.StarValor.ModCore {
    [HasPatches]
    public sealed class EquipmentEx : Equipment, IIndexableInstance {
        #region Patches
        static Dictionary<int, Equipment> equipmentDict = new Dictionary<int, Equipment>();
        static MethodInfo _EquipmentDB_ValidateDatabase = typeof(EquipmentDB).GetMethod("ValidateDatabase", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        static Action EquipmentDB_ValidateDatabase = () => _EquipmentDB_ValidateDatabase.Invoke(null, null);

        /// <summary>
        /// When database is reloaded, add EquipmentEx to the database and the lookup table; add default equipment to the lookup table
        /// </summary>
        /// <param name="___equipments"></param>
        /// <exception cref="Exception"></exception>
        /// 
        static bool defaultEquipmentRegistered = false;
        [HarmonyPatch(typeof(EquipmentDB), nameof(EquipmentDB.LoadDatabaseForce))]
        [HarmonyPostfix]
        public static void LoadDatabaseForce(List<Equipment> ___equipments) {
            if (!defaultEquipmentRegistered) {
                defaultEquipmentRegistered = true;
                foreach (var equipment in ___equipments)
                    DefaultEquipmentTemplate.Register(equipment);
            }

            ___equipments = new List<Equipment>();
            foreach (var o in IndexSystem.Instance.GetAllocatedInstances<EquipmentEx>()) {
                ___equipments.Add(o);
                equipmentDict.Add(o.id, o);
            }
        }

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
        #endregion

        #region IIndexableInstance
        public IndexableTemplate Template => Data.Template;
        public IndexableInstanceData Data { get; }
        public EquipmentEx() => Data = new IndexableInstanceData(this);
        public int GetHashCode(HashContext context) => Data.GetHashCode(context);
        public int Id {
            get => id;
            set => id = value;
        }
        public void Allocate() => IndexSystem.Instance.Ref(this);
        public bool Release() => IndexSystem.Instance.Deref(this);
        public object GetSerialization() => Data.GetSerialization();
        public void Deserialize(object serialization) => Data.Deserialize(serialization);
        #endregion
    }
}
