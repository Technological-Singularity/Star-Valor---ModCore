namespace Charon.StarValor.ModCore {
    public interface ISerializable {
        string Guid { get; }
        object GetSerialization();
        void Deserialize(bool found, object serialization);
    }
}
