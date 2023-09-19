using System.Collections.Generic;
using System.Linq;

namespace Charon.StarValor.ModCore {
    public class ResourceAllocator<T> {
        const int populateCount = 10;
        const int minIndex = 1 << 24;
        Dictionary<int, T> allocatedIdx = new Dictionary<int, T>();
        Dictionary<T, int> allocatedObjects = new Dictionary<T, int>();
        int maxIndex = minIndex;
        Queue<int> available = new Queue<int>();

        public IEnumerable<T> Values => allocatedObjects.Keys;
        public int AllocateStatic(int id, T o) {
            if (allocatedObjects.TryGetValue(o, out var _id))
                if (_id == id)
                    return id;
                else if (_id != id)
                    throw new System.Exception("Attempted to reallocate fixed id");

            allocatedObjects[o] = id;
            allocatedIdx[id] = o;
            return id;
        }
        public int Allocate(T o) {
            if (allocatedObjects.TryGetValue(o, out var id))
                return id;
            if (available.Count == 0) {
                int val = maxIndex;
                for (int i = 0; i < populateCount; ++i) {
                    while (allocatedIdx.ContainsKey(val))
                        ++val;
                    available.Enqueue(val);
                }
            }
            id = available.Dequeue();

            allocatedObjects[o] = id;
            allocatedIdx[id] = o;

            if (maxIndex < id)
                maxIndex = id;
            if (maxIndex == int.MaxValue)
                maxIndex = minIndex;
            return id;
        }
        public bool Deallocate(int id) {
            if (allocatedIdx.TryGetValue(id, out var value)) {
                allocatedObjects.Remove(value);
                allocatedIdx.Remove(id);
                available.Enqueue(id);
                return true;
            }
            return false;
        }
        public bool TryGetValue(int id, out T obj) => allocatedIdx.TryGetValue(id, out obj);
        public bool Contains(T obj) => allocatedObjects.ContainsKey(obj);
        public object GetSerialization() => allocatedIdx;
        public void Deserialize(object serialization) {
            allocatedIdx = (Dictionary<int, T>)serialization;
            allocatedObjects = allocatedIdx.ToDictionary(o => o.Value, o => o.Key);
        }
    }
}
