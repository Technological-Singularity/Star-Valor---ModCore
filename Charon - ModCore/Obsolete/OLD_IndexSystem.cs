//using System;
//using System.Collections.Generic;
//using System.Reflection;
//using UnityEngine;

//namespace Charon.StarValor.ModCore {
//    public enum IndexType : int {
//        ActiveEffect = 0,
//        Effect = 1,
//        EquipmentEx = 2,
//        Ship = 3,
//        Reserved0 = 4,
//        Reserved1 = 5,
//        Reserved2 = 6,
//        Reserved3 = 7,
//        Reserved4 = 8,
//        Reserved5 = 9,
//        Reserved6 = 10,
//        Reserved7 = 11,
//    }
//    public class IndexSystem {
//        const int indexOffset = 1 << 24;
//        static readonly Dictionary<IndexType, Dictionary<string, int>> indices = new Dictionary<IndexType, Dictionary<string, int>>();
//        static Dictionary<IndexType, Dictionary<int, string>> names = new Dictionary<IndexType, Dictionary<int, string>>();
//        static Dictionary<IndexType, Dictionary<int, Type>> types = new Dictionary<IndexType, Dictionary<int, Type>>();
//        static Dictionary<IndexType, int> nextIndex = new Dictionary<IndexType, int>();

//        #region Defaults
//        static Dictionary<IndexType, object> defaultObjects = null;
//        class AE_Default : ActiveEquipment {
//            public override void ActivateDeactivate(bool shiftPressed, Transform target) { }
//            public override void AfterActivate() { }
//            public override void AfterDeactivate() { }
//        }
//        public static void LoadDefaults() {
//            //Unimplmenented

//            //if (defaultObjects != null)
//            //	return;

//            //         defaultObjects = new Dictionary<IndexType, object>();
//            //         foreach (IndexType itype in Enum.GetValues(typeof(IndexType))) {
//            //	switch (itype) {
//            //		case IndexType.ActiveEffect: 
//            //			defaultObjects[itype] = typeof(AE_Default); 
//            //			break;
//            //		case IndexType.Effect: 
//            //			defaultObjects[itype] = new Effect() { description = "ERROR_UNINITIALIZED_INDEX" }; 
//            //			break;
//            //		case IndexType.Equipment: 
//            //			var equipment = ScriptableObject.CreateInstance<Equipment>(); ;
//            //			equipment.description = "ERROR_UNINITIALIZED_INDEX";
//            //			equipment.effects = new List<Effect>();
//            //                     defaultObjects[itype] = ScriptableObject.CreateInstance<Equipment>();
//            //			break;
//            //                 default: 
//            //			throw new Exception("Uninstantiated default type in index " + Enum.GetName(typeof(IndexType), itype));
//            //	}
//            //}
//        }
//        static T GetDefault<T>(IndexType type) => (T)defaultObjects[type];
//        #endregion

//        public static object GetSerialization() => new object[] { names, types };

//        public static void Deserialize(object serialization) {
//            var objs = (object[])serialization;
//            names = (Dictionary<IndexType, Dictionary<int, string>>)objs[0];
//            types = (Dictionary<IndexType, Dictionary<int, Type>>)objs[1];
//            indices.Clear();
//            nextIndex.Clear();
//            foreach (var type_dict in names) {
//                var rev = new Dictionary<string, int>();
//                nextIndex[type_dict.Key] = indexOffset; //2^24 positions reserved for base game; rest used for modders (2^31-2^24 - approximately 2 billion for each type)
//                indices[type_dict.Key] = rev;
//                foreach (var id_name in type_dict.Value)
//                    rev[id_name.Value] = id_name.Key;
//            }
//        }

//        static IndexSystem() {
//            foreach (IndexType type in Enum.GetValues(typeof(IndexType))) {
//                var dict = new Dictionary<string, int>();
//                var nameDict = new Dictionary<int, string>();
//                nextIndex[type] = indexOffset;
//                indices[type] = dict;
//                names[type] = nameDict;
//                types[type] = new Dictionary<int, Type>();
//            }
//        }

