//////To do: bind fields from spaceship stats to effects, create some binding method for weapons/other objects

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

//namespace Charon.StarValor.ModCore {
//    public class EquipmentGenerator {
//        Dictionary<QualifiedName, EffectDescriptor> descriptors = new Dictionary<QualifiedName, EffectDescriptor>();
//        PluginContext context;
//        public EquipmentEx Instance { get; }
//        public EffectDescriptor this[string name] => GetDescriptorQualified(ModCorePlugin.Qualify(context.Guid, name));

//        public EquipmentGenerator(PluginContext context, EquipmentExTemplate template, object data) {
//            this.context = context;
//            Instance = (EquipmentEx)template.Allocate(data);
//        }

//        public EffectDescriptor GetDescriptorQualified(QualifiedName qualifiedName) {
//            if (descriptors.TryGetValue(qualifiedName, out var wr))
//                return wr;
//            wr = default; //fixme
//            descriptors[qualifiedName] = wr;
//            return wr;
//        }
//        public void ForEach(Action<(QualifiedName qualifiedName, EffectDescriptor descriptor)> func) {
//            foreach (var o in descriptors)
//                func((o.Key, o.Value));
//        }
//        public List<EffectEx> GetEffects() => descriptors.Values.Select(o => o.CreateEffect()).ToList();
//        public List<EquipmentComponent> Components { get; } = new List<EquipmentComponent>();
//    }
//}
