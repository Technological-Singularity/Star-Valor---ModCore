using System;

namespace Charon.StarValor.ModCore {
    public interface IHasLocation {
        Location CurrentLocation { get; set; }
        Location LocationId { get; }
        void Move(IHasLocation target);
        void Receive(IHasLocation target);
        void Destroy();
        void ActivateLocation();
        void DeactivateLocation();
    }
    public struct Location : IEquatable<Location> {
        public static Location Default { get; } = new Location();
        public LocationType Type;
        public int Id;
        public Location(LocationType type, int id) => (Type, Id) = (type, id);
        public override int GetHashCode() => Utilities.GetHashCode(Type.GetHashCode(), Id.GetHashCode());
        public bool Equals(Location o) => o.Type == Type && o.Id == Id;
        public static bool operator ==(Location b1, Location b2) => b1.Equals(b2);
        public static bool operator !=(Location b1, Location b2) => !(b1.Equals(b2));
        public override bool Equals(object obj) => obj is Location lo && lo == this;
    }
    
}
