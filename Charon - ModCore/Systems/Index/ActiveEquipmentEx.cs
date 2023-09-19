using System;
using HarmonyLib;
using UnityEngine;

namespace Charon.StarValor.ModCore {
    [HasPatches]
    public abstract class ActiveEquipmentEx : ActiveEquipment, IIndexableInstance {
        #region Patches
        [HarmonyPatch(typeof(ActiveEquipment), nameof(ActiveEquipment.AddActivatedEquipment))]
        [HarmonyPrefix]
        public static bool AddActivatedEquipment(ref ActiveEquipment __result, Equipment equipment, SpaceShip ss, KeyCode key, int rarity, int qnt) {
            if (!IndexSystem.Instance.TryGetAllocatedType<ActiveEquipmentExTemplate>(equipment.activeEquipmentIndex, out var type))
                return true;
            var aex = (ActiveEquipmentEx)Activator.CreateInstance(type);
            aex.Initialize((EquipmentEx)equipment, ss, key, rarity, qnt);
            ss.activeEquips.Add(aex);
            __result = aex;
            return false;
        }
        #endregion
        #region IIndexableInstance
        public IndexableTemplate Template => Data.Template;
        public IndexableInstanceData Data { get; }
        public ActiveEquipmentEx(int id) => (this.id, Data) = (id, new IndexableInstanceData(this));
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
        #region Initialization
        void Initialize(EquipmentEx equipment, SpaceShip ss, KeyCode key, int rarity, int qnt) {
            this.id = equipment.id;
            this.key = key;
            this.ss = ss;
            this.isPlayer = ss != null && ss.CompareTag("Player");
            this.equipment = equipment;
            this.rarity = rarity;
            this.qnt = qnt;
            this.AfterConstructor();
        }
        #endregion
        #region Internal
        public abstract QualifiedName QualifiedName { get; }
        public TargetModeInfo TargetInfo { get; } = new TargetModeInfo();
        protected EquipmentEx EquipmentEx => (EquipmentEx)equipment;
        protected virtual bool SaveCooldown { get; } = false;
        #endregion
        #region Framework
        public abstract int GetInstanceHash(object data);
        public abstract object GetDefaultData();
        public abstract object GenerateInstance(object data);
        #endregion
    }
}
