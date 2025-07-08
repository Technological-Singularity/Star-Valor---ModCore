using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

/*
 * Inventory.EquipItem
 * ShipInfo.RemoveItem
 * ShipInfo.ChangeWeaponSlot




*/
//CargoSystem =>
//	itemType == 1 > GameData.data.EquipWeapon (weapon) EquipedWeapon GameData.data.weaponList TWeapon >> all indexing for weapons is done by accessing array position in GameData.data.weaponList
//		fortunately every entry is fully customized, and this list is serialized
//			GameData.GetPredefinedData()
//			WeaponCrafting.BuildWeapon()
//          GameDataInfo.AddWeaponData()
//          EquipedWeapon (class) ==> EquippedWeaponEx ??
//			Tweapon has:
//
//	itemType == 2 > EquipmentDB
//	itemType == 3 > ItemDB (item?)
//	itemType == 4 > 
//	itemType == 5 > UnlockCrewMember (crew)
//	itemType == 6 > inventory.newFleetCount (ship?)


//Plan
//	Use msb of itemId to indicate that it's a custom item
//	other 31 bits are used to look up info in table
//	

//Shouldn't need to use this - patch the following instead
//	GetEquipment ==> get equipment by database id; commonly used
//	GetEquipmentByIndex ==> get equipment by its index in the table >> seems to be something for the editor
//	GetEquipmentByType ==> default loadouts for equipment using a specific id >> only used for creating AI loadouts; will need to be patched for custom equipment
//	GetEquipmentString ==> get equipment name; will need to be patched
//	GetRandomEquipment ==> get random equipment with a given effect type as its primary effect, or -1 for any type
//	GetEffect ==> uses EquipmentDB to fetch effects given item id; patch to route to custom equipment
//	GetDiminishingEffect ==> see GetEffect

namespace Charon.StarValor.ModCore {
    [RegisterManual]
    public class DefaultEquipmentTemplate : EquipmentExTemplate {
        static Dictionary<int, DefaultEquipmentTemplate> registered = new Dictionary<int, DefaultEquipmentTemplate>();
        public static DefaultEquipmentTemplate Register(Equipment equipment) {
            if (registered.TryGetValue(equipment.id, out var wr))
                return wr;
            ModCore.Instance.Log.LogMessage($"    Registering {equipment.name} as EquipmentEx [{equipment.id}]");
            wr = new DefaultEquipmentTemplate(equipment);
            IndexSystem.Instance.AllocateTypeInstance(wr, Utilities.Int_to_Guid(equipment.id));
            registered.Add(equipment.id, wr);
            return wr;
        }

        public string Name { get; }
        public override string DisplayName => null;

        public override bool UseQualifiedName { get; } = false;
        public override bool UniqueType { get; } = false;

        List<EffectExTemplate> effects;

        #region Binds
        static List<(PropertyInfo, FieldInfo)> binds { get; } = Utilities.GetBindsPropertyField<BaseValues, Equipment>();
        class BaseValues {
            public int Id { get; set; }
            public string RefName { get; set; }
            public ShipClassLevel MinShipClass { get; set; } = ShipClassLevel.Shuttle;
            public bool Activated { get; set; }
            public bool EnableChangeKey { get; set; } = true;
            public float Space { get; set; } = 1f;
            public float EnergyCost { get; set; }
            public bool EnergyCostPerShipClass { get; set; }
            public float RarityCostMod { get; set; }
            public int TechLevel { get; set; } = 1;
            public int SortPower { get; set; } = 1;
            public float MassChange { get; set; }
            public EquipmentType Type { get; set; }
            public List<Effect> Effects { get; set; }
            public bool UniqueReplacement { get; set; }
            public float RarityMod { get; set; } = 1f;
            public int SellChance { get; set; } = 100;
            public ReputationRequisite RepReq { get; set; }
            public DropLevel DropLevel { get; set; }
            public int LootChance { get; set; } = 100;
            public bool SpawnInArena { get; set; } = true;
            public Sprite Sprite { get; set; }
            public int ActiveEquipmentIndex { get; set; }
            public KeyCode DefaultKey { get; set; }
            public GameObject Buff { get; set; }
            public int RequiredItemID { get; set; } = -1;
            public int RequiredQnt { get; set; }
            public string EquipName { get; set; }
            public string Description { get; set; }
            public List<CraftMaterial> CraftingMaterials { get; set; }
        }
        BaseValues bindValues = new BaseValues();
        #endregion

        DefaultEquipmentTemplate(Equipment equipment) {
            Name = equipment.name;
            effects = equipment.effects?.Select(o => DefaultEffectTemplate.Register(o)).ToList() ?? null;
            Utilities.BindSet(bindValues, binds, bindValues, equipment);
        }       
        public override IEnumerable<EquipmentEx> GetAllPermutations() {
            return new List<EquipmentEx>(Instances.Select(o => (EquipmentEx)o.Value));
        }
        protected override void BeginInstantiation(EquipmentEx eq) { }
        protected override void FinishInstantiation(EquipmentEx eq) { }

        public override void OnApplying(IIndexableInstance instance) {
            base.OnApplying(instance);
            var eq = (EquipmentEx)instance;

            Utilities.BindSet(instance, binds, bindValues, instance);
            //ModCore.Instance.Log.LogWarning("---- Binds for " + eq.name);
            //foreach (var (name, val) in Utilities.BindDump(instance, binds))
            //    ModCore.Instance.Log.LogWarning("    " + name + " : " + val);
            //ModCore.Instance.Log.LogWarning("----");

            eq.id = Utilities.Guid_to_Int(Guid);
            eq.effects = effects.Select(o => (Effect)o.CreateInstance(null, null)).ToList();
            eq.ActiveEquipment = null;
        }
        public override void OnRemoving(IIndexableInstance instance) {
            base.OnRemoving(instance);
            var eq = (EquipmentEx)instance;
            eq.ActiveEquipment = null;
            eq.activeEquipmentIndex = 0;
        }
    }
}
