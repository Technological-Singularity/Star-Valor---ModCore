////To do: bind fields from spaceship stats to effects, create some binding method for weapons/other objects

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

using System;
using System.Collections.Generic;
using System.Linq;

namespace Charon.StarValor.ModCore {
    public abstract class EquipmentExTemplate : IndexableTemplate {
        protected override Type InstanceType => typeof(EquipmentEx);

        public abstract string DisplayName { get; }
        public virtual string Description { get; }
        protected virtual IEnumerable<Type> ValidSubcomponentTypes { get; } = null;
        protected virtual ActiveEquipmentExTemplate ActiveEquipmentTemplate { get; } = null;

        protected virtual void BeginInstantiation(EquipmentEx eq) { }
        protected virtual void FinishInstantiation(EquipmentEx eq) { }

        protected EquipmentExTemplate() => QualifiedName = GetQualifiedName();
        public override bool CanRegister() => true;
        public override void OnRegister() => this.GetAllPermutations();

        protected override QualifiedName GetInstanceQualifiedName(IIndexableInstance instance) {
            var eq = (EquipmentEx)instance;
            string name = base.GetInstanceQualifiedName(instance).Name;
            if (eq.ComponentsByType.Count > 0)
                name += "|" + string.Join("|", eq.ComponentsByType.Keys.Select(o => o.Name));
            return new QualifiedName(this, name);
        }
        QualifiedName GetQualifiedName(IEnumerable<Type> subcomponents) {
            string name = new QualifiedName(this).Name;
            if (!(subcomponents is null))
                name += "|" + string.Join("|", subcomponents.Select(o => o.Name));
            return new QualifiedName(this, name);
        }
        public static IEnumerable<EquipmentEx> GetAllPermutations<T>() where T : EquipmentExTemplate => IndexSystem.Instance.GetTypeInstance<T>().GetAllPermutations();
        public virtual IEnumerable<EquipmentEx> GetAllPermutations() {
            List<EquipmentEx> wr = new List<EquipmentEx>();
            if (ValidSubcomponentTypes is null) {
                wr.Add(InstantiatePermutation(GetQualifiedName(null), null));
            }
            else {
                List<List<Type>> types = new List<List<Type>>();
                foreach (var rtype in ValidSubcomponentTypes)
                    types.Add(Utilities.EnumerateTypes(o => rtype.IsAssignableFrom(o) && !o.IsAbstract).ToList());

                //Treat this as a multidimensional array being indexed via a single number
                int[] strides = new int[ValidSubcomponentTypes.Count()];
                strides[strides.Length - 1] = 1;
                for (int i = strides.Length - 1; i > 0; --i)
                    strides[i - 1] = strides[i] * types[i].Count;

                //For every possible permutation with index i, calculate each dimension index
                for (int i = 0; i < strides[0] * types[0].Count; ++i) {
                    Type[] perm_types = new Type[strides.Length];
                    int rem = i;
                    for (int j = 0; j < strides.Length; ++j) {
                        int idx = rem / strides[j];
                        perm_types[j] = types[j][idx];
                        rem -= idx * strides[j];
                    }
                    AddPermutation(wr, perm_types);
                }
            }
            return wr;
        }
        void AddPermutation(List<EquipmentEx> list, IEnumerable<Type> subcomponents) {
            var qualifiedName = GetQualifiedName(subcomponents);
            if (!Instances.TryGetValue(qualifiedName, out var eq))
                eq = InstantiatePermutation(qualifiedName, subcomponents);
            list.Add((EquipmentEx)eq);
        }
        EquipmentEx InstantiatePermutation(QualifiedName qualifiedName, IEnumerable<Type> subcomponents) {
            Dictionary<int, List<EquipmentComponent>> nameComponents = new Dictionary<int, List<EquipmentComponent>>();
            void addNameComponent(Dictionary<int, List<EquipmentComponent>> dict, EquipmentComponent eqcomp) {
                var priority = eqcomp.NamePriority;
                if (!dict.TryGetValue(priority, out var list)) {
                    list = new List<EquipmentComponent>();
                    dict.Add(priority, list);
                }
                list.Add(eqcomp);
            }
            string getName(Dictionary<int, List<EquipmentComponent>> dict) {
                string name = "";
                bool displayNameAdded = false;
                var pairs = dict.OrderBy(o => o.Key);
                foreach(var pair in pairs) {
                    var(priority, list) = (pair.Key, pair.Value);
                    foreach(var component in list) {
                        if (priority <= 0) {
                            name += component.DisplayName + component.DisplayNameSeparator;
                        }
                        else {
                            if (!displayNameAdded) {
                                name += DisplayName;
                                displayNameAdded = true;
                            }
                            name += component.DisplayNameSeparator + component.DisplayName;
                        }
                    }
                }
                if (!displayNameAdded)
                    name += DisplayName;
                return name;
            }

            var eq = (EquipmentEx)CreateInstance(qualifiedName, null);
            BeginInstantiation(eq);
            if (!(subcomponents is null)) {
                foreach (var (componentType, subtype) in ValidSubcomponentTypes.Zip(subcomponents, (o, n) => (o, n))) {
                    var component = (EquipmentComponent)Activator.CreateInstance(subtype);
                    eq.ComponentsByType.Add(componentType, component);
                    component.BeginInstantiation(eq);
                }
                foreach (var o in eq.ComponentsByType.Values)
                    o.FinishInstantiation(eq);
                foreach (var o in eq.ComponentsByType.Values)
                    addNameComponent(nameComponents, o);
            }
            FinishInstantiation(eq);
            eq.equipName = getName(nameComponents);
            return eq;
        }
        public override void OnApplying(IIndexableInstance instance) {
            var eq = (EquipmentEx)instance;
            eq.name = instance.QualifiedName.Name;
            eq.id = Utilities.Guid_to_Int(eq.Guid);
            if (!(ActiveEquipmentTemplate is null))
                eq.ActiveEquipment = (ActiveEquipmentEx)ActiveEquipmentTemplate.CreateInstance(null, eq.Guid);
        }
        public override void OnRemoving(IIndexableInstance instance) {
            var eq = (EquipmentEx)instance;
        }
    }
}
