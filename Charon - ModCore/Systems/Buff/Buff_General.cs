using System;
using UnityEngine;

namespace Charon.StarValor.ModCore {
    public abstract class BuffGeneral : BuffBase {
        protected GameObject updaterGO;
        protected Updater updater;

        public virtual void Initialize(SpaceShip ss, Equipment equipment, int rarity, int qnt) {
            this.targetSS = ss;
            updaterGO = new GameObject();
            updater = updaterGO.AddComponent<Updater>();
            updaterGO.transform.SetParent(transform, false);
        }
        protected void SetOnUpdate(Action action, float? period) => updater.SetOnUpdate(action, period);
        protected void SetOnFixedUpdate(Action action, bool enabled) => updater.SetOnFixedUpdate(action, enabled);
        protected override void Begin() {
            updater.enabled = true;
            base.Begin();
        }
        protected override void End() {
            if (this.enabled) {
                updater.enabled = false;
            }
            base.End();
        }
        void OnDestroy() {
            if (updater != null)
                updater.enabled = false;
        }
    }
}
