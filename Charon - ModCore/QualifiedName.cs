namespace Charon.StarValor.ModCore {
    public struct QualifiedName {
        public string Guid { get; }
        public string Name { get; }
        public override string ToString() => FullName;
        public string FullName => Guid + "." + Name;
        public QualifiedName(string guid, string name) => (Guid, Name) = (guid, name);
        public QualifiedName(QualifiedName parent, string child) : this(parent.Guid, parent.Name + "." + child) { }
        public override bool Equals(object obj) => obj is QualifiedName qn && qn.Name == Name && qn.Guid == Guid;
        public override int GetHashCode() => Utilities.GetHashCode(Guid?.GetHashCode() ?? 0, Name?.GetHashCode() ?? 0);
    }
}
