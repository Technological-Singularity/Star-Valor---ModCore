using UnityEngine;

namespace Charon.StarValor.ModCore {
    public abstract class BuffGeneral : BuffBase {
        protected GameObject updaterGO = new GameObject();
        protected Updater updater;
        protected BuffGeneral() {
            updater = updaterGO.AddComponent<Updater>();
            updater.RegisterFixed(OnFixedUpdate);
        }

        public virtual void Initialize(SpaceShip ss, Equipment equipment, int rarity, int qnt) {
            this.targetSS = ss;
        }
        protected virtual void OnUpdate() { }
        protected virtual void OnFixedUpdate() { }
        protected override void Begin() {
            base.Begin();
            if (updater != null)
                updater.enabled = true;
        }
        protected override void End() {
            base.End();
            if (updater != null)
                updater.enabled = false;
        }
    }
}
