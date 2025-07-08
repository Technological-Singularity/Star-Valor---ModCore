using System.Collections.Generic;
using HarmonyLib;
using System.Linq;
using UnityEngine;
using BepInEx;
using System;

namespace Charon.StarValor.ModCore {
    class LocationMercenary : LocationSystem.LocationTemplate<AIMercenaryCharacter> {
        protected override LocationType LocationType => LocationType.Mercenary;
        public override AIMercenaryCharacter Object => PChar.Char.mercenaries[LocationId.Id];
        public LocationMercenary(int id) : base(id) { }
    }
    sealed class LocationNone : LocationSystem.LocationTemplate<object> {
        protected override LocationType LocationType => LocationType.None;
        public override object Object => null;
        public LocationNone() : base(0) { }
    }
    class LocationPlayer : LocationSystem.LocationTemplate<PlayerCharacter> {
        protected override LocationType LocationType => LocationType.Player;
        public override PlayerCharacter Object => PChar.Char;
        public LocationPlayer(int id) : base(id) { }
    }
    class LocationSector : LocationSystem.LocationTemplate<TSector> {
        protected override LocationType LocationType => LocationType.Sector;
        public override TSector Object => GameData.data.sectors[LocationId.Id];
        public LocationSector(int id) : base(id) { }
    }
    class LocationStation : LocationSystem.LocationTemplate<Station> {
        protected override LocationType LocationType => LocationType.Sector;
        public override Station Object => GameData.data.stationList[LocationId.Id];
        public LocationStation(int id) : base(id) { }
    }

    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    [BepInProcess("Star Valor.exe")]
    [HasPatches]
    public class LocationSystem : ModCorePlugin, ISerializableGuid {
        public const string pluginGuid = "starvalor.charon.modcore.location_system";
        public const string pluginName = "Charon - Modcore - Location System";
        public const string pluginVersion = "0.0.0.0";
        
        static IHasLocation DefaultLocation { get; set; }
        public static LocationSystem Instance { get; private set; }

        public abstract class LocationTemplate<T> : IHasLocation where T : class {
            public abstract T Object { get; }

            public Location CurrentLocation {
                get => _currentLocation;
                set => _currentLocation = value;
            }
            public Location LocationId => _locationId;

            Location _currentLocation;
            Location _locationId;

            protected LocationTemplate(int id) => _locationId = Instance.InstantiateForced(this, LocationType, id);

            protected abstract LocationType LocationType { get; }

            public virtual void ActivateLocation() { }
            public virtual void DeactivateLocation() { }
            public virtual void Move(Location target) { }
            public virtual void Destroy() { }
            public virtual void Move(IHasLocation target) { }
            public virtual void Receive(IHasLocation target) { }
        }


        void Awake() {
            Instance = this;
            DefaultLocation = new LocationNone();
        }
        public override void OnPluginLoad() => SerializeSystem.Instance.Add(this);

        #region Patches
        [HarmonyPatch(typeof(GameManager), "PrepareNormalGame")]
        [HarmonyPostfix]
        static void GameManager_PrepareNormalGame_Dummy(Transform tempObjects) {
            int currentSectorIndex = GameData.data.currentSectorIndex;
            if (currentSectorIndex < 0)
                return;
            if (GameData.data.sectors.Count <= currentSectorIndex || GameData.data.sectors[currentSectorIndex] == null)
                return;
            Instance.ChangeLocation(new Location() { Type = LocationType.Sector, Id = currentSectorIndex });
        }
        #endregion

        [Serialize]
        Dictionary<LocationType, Dictionary<int, HashSet<IHasLocation>>> locationables = new Dictionary<LocationType, Dictionary<int, HashSet<IHasLocation>>>();
        
        [Serialize]
        Dictionary<(LocationType type, int id), IHasLocation> locationOwners = new Dictionary<(LocationType, int), IHasLocation>();

        //[Serialize]
        //IHasLocation activeLocation;
        
        [Serialize]
        int generalLocationMaxIndex = 0;

        public object OnSerialize() => null;
        public void OnDeserialize(object data) { }
        public IHasLocation GetLocation(Location location) => GetLocation(location.Type, location.Id);
        public IHasLocation GetLocation(LocationType type, int id) {
            if (type == LocationType.None)
                return DefaultLocation;

            if (!locationOwners.TryGetValue((type, id), out var ihl)) {
                switch (type) {
                    case LocationType.Mercenary:
                        ihl = new LocationMercenary(id);
                        break;
                    case LocationType.Player:
                        ihl = new LocationPlayer(id);
                        break;
                    case LocationType.Sector:
                        ihl = new LocationSector(id);
                        break;
                    case LocationType.Station:
                        ihl = new LocationStation(id);
                        break;
                    case LocationType.General:
                    default:
                        throw new Exception("Could not find id " + id + " in " + type);
                }
                InstantiateForced(ihl, type, id);
            }
            return ihl;
        }
        void ChangeLocation(Location location) {
            foreach (var aichar in GetChildren(location))
                aichar.ActivateLocation();
        }
        public IEnumerable<IHasLocation> GetChildren(Location location) {
            var (type, id) = (location.Type, location.Id);
            if (locationables.TryGetValue(type, out var idxx) && idxx.TryGetValue(id, out var wr))
                return wr;
            return Enumerable.Empty<IHasLocation>();
        }
        public Location Instantiate(IHasLocation obj, LocationType type) {
            int idx = generalLocationMaxIndex;
            while (locationables.TryGetValue(type, out var dict) && dict.ContainsKey(idx))
                ++idx;
            generalLocationMaxIndex = idx + 1;
            return InstantiateForced(obj, type, idx);
        }
        Location InstantiateForced(IHasLocation obj, LocationType type, int id_forced) {
            var locationId = new Location(type, id_forced);
            if (type != LocationType.None) {
                locationOwners.Add((type, id_forced), obj);
                obj.CurrentLocation = DefaultLocation.LocationId;
            }
            return locationId;
        }
        public void Destroy(IHasLocation obj) {
            Remove(obj);
            locationOwners.Remove((obj.LocationId.Type, obj.LocationId.Id));
        }
        void Add(IHasLocation obj, Location location) {
            var (type, id) = (location.Type, location.Id);
            if (!locationables.TryGetValue(type, out var dict)) {
                dict = new Dictionary<int, HashSet<IHasLocation>>();
                locationables[type] = dict;
            }
            if (!dict.TryGetValue(id, out var subdict)) {
                subdict = new HashSet<IHasLocation>();
                dict[id] = subdict;
            }
            subdict.Add(obj);
        }
        void Remove(IHasLocation obj) {
            var location = obj.CurrentLocation;

            if (!locationables.TryGetValue(obj.CurrentLocation.Type, out var dict))
                return;
            if (!dict.TryGetValue(location.Id, out var subdict))
                return;

            subdict.Remove(obj);
            if (subdict.Count == 0)
                subdict.Remove(obj);
            if (dict.Count == 0)
                dict.Remove(location.Id);
        }
        public Location Move(IHasLocation obj, IHasLocation target) {
            var location = target.LocationId;
            Remove(obj);
            Add(obj, location);
            return location;
        }
    }
}
