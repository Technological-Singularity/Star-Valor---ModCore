using System;
using System.Collections.Generic;
using Charon.StarValor.ModCore;
using Charon.StarValor.ModCore.Procedural;
using UnityEngine;

namespace Charon.StarValor.MasterTinker {
    public partial class Equipment_DeflectorShield : EquipmentItem {
        public static void Initialize() {
            void register<T>() where T : EffectExTemplate, new() {
                var qname = Plugin.Instance.GetQualifiedName(typeof(T).Name);
                IndexSystem.Instance.Register(qname, new T());
            }
            register<Effects.Targets>();
            register<Effects.Magnitudes>();
            register<Effects.Force>();
            register<Effects.Range>();
            register<Effects.Hardness>();
            register<Effects.Emitters>();
        }

        protected override ModCorePlugin Context => Plugin.Instance;
        public override string Name => DisplayName.ToLowerInvariant();
        public override string DisplayName => "Array";
        public override string Description => "Protects the ship from damage by";
        float Hardness { get; } = 80;

        protected override IEnumerable<Type> ValidSubcomponentTypes { get; } =
            new List<Type>() {
                typeof(Targeting),
                typeof(Mode),
                typeof(Size),
            };
        protected override void OnGenerate(EquipmentGenerator generator) {
            var eq = generator.Template;
            eq.activated = true;
            eq.activeEquipmentIndex = IndexSystem.Instance.GetRegistered<ActiveEquipmentExTemplate> Get(IndexType.ActiveEffect, typeof(AE_DeflectorShield).FullName);
            eq.defaultKey = KeyCode.X;
            eq.dropLevel = DropLevel.DontDrop;
            eq.rarityCostMod = 1;
            eq.rarityMod = 1;
            eq.space = 0;
            eq.sprite = EquipmentDB.GetEquipmentByIndex(0).sprite;
            eq.techLevel = 1;
            eq.type = EquipmentType.Utility;
            eq.uniqueReplacement = true;

            generator["deflector_hardness"].value = Hardness;
        }
        public override void Finish(EquipmentGenerator generator) {
            generator.Template.description = string.Join(" ", Description, generator.Components[1].Description, generator.Components[0].Description);
        }
    }
}
