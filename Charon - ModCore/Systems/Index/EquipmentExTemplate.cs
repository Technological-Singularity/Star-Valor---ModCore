using UnityEngine;

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
    public abstract class EquipmentExTemplate : IndexableTemplate {
        public override IIndexableInstance GenerateInstance(int id, object data) {
            IIndexableInstance instance = ScriptableObject.CreateInstance<EquipmentEx>();
            instance.Id = id;
            Apply(instance, data);
            return instance;
        }
    }
}
