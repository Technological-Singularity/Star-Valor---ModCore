////To do: bind fields from spaceship stats to effects, create some binding method for weapons/other objects

namespace Charon.StarValor.ModCore.Procedural {
    public abstract class EquipmentComponent : EquipmentBase {
        public void Generate(EquipmentGenerator generator) {
            AddName(generator.Template);
            OnGenerate(generator);
        }
    }
}
