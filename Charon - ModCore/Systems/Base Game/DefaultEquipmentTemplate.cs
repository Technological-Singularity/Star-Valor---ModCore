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
        public static List<(PropertyInfo, FieldInfo)> Binds { get; } = Utilities.GetBindsPropertyField<DefaultComponent, Equipment>();

        public List<EffectExTemplate> effects;
        public DefaultComponent values;
        GameObject GO { get; } = new GameObject();

        public string Name { get; }
        public override string DisplayName => null;

        public override bool UseQualifiedName { get; } = false;
        public override bool UniqueType { get; } = false;
        protected override ActiveEquipmentExTemplate ActiveEquipmentTemplate => null;

        DefaultEquipmentTemplate(Equipment equipment) {
            Name = equipment.name;
            IndexSystem.Instance.AllocateTypeInstance(this, equipment.id);
            effects = equipment.effects?.Select(o => DefaultEffectTemplate.Register(o)).ToList() ?? null;
            values = GO.AddComponent<DefaultComponent>();
            Utilities.BindSet(values, Binds, values, equipment); //copy base values
            equipment.id = -1;
        }
        ~DefaultEquipmentTemplate() {
            UnityEngine.Object.Destroy(values);
            UnityEngine.Object.Destroy(GO);
        }
        public static DefaultEquipmentTemplate Register(Equipment equipment) {
            ModCore.Instance.Log.LogMessage($"Registering {equipment.name} as EquipmentEx [{equipment.id}]");
            var template = new DefaultEquipmentTemplate(equipment);
            IndexSystem.Instance.AllocateTypeInstance(template, equipment.id);
            return template;
        }
        public Equipment CreateInstance() => (Equipment)CreateInstance(new QualifiedName(typeof(Equipment), Name), Id);
        
        public override IEnumerable<EquipmentEx> GetAllPermutations() {
            return new List<EquipmentEx>(Instances.Select(o => (EquipmentEx)o.Value));
        }
        protected override void BeginInstantiation(EquipmentEx eq) { }
        protected override void FinishInstantiation(EquipmentEx eq) { }

        public override void OnApplying(IIndexableInstance instance) {
            var eq = (EquipmentEx)instance;
            var component = eq.TemplateData.GetComponent<DefaultComponent>();
            component.OnApplying(eq);
            if (component.Activated)
                eq.ActiveEquipment = IndexSystem.Instance.GetTypeInstance<ActiveEquipmentEx>(eq.activeEquipmentIndex);
            eq.effects = effects.Select(o => (Effect)o.CreateInstance(null, null)).ToList();
        }
        public override void OnRemoving(IIndexableInstance instance) {
            var eq = (EquipmentEx)instance;
            eq.ActiveEquipment = null;
            eq.activeEquipmentIndex = 0;
            UnityEngine.Object.Destroy(eq.TemplateData.GetComponent<DefaultComponent>());
        }
    }
}
