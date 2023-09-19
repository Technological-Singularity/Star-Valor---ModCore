using System;
using System.Collections.Generic;
using Charon.StarValor.ModCore;
using Charon.StarValor.ModCore.Procedural;
using UnityEngine;

namespace Charon.StarValor.MasterTinker {
    [HasEquipments]
    public partial class Equipment_HyperspatialAnchor : EquipmentItem {
        static List<EquipmentEx> equipments = null;
        public static List<EquipmentEx> GetEquipment() {
            if (equipments == null)
                equipments = GetAllPermutations<Equipment_HyperspatialAnchor>();
            return equipments;
        }

        public static void Initialize() {
            var es = Plugin.Instance.EffectSystem;

            es.Register(name: "anchor_count", description: "Anchor Count: %ceil%", rarityMod: 1f, uniqueLevel: UniqueLevel.Heavy);
            es.Register(name: "anchor_force", description: "Anchor Force: %floor%", rarityMod: 0.4f);
            es.Register(name: "anchor_range", description: "Anchor Range: %floor%", rarityMod: 0.3f, invertRarityMod: true);
        }

        protected override ModCorePlugin Context => Plugin.Instance;
        public override string Name => DisplayName.ToLowerInvariant().Replace(" ", "_");
        public override string DisplayName => "Hyperspatial Anchor";
        public override string Description => "Deploys spatial anchors to this area of space, opposing all movement";

        protected override IEnumerable<Type> ValidSubcomponentTypes => new List<Type>() {
                typeof(Size),
            };
        protected override void OnGenerate(EquipmentGenerator generator) {
            var eq = generator.Template;
            eq.activated = true;
            eq.activeEquipmentIndex = Plugin.Instance.IndexSystem.Get(IndexType.ActiveEffect, typeof(AE_HyperspatialAnchor).FullName);
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
        public override void Finish(EquipmentGenerator generator) {
            generator.Template.description = string.Join(" ", Description, generator.Components[0].Description);
        }
    }
}
