using System;
using System.Reflection;
using UnityEngine;

namespace Charon.StarValor.ModCore {
    [Serializable]
    public abstract class AIDummyCharacter : AIMercenaryCharacter, IHasLocation {
        static MethodInfo __AIControl_Awake = typeof(AIControl).GetMethod("Awake", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        static FieldInfo __GameManager_spaceshipsGroup = typeof(GameManager).GetField("spaceshipsGroup", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        public bool valid = true;
        string shipName;

        public Location CurrentLocation { get; set; }
        public Location LocationId { get; }

        Vector3 position, velocity, angularVelocity;
        Quaternion rotation;

        [NonSerialized]
        GameObject lastGO;
        [NonSerialized]
        Transform owner;

        public AIDummyCharacter(SpaceShipData shipData) {
            this.shipData = shipData;
            LocationId = LocationSystem.Instance.Instantiate(this, LocationType.General);

            if (rotation == default)
                rotation = Quaternion.identity;

            name = "DummyAI";
            behavior.initiated = true;
            currTactic = -1;
            travelSpeed = float.MaxValue;
            AIType = -1;
            alive = true;
            rank = 0;

            Refresh();
        }
        public void UpdatePhysics(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity) => (this.position, this.rotation, this.velocity, this.angularVelocity) = (position, rotation, velocity, angularVelocity);
        public void UpdatePhysics(Rigidbody rb) => (this.position, this.rotation, this.velocity, this.angularVelocity) = (rb.position, rb.rotation, rb.velocity, rb.angularVelocity);

        #region Location system
        public void Move(IHasLocation target) {
            switch (target.LocationId.Type) {
                case LocationType.Sector:
                    this.hangarDocked = false;
                    this.dockedStationID = -1;
                    break;
                case LocationType.Station:
                    this.hangarDocked = false;
                    this.dockedStationID = target.LocationId.Id;
                    break;
                case LocationType.Player:
                case LocationType.Mercenary:
                case LocationType.General:
                default:
                    break;
            }
            CurrentLocation = LocationSystem.Instance.Move(this, target);
        }
        public void Receive(IHasLocation location) { }
        public void ActivateLocation() {
            if (CurrentLocation.Type == LocationType.Sector)
                Spawn(true);
        }
        public void DeactivateLocation() { }
        public void Destroy() => LocationSystem.Instance.Destroy(this);
        #endregion
        public new string GetFleetMemberString() {
            string text = "<b>" + this.CommanderName(14, false) + "</b>\n\n";
            ShipModelData shipModelData = this.ModelData();
            string rarityColor = ItemDB.GetRarityColor(shipModelData.rarity);
            text = string.Concat(new string[] {
                text,
                ColorSys.gray,
                Lang.Get(23, 93),
                ": </color><b>",
                rarityColor,
                shipModelData.modelName,
                "</color></b>\n\n"
            });
            text = text + ColorSys.mediumGray + Lang.Get(0, 120) + "</color>\n";
            if (shipData.weapons.Count == 0)
                text += "-";
            else
                text += shipData.GetInstalledWeaponsString(null);
            return text;
        }

        public void Refresh() => shipName = shipData.GetShipModelData().modelName;
        public override void ChangeTactic(int chance) { }
        public override void DefineTactics(ShipClassLevel shipSize, SpaceShip ss) { }
        public virtual string WorkshopName() => shipName;
        public override string Name() => "Derelict " + shipName;
        public override string CommanderName(int fontSize, bool colored) => shipName;
        protected abstract Type AIControlType { get; }
        public GameObject Spawn(bool enable, Vector3 position, Quaternion rotation, Vector3 velocity = new Vector3(), Vector3 angularVelocity = new Vector3()) {
            UpdatePhysics(position, rotation, velocity, angularVelocity);
            return Spawn(enable);
        }
        public GameObject Spawn(bool enable) {
            LocationSystem.Instance.Move(this, LocationSystem.Instance.GetLocation(LocationType.Sector, GameData.data.currentSectorIndex));

            var go = UnityEngine.Object.Instantiate(Utilities.BlankShipGO, position, rotation);
            var ai = (AIControl)go.AddComponent(AIControlType);
            ai.Char = this;
            go.name = name;
            //__AIControl_Awake.Invoke(ai, null);
            this.owner = go.transform;

            var root = (Transform)__GameManager_spaceshipsGroup.GetValue(GameManager.instance);
            go.transform.SetParent(root);

            var rb = go.GetComponent<Rigidbody>();
            rb.velocity = velocity;
            rb.angularVelocity = angularVelocity;

            if (enable)
                go.SetActive(true);
            lastGO = go;
            return go;

            //if (LocationInfo.Type == LocationType.Station) {
            //    //GameData.data.stationList[0];
            //    //info already saved, don't do anything
            //    //patch inventory.LoadItems appropriately for station inventory
            //}
            //if (LocationInfo.Type == LocationType.Mercenary) {
            //    //PChar.Char.mercenaries;
            //    //patch inventory.LoadItems appropriately for character inventory
            //}
            //if (LocationInfo.Type == LocationType.Universal) {
            //    //patch inventory.LoadItems appropriately for character inventory
            //}
        }
        public void Despawn() {
            if (lastGO == null)
                return;

            var rb = lastGO.GetComponent<Rigidbody>();
            if (rb != null)
                UpdatePhysics(rb);

            UnityEngine.Object.Destroy(lastGO);
            lastGO = null;
        }
    }
}
