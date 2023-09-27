using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using UnityEngine.Events;

namespace Charon.StarValor.ModCore {
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    [BepInProcess("Star Valor.exe")]
    public partial class IndexSystem : ModCorePlugin, ISerializableGuid {
        public const string pluginGuid = "starvalor.charon.modcore.index_system";
        public const string pluginName = "Charon - Modcore - Index System";
        public const string pluginVersion = "0.0.0.0";

        public static IndexSystem Instance { get; private set; }
        void Awake() {
            Instance = this;
        }
        public override void OnPluginLoad() {
            AddBaseType<ActiveEquipmentEx>();
            AddBaseType<ActiveEquipmentExTemplate>();

            AddBaseType<EquipmentEx>();
            AddBaseType<EquipmentExTemplate>();

            AddBaseType<EffectEx>();
            AddBaseType<EffectExTemplate>();

            var templateObjs = new List<IndexableTemplate>();
            IEnumerable<Type> templateTypes = Utilities.EnumerateTypes(o => typeof(IndexableTemplate).IsAssignableFrom(o) && !o.IsAbstract && !o.IsDefined(typeof(RegisterManualAttribute)));
            ModCore.Instance.Log.LogMessage($"    Registering {templateTypes.Count()} template types");
            foreach (var type in templateTypes) {
                ModCore.Instance.Log.LogMessage($"      Register: {type.FullName}");
                var obj = (IndexableTemplate)Activator.CreateInstance(type);
                templateObjs.Add(obj);
                if (obj.CanRegister()) {
                    AllocateTypeInstance(obj);
                    RegisterTypeInstance(obj);
                }
            }
            foreach (var obj in templateObjs)
                obj.OnRegister();

            foreach (var type in templateTypes)
                foreach (var o in GetAllTypeInstance(type))
                    ((IndexableTemplate)o).Initialize();

            SerializeSystem.Instance.Add(this);
        }

        const int minIndex = 1 << 24;

        HashSet<Type> validBaseTypes { get; } = new HashSet<Type>();
        
        [Serialize]
        SerializableDictionary<Type, ResourceAllocator> allocated = new SerializableDictionary<Type, ResourceAllocator>();
        
        Dictionary<QualifiedName, IIndexable> registeredNames = new Dictionary<QualifiedName, IIndexable>();
        Dictionary<Type, IIndexable> registeredUnique = new Dictionary<Type, IIndexable>();

        void AddBaseType<T>() {
            validBaseTypes.Add(typeof(T));
        }

        Type GetBaseType(Type otype) {
            foreach (var found in validBaseTypes.Where(o => o.IsAssignableFrom(otype)))
                return found;
            throw new ArgumentException(otype.FullName);
        }

        public object OnSerialize() => null;
        public void OnDeserialize(object data) {
            registeredNames.Clear();
            registeredUnique.Clear();
            foreach (var inst in allocated.Values.SelectMany(o => o.Values)) {
                if (inst.UniqueType)
                    registeredUnique.Add(inst.GetType(), inst);
                if (inst.UseQualifiedName)
                    registeredNames.Add(inst.QualifiedName, inst);
            }
        }

