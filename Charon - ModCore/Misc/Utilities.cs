using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using HarmonyLib;
using System.Runtime.ExceptionServices;

namespace Charon.StarValor.ModCore {
    [HasPatches]
    public static partial class Utilities {
        public static GameObject BlankShipGO { get; private set; }

        [HarmonyPatch(typeof(GameManager), "PrepareNormalGame")]
        [HarmonyPostfix]
        static void GameManager_PrepareNormalGame_Dummy(Transform tempObjects) {
            if (BlankShipGO != null)
                return;
            GameManager.instance.mercenaryBaseObj.SetActive(false);
            BlankShipGO = UnityEngine.Object.Instantiate(GameManager.instance.mercenaryBaseObj, new Vector3(), Quaternion.identity);
            UnityEngine.Object.DestroyImmediate(BlankShipGO.GetComponent<AIMercenary>());
            BlankShipGO.SetActive(false);
        }

        #region Binding
        public static List<(FieldInfo first, FieldInfo second)> GetBindsFieldField(Type type_first, Type type_second) {
            var binds = new List<(FieldInfo, FieldInfo)>();
            var first = type_first.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).ToDictionary(o => o.Name.ToLowerInvariant(), o => o);
            var second = type_second.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).ToDictionary(o => o.Name.ToLowerInvariant(), o => o);

            foreach (var kvp in first)
                if (second.TryGetValue(kvp.Key, out var paired))
                    binds.Add((kvp.Value, paired));

