using System;
using System.Collections.Generic;
using Charon.StarValor.ModCore;
using UnityEngine;

namespace Charon.StarValor.MasterTinker {
    public partial class Equipment_HyperspatialAnchor : EquipmentItem {
        public override bool UseQualifiedName => true;
        public override bool UniqueType => true;
        public override string DisplayName => "Hyperspatial Anchor";
        public override string Description => "Deploys spatial anchors to this area of space, opposing all movement";
        protected override ActiveEquipmentExTemplate ActiveEquipmentTemplate { get; } = ActiveEquipmentExTemplate_BuffBased.Create<Buff_HyperspatialAnchor>(saveState: true, energyChange: 1f);

        protected override IEnumerable<Type> ValidSubcomponentTypes { get; } =
            new List<Type>() {
                typeof(Size),
            };

        protected override void BeginInstantiation(EquipmentEx eq) {
            eq.activated = true;
            eq.activeEquipmentIndex = ((ActiveEquipmentExTemplate_BuffBased)ActiveEquipmentTemplate).Id;
            eq.defaultKey = KeyCode.K;
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
            eq.description = string.Join(" ", Description, eq.ComponentsByType[typeof(Size)].Description);
        }
        public override void OnApplying(IIndexableInstance instance) {
            var eq = (EquipmentEx)instance;
            AddEffect<Effects.Count>(eq);
            AddEffect<Effects.Force>(eq);
            AddEffect<Effects.Range>(eq);

            eq.GetEffect<Effects.Count>().mod = 1f;
            eq.GetEffect<Effects.Force>().mod = 0.4f;
            eq.GetEffect<Effects.Range>().mod = -0.3f;

            base.OnApplying(instance);
        }
    }
}
