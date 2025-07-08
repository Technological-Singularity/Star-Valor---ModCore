using System;
using System.Collections.Generic;
using Charon.StarValor.ModCore;
using UnityEngine;

namespace Charon.StarValor.MasterTinker {
    public partial class Equipment_DeflectorArray : EquipmentItem {
        public override bool UseQualifiedName => true;
        public override bool UniqueType => true;
        public override object OnSerialize() => null;
        public override void OnDeserialize(object data) { }

        public override string DisplayName => "Array";
        public override string Description => "Protects the ship from damage by";
        protected override ActiveEquipmentExTemplate ActiveEquipmentTemplate { get; } = ActiveEquipmentExTemplate_BuffBased.Create<Buff_DeflectorArray>(saveState: true, energyChange: 1f);

        float Hardness { get; } = 80;

        protected override IEnumerable<Type> ValidSubcomponentTypes { get; } =
            new List<Type>() {
                typeof(Targeting),
                typeof(Mode),
                typeof(Size),
            };

        protected override void BeginInstantiation(EquipmentEx eq) {
            eq.activated = true;
            eq.activeEquipmentIndex = ActiveEquipmentEx.DummyIndex;
            eq.defaultKey = KeyCode.X;
            eq.dropLevel = DropLevel.DontDrop;
            eq.rarityCostMod = 1;
            eq.rarityMod = 1;
            eq.space = 0;
            eq.sprite = EquipmentDB.GetEquipmentByIndex(0).sprite;
            eq.techLevel = 1;
            eq.type = EquipmentType.Utility;
            eq.uniqueReplacement = true;
        }
        protected override void FinishInstantiation(EquipmentEx eq) {
            eq.description = string.Join(" ", Description, eq.ComponentsByType[typeof(Mode)].Description, eq.ComponentsByType[typeof(Targeting)].Description);
        }
        public override void OnApplying(IIndexableInstance instance) {
            base.OnApplying(instance);
            var eq = (EquipmentEx)instance;
            AddEffect<Effects.Emitters>(eq);
            AddEffect<Effects.Force>(eq);
            AddEffect<Effects.Hardness>(eq);
            AddEffect<Effects.Range>(eq);
            AddEffect<Effects.Targets>(eq);
            AddEffect<Effects.Magnitudes.Dispersion>(eq);
            AddEffect<Effects.Magnitudes.Repulsion>(eq);
            AddEffect<Effects.Magnitudes.Vectoring>(eq);

            eq.GetEffect<Effects.Emitters>().mod = 0.4f;
            eq.GetEffect<Effects.Force>().mod = 0.4f;
            eq.GetEffect<Effects.Hardness>().value = Hardness;
            eq.GetEffect<Effects.Hardness>().mod = 0.3f;
        }
    }
}
