//using System;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

////CargoSystem =>
////	itemType == 1 > GameData.data.EquipWeapon (weapon) EquipedWeapon GameData.data.weaponList TWeapon >> all indexing for weapons is done by accessing array position in GameData.data.weaponList
////		fortunately every entry is fully customized, and this list is serialized
////			GameData.GetPredefinedData()
////			WeaponCrafting.BuildWeapon()
////          GameDataInfo.AddWeaponData()
////          EquipedWeapon (class) ==> EquippedWeaponEx ??
////			Tweapon has:
////
////	itemType == 2 > EquipmentDB
////	itemType == 3 > ItemDB (item?)
////	itemType == 4 > 
////	itemType == 5 > UnlockCrewMember (crew)
////	itemType == 6 > inventory.newFleetCount (ship?)


////Plan
////	Use msb of itemId to indicate that it's a custom item
////	other 31 bits are used to look up info in table
////	

////Shouldn't need to use this - patch the following instead
////	GetEquipment ==> get equipment by database id; commonly used
////	GetEquipmentByIndex ==> get equipment by its index in the table >> seems to be something for the editor
////	GetEquipmentByType ==> default loadouts for equipment using a specific id >> only used for creating AI loadouts; will need to be patched for custom equipment
////	GetEquipmentString ==> get equipment name; will need to be patched
////	GetRandomEquipment ==> get random equipment with a given effect type as its primary effect, or -1 for any type
////	GetEffect ==> uses EquipmentDB to fetch effects given item id; patch to route to custom equipment
////	GetDiminishingEffect ==> see GetEffect

//namespace Charon.StarValor.ModCore {
//    public class EquipmentExTemplate2 : IndexableTemplate {
//        QualifiedName QualifiedName { get; }
//        public EquipmentItem Parent { get; }
//        public List<Type> Subcomponents { get; } = new List<Type>();        

//        static QualifiedName GetQualifiedName(PluginContext context, EquipmentItem parent, IEnumerable<Type> subcomponents) {
//            string name = parent.Name;
//            if (!(subcomponents is null))
//                name += "++" + string.Join("+", subcomponents.Select(o => o.Name));
//            return context.Qualify(name);
//        }

//        public static EquipmentExTemplate GetTemplate(PluginContext context, EquipmentItem parent, IEnumerable<Type> subcomponents) {
//            var qname = GetQualifiedName(context, parent, subcomponents);
//            if (IndexSystem.Instance.TryGetRegistered<EquipmentExTemplate>(qname, out var wr))
//                return wr;
//            wr = new EquipmentExTemplate(context, parent, subcomponents);
//            return wr;
//        }

//        protected EquipmentExTemplate(QualifiedName qualifiedName) {
//            Parent = null;
//            Subcomponents = null;
//            QualifiedName = qualifiedName;
//        }
//        protected EquipmentExTemplate(PluginContext context, EquipmentItem parent, IEnumerable<Type> subcomponents) {
//            Parent = parent;
//            Subcomponents = subcomponents is null ? null : new List<Type>(subcomponents);
//            QualifiedName = GetQualifiedName(context, parent, subcomponents);
//            IndexSystem.Instance.Register(QualifiedName, this);
//        }

//        public override void OnApplying(IIndexableInstance instance, object data) {
//            var eq = (EquipmentEx)instance;
//            eq.effects = eq.GetEffects()?.Select(o => (Effect)o).ToList() ?? new List<Effect>();
//            eq.name = QualifiedName.FullName;
//        }

//        public override IIndexableInstance GenerateInstance(int id, object data) {
//            var eq = ScriptableObject.CreateInstance<EquipmentEx>();
//            eq.id = id;

//            Apply(eq, data);
//            Parent?.Generate(eq, Subcomponents);            
           
//            ModCorePlugin.Log.LogWarning("Generated " + eq.name);

//            return eq;
//        }
//    }
//}
