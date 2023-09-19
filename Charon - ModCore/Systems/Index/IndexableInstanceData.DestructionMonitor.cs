using UnityEngine;

namespace Charon.StarValor.ModCore {
    public sealed partial class IndexableInstanceData {
        class DestructionMonitor : MonoBehaviour {
            public IndexableInstanceData InstanceData { get; set; }
            void OnDestroy() => InstanceData.OnMonitorDestroyed();
        }
    }
}
