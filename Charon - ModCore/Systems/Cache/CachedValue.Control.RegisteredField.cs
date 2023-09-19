using System;
using System.Reflection;

namespace Charon.StarValor.ModCore {
    public partial class CachedValue {
        partial class Control {
            class RegisteredField {
                float lastValue;
                object instance;
                FieldInfo field;
                Func<object, object> valConvert;
                float lastCommit;
                object committed;
                float differential;
                bool pulledInvalid = true;

                public RegisteredField(object instance, Type type, string fieldName) {
                    field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                    if (field == null)
                        throw new Exception($"{fieldName} in type {type.FullName} not found");

                    this.instance = field.IsStatic ? null : instance;
                    if (field.FieldType == typeof(float))
                        valConvert = (f) => f;
                    else if (field.FieldType == typeof(int))
                        valConvert = (f) => (int)f;
                    else if (field.FieldType == typeof(sbyte))
                        valConvert = (f) => (sbyte)f;
                    else
                        throw new Exception("Type cast not allowed - requested type: " + field.DeclaringType.FullName);
                }
                public bool Pull(out float value) {
                    value = (float)field.GetValue(instance);
                    pulledInvalid = value != lastValue;
                    lastValue = value;
                    value -= differential;
                    return pulledInvalid;
                }
                public void Commit(float value) {
                    if (value == lastCommit && !pulledInvalid)
                        return;
                    lastCommit = value;
                    committed = valConvert(value);
                    var oldDifferential = differential;
                    differential += (float)committed - lastValue;
                }
                public void Push() {
                    if ((float)committed == lastValue)
                        return;
                    lastValue = (float)committed;
                    field.SetValue(instance, committed);
                }
                public void Reset() {
                    if (differential == 0)
                        return;
                    lastValue -= differential;
                    differential = 0;
                    lastCommit = lastValue;
                    field.SetValue(instance, lastValue);
                }
            }
        }
    }
}
