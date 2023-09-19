using UnityEngine;

namespace Charon.StarValor.ModCore.Systems.Buff {
    public abstract class BuffGeneral : BuffBase {
        protected GameObject updaterGO = new GameObject();
        protected Updater updater;
        BuffGeneral() {
            updater = updaterGO.AddComponent<Updater>();
            updater.RegisterFixed(OnFixedUpdate);
        }
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
