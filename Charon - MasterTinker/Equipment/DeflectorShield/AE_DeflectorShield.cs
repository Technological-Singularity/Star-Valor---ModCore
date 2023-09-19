using Charon.StarValor.ModCore;

namespace Charon.StarValor.MasterTinker {
    [HasActiveIndex(Plugin.pluginGuid)]
    public class AE_DeflectorShield : AE_GeneralBuffBased {
        public AE_DeflectorShield() {
            this.targetIsSelf = true;
            this.saveState = true;
        }
        protected override bool AfterInstantiateBuffGO() {
            buffGO.AddComponent<Buff_DeflectorShield>();
            base.AddEnergyChange(1f);
            return true;
        }
        protected override bool AfterSetup(bool shiftPressed) {
            buffGO.GetComponent<Buff_DeflectorShield>().Initialize(this.equipment, this.rarity, this.qnt);
            return base.AfterSetup(shiftPressed);
        }
    }
}