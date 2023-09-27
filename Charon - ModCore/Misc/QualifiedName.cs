using System;
using System.Reflection;

namespace Charon.StarValor.ModCore {
    public struct QualifiedName {
        public string Name { get; }
        public string Namespace { get; }
        public string Assembly { get; }
        public string FullName => $"{Namespace}+{Name}";
        public string AssemblyQualifiedName => $"{Assembly}+{FullName}";
        public override string ToString() => FullName;
        QualifiedName(string @name, string @namespace, string @assembly) {
            this.Name = @name;
            this.Namespace = @namespace;
            this.Assembly = @assembly;
        }
        public QualifiedName(Type type, string newName) : this(newName, type.Namespace, type.Assembly.ToString()) { }
        public QualifiedName(object o) : this(o.GetType()) { }
        public QualifiedName(Type type) : this(type, type.Name) { }
        public QualifiedName(object o, string newName) : this(o.GetType(), newName) { }

        public override bool Equals(object obj) => obj is QualifiedName qn && Name == qn.Name && Namespace == qn.Namespace && Assembly == qn.Assembly;
        public override int GetHashCode() => Utilities.GetHashCode(Name?.GetHashCode() ?? 0, Namespace?.GetHashCode() ?? 0, Assembly?.GetHashCode() ?? 0);
        public object GetSerialization() {
            return new string[] { Name, Namespace, Assembly };
        }
        public static QualifiedName Deserialize(object serialization) {
            var objs = (string[])serialization;
            return new QualifiedName(objs[0], objs[1], objs[2]);
        }
    }
}
