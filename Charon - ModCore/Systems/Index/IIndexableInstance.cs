namespace Charon.StarValor.ModCore {
    public interface IIndexableInstance {
        IndexableTemplate Template { get; }
        IndexableInstanceData Data { get; }
        int GetHashCode(HashContext context);
        int Id { get; set; }
        void Allocate();
        bool Release();
        object GetSerialization();
        void Deserialize(object serialization);
    }
}
