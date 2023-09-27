using UnityEngine;

namespace Charon.StarValor.ModCore {
    public abstract class ComponentEx : MonoBehaviour, ISerializable {
        [Serialize]
        public IndexableInstanceData InstanceData { get; set; }
        public virtual void OnApplying(IIndexableInstance instance) { }
        public virtual object OnSerialize() => null;
        public virtual void OnDeserialize(object data) { }
    }
}
