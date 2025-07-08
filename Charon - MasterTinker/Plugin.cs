using System.Linq;
using BepInEx;
using Charon.StarValor.ModCore;
using HarmonyLib;
using UnityEngine;


//[HarmonyPatch(typeof(LootSystem), nameof(LootSystem.InstantiateDrop))]
//[HarmonyPrefix]
//public static void InstantiateDrop_RarityFix(int itemType, int itemID, ref int rarity, Vector3 pos, int qnt, float pricePaid, int sellerID, float force, int shipLoadoutID, CI_Data extraData) {
//    if (itemType == 3)
//        rarity = ItemDB.GetItem(itemID).rarity;
//    if (itemType == 4)
//        rarity = ShipDB.GetModel(itemID).rarity;
//}

namespace Charon.StarValor.MasterTinker {
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    [BepInProcess("Star Valor.exe")]
    [HasPatches]
    public class Plugin : ModCorePlugin {
        public const string pluginGuid = "ModCore.MasterTinker";
        public const string pluginName = "MasterTinker";
        public const string pluginVersion = "0.0.0.0";
        public static Plugin Instance { get; private set; }
        void Awake() => Instance = this;

        bool equipAdded = false;

        public override void OnPluginLoadLate() {
            EquipmentItem.GetAllPermutations<Equipment_HyperspatialAnchor>();
            EquipmentItem.GetAllPermutations<Equipment_DeflectorArray>();
        }

        [HarmonyPatch(typeof(CharacterScreen), nameof(CharacterScreen.Open))]
        [HarmonyPostfix]
        public static void Open(int mode) {
            if (Instance.equipAdded)
                return;

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player is null)
                Instance.Log.LogWarning("player not found");
            var cargo_system = player.GetComponent<CargoSystem>();
            if (cargo_system is null)
                Instance.Log.LogWarning("cargo_system not found");
            var ship = cargo_system.transform.GetComponent<SpaceShip>();
            if (ship is null)
                Instance.Log.LogWarning("ship not found");

            //Instance.Log.LogMessage("Showing weapons");
            //foreach (var w in ship.weapons)
            //    foreach (var c in w.projectileRef.GetComponents<Component>())
            //        Instance.Log.LogMessage($"    {c.name} {c.GetType().FullName}");
            //if (player.GetComponent<CachedValue.Debugger>() == null)
            //    player.AddComponent<CachedValue.Debugger>();

            Instance.Log.LogMessage("Spawning items");
            void grantEquipment<T>() where T : EquipmentItem {
                foreach (var eq in EquipmentItem.GetAllPermutations<T>().Where(o => o.TemplateData.Template is Equipment_DeflectorArray)) {
                    //Instance.Log.LogMessage("Spawning " + eq.name);
                    //if ((int)eq.minShipClass <= ship.sizeClass) {
                        Instance.Log.LogMessage($"    Adding {eq.name} {eq.id}");
                        for (int rarity = 0; rarity <= 5; ++rarity)
                        cargo_system.StoreItem(2, eq.id, rarity, 1, 0f, -1, -1);
                    //}
                }
            }
            //grantEquipment<Equipment_HyperspatialAnchor>();
            grantEquipment<Equipment_DeflectorArray>();
            Instance.equipAdded = true;


            //Log.LogWarning("Cargo test");
            //foreach(var o in ship.shipData.cargo)
            //	if (o.extraData is TestClass tc)
            //		Log.LogWarning("Got value " + tc.Evaluate);

            //var data = new TestClass();
            //         var testCargo = new CargoItem() {
            //	stockStationID = int.MinValue,
            //	extraData = data,
            //};
            //ship.shipData.cargo.Add(testCargo);
            //data.Evaluate = "QWERTY";
        }

        //[HarmonyPatch(typeof(SpaceShipData), nameof(SpaceShipData.SortEquipments))]
        //[HarmonyPrefix]
        //public static bool SortEquipments(SpaceShipData __instance) {
        //	int sorter(InstalledEquipment first, InstalledEquipment second) {
        //		if (first == null && second == null)
        //			return 0;
        //		if (first == null)
        //			return 1;
        //		if (second == null)
        //			return -1;

        //		var eqFirst = EquipmentDB.GetEquipmentByIndex(first.equipmentID);
        //		var eqSecond = EquipmentDB.GetEquipmentByIndex(second.equipmentID);
        //		if ((eqFirst == null || eqFirst.equipName == null) && (eqSecond == null || eqSecond.equipName == null))
        //			return 0;
        //		if ((eqFirst == null || eqFirst.equipName == null))
        //			return 1;
        //		if ((eqSecond == null || eqSecond.equipName == null))
        //			return -1;

        //		var nameComp = eqFirst.equipName.CompareTo(eqSecond.equipName);
        //		if (nameComp != 0)
        //			return nameComp;
        //		return first.rarity.CompareTo(second.rarity);
        //          }
        //	__instance.equipments.Sort(sorter);

        //	foreach(var o in __instance.equipments) {
        //		if (o == null) continue;
        //		var eq = EquipmentDB.GetEquipmentByIndex(o.equipmentID);
        //		if (eq == null) continue;
        //		if (eq.equipName == null) continue;
        //		Core.Log.LogWarning(eq.equipName);
        //	}
        //	return false;
        //}

        //[HarmonyPatch(typeof(CargoSystem), nameof(CargoSystem.SortItems))]
        //[HarmonyPrefix]
        //public static bool SortItems(List<CargoItem> cargo) {
        //	int sorter(CargoItem first, CargoItem second) {
        //		if (first == null && second == null)
        //			return 0;
        //		if (first == null)
        //			return 1;
        //		if (second == null)
        //			return -1;

        //		var typeComp = first.itemType.CompareTo(second.itemType);
        //		if (typeComp != 0)
        //			return -typeComp;

        //		if (first.itemType == 1) {
        //			var idComp = first.itemID.CompareTo(second.itemID);
        //			if (idComp != 0)
        //				return idComp;
        //			return -first.rarity.CompareTo(second.rarity);
        //              }
        //		else if (first.itemType == 2) {
        //			var eqFirst = EquipmentDB.GetEquipmentByIndex(first.itemID);
        //			var eqSecond = EquipmentDB.GetEquipmentByIndex(second.itemID);
        //			if ((eqFirst == null || eqFirst.equipName == null) && (eqSecond == null || eqSecond.equipName == null))
        //				return 0;
        //			if ((eqFirst == null || eqFirst.equipName == null))
        //				return 1;
        //			if ((eqSecond == null || eqSecond.equipName == null))
        //				return -1;

        //			var nameCheck = eqFirst.equipName.CompareTo(eqSecond.equipName);
        //			if (nameCheck != 0)
        //				return -nameCheck;
        //			return -first.rarity.CompareTo(second.rarity);
        //		}
        //		else if (first.itemType == 3) {
        //			var eqFirst = ItemDB.GetItem(first.itemID);
        //			var eqSecond = ItemDB.GetItem(second.itemID);
        //			if ((eqFirst == null || eqFirst.itemName == null) && (eqSecond == null || eqSecond.itemName == null))
        //				return 0;
        //			if ((eqFirst == null || eqFirst.itemName == null))
        //				return 1;
        //			if ((eqSecond == null || eqSecond.itemName == null))
        //				return -1;

        //			var nameCheck = eqFirst.itemName.CompareTo(eqSecond.itemName);
        //			if (nameCheck != 0)
        //				return -nameCheck;
        //			return -first.rarity.CompareTo(second.rarity);
        //		}
        //		return 0;
        //	}
        //	cargo.Sort(sorter);
        //	return false;
        //}
    }
}
