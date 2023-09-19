using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;

namespace Charon.StarValor.ModCore {
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    [BepInProcess("Star Valor.exe")]
    public partial class IndexSystem : ModCorePlugin, ISerializable {
        public const string pluginGuid = "starvalor.charon.modcore.index_system";
        public const string pluginName = "Charon - Modcore - Index System";
        public const string pluginVersion = "0.0.0.0";

        public static IndexSystem Instance { get; private set; }
        void Awake() {
            Instance = this;
            AddTypes<ActiveEquipmentEx, ActiveEquipmentExTemplate>();
            AddTypes<EquipmentEx, EquipmentExTemplate>();
            AddTypes<EffectEx, EffectExTemplate>();
        }
        public override void OnPluginLoad() => SerializeSystem.Instance.Add(this);

        const int minIndex = 1 << 24;

        HashSet<Type> validInstanceTypes { get; } = new HashSet<Type>();
        HashSet<Type> validTemplateTypes { get; } = new HashSet<Type>();
        Dictionary<Type, ResourceAllocator<InstanceContainer>> allocatedContainers { get; } = new Dictionary<Type, ResourceAllocator<InstanceContainer>>();
        Dictionary<Type, ResourceAllocator<Type>> allocatedTypes { get; } = new Dictionary<Type, ResourceAllocator<Type>>();
        Dictionary<Type, Dictionary<QualifiedName, RegisteredTemplate>> registeredNames { get; } = new Dictionary<Type, Dictionary<QualifiedName, RegisteredTemplate>>();
        Dictionary<IndexableTemplate, RegisteredTemplate> registeredTemplates { get; } = new Dictionary<IndexableTemplate, RegisteredTemplate>();

        public void AddTypes<TInstance, TTemplate>() where TInstance : IIndexableInstance where TTemplate : IndexableTemplate {
            validInstanceTypes.Add(typeof(TInstance));
            validTemplateTypes.Add(typeof(TTemplate));
        }

        Type GetBaseInstanceType(Type otype) {
            foreach (var found in validInstanceTypes.Where(o => o.IsAssignableFrom(otype)))
                return found;
            throw new ArgumentException(otype.FullName);
        }
        Type GetBaseRegisterableType(Type otype) {
            foreach (var found in validTemplateTypes.Where(o => o.IsAssignableFrom(otype)))
                return found;
            throw new ArgumentException(otype.FullName);
        }

        public object GetSerialization() {
            return new object[] {
                allocatedContainers.Keys.ToArray(),
                allocatedContainers.Values.Select(o => o.GetSerialization()).ToArray(),
                allocatedTypes.Values.Select(o => o.GetSerialization()).ToArray(),
            };
        }
        public void Deserialize(bool found, object serialization) {
            if (!found) {
                Log.LogMessage("No index file found; could be from a fresh installation.");
                return;
            }

            var objs = (object[])serialization;
            Type[] types = (Type[])objs[0];
            object[] serializedDicts = (object[])objs[1];
            object[] serializedTypeDicts = (object[])objs[2];

            allocatedContainers.Clear();
            allocatedTypes.Clear();
            for (int i = 0; i < types.Length; ++i) {
                var dict = new ResourceAllocator<InstanceContainer>();
                dict.Deserialize(serializedDicts[i]);
                allocatedContainers.Add(types[i], dict);

                var typeDict = new ResourceAllocator<Type>();
                typeDict.Deserialize(serializedTypeDicts[i]);
                allocatedTypes.Add(types[i], typeDict);
            }

            registeredNames.Clear();
            registeredTemplates.Clear();
        }

        public IEnumerable<T> GetAllocatedInstances<T>() where T : IIndexableInstance {
            var type = GetBaseInstanceType(typeof(T));
            if (!allocatedContainers.TryGetValue(type, out var container))
                return new T[] { };
            return container.Values.Select(o => (T)o.Instance);
        }

        public T Register<T>(QualifiedName qualifiedName, T obj) where T : IndexableTemplate => RegisteredTemplate.Register(qualifiedName, obj);
        public bool TryGetRegistered<T>(QualifiedName qualifiedName, out T obj) where T : IndexableTemplate => RegisteredTemplate.TryGetRegistered(qualifiedName, out obj);
        public T GetRegistered<T>(QualifiedName qualifiedName) where T : IndexableTemplate => RegisteredTemplate.GetRegisteredTemplate<T>(qualifiedName);
        public IIndexableInstance Allocate(IndexableTemplate obj, object data) => RegisteredTemplate.GetRegisteredObject(obj).Allocate(data).Instance;
        public IIndexableInstance Allocate<T>(QualifiedName qualifiedName, object data) where T : IndexableTemplate => Allocate(GetRegistered<T>(qualifiedName), data);
        public IIndexableInstance AllocateStatic(int id, IndexableTemplate obj, object data) => RegisteredTemplate.GetRegisteredObject(obj).AllocateStatic(id, data).Instance;

        public IIndexableInstance GetAllocated<T>(int id) where T : IIndexableInstance {
            var baseType = GetBaseInstanceType(typeof(T));
            if (!allocatedContainers.TryGetValue(baseType, out var dict) || !dict.TryGetValue(id, out var obj))
                throw new ArgumentException("Not allocated", "id");
            return obj.Instance;
        }
        public bool TryGetAllocated<T>(int id, out T value) where T : IIndexableInstance {
            var baseType = GetBaseInstanceType(typeof(T));
            if (!allocatedContainers.TryGetValue(baseType, out var dict) || !dict.TryGetValue(id, out var container)) {
                value = default;
                return false;
            }
            value = (T)container.Instance;
            return true;
        }
        public void Ref(IIndexableInstance instance) {
            var baseType = GetBaseInstanceType(instance.GetType());
            if (!allocatedContainers.TryGetValue(baseType, out var dict) || !dict.TryGetValue(instance.Id, out var obj))
                throw new ArgumentException("Not allocated", "instance");
            obj.Ref();
        }
        public bool Deref(IIndexableInstance instance) {
            var baseType = GetBaseInstanceType(instance.GetType());
            if (!allocatedContainers.TryGetValue(baseType, out var dict) || !dict.TryGetValue(instance.Id, out var obj))
                throw new ArgumentException("Not allocated", "instance");
            return obj.Deref();
        }

        public int AllocateType<T>(Type type) where T : IndexableTemplate => AllocateType(typeof(T), type);
        int AllocateType(Type baseType, Type type) {
            baseType = GetBaseRegisterableType(baseType);
            if (!allocatedTypes.TryGetValue(baseType, out var dict)) {
                dict = new ResourceAllocator<Type>();
                allocatedTypes[baseType] = dict;
            }
            if (dict.Contains(type))
                throw new ArgumentException("Already allocated", "type");
            return dict.Allocate(type);
        }
        public bool TryGetAllocatedType<T>(int id, out Type value) where T : IndexableTemplate => TryGetAllocatedType(typeof(T), id, out value);
        bool TryGetAllocatedType(Type baseType, int id, out Type value) {
            baseType = GetBaseRegisterableType(baseType);
            if (!allocatedTypes.TryGetValue(baseType, out var dict) || !dict.TryGetValue(id, out value)) {
                value = null;
                return false;
            }
            return true;
        }
        public Type GetAllocateType<T>(int id) where T : IndexableTemplate => GetAllocatedType(typeof(T), id);
        Type GetAllocatedType(Type baseType, int id) {
            baseType = GetBaseRegisterableType(baseType);
            if (!TryGetAllocatedType(baseType, id, out var obj))
                throw new ArgumentException("Not allocated", "id");
            return obj;
        }
    }
}
