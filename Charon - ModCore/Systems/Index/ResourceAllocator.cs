using System;
using System.Collections.Generic;
using System.Linq;

namespace Charon.StarValor.ModCore {
    public class ResourceAllocator : ISerializable {       
        [Serialize]
        SerializableDictionary<Guid, IIndexable> allocatedGuid = new SerializableDictionary<Guid, IIndexable>();
        
        Dictionary<IIndexable, Guid> allocatedObjects = new Dictionary<IIndexable, Guid>();
        public IEnumerable<IIndexable> Values => allocatedGuid.Values;

        //Using GUID to emulate int - until int can be totally replaced
        const int populateCount = 1;
        const int indexRegionStart = 1 << 24;
        int indexRegionCurrentMax = indexRegionStart;
        LinkedList<Guid> available = new LinkedList<Guid>();
        Guid GetNextGuid() {
            if (available.Count == 0) {
                int val = indexRegionCurrentMax;
                for (int i = 0; i < populateCount; ++i) {
                    Guid toAlloc;
                    do {
                        toAlloc = Utilities.Int_to_Guid(val++);
                    } while (allocatedGuid.ContainsKey(toAlloc));
                    available.AddLast(toAlloc);
                }
            }
            var guid = available.Last.Value;
            available.RemoveLast();
            var id = Utilities.Guid_to_Int(guid);
            if (indexRegionCurrentMax < id)
                indexRegionCurrentMax = id;
            if (indexRegionCurrentMax == int.MaxValue)
                indexRegionCurrentMax = indexRegionStart;
            return guid;

            //Use below once int is removed
            //Guid guid;
            //do {
            //    guid = Guid.NewGuid();
            //} while (allocatedGuid.ContainsKey(guid));
            //return guid;
        }

        public Guid Allocate(IIndexable instance, Guid? staticGUID) {
            if (staticGUID is null) {
                if (allocatedObjects.TryGetValue(instance, out var guid))
                    return guid;                
                guid = GetNextGuid();
                allocatedObjects[instance] = guid;
                allocatedGuid[guid] = instance;
                return guid;
            }
            else {
                var guid = staticGUID.Value;
                if (allocatedGuid.ContainsKey(guid))
                    throw new ArgumentException("Already allocated Guid", instance.QualifiedName.Name + " " + guid.ToString());
                available.Remove(guid);
                allocatedObjects[instance] = guid;
                allocatedGuid[guid] = instance;
                return guid;
            }
        }
        public bool Deallocate(Guid guid) {
            if (!allocatedGuid.TryGetValue(guid, out var instance))
                throw new ArgumentException("Not allocated Guid", guid.ToString());
            allocatedObjects.Remove(instance);
            allocatedGuid.Remove(guid);
            available.AddFirst(guid);
            return allocatedObjects.Count == 0;
        }
        public bool TryGetValue(Guid guid, out IIndexable obj) => allocatedGuid.TryGetValue(guid, out obj);
        public IIndexable FirstOrDefault() => allocatedGuid.Values.FirstOrDefault() ?? default;
        public object OnSerialize() => null;
        public void OnDeserialize(object data) {
            allocatedObjects.Clear();
            foreach (var o in allocatedGuid) {
                o.Value.Guid = o.Key;
                allocatedObjects.Add(o.Value, o.Key);
            }
        }
    }
}
