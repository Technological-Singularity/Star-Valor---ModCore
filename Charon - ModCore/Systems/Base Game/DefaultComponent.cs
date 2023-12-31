﻿using System.Collections.Generic;
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
    public class DefaultComponent : ComponentEx {
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

        public override void OnApplying(IIndexableInstance instance) {
            Utilities.BindSet(instance, DefaultEquipmentTemplate.Binds, this, instance);
        }
    }
}
