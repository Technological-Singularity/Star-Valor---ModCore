using System;
using System.Collections.Generic;
using System.Linq;

namespace Charon.StarValor.ModCore {
    public class ResourceAllocator : ISerializable {
        const int populateCount = 10;
        const int indexRegionStart = 1 << 24;
        
        [Serialize]
        SerializableDictionary<int, IIndexable> allocatedIdx = new SerializableDictionary<int, IIndexable>();
        
        Dictionary<IIndexable, int> allocatedObjects = new Dictionary<IIndexable, int>();
        int indexRegionCurrentMax = indexRegionStart;
        Queue<int> available = new Queue<int>();

        public IEnumerable<IIndexable> Values => allocatedIdx.Values;

        public int Allocate(IIndexable instance, int? staticId) {
            if (staticId is null) {
                if (allocatedObjects.TryGetValue(instance, out var id))
                    return id;
                if (available.Count == 0) {
                    int val = indexRegionCurrentMax;
                    for (int i = 0; i < populateCount; ++i) {
                        while (allocatedIdx.ContainsKey(val))
                            ++val;
                        available.Enqueue(val++);
                    }
                }
                id = available.Dequeue();

                allocatedObjects[instance] = id;
                allocatedIdx[id] = instance;

                if (indexRegionCurrentMax < id)
                    indexRegionCurrentMax = id;
                if (indexRegionCurrentMax == int.MaxValue)
                    indexRegionCurrentMax = indexRegionStart;

                return id;
            }
            else {
                var id = staticId.Value;
                if (allocatedIdx.ContainsKey(id))
                    throw new ArgumentException("Already allocated", "id");
                allocatedObjects[instance] = id;
                allocatedIdx[id] = instance;
                return id;
            }
        }
        public bool Deallocate(int id) {
            if (!allocatedIdx.TryGetValue(id, out var instance))
                throw new ArgumentException("Not allocated", "id");
            allocatedObjects.Remove(instance);
            allocatedIdx.Remove(id);
            return allocatedObjects.Count == 0;
        }
        public bool TryGetValue(int id, out IIndexable obj) => allocatedIdx.TryGetValue(id, out obj);
        public IIndexable FirstOrDefault() => allocatedIdx.Values.FirstOrDefault() ?? default;
        public object OnSerialize() => null;
        public void OnDeserialize(object data) {
            allocatedObjects.Clear();
            foreach (var o in allocatedIdx) {
                o.Value.Id = o.Key;
                allocatedObjects.Add(o.Value, o.Key);
            }
        }
    }
}