//        readonly ModCorePlugin context;

//        public IndexSystem(ModCorePlugin context) {
//            this.context = context;
//        }

//        //After deserialization
//        public void Initialize() {
//            foreach (var type in Extensions.EnumerateTypes(t => t.IsDefined(typeof(HasActiveIndexAttribute)))) {
//                var attr = (HasActiveIndexAttribute)type.GetCustomAttribute(typeof(HasActiveIndexAttribute));
//                if (attr.Guid != context.Guid)
//                    continue;
//                var index = Set(IndexType.ActiveEffect, type.FullName);
//                types[IndexType.ActiveEffect][index] = type;
//            }
//        }
//        static void CheckNextIndex(IndexType type, int id) {
//            if (nextIndex[type] <= id)
//                nextIndex[type] = id + 1;
//        }
//        public static bool TrySetQualified(IndexType type, string qualifiedName, out int id) {
//            Dictionary<string, int> dict = indices[type];
//            if (dict.TryGetValue(qualifiedName, out id)) {
//                CheckNextIndex(type, id);
//                return false;
//            }

//            id = nextIndex[type]++;
//            if (nextIndex[type] == int.MaxValue) {
//                ModCorePlugin.Log.LogFatal($"Index out of memory. There are ~2^31 available indices; how did this happen?");
//                throw new OutOfMemoryException();
//            }
//            dict[qualifiedName] = id;
//            names[type][id] = qualifiedName;
//            CheckNextIndex(type, id);
//            return true;
//        }
//        public static bool TrySet(IndexType type, string pluginGuid, string name, out int value) => TrySetQualified(type, ModCorePlugin.Qualify(pluginGuid, name), out value);
//        public bool TrySet(IndexType type, string name, out int value) => TrySetQualified(type, context.Qualify(name), out value);
//        public int Set(IndexType type, string name) {
//            TrySet(type, name, out var wr);
//            return wr;
//        }
//        public static bool TryGetQualified(IndexType type, string fullName, out int id) => indices[type].TryGetValue(fullName, out id);
//        public static bool TryGet(IndexType type, string pluginGuid, string name, out int value) => indices[type].TryGetValue(ModCorePlugin.Qualify(pluginGuid, name), out value);
//        public bool TryGet(IndexType type, string name, out int value) => TryGet(type, context.Guid, name, out value);
//        public int Get(IndexType type, string name) {
//            if (TryGet(type, name, out var wr))
//                return wr;
//            ModCorePlugin.Log.LogFatal("Name not found: " + name);
//            ModCorePlugin.Log.LogFatal(Environment.StackTrace);
//            throw new Exception();
//        }
//        public static bool TryGetQualifiedName(IndexType type, int id, out string fullName) => names[type].TryGetValue(id, out fullName);
//        public static bool TryGetName(IndexType type, string pluginGuid, int id, out string name) {
//            var prefix = ModCorePlugin.Qualify(pluginGuid, "");
//            if (names[type].TryGetValue(id, out name) && name.StartsWith(prefix)) {
//                name = name.Substring(prefix.Length);
//                return true;
//            }
//            name = null;
//            return false;
//        }
//        public bool TryGetName(IndexType type, int id, out string name) {
//            return TryGetName(type, context.Guid, id, out name);
//        }
//        public string GetName(IndexType type, int id) {
//            if (TryGetName(type, id, out var name))
//                return name;
//            ModCorePlugin.Log.LogFatal("Id not found: " + id);
//            ModCorePlugin.Log.LogFatal(Environment.StackTrace);
//            throw new Exception();
//        }

//        public static bool TryGetType(IndexType type, int index, out Type value) {
//            if (types.TryGetValue(type, out var dict) && dict.TryGetValue(index, out value))
//                return true;
//            value = typeof(object);
//            return false;
//        }
//    }
//}
