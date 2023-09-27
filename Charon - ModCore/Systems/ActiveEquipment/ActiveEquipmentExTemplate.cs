using System;
using UnityEngine;

namespace Charon.StarValor.ModCore {
    public abstract class ActiveEquipmentExTemplate : IndexableTemplate {
        public override void OnApplying(IIndexableInstance instance) {
            var aex = (ActiveEquipmentEx_BuffBased)instance;
            aex.OnInitialization += OnInitialization;
            aex.OnActivateDeactivate += OnActivateDeactivate;
            aex.OnActivate += OnActivate;
        }
        public override void OnRemoving(IIndexableInstance instance) {
            var aex = (ActiveEquipmentEx_BuffBased)instance;
            aex.OnInitialization -= OnInitialization;
            aex.OnActivateDeactivate -= OnActivateDeactivate;
            aex.OnActivate -= OnActivate;
        }

        protected virtual void OnInitialization(ActiveEquipmentEx sender) { }
        protected virtual (Transform target, bool result) OnActivateDeactivate(ActiveEquipmentEx sender, bool shiftPressed, Transform target) => (target, true);
        protected virtual bool OnActivate(ActiveEquipmentEx sender, bool shiftPressed, Transform target) => true;
    }
}
