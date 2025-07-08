using Charon.StarValor.ModCore;
namespace Charon.StarValor.MasterTinker {
    public partial class Equipment_HyperspatialAnchor {
        abstract class Size : EquipmentComponent {
            protected abstract ShipClassLevel MinSize { get; }
            public override int NamePriority => -10;
            protected abstract float Count { get; }
            protected abstract float Range { get; }
            protected abstract float Force { get; }
            public override void BeginInstantiation(EquipmentEx eq) {
                eq.minShipClass = MinSize;
                eq.GetEffect<Effects.Count>().value = Count;
                eq.GetEffect<Effects.Force>().value = Force;
                eq.GetEffect<Effects.Range>().value = Range;
            }
            class Small : Size {
                public override string DisplayName => null;
                public override string DisplayNameSeparator => null;
                protected override ShipClassLevel MinSize => ShipClassLevel.Shuttle;
                protected override float Count => 1;
                protected override float Range => 40;
                protected override float Force => 10;
            }
            class Large : Size {
                public override string DisplayName => "Large";
                protected override ShipClassLevel MinSize => ShipClassLevel.Corvette;
                protected override float Count => 4;
                protected override float Range => 70;
                protected override float Force => 17;
            }
            class Capital : Size {
                public override string DisplayName => "Capital";
                protected override ShipClassLevel MinSize => ShipClassLevel.Cruiser;
                protected override float Count => 6;
                protected override float Range => 160;
                protected override float Force => 30;
            }
        }
    }
}
