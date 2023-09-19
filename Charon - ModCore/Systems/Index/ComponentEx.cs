using UnityEngine;

namespace Charon.StarValor.ModCore {
    public abstract class ComponentEx : MonoBehaviour {
        public IndexableInstanceData InstanceData { get; set; }
        public abstract int GetHashCode(HashContext context);
        public abstract object GetSerialization();
        public abstract void Deserialize(object serialization);
        public virtual void OnApplying(IIndexableInstance instance) { }
    }
}
