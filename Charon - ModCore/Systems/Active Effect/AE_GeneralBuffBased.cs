using UnityEngine;

namespace Charon.StarValor.ModCore {
    public abstract class AE_GeneralBuffBased : AE_BuffBased {
        protected override bool showBuffIcon => this.isPlayer;
        public override void ActivateDeactivate(bool shiftPressed, Transform target) {
            if (this.buffGO is null && this.equipment.buff is null) {
                this.buffGO = new GameObject();
                var control = this.buffGO.AddComponent<BuffControl>();

                control.owner = this.ss;
                control.transform.SetParent(this.ss.transform);
                control.activeEquipment = this;

                var energy = this.buffGO.AddComponent<BuffEnergyChange>();
                energy.affectOwner = true;

                this.AfterInstantiateBuffGO();

                control.Setup();
            }
            base.ActivateDeactivate(shiftPressed, target);
        }
    }
}
