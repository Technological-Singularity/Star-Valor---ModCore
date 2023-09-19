////To do: bind fields from spaceship stats to effects, create some binding method for weapons/other objects

namespace Charon.StarValor.ModCore {
    public partial class EffectSystem {
        public class EffectDescriptor {
            public int type;
            public string description;

            //if this value is < 0, it denotes that the value should be *inverted* (not reversed)
            //to reverse values, i.e. get negative values, use rarityMod on the equipment items
            public float rarityMod;

            public int uniqueLevel;
            public float value;

            public EffectEx CreateEffect(float value) {
                //return new EffectEx() {
                //    type = type,
                //    description = description,
                //    value = value,
                //    mod = rarityMod,
                //    uniqueLevel = uniqueLevel,
                //};
                return default; //fixme
            }
            public EffectEx CreateEffect() => CreateEffect(value);
            public EffectDescriptor Clone() => (EffectDescriptor)MemberwiseClone();
        }
    }
}
