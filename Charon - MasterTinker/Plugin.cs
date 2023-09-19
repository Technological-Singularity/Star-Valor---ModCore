using BepInEx;
using Charon.StarValor.ModCore;
using Charon.StarValor.ModCore.Procedural;
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
        public const string pluginGuid = "starvalor.jram.test";
        public const string pluginName = "JRAM Test Mod";
        public const string pluginVersion = "0.0.0.0";

        public static ModCorePlugin Instance;
        void Awake() => Instance = this;

        public override void OnPluginLoadLate() {
            Equipment_DeflectorShield.Initialize();
            Equipment_HyperspatialAnchor.Initialize();
        }

        class TestClass : CI_Data {
            public string Evaluate { get; set; } = "OK";
        }

        [HarmonyPatch(typeof(CharacterScreen), nameof(CharacterScreen.Open))]
        [HarmonyPostfix]
        public static void Open(int mode) {
            Log.LogMessage("Spawning items");

            var player = GameObject.FindGameObjectWithTag("Player");
            var cargo_system = player.GetComponent<CargoSystem>();
            var ship = cargo_system.transform.GetComponent<SpaceShip>();
            if (player.GetComponent<CachedValue.Debugger>() == null)
                player.AddComponent<CachedValue.Debugger>();


            foreach (var w in ship.weapons)
                foreach (var c in w.projectileRef.GetComponents<Component>())
                    Log.LogMessage(c.name + " " + c.GetType().FullName);



            void grantEquipment<T>() where T : EquipmentItem {
                foreach (var equipment in EquipmentItem.GetAllPermutations<T>())
                    if (Mathf.Abs((int)equipment.minShipClass - ship.sizeClass) < 2 && (equipment.equipName.Contains("Multiplex") || !equipment.equipName.Contains("Array")))
                        for (int rarity = 0; rarity <= 5; ++rarity)
                            cargo_system.StoreItem(2, equipment.id, rarity, 1, 0f, -1, -1, -1);
            }
            grantEquipment<Equipment_HyperspatialAnchor>();
            grantEquipment<Equipment_DeflectorShield>();


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
