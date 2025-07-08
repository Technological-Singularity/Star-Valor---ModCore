using System;
using UnityEngine;

namespace Charon.StarValor.ModCore {
    public class ActiveEquipmentEx_BuffBased : ActiveEquipmentEx {
        protected GameObject buffGO;

        protected override bool showBuffIcon => this.isPlayer;

        public object AddBuff(Type type) => buffGO.GetComponent(type) ?? buffGO.AddComponent(type);
        public override void AfterConstructor() {
            if (saveCooldownID >= 0)
                timeDeactivated = Time.time - CooldownTime + ss.shipData.GetAECooldown(saveCooldownID);
            if (CooldownTime > 2f && CooldownRemaining > 0f)
                ShowCooldownIcon();
        }
        public override void Uninstall() {            
            base.Uninstall();
            if (cooldownIconControl != null) {
                BuffIcons.RemoveBuff(cooldownIconControl, freeBuffer: true);
                cooldownIconControl = null;
                if (saveCooldownID >= 0) {
                    ss.shipData.SaveAECooldown(saveCooldownID, CooldownRemaining);
                }
            }
            if (buffGO != null) {
                var control = buffGO.GetComponent<BuffControl>();
                if (control?.enabled ?? false)
                    control.Terminate(cascade: true);
            }
        }
        public override void ActivateDeactivate(bool shiftPressed, Transform target) {
            if (!TryActivateDeactivate(shiftPressed, ref target))
                return;

            if (this.buffGO == null) {
                buffGO = new GameObject();
                buffGO.transform.SetParent(target, worldPositionStays: true);
                var control = buffGO.AddComponent<BuffControl>();
                control.owner = this.ss;
                control.activeEquipment = this;
                control.targetEntity = target.GetComponent<Entity>();
                //control.Setup(); //for audio and vfx
                var energy = buffGO.AddComponent<BuffEnergyChange>();
                energy.affectOwner = true;
                OnInitializationInvoke();
            }

            if (buffGO.transform.parent != target) {
                buffGO.transform.SetParent(target, worldPositionStays: true);
                //buffGO.GetComponent<BuffControl>().Setup(); //for audio and vfx
            }

            if (!active)
                Activate(shiftPressed, target);
            else
                Deactivate(shiftPressed, target);
        }
        void Activate(bool shiftPressed, Transform target) {
            if (!OnActivateInvoke(shiftPressed, target))
                return;            
            ConsumedRequiredItem(false);
            active = true;
            buffGO.GetComponent<BuffControl>().Begin();
            AfterActivate();
        }
        protected virtual void Deactivate(bool shiftPressed, Transform target) {
            if (!OnDeactivateInvoke(shiftPressed, target))
                return;
            if (buffGO is null || target is null || !canTurnOff)
                return;
            active = false;
            buffGO.GetComponent<BuffControl>().End();
            AfterDeactivate();
        }
        public override sealed void AfterDeactivate() {
            base.AfterDeactivate();
            if (CooldownTime > 0f) {
                timeDeactivated = Time.time;
                ShowCooldownIcon();
            }
        }
        public float AddEnergyChange(float modifier) {
            float cost = equipment.EnergyCost(ss.shipClass, rarity) * (float)qnt * modifier;
            buffGO.GetComponent<BuffEnergyChange>().energyChange = -cost;
            return cost;
        }
        public virtual void ShowCooldownIcon() {
            if (CooldownTime > 2f) {
                float countdown = CooldownRemaining;
                string nameModified = equipment.GetNameModified(rarity, 1);
                if (cooldownIconControl is null)
                    cooldownIconControl = BuffIcons.AddBuff(equipment.sprite, 1, nameModified, countdown.ToString("0"), countdown, animated: true);
                else
                    cooldownIconControl.UpdateInfo(nameModified, countdown.ToString("0"), countdown);
            }
        }

        public delegate bool OnDeactivateHandler(ActiveEquipmentEx sender, bool shifted, Transform target);
        public event OnDeactivateHandler OnDeactivate;
        protected bool OnDeactivateInvoke(bool shifted, Transform target) => OnDeactivate?.Invoke(this, shifted, target) ?? true;
    }
}
