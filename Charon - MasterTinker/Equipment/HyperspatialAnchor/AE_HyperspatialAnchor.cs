using Charon.StarValor.ModCore;
namespace Charon.StarValor.MasterTinker {
    [HasActiveIndex(Plugin.pluginGuid)]
    public class AE_HyperspatialAnchor : AE_GeneralBuffBased {
        public AE_HyperspatialAnchor() {
            this.targetIsSelf = true;
            this.saveState = true;
        }
        protected override bool AfterInstantiateBuffGO() {
            buffGO.AddComponent<Buff_HyperspatialAnchor>();
            base.AddEnergyChange(1f);
            return true;
        }
        protected override bool AfterSetup(bool shiftPressed) {
            buffGO.GetComponent<Buff_HyperspatialAnchor>().Initialize(this.equipment, this.rarity, this.qnt);
            return base.AfterSetup(shiftPressed);
        }
    }
}