////To do: bind fields from spaceship stats to effects, create some binding method for weapons/other objects

using System;
using System.Collections.Generic;
using System.Linq;

namespace Charon.StarValor.ModCore.Procedural {
    public abstract class EquipmentItem {
        public abstract string Name { get; }
        public abstract string DisplayName { get; }
        public virtual string Description { get; }
        protected virtual bool PrependName { get; } = false;
        protected virtual string DisplayNameSeparator { get; } = " ";
        protected void AddName(EquipmentEx equipment, bool appendInternal = true) {
            if (appendInternal)
                equipment.name += "_" + Name;

            if (DisplayName != null) {
                if (string.IsNullOrEmpty(equipment.equipName))
                    equipment.equipName = DisplayName;
                else if (PrependName)
                    equipment.equipName = DisplayName + DisplayNameSeparator + equipment.equipName;
                else
                    equipment.equipName = equipment.equipName + DisplayNameSeparator + DisplayName;
            }
        }
        protected virtual void OnGenerate(EquipmentGenerator generator) { }
        public virtual void Finish(EquipmentGenerator generator) { }

        protected abstract IEnumerable<Type> ValidSubcomponentTypes { get; }
        protected abstract ModCorePlugin Context { get; }

        public static List<EquipmentEx> GetAllPermutations<T>() => ((EquipmentItem)Activator.CreateInstance(typeof(T))).GetAllPermutations();
        public List<EquipmentEx> GetAllPermutations() {
            List<EquipmentEx> wr = new List<EquipmentEx>();
            int[] strides = new int[ValidSubcomponentTypes.Count()];
            List<List<Type>> types = new List<List<Type>>();
            foreach (var rtype in ValidSubcomponentTypes)
                types.Add(Utilities.EnumerateTypes(o => rtype.IsAssignableFrom(o) && !o.IsAbstract).ToList());

            strides[strides.Length - 1] = 1;
            for (int i = strides.Length - 1; i > 0; --i)
                strides[i - 1] = strides[i] * types[i].Count;

            for (int i = 0; i < strides[0] * types[0].Count; ++i) {
                Type[] perm_types = new Type[strides.Length];
                int rem = i;
                for (int j = 0; j < strides.Length; ++j) {
                    int idx = rem / strides[j];
                    perm_types[j] = types[j][idx];
                    rem -= idx * strides[j];
                }
                wr.Add(GenerateTemplate(perm_types));
            }
            return wr;
        }

        public EquipmentEx GenerateTemplate(IEnumerable<Type> componentTypes) {
            EquipmentGenerator generator = new EquipmentGenerator(Context);
            var equipment = generator.Template;
            equipment.name = Name;
            OnGenerate(generator);
            foreach (var type in componentTypes) {
                var component = (EquipmentComponent)Activator.CreateInstance(type);
                generator.Components.Add(component);
                component.Generate(generator);
            }
            foreach (var component in generator.Components)
                component.Finish(generator);

            Finish(generator);
            AddName(equipment, false);
            equipment.effects = generator.GetEffects().Select(o => (Effect)o).ToList();
            equipment.name = Context.Qualify(equipment.name).FullName;
            ModCorePlugin.Log.LogWarning("Generated " + equipment.name);
            return equipment;
        }
    }
}