        public void AllocateTypeInstance(IIndexable instance, int? staticId = null) {
            var type = GetBaseType(instance.GetType());
            if (!allocated.TryGetValue(type, out var resources))
                resources = (allocated[type] = new ResourceAllocator());
            instance.Id = resources.Allocate(instance, staticId);
        }
        public void RegisterTypeInstance(IIndexable instance) {
            if (instance.UseQualifiedName) {
                if (registeredNames.ContainsKey(instance.QualifiedName))
                    throw new ArgumentException("Already allocated name", instance.QualifiedName.FullName);
                registeredNames.Add(instance.QualifiedName, instance);
            }
            if (instance.UniqueType) {
                if (registeredUnique.ContainsKey(instance.GetType()))
                    throw new ArgumentException("Already allocated type", instance.GetType().FullName);
                registeredUnique.Add(instance.GetType(), instance);
            }
        }
        public void UnregisterTypeInstance(IIndexable instance) {
            if (instance.UseQualifiedName) {
                if (!registeredNames.TryGetValue(instance.QualifiedName, out var registered) || registered != instance)
                    throw new ArgumentException("Not allocated name", instance.QualifiedName.FullName);
                registeredNames.Remove(instance.QualifiedName);
            }
            if (instance.UniqueType) {
                if (!registeredUnique.TryGetValue(instance.GetType(), out var registered) || registered != instance)
                    throw new ArgumentException("Not allocated type", instance.GetType().FullName);
                registeredUnique.Remove(instance.GetType());
            }
        }
        public void DeallocateTypeInstance(IIndexable instance) {
            var type = GetBaseType(instance.GetType());
            if (instance.UseQualifiedName) {
                if (!registeredNames.ContainsKey(instance.QualifiedName))
                    throw new ArgumentException("Not allocated name", instance.QualifiedName.FullName);
                registeredNames.Remove(instance.QualifiedName);
            }
            if (instance.UniqueType) {
                if (!registeredUnique.ContainsKey(instance.GetType()))
                    throw new ArgumentException("Not allocated type", instance.GetType().FullName);
                registeredUnique.Remove(instance.GetType());
            }
            if (!allocated.TryGetValue(type, out var resources))
                throw new ArgumentException("Not allocated basetype", type.FullName);
            if (resources.Deallocate(instance.Id))
                allocated.Remove(type);
        }
        public IEnumerable<T> GetAllTypeInstance<T>() where T : IIndexable => GetAllTypeInstance(typeof(T)).Select(o => (T)o);
        public IIndexable GetTypeInstance(Type type, int id) {
            type = GetBaseType(type);
            if (!allocated.TryGetValue(type, out var resources) || !resources.TryGetValue(id, out var wr))
                throw new ArgumentException("Not allocated id or basetype", $"{id} {type.FullName}");
            return (IIndexable)wr;
        }
        public T GetTypeInstance<T>(int id) where T : IIndexable => (T)GetTypeInstance(typeof(T), id);
        public IIndexable GetTypeInstance(QualifiedName name) {
            if (!registeredNames.TryGetValue(name, out var instance))
                throw new ArgumentException("Not allocated name", name.FullName);
            return instance;
        }
        public IIndexable GetTypeInstance(Type type) {
            if (!registeredUnique.TryGetValue(type, out var instance))
                throw new ArgumentException("Not allocated type", type.FullName);
            return instance;
        }
        public T GetTypeInstance<T>() => (T)GetTypeInstance(typeof(T));
        public IEnumerable<IIndexable> GetAllTypeInstance(Type type) {
            if (allocated.TryGetValue(type, out var resources))
                return resources.Values;
            return Enumerable.Empty<IIndexable>();
        }

        public bool TryGetTypeInstance(Type type, int id, out IIndexable instance) {
            type = GetBaseType(type);
            if (allocated.TryGetValue(type, out var resources) && resources.TryGetValue(id, out instance))
                return true;
            instance = default;
            return false;
        }
        public bool TryGetTypeInstance<T>(int id, out T instance) where T : IIndexable {
            if (TryGetTypeInstance(typeof(T), id, out var _inst)) {
                instance = (T)_inst;
                return true;
            }
            instance = default;
            return false;
        }
        public bool TryGetTypeInstance(QualifiedName name, out IIndexable instance) {
            if (registeredNames.TryGetValue(name, out instance))
                return true;
            instance = default;
            return false;
        }
        public bool TryGetTypeInstance(Type type, out IIndexable instance) {
            if (registeredUnique.TryGetValue(type, out instance))
                return true;
            instance = default;
            return false;
        }
        public bool TryGetTypeInstance<T>(out T instance) {
            if (TryGetTypeInstance(typeof(T), out var _inst)) {
                instance = (T)_inst;
                return true;
            }
            instance = default;
            return false;
        }
        public IEnumerable<IIndexable> TryGetAllTypeInstance(Type type) {
            if (allocated.TryGetValue(type, out var resources))
                return resources.Values;
            return Enumerable.Empty<IIndexable>();
        }
    }
}