            return binds;
        }
        public static void BindSet(object dst, List<(FieldInfo first, FieldInfo second)> binds, object hasFirst, object hasSecond) {
            if (dst == hasFirst)
                foreach (var (first, second) in binds)
                    first.SetValue(hasFirst, second.GetValue(hasSecond));
            else if (dst == hasSecond)
                foreach (var (first, second) in binds)
                    second.SetValue(hasSecond, first.GetValue(hasFirst));
            else
                throw new ArgumentException("dst must be first or second");
        }
        public static List<(FieldInfo first, PropertyInfo second)> GetBindsFieldProperty<TFirst, TSecond>() {
            var binds = new List<(FieldInfo, PropertyInfo)>();
            var first = typeof(TFirst).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).ToDictionary(o => o.Name.ToLowerInvariant(), o => o);
            var second = typeof(TSecond).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).ToDictionary(o => o.Name.ToLowerInvariant(), o => o);
            foreach (var kvp in first)
                if (second.TryGetValue(kvp.Key, out var paired))
                    binds.Add((kvp.Value, paired));
            return binds;
        }
        public static void BindSet(object dst, List<(FieldInfo first, PropertyInfo second)> binds, object hasFirst, object hasSecond) {
            if (dst == hasFirst)
                foreach (var (first, second) in binds)
                    first.SetValue(hasFirst, second.GetValue(hasSecond));
            else if (dst == hasSecond)
                foreach (var (first, second) in binds)
                    second.SetValue(hasSecond, first.GetValue(hasFirst));
            else
                throw new ArgumentException("dst must be first or second");
        }
        public static List<(PropertyInfo first, FieldInfo second)> GetBindsPropertyField<TFirst, TSecond>() {
            var binds = new List<(PropertyInfo, FieldInfo)>();
            var first = typeof(TFirst).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).ToDictionary(o => o.Name.ToLowerInvariant(), o => o);
            var second = typeof(TSecond).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).ToDictionary(o => o.Name.ToLowerInvariant(), o => o);
            foreach (var kvp in first)
                if (second.TryGetValue(kvp.Key, out var paired))
                    binds.Add((kvp.Value, paired));
            return binds;
        }
        public static void BindSet(object dst, List<(PropertyInfo first, FieldInfo second)> binds, object hasFirst, object hasSecond) {
            if (dst == hasFirst)
                foreach (var (first, second) in binds)
                    first.SetValue(hasFirst, second.GetValue(hasSecond));
            else if (dst == hasSecond)
                foreach (var (first, second) in binds)
                    second.SetValue(hasSecond, first.GetValue(hasFirst));
            else
                throw new ArgumentException("dst must be first or second");
        }
        public static List<(string, string)> BindDump(object obj, List<(PropertyInfo first, FieldInfo second)> binds) {
            List<(string, string)> wr = new List<(string, string)>();
            foreach(var (first, second) in binds) {
                string name = null;
                string val = null;
                if (first.DeclaringType.IsAssignableFrom(obj.GetType()))
                    (name, val) = (first.Name, first.GetValue(obj)?.ToString());
                else if (second.DeclaringType.IsAssignableFrom(obj.GetType()))
                    (name, val) = (second.Name, second.GetValue(obj)?.ToString());
                if (!(name is null))
                    wr.Add((name, val));
            }
            return wr;
        }
        public static List<(PropertyInfo first, PropertyInfo second)> GetBindsPropertyProperty<TFirst, TSecond>() {
            var binds = new List<(PropertyInfo, PropertyInfo)>();
            var first = typeof(TFirst).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).ToDictionary(o => o.Name.ToLowerInvariant(), o => o);
            var second = typeof(TSecond).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).ToDictionary(o => o.Name.ToLowerInvariant(), o => o);
            foreach (var kvp in first)
                if (second.TryGetValue(kvp.Key, out var paired))
                    binds.Add((kvp.Value, paired));
            return binds;
        }
        public static void BindSet(object dst, List<(PropertyInfo first, PropertyInfo second)> binds, object hasFirst, object hasSecond) {
            if (dst == hasFirst)
                foreach (var (first, second) in binds)
                    first.SetValue(hasFirst, second.GetValue(hasSecond));
            else if (dst == hasSecond)
                foreach (var (first, second) in binds)
                    second.SetValue(hasSecond, first.GetValue(hasFirst));
            else
                throw new ArgumentException("dst must be first or second");
        }
        public static void MemberwiseCopy(object dst, object src) {
            for (var type = dst.GetType(); type != typeof(object); type = type.BaseType) {
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(o => o.CanRead && o.CanWrite);
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                foreach (var p in properties)
                    p.SetValue(dst, p.GetValue(src));
                foreach (var f in fields)
                    f.SetValue(dst, f.GetValue(src));
            }
        }
        #endregion
        #region Serialization
        //public static object GetSerializationInfo<T>() {
        //    typeof(T).GetProperties().Where(o => o.IsDefined(typeof(SerializeAttribute)))
        //}
        //public static Func<T, object, object> GetSerializer<T>() {
        //    object serializer(T instance) {
        //        var objs = new List<object>();
        //        return null;
        //    }
        //    return serializer;
        //}
        #endregion

        public static int GetHashCode(IEnumerable<int> hashCodes, params int[] otherHashCodes) => GetHashCode(hashCodes.Concat(otherHashCodes));
        public static int GetHashCode(params int[] hashCodes) => GetHashCode((IEnumerable<int>)hashCodes);
        public static int GetHashCode(IEnumerable<int> hashCodes) {
            //FNV-1a https://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
            unchecked {
                const int fnvOffsetBasis = (int)0x811c9dc5;
                const int fnvPrime = (int)0x01000193;
                int hash = fnvOffsetBasis;
                foreach (var value in hashCodes)
                    hash = fnvPrime * (hash ^ value);
                return hash;
            }
        }

        public static Vector3 GetRaycastColliderPoint(GameObject colliderOwner, Vector3 origin, out Collider closestCollider) {
            var relPosition = colliderOwner.transform.position - origin;
            Ray ray = new Ray(origin, relPosition);
            var maxDist = relPosition.magnitude;

            var min = float.MaxValue;
            var found = new Vector3();
            closestCollider = null;

            foreach (Collider source in colliderOwner.GetComponentsInChildren(typeof(Collider))) {
                if (!source.Raycast(ray, out var hitInfo, maxDist))
                    continue;

                var sourcePos = hitInfo.point;
                float mag = hitInfo.distance;

                if (mag < min) {
                    min = mag;
                    found = sourcePos;
                    closestCollider = source;
                }
            }
            return found;
        }
        public static Vector3 GetClosestColliderPoint(GameObject colliderOwner, Vector3 origin, out Collider closestCollider) {
            var min = float.MaxValue;
            var found = new Vector3();
            closestCollider = null;

            foreach (Collider source in colliderOwner.GetComponentsInChildren(typeof(Collider))) {
                //var point = source.ClosestPoint(from); //-- too slow to use when looping; assume that closest collider also has closest COM
                var sourcePos = source.transform.position;
                float mag = Vector3.SqrMagnitude(origin - sourcePos);

                if (mag < min) {
                    min = mag;
                    found = sourcePos;
                    closestCollider = source;
                }
            }
            if (closestCollider != null)
                found = closestCollider.ClosestPoint(origin);
            return found;
        }
        public static IEnumerable<Type> EnumerateTypes(Func<Type, bool> predicate) => AppDomain.CurrentDomain.GetAssemblies().SelectMany(o => o.GetTypes().Where(t => predicate(t)));
        //Patched version of base version; used here to preserve base version
        public static float GetRarityMod(int rarity, float equipRarityMod, float effectMod) {
            float result = 1f;
            bool invert = effectMod < 0;
            if (invert)
                effectMod *= -1;

            switch (rarity) {
                case 0:
                    result = 1f / (1f + 1f * equipRarityMod * effectMod);
                    break;
                case 1:
                    result = 1f;
                    break;
                case 2:
                    result = 1f + 0.2f * equipRarityMod * effectMod;
                    break;
                case 3:
                    result = 1f + 0.5f * equipRarityMod * effectMod;
                    break;
                case 4:
                    result = 1f + 1f * equipRarityMod * effectMod;
                    break;
                case 5:
                    result = 1f + 1.6f * equipRarityMod * effectMod;
                    break;
            }

            if (invert)
                result = 1 / result;

            return result;
        }
        public static int GetLayerMask(params Layer[] layers) {
            int ret = 0;
            for (int i = 0; i < layers.Length; ++i)
                ret |= 1 << (int)layers[i];
            return ret;
        }

        public static Guid Int_to_Guid(int value) {
            var bytes = new byte[16];
            BitConverter.GetBytes(value).CopyTo(bytes, 0);
            return new Guid(bytes);
        }
        public static int Guid_to_Int(Guid guid) {
            var bytes = guid.ToByteArray();
            return BitConverter.ToInt32(bytes, 0);
        }
    }
}
