namespace Charon.StarValor.ModCore {
    public interface ISerializable {
        object OnSerialize();
        void OnDeserialize(object data);
    }
}
