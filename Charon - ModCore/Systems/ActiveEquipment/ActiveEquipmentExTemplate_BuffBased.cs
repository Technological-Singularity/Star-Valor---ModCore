using System;
using UnityEngine;

namespace Charon.StarValor.ModCore {
    [RegisterManual]
    public class ActiveEquipmentExTemplate_BuffBased : ActiveEquipmentExTemplate {
        public override bool UniqueType { get; } = false;
        public override bool UseQualifiedName { get; } = true;

        [Serialize]
        Type BuffType { get; set; }
        [Serialize]
        bool SaveState { get; set; }
        [Serialize]
        float EnergyChange { get; set; }
        protected override Type InstanceType { get; } = typeof(ActiveEquipmentEx_BuffBased);

        public static ActiveEquipmentExTemplate_BuffBased Create<T>(bool saveState, float energyChange) where T : BuffGeneral {
            var aex = new ActiveEquipmentExTemplate_BuffBased() {
                BuffType = typeof(T),
                SaveState = saveState,
                EnergyChange = energyChange,
            };
            aex.QualifiedName = aex.GetQualifiedName();
            if (IndexSystem.Instance.TryGetTypeInstance(aex.QualifiedName, out var wr))
                return (ActiveEquipmentExTemplate_BuffBased)wr;
            IndexSystem.Instance.AllocateTypeInstance(aex, null);
            IndexSystem.Instance.RegisterTypeInstance(aex);
            return aex;
        }

        public override QualifiedName GetQualifiedName() => new QualifiedName(BuffType, $"{BuffType.Name}++{this.GetType().Name}");
        protected override QualifiedName GetInstanceQualifiedName(IIndexableInstance instance) {
            return new QualifiedName(BuffType, $"{((IIndexable)this).QualifiedName.Name}.{instance.Guid}");
        }
        public override void OnApplying(IIndexableInstance instance) {
            var aex = (ActiveEquipmentEx_BuffBased)instance;
            aex.saveState = SaveState;
            aex.OnDeactivate += OnDeactivate;
            base.OnApplying(instance);
        }
        public override void OnRemoving(IIndexableInstance instance) {
            var aex = (ActiveEquipmentEx_BuffBased)instance;
            aex.OnDeactivate -= OnDeactivate;
            base.OnRemoving(instance);
        }
        protected override void OnInitialization(ActiveEquipmentEx sender) {
            var aex = (ActiveEquipmentEx_BuffBased)sender;
            var buff = (BuffGeneral)aex.AddBuff(BuffType);
            if (EnergyChange != 0)
                aex.AddEnergyChange(EnergyChange);
            buff.Initialize(aex.ss, aex.equipment, aex.rarity, aex.qnt);
        }

        protected virtual bool OnDeactivate(ActiveEquipmentEx sender, bool shiftPressed, Transform target) => true;
    }
}
