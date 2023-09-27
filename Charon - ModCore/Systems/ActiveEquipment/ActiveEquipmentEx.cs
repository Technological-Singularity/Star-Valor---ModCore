using System;
using HarmonyLib;
using UnityEngine;

namespace Charon.StarValor.ModCore {
    [HasPatches]
    public abstract class ActiveEquipmentEx : ActiveEquipment, IIndexableInstance, ISerializable {
        [HarmonyPatch(typeof(ActiveEquipment), nameof(ActiveEquipment.AddActivatedEquipment))]
        [HarmonyPrefix]
        public static bool AddActivatedEquipment(ref ActiveEquipment __result, Equipment equipment, SpaceShip ss, KeyCode key, int rarity, int qnt) {
            if (equipment is EquipmentEx eq && !(eq.ActiveEquipment is null)) {
                ss.activeEquips.Add(eq.ActiveEquipment);
                eq.ActiveEquipment.Initialize(eq, ss, key, rarity, qnt);
                eq.ActiveEquipment.AfterConstructor();
                __result = eq.ActiveEquipment;
                return false;
            }
            //Empty ActiveEquipment but ActiveEquipmentIndex > 0 indicates use of base game AE assignment
            return true;
        }

        public QualifiedName QualifiedName { get; set; }
        int IIndexable.RefCount { get; set; } = 0;
        public virtual bool UseQualifiedName { get; } = true;
        public virtual bool UniqueType { get; } = false;

        [Serialize]
        public IndexableInstanceData TemplateData { get; set; }

        protected ValueModifier StartEnergyCost { get; private set; }
        protected ValueModifier FluxChargesCost { get; private set; }
        protected ValueModifier CooldownTime { get; private set; }

        public float CooldownRemaining => CooldownTime - (Time.time - timeDeactivated);
        protected float timeDeactivated;

        public ActiveEquipmentEx() => TemplateData = new IndexableInstanceData(this);
        public int Id {
            get => id;
            set => id = value;
        }
        public virtual object OnSerialize() => null;
        public virtual void OnDeserialize(object serialization) => TemplateData.Deserialize(serialization);

        protected virtual void Initialize(EquipmentEx equipment, SpaceShip ss, KeyCode key, int rarity, int qnt) {            
            this.id = equipment.id;
            this.key = key;
            this.ss = ss;
            this.isPlayer = ss != null && ss.CompareTag("Player");
            this.equipment = equipment;
            this.rarity = rarity;
            this.qnt = qnt;
            this.active = false;

            //to do: create types for each of these effects, use their types to create ValueModifiers
            StartEnergyCost = new ValueModifier(nameof(StartEnergyCost));
            FluxChargesCost = new ValueModifier(nameof(FluxChargesCost));
            CooldownTime = new ValueModifier(nameof(CooldownTime));

            StartEnergyCost.Link(ss.transform);
            FluxChargesCost.Link(ss.transform);
            CooldownTime.Link(ss.transform);
        }

        public TargetModeInfo TargetInfo { get; } = new TargetModeInfo();
        protected EquipmentEx EquipmentEx => (EquipmentEx)equipment;
        protected virtual bool SaveCooldown { get; } = false;
        
        public void AddStartEnergyCost(float modifier) => StartEnergyCost.Modifier = equipment.EnergyCost(ss.shipClass, rarity) * qnt * modifier;
        
        protected bool TryActivateDeactivate(bool shiftPressed, ref Transform target) {
            if (isPlayer)
                SoundSys.PlaySound(active ? 13 : 14, keepPlaying: false);

            if (TargetInfo.OnlyTargetSelf || (target == null && TargetInfo.DefaultTargetSelf))
                target = ss.transform;

            bool doContinue;
            (target, doContinue) = OnActivateDeactivateInvoke(shiftPressed, target);

            if (!doContinue)
                return false;

            if (target == null || !IsValidDistance(target))
                return false;

            if (!active && !CanConsumedRequiredItem(true))
                return false;

            if (CooldownTime > 0f && CooldownRemaining > 0f) {
                base.ipc.ShowWarning(Lang.Get(6, 84), 1, playAudio: false);
                return false;
            }
            if (StartEnergyCost != 0f) {
                if (ss.stats.currEnergy < StartEnergyCost) {
                    base.ipc.ShowWarning(Lang.Get(6, 45), 1, playAudio: false);
                    return false;
                }
                ss.stats.currEnergy -= StartEnergyCost;
            }
            if (FluxChargesCost > 0 && !ss.fluxChargeSys.ExpendNCharges((int)FluxChargesCost)) {
                base.ipc.ShowWarning(Lang.Get(6, 99, Lang.Get(5, 342)), 1, playAudio: true);
                return false;
            }

            return true;
        }
        public override void ActivateDeactivate(bool shiftPressed, Transform target) {
            if (!TryActivateDeactivate(shiftPressed, ref target))
                return;
            if (!OnActivateInvoke(shiftPressed, target))
                return;
            ConsumedRequiredItem(false);
            timeDeactivated = Time.time;
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
        protected bool CanConsumedRequiredItem(bool showWarning) {
            const int itemType = 3;
            var wr = ss.GetComponent<CargoSystem>().CheckCargoItemQuantity(itemType, equipment.requiredItemID, -1, false) >= equipment.requiredQnt;
            if (!wr && showWarning)
                InfoPanelControl.inst.ShowWarning("<b>" + ItemDB.GetItem(equipment.requiredItemID).itemName + "</b> " + Lang.Get(6, 0) + "!", 1, playAudio: true);
            return wr;
        }
        protected void ConsumedRequiredItem(bool showWarning) {
            const int itemType = 3;
            if (equipment.requiredItemID < 0)
                return;
            if (!ss.GetComponent<CargoSystem>().ConsumeItemVerified(itemType, equipment.requiredItemID, equipment.requiredQnt, -1) && showWarning)
                InfoPanelControl.inst.ShowWarning("<b>" + ItemDB.GetItem(equipment.requiredItemID).itemName + "</b> " + Lang.Get(6, 0) + "!", 1, playAudio: true);
        }

        #region Events
        public delegate void OnInitializationHandler(ActiveEquipmentEx sender);
        public event OnInitializationHandler OnInitialization = null;
        protected void OnInitializationInvoke() => OnInitialization?.Invoke(this);

        public delegate (Transform target, bool result) OnActivateDeactivateHandler(ActiveEquipmentEx sender, bool shifted, Transform target);
        public event OnActivateDeactivateHandler OnActivateDeactivate = null;
        protected (Transform target, bool result) OnActivateDeactivateInvoke(bool shifted, Transform target) => OnActivateDeactivate?.Invoke(this, shifted, target) ?? (target, true);

        public delegate bool OnActivateHandler(ActiveEquipmentEx sender, bool shifted, Transform target);
        public event OnActivateHandler OnActivate;
        protected bool OnActivateInvoke(bool shifted, Transform target) => OnActivate?.Invoke(this, shifted, target) ?? true;
        #endregion
    }
}
