using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Charon.StarValor.ModCore {
    public static class SerializeSystem_Extensions {        
        static void SetMemberInfo(MemberInfo info, object obj, object value) {
            if (info is FieldInfo f)
                f.SetValue(obj, value);
            else if (info is PropertyInfo p)
                p.SetValue(obj, value);
        }
        static object GetMemberInfo(MemberInfo info, object obj) {
            if (info is FieldInfo f)
                return f.GetValue(obj);
            else if (info is PropertyInfo p)
                return p.GetValue(obj);
            return null;
        }
        static Type GetMemberInfoUnderlyingType(MemberInfo info) {
            if (info is FieldInfo f)
                return f.FieldType;
            else if (info is PropertyInfo p)
                return p.PropertyType;
            return default;
        }
        static string GetMemberInfoName(MemberInfo info) {
            if (info is FieldInfo f)
                return f.Name;
            else if (info is PropertyInfo p)
                return p.Name;
            return default;
        }
        public static object Serialize(this ISerializable inst) {
            Dictionary<string, object> objects = new Dictionary<string, object>();
            void serializeMember(MemberInfo m) {
                var type = GetMemberInfoUnderlyingType(m);
                var name = GetMemberInfoName(m);
                if (objects.ContainsKey(name))
                    return;
                var val = GetMemberInfo(m, inst);
                if (val is null)
                    val = (type, (object)null);
                else if (val is ISerializable iser)
                    val = (val.GetType(), iser.Serialize());
                objects.Add(name, val);
            }
            for(var type = inst.GetType(); type != typeof(object); type = type.BaseType) {
                foreach (var o in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic).Where(o => o.IsDefined(typeof(SerializeAttribute))))
                    serializeMember(o);
                foreach (var o in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic).Where(o => o.IsDefined(typeof(SerializeAttribute))))
                    serializeMember(o);
            }
            var remaining = inst.OnSerialize();
            return (objects, remaining);
        }
        public static void Deserialize(this ISerializable inst, object data) {
            var datas = ((Dictionary<string, object> objects, object remaining))data;
            var objects = datas.objects;
            HashSet<string> instantiated = new HashSet<string>();

            void deserializeMember(MemberInfo m) {
                var type = GetMemberInfoUnderlyingType(m);
                var name = GetMemberInfoName(m);
                if (!instantiated.Add(name))
                    return;                
                if (!objects.TryGetValue(name, out var value))
                    return;
                if (value is null)
                    return;
                if (typeof(ISerializable).IsAssignableFrom(type)) {
                    var (_type, _value) = ((Type, object))value;
                    var obj = typeof(ScriptableObject).IsAssignableFrom(_type) ? (ISerializable)ScriptableObject.CreateInstance(_type) : (ISerializable)Activator.CreateInstance(_type);
                    obj.Deserialize(_value);
                    value = obj;
                }
                SetMemberInfo(m, inst, value);              
            }
            for (var type = inst.GetType(); type != typeof(object); type = type.BaseType) {
                foreach (var o in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic).Where(o => o.IsDefined(typeof(SerializeAttribute))))
                    deserializeMember(o);
                foreach (var o in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic).Where(o => o.IsDefined(typeof(SerializeAttribute))))
                    deserializeMember(o);
            }
            inst.OnDeserialize(datas.remaining);
        }
    }
}
