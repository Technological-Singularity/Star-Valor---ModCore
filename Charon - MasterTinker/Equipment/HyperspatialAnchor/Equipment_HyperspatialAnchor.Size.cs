using Charon.StarValor.ModCore.Procedural;
namespace Charon.StarValor.MasterTinker {
    public partial class Equipment_HyperspatialAnchor {
        abstract class Size : EquipmentComponent {
            protected abstract ShipClassLevel MinSize { get; }
            protected override bool PrependName => true;
            protected abstract float Count { get; }
            protected abstract float Range { get; }
            protected abstract float Force { get; }
            protected override void OnGenerate(EquipmentGenerator generator) {
                generator.Template.minShipClass = MinSize;
                generator["anchor_count"].value = Count;
                generator["anchor_range"].value = Range;
                generator["anchor_force"].value = Force;
            }
            class Small : Size {
                public override string Name => "0";
                public override string DisplayName => null;
                protected override ShipClassLevel MinSize => ShipClassLevel.Shuttle;
                protected override float Count => 1;
                protected override float Range => 40;
                protected override float Force => 10;
            }
            class Large : Size {
                public override string Name => "3";
                public override string DisplayName => "Large";
                protected override ShipClassLevel MinSize => ShipClassLevel.Corvette;
                protected override float Count => 4;
                protected override float Range => 70;
                protected override float Force => 17;
            }
            class Capital : Size {
                public override string Name => "5";
                public override string DisplayName => "Capital";
                protected override ShipClassLevel MinSize => ShipClassLevel.Cruiser;
                protected override float Count => 6;
                protected override float Range => 160;
                protected override float Force => 30;
            }
        }
    }
}
