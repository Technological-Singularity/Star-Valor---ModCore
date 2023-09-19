using UnityEngine;

namespace Charon.StarValor.ModCore {
    public abstract class ActiveEquipmentEx_BuffBased : ActiveEquipmentEx {
        #region Internal
        protected GameObject buffGO;
        public float CooldownRemaining => CooldownTime - (Time.time - timeDeactivated);
        protected float timeDeactivated;
        protected override bool showBuffIcon => this.isPlayer;
        #endregion
        #region External
        protected ValueModifier StartEnergyCost { get; }
        protected ValueModifier FluxChargesCost { get; }
        protected ValueModifier CooldownTime { get; }
        #endregion
        #region Initialization
        protected ActiveEquipmentEx_BuffBased(int id) : base(id) {
            StartEnergyCost = new ValueModifier(new QualifiedName(QualifiedName, nameof(StartEnergyCost)));
            FluxChargesCost = new ValueModifier(new QualifiedName(QualifiedName, nameof(FluxChargesCost)));
            CooldownTime = new ValueModifier(new QualifiedName(QualifiedName, nameof(CooldownTime)));
        }
        #endregion
        #region Hooks
        public override sealed void AfterConstructor() {
            this.buffGO = new GameObject();
            var control = this.buffGO.AddComponent<BuffControl>();

            control.owner = this.ss;
            control.activeEquipment = this;

            var energy = this.buffGO.AddComponent<BuffEnergyChange>();
            energy.affectOwner = true;

            if (saveCooldownID >= 0)
                timeDeactivated = Time.time - CooldownTime + ss.shipData.GetAECooldown(saveCooldownID);
            if (CooldownTime > 2f && CooldownRemaining > 0f)
                ShowCooldownIcon();
            OnInitialize();
        }
        public override sealed void Uninstall() {
            base.Uninstall();
            if (cooldownIconControl != null) {
                BuffIcons.RemoveBuff(cooldownIconControl, freeBuffer: true);
                cooldownIconControl = null;
                if (saveCooldownID >= 0) {
                    ss.shipData.SaveAECooldown(saveCooldownID, CooldownRemaining);
                }
            }
            if (buffGO != null) {
                var component = buffGO.GetComponent<BuffControl>();
                if (component.enabled)
                    component.Terminate(cascade: true);
            }
        }
        public override sealed void ActivateDeactivate(bool shiftPressed, Transform target) {
            base.ActivateDeactivate(shiftPressed, target);

            if (TargetInfo.OnlyTargetSelf || (target == null && TargetInfo.DefaultTargetSelf))
                target = ss.transform;
            OnActivateDeactivate(shiftPressed, ref target);

            if (active)
                Activate(shiftPressed, target);
            else
                Deactivate(shiftPressed, target);
        }
        void Activate(bool shiftPressed, Transform target) {
            if (target == null || !IsValidDistance(target) || !ConsumedRequiredItem())
                return;

            if (CooldownTime > 0f && CooldownRemaining > 0f) {
                base.ipc.ShowWarning(Lang.Get(6, 84), 1, playAudio: false);
                return;
            }
            if (StartEnergyCost != 0f) {
                if (ss.stats.currEnergy < StartEnergyCost) {
                    base.ipc.ShowWarning(Lang.Get(6, 45), 1, playAudio: false);
                    return;
                }
                ss.stats.currEnergy -= StartEnergyCost;
            }
            if (FluxChargesCost > 0 && !ss.fluxChargeSys.ExpendNCharges(FluxChargesCost)) {
                base.ipc.ShowWarning(Lang.Get(6, 99, Lang.Get(5, 342)), 1, playAudio: true);
                return;
            }
            if (buffGO.transform.parent != target) {
                buffGO.transform.SetParent(target, worldPositionStays: false);
                buffGO.GetComponent<BuffControl>().Setup();
            }
            if (TrySetup(shiftPressed))
                buffGO.GetComponent<BuffControl>().Begin();
            OnActivate(shiftPressed, target);
        }
        void Deactivate(bool shiftPressed, Transform target) {
            if (target == null || !canTurnOff)
                return;
            if (buffGO != null)
                buffGO.GetComponent<BuffControl>().End();
            OnDeactivate(shiftPressed, target);
        }
        public override sealed void AfterDeactivate() {
            base.AfterDeactivate();
            if (CooldownTime > 0f) {
                timeDeactivated = Time.time;
                ShowCooldownIcon();
            }
        }
        #endregion
        #region Framework
        protected void AddStartEnergyCost(float modifier) => StartEnergyCost.Modifier = equipment.EnergyCost(ss.shipClass, rarity) * qnt * modifier;
        protected float AddEnergyChange(float modifier) {
            float cost = equipment.EnergyCost(ss.shipClass, rarity) * (float)qnt * modifier;
            buffGO.GetComponent<BuffEnergyChange>().energyChange = -cost;
            return cost;
        }
        protected bool IsValidDistance(Transform target) {
            if (target == ss.transform || target == null)
                return true;

            BuffDistanceLimit component = equipment.buff.GetComponent<BuffDistanceLimit>();
            if ((bool)component) {
                float maxDistance = component.maxDistance;
                if (Vector3.Distance(ss.transform.position, target.position) > maxDistance) {
                    base.ipc.ShowWarning(Lang.Get(6, 52), 1, playAudio: true);
                    return false;
                }
            }
            return true;
        }
        protected virtual bool ConsumedRequiredItem() {
            if (equipment.requiredItemID < 0)
                return true;
            if (!ss.GetComponent<CargoSystem>().ConsumeItemVerified(3, equipment.requiredItemID, equipment.requiredQnt, -1)) {
                active = false;
                ss.ipc.ShowWarning("<b>" + ItemDB.GetItem(equipment.requiredItemID).itemName + "</b> " + Lang.Get(6, 0) + "!", 1, playAudio: true);
                active = false;
                return false;
            }
            return true;
        }
        public virtual void ShowCooldownIcon() {
            if (CooldownTime > 2f) {
                float countdown = CooldownRemaining;
                string nameModified = equipment.GetNameModified(rarity, 1);
                if (cooldownIconControl == null) {
                    cooldownIconControl = BuffIcons.AddBuff(equipment.sprite, 1, nameModified, countdown.ToString("0"), countdown, animated: true);
                }
                else {
                    cooldownIconControl.UpdateInfo(nameModified, countdown.ToString("0"), countdown);
                }
            }
        }
        protected virtual void OnInitialize() { }
        protected virtual bool TrySetup(bool shiftPressed) => true;
        protected virtual void OnActivateDeactivate(bool shiftPressed, ref Transform target) { }
        protected virtual void OnActivate(bool shiftPressed, Transform target) { }
        protected virtual void OnDeactivate(bool shiftPressed, Transform target) { }
        #endregion
    }
}
