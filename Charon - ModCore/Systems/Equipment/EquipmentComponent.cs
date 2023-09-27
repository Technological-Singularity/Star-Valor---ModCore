////To do: bind fields from spaceship stats to effects, create some binding method for weapons/other objects

namespace Charon.StarValor.ModCore {
    public abstract class EquipmentComponent {
        public abstract string DisplayName { get; }
        public virtual string Description { get; }
        public virtual int NamePriority => 0;
        public virtual string DisplayNameSeparator { get; } = " ";

        public virtual void BeginInstantiation(EquipmentEx eq) { }
        public virtual void FinishInstantiation(EquipmentEx eq) { }
    }
}
