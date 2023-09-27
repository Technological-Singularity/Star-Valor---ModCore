using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Charon.StarValor.ModCore {
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializable {
        static object DoSerialize(object o) {
            var type = o.GetType();
            if (type.IsValueType)
                return (type, o);
            if (!(o is ISerializable iser))
                throw new ArrayTypeMismatchException("TKey");
            return (type, iser.Serialize());
        }
        static object DoDeserialize(object o) {
            var (_type, _value) = ((Type, object))o;
            if (_type.IsValueType)
                return o;
            if (!(typeof(ISerializable).IsAssignableFrom(_type)))
                throw new ArrayTypeMismatchException("TKey");
            var _inst = typeof(ScriptableObject).IsAssignableFrom(_type) ? (ISerializable)ScriptableObject.CreateInstance(_type) : (ISerializable)Activator.CreateInstance(_type);
            _inst.Deserialize(_value);
            return _inst;
        }

        object ISerializable.OnSerialize() => this.Select(o => (DoSerialize(o.Key), DoSerialize(o.Value))).ToArray();
        void ISerializable.OnDeserialize(object data) {
            Clear();
            var objs = (object[])data;
            foreach(var o in objs) {
                var (okey, ovalue) = ((object, object))o;
                Add((TKey)DoDeserialize(okey), (TValue)DoDeserialize(ovalue));
            }
        }
    }
}
