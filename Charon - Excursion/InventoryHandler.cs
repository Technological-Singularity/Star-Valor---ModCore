using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Charon.StarValor.ModCore;

namespace Charon.StarValor.Excursion {
    class WorkshopInventoryHandler {
        public static WorkshopInventoryHandler Instance { get; } = new WorkshopInventoryHandler();
        #region Patches
        [HarmonyPatch(typeof(GameManager), nameof(GameManager.MakeSafePosition))]
        [HarmonyPrefix]
        static bool GameManager_MakeSafePosition_Disable(Vector3 pos, float area) {
            return false;
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.SelectItem))]
        [HarmonyPostfix]
        static void Inventory_SelectItem_NewButtons(int itemIndex, int slotIndex, bool allowAutoActions, Inventory __instance, int ___cargoMode, CargoSystem ___cs, Transform ___itemPanel) {
            Instance.InventorySelect(__instance, itemIndex, slotIndex, ___cargoMode, ___cs, ___itemPanel);
        }
        //[HarmonyPatch(typeof(PChar), nameof(PChar.GetCarrierSpaceOcupied))]
        //[HarmonyPrefix]
        //static bool PChar_GetCarrierSpaceOccupied_CorrectForDocked(ref int __result) {
        //    var cc = PlayerControl.inst.GetSpaceShip.GetComponent<CarrierControl>();
        //    __result = GetDockedShips(cc).Select(o => o.shipModelData.fleetSpaceOcupied).Sum();
        //    return false;
        //}

        class AIDummyCharacterData : MonoBehaviour {
            public static IEnumerable<AIDummyCharacter> Get(Transform t) => t.GetComponents<AIDummyCharacterData>().Select(o => o.Data);
            public static void Clear(Transform t) => t.GetComponents<AIDummyCharacterData>().ToList().ForEach(o => Destroy(o));
            public AIDummyCharacter Data;
            public void OnMouseEnter(InventorySlot slot, Inventory inventory) {
                var tt = (Tooltip)__Inventory_tooltip.GetValue(inventory);
                tt.sprite = Data.ModelData().image;
                tt.showExtras = false;
                tt.showImageText = false;
                var prefix = tt.sprite == null ? null : "               ";
                tt.ShowItem(prefix + Data.GetFleetMemberString(), false, false);
            }
        }

        [HarmonyPatch(typeof(InventorySlot), nameof(InventorySlot.SlotMouseEnter))]
        [HarmonyPrefix]
        static bool InventorySlot_SlotMouseEnter_FixCustomData(InventorySlot __instance, Inventory ___inv) {
            foreach (var o in __instance.transform.GetComponents<AIDummyCharacterData>()) {
                o.OnMouseEnter(__instance, ___inv);
                return false;
            }
            return true;
        }
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.LoadItems))]
        [HarmonyPrefix]
        static void Inventory_LoadItems_ClearCustom(Transform ___itemPanel, int ___cargoMode) {
            Instance.ResetButtons();

            if (___cargoMode != 2)
                return;

            for (int i = 0; i < ___itemPanel.childCount; ++i)
                AIDummyCharacterData.Clear(___itemPanel.GetChild(i));
        }
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.LoadItems))]
        [HarmonyPostfix]
        static void Inventory_LoadItems_AddDocked(Transform ___itemPanel, int ___cargoMode) {
            if (___cargoMode != 2)
                return;

            //find itemPanel id
            int offset = ___itemPanel.childCount;
            while (offset > 0 && !___itemPanel.GetChild(offset - 1).gameObject.activeSelf)
                --offset;

            int start_offset = offset;
            //Stack<Transform> newTransforms = new Stack<Transform>();

            Transform CreateNextSlot(out InventorySlot root) {
                Transform slot;
                if (offset >= ___itemPanel.childCount) {
                    slot = UnityEngine.Object.Instantiate(Inventory.instance.Slot).transform;
                    slot.SetParent(___itemPanel, false);
                }
                else {
                    slot = ___itemPanel.GetChild(offset);
                    slot.gameObject.SetActive(true);
                }
                root = slot.GetComponent<InventorySlot>();
                root.slotIndex = offset;
                //newTransforms.Push(slot.transform);
                ++offset;
                return slot.GetChild(0);
            }

            var totalWorkshop = GetTotalWorkshopSpace(PlayerControl.inst.transform);
            if (totalWorkshop > 0) {
                {
                    var slot = CreateNextSlot(out var slotInfo);

                    var usedWorkshop = GetUsedWorkshopSpace(PlayerControl.inst.transform);
                    var size = GetMaxWorkshopSize(PlayerControl.inst.transform);
                    var sizeString = (size < (int)ShipClassLevel.Yacht || size >= (int)ShipClassLevel.Dreadnought) ? null : "[" + Enum.GetName(typeof(ShipClassLevel), size) + "]";

                    string text = string.Join("",
                        ColorSys.mediumGray,
                        "<size=15>",
                        "Workshop",
                        " (",
                        usedWorkshop.ToString("0.#"),
                        "/",
                        totalWorkshop.ToString("0.#"),
                        ") ",
                        sizeString,
                        "</size></color>"
                        );

                    slot.GetComponentInChildren<Text>().text = text;
                    slot.GetChild(2).GetComponent<Image>().enabled = false;
                    slot.GetComponent<Button>().interactable = false;

                    slotInfo.SetTextAlignCenter();
                    slotInfo.itemIndex = -1;
                    slotInfo.isFleet = false;
                }

                foreach (var child in LocationSystem.Instance.GetChildren(new Location(LocationType.Player, 0))) {
                    if (!(child is AIDummyCharacter usc))
                        continue;

                    var slot = CreateNextSlot(out var slotInfo);
                    var data = slotInfo.gameObject.AddComponent<AIDummyCharacterData>();
                    data.Data = usc;

                    slot.GetComponentInChildren<Text>().text = usc.WorkshopName();
                    slot.GetChild(2).GetComponent<Image>().sprite = usc.ModelData().image;
                    slot.GetChild(2).GetComponent<Image>().enabled = true;
                    slot.GetComponent<Button>().interactable = true;

                    slotInfo.SetTextAlignLeft();
                    slotInfo.itemIndex = 0;
                    slotInfo.isFleet = true;
                }
            }

            //while (newTransforms.Count > 0)
            //    newTransforms.Pop().SetAsFirstSibling();

            for (int i = 0; i < ___itemPanel.childCount; ++i) {
                var islot = ___itemPanel.GetChild(i).GetComponent<InventorySlot>();
                if (islot.slotIndex >= 0)
                    islot.slotIndex = i;
            }

            Vector2 sizeDelta = new Vector2(___itemPanel.GetComponent<RectTransform>().sizeDelta.x, (float)(offset * 18 + 5));
            ___itemPanel.GetComponent<RectTransform>().sizeDelta = sizeDelta;
        }
        #endregion
        #region Reflection
        static FieldInfo __Inventory_tooltip = typeof(Inventory).GetField("tooltip", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        static FieldInfo __CarrierControl_dockedShips = typeof(CarrierControl).GetField("dockedShips", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        static List<DockedShip> GetDockedShips(CarrierControl cc) => (List<DockedShip>)__CarrierControl_dockedShips.GetValue(cc);
        #endregion
        public static int GetMaxWorkshopSize(Transform transform) {
            var cc = transform.GetComponent<CarrierControl>();
            if (cc == null)
                return -1;
            return (int)cc.maxShipSize;
        }
        public static float GetUsedWorkshopSpace(Transform transform) {
            Location location = default;
            if (transform == PlayerControl.inst.transform) {
                location = new Location(LocationType.Player, 0);
            }

            if (location == Location.Default) {
                var usc = transform.GetComponent<AIDummyControl>();
                if (usc != null) {
                    var uscc = (UncontrolledShipCharacter)usc.Char;
                    location = uscc.LocationId;
                }
            }

            if (location == Location.Default)
                return 0;

            return LocationSystem.Instance.GetChildren(location).OfType<AIDummyCharacter>().Select(o => o.shipData.GetShipModelData().fleetSpaceOcupied).Sum();
        }
        public static int GetTotalWorkshopSpace(Transform transform) {
            var cc = transform.GetComponent<CarrierControl>();
            if (cc == null)
                return 0;
            return 2 * cc.hangarSpace;
        }
        //btnAddToFleet
        //btn

        //Three classes of ships:
        //  Manned - normal
        //  Derelict - no crew, will drift. equipment will run normally (TBD)
        //  Idle - has crew, but no captain. will slow itself to a stop. acts as a manned ship, but cannot be commanded.
        //      Fleet ships can be converted to/from idle when:
        //          This ship is docked at the idle ship (mothership)
        //          Ship is docked at this ship (mothership)
        //          Both ships are docked at the same station or mothership
        //New buttons
        //      Idle or Derelict ship, targeted, floating nearby in space (next to Hail):
        //          Investigate -> Open refit panel, as station. Local inventory is the Player ship.
        //      Fleet ship, docked at any mothership (Fleet panel):
        //          Hangar -> Open refit panel, as station. Local inventory is the mothership
        //      Player ship, docked at mothership:
        //          Disembark -> transfer player to mothership
        //      Idle or Derelict ship, docked at player mothership:
        //          Embark -> transfer player to docked ship
        //          Assign fleet Ship -> transfer AI to docked ship
        //          Jettison
        //          Transfer to cargo
        //      Idle or Derelict ship, in cargo hold of player ship
        //          Embark
        //          Transfer to hangar
        //          Jettison
        //          Assign fleet ship
        //          (Note: cannot Refit ship while it is stored in cargo hold)

        //cargo -> add to fleet | jettison >> Embark | Transfer to Hangar >> when transferred or jettisoned, convert to dummy ship
        //hangar -> launch | set nickname >> Embark | Transfer to Cargo >> when transferred, verify ship is repaired then convert to cargo item
        //hangar derelicts shouldn't use up fleet capacity or go in player fleet control panel

        GameObject inventoryParent;
        Vector3 verticalOffset;

        GameObject btnCargoEmbark;
        GameObject btnCargoTransferToWorkshop;

        GameObject btnWorkshopDisembark;
        GameObject btnWorkshopEmbark;
        GameObject btnWorkshopRefit;
        GameObject btnWorkshopTransferToCargo;

        List<GameObject> allButtons = new List<GameObject>();

        int lastIndex, lastSlotIndex;
        CargoSystem lastCargoSystem;
        object lastData;

        GameObject CreateButton(GameObject parent, GameObject basis, int verticalIndex, string title, UnityEngine.Events.UnityAction action) {
            var gameObject = UnityEngine.Object.Instantiate(basis, parent.transform);
            gameObject.SetActive(false);
            gameObject.transform.position += verticalIndex * verticalOffset;
            var text = gameObject.transform.GetComponentInChildren<Text>();
            text.text = title;
            var btn = gameObject.transform.GetComponent<Button>();
            btn.onClick = new Button.ButtonClickedEvent();
            btn.onClick.AddListener(action);
            allButtons.Add(gameObject);
            return gameObject;
        }
        public void TryInitializeButtons() {
            if (inventoryParent != null)
                return;

            allButtons.Clear();
            inventoryParent = Inventory.instance.transform.Find("InventoryUI").gameObject;

            var btnBasis = inventoryParent.transform.Find("BtnAddToFleet").gameObject;
            verticalOffset = inventoryParent.transform.Find("BtnJettison").position - btnBasis.transform.position; //vertical positioning

            btnCargoEmbark = CreateButton(inventoryParent, btnBasis, 2, "Embark", Click_btnCargoEmbark);
            btnCargoTransferToWorkshop = CreateButton(inventoryParent, btnBasis, 3, "Transfer to Workshop", Click_btnCargoTransferToWorkshop);

            btnWorkshopDisembark = CreateButton(inventoryParent, btnBasis, 2, "Disembark", Click_btnWorkshopDisembark);
            btnWorkshopEmbark = CreateButton(inventoryParent, btnBasis, 3, "Embark", Click_btnWorkshopEmbark);
            btnWorkshopRefit = CreateButton(inventoryParent, btnBasis, 4, "Refit", Click_btnWorkshopRefit);
            btnWorkshopTransferToCargo = CreateButton(inventoryParent, btnBasis, 5, "Transfer to Cargo", Click_btnHangarTransferToCargo);
        }
        public void ResetButtons() {
            Instance.TryInitializeButtons();
            foreach (var b in allButtons)
                b.SetActive(false);
        }
        public void InventorySelect(Inventory inventory, int itemIndex, int slotIndex, int cargoMode, CargoSystem cs, Transform itemPanel) {
            ResetButtons();
            if (itemIndex < 0 || (cargoMode < 2 && itemIndex >= cs.cargo.Count))
                return;

            lastIndex = itemIndex;
            lastSlotIndex = slotIndex;
            lastCargoSystem = cs;
            lastData = null;

            var slot = itemPanel.GetChild(slotIndex).GetComponent<InventorySlot>();

            if (cargoMode != 2 && cs.cargo[itemIndex].itemType == 4)
                InventorySelectCargo(cs, slot);
            else if (cargoMode == 2)
                InventorySelectHangarWorkshop(slot);
        }
        void InventorySelectCargo(CargoSystem cs, InventorySlot slot) {
            //var item = cs.cargo[slot.itemIndex];
            //itemIndex is index of the item in cs.cargo (also == slotIndex)

            btnCargoEmbark.SetActive(true);
            btnCargoTransferToWorkshop.SetActive(true);
        }
        void InventorySelectHangarWorkshop(InventorySlot slot) {
            //itemIndex is index of the Char in PChar.Char.mercenaries (why do it this way?)
            var usc_data = slot.GetComponent<AIDummyCharacterData>();
            if (usc_data != null)
                lastData = usc_data.Data;

            btnWorkshopDisembark.SetActive(true);
            btnWorkshopEmbark.SetActive(true);
            btnWorkshopRefit.SetActive(true);
            btnWorkshopTransferToCargo.SetActive(true);
        }
        void TestLog(string message = default, [CallerMemberName] string caller = default) => Plugin.Log.LogMessage(caller + ": " + message);

        void Click_btnCargoEmbark() {
            TestLog();

            const int mask = (1 << 8) | (1 << 9) | (1 << 10) | (1 << 13);

            var pcTransform = PlayerControl.inst.GetSpaceShip.transform;
            var pcPos = pcTransform.position;

            var cargoItem = lastCargoSystem.cargo[lastIndex];
            var loadout = cargoItem.GetShipLoadout();
            Plugin.Log.LogMessage("Embark " + loadout.GetShipModelData().modelName);

            Vector3 rayAngle = pcTransform.forward;// Quaternion.Euler(0, UnityEngine.Random.Range(0f, 360f), 0) * Vector3.forward;
            var usc = new UncontrolledShipCharacter(loadout);
            var shipGO = usc.Spawn(false, pcPos, pcTransform.rotation);

            var bounds = Plugin.GetBounds(shipGO.transform, true);
            var targetRadius = Mathf.Max(bounds.size.x, bounds.size.z);

            var pcColliders = Plugin.SetColliders(pcTransform, true);
            //var pcEnabled = Plugin.SetEnabled(pcTransform, true);
            //var targetColliders = SetColliders(shipGO.transform, false);

            bool unblocked = true;
            var targetPos = pcPos;

            float hitDist = float.MaxValue;
            foreach (var hit in Physics.SphereCastAll(pcPos + rayAngle * 100 * targetRadius, targetRadius, -rayAngle, 100 * targetRadius, mask, QueryTriggerInteraction.Ignore)) {
                bool proceed = hit.transform == pcTransform;
                if (!proceed) {
                    var cc = hit.transform.GetComponent<ColliderControl>();
                    if (cc != null && cc.ownerEntity.transform == pcTransform)
                        proceed = true;
                }
                if (proceed && hit.distance < hitDist) {
                    targetPos = hit.point + 1 * rayAngle;
                    hitDist = hit.distance;
                }
            }
            foreach (var o in Physics.OverlapSphere(targetPos, targetRadius, mask, QueryTriggerInteraction.Ignore)) {
                unblocked = o.transform == pcTransform;
                if (!unblocked) {
                    var cc = o.transform.GetComponent<ColliderControl>();
                    if (cc != null && cc.ownerEntity.transform == pcTransform)
                        unblocked = true;
                }
            }
            Plugin.ResetColliders(pcColliders);
            //Plugin.ResetEnabled(pcEnabled);

            //for (int iRadius = 0; unblocked && iRadius < 100; ++iRadius) {
            //    Plugin.Log.LogMessage("checking " + iRadius * targetRadius);

            //    var pos = pcPos + rayAngle * iRadius * targetRadius;
            //    var o = Physics.OverlapSphere(pos, targetRadius, mask, QueryTriggerInteraction.Ignore).FirstOrDefault();
            //    if (o == default) {
            //        targetPos = pos;
            //        break;
            //    }
            //    else {
            //        unblocked = o.transform == pcTransform;
            //        if (!unblocked) {
            //            var cc = o.transform.GetComponent<ColliderControl>();
            //            if (cc != null && cc.ownerEntity.transform == pcTransform)
            //                unblocked = true;
            //        }
            //    }
            //}

            if (!unblocked) {
                UnityEngine.Object.Destroy(shipGO);
                usc.Destroy();
                InfoPanelControl.inst.ShowWarning("Embarkation blocked", 1, true);
                return;
            }

            GameData.data.DeleteShipLoadout(cargoItem.shipLoadoutID);
            shipGO.transform.position = targetPos;
            usc.UpdatePhysics(shipGO.GetComponent<Rigidbody>());
            shipGO.SetActive(true);
            //ResetColliders(targetColliders);        

            Inventory.instance.DestroyCargoItem(true);
            Plugin.SwapPlayerShip(shipGO.transform.GetComponent<SpaceShip>(), true);
            Inventory.instance.LoadItems();
            ShipEnhancement.inst.RefreshScreen();
        }
        void Click_btnCargoTransferToWorkshop() {
            TestLog();

            var cargoItem = lastCargoSystem.cargo[lastIndex];
            var loadout = cargoItem.GetShipLoadout();
            var model = loadout.GetShipModelData();
            if (GetMaxWorkshopSize(PlayerControl.inst.transform) < (int)model.shipClass) {
                InfoPanelControl.inst.ShowWarning(Lang.Get(6, 38), 1, true);
                return;
            }
            if (GetTotalWorkshopSpace(PlayerControl.inst.transform) - GetUsedWorkshopSpace(PlayerControl.inst.transform) < model.fleetSpaceOcupied) {
                InfoPanelControl.inst.ShowWarning(Lang.Get(6, 37), 1, true);
                return;
            }

            GameData.data.DeleteShipLoadout(cargoItem.shipLoadoutID);
            var usc = new UncontrolledShipCharacter(loadout);
            usc.Move(LocationSystem.Instance.GetLocation(LocationType.Player, 0));
            ++Inventory.instance.newFleetCount;
            Inventory.instance.DestroyCargoItem(true);
        }
        void Click_btnWorkshopDisembark() {
            TestLog();
        }
        void Click_btnWorkshopEmbark() {
            TestLog();
        }
        void Click_btnWorkshopRefit() {
            TestLog();
        }
        void Click_btnHangarTransferToCargo() {
            TestLog();

            if (!(lastData is UncontrolledShipCharacter usc))
                throw new Exception("btnHangarTransferToCargo tried to transfer invalid character");

            var model = usc.shipData.GetShipModelData();
            if (Inventory.instance.currStation == null && lastCargoSystem.FreeSpace(false) < model.spaceOcupied) {
                InfoPanelControl.inst.ShowWarning(Lang.Get(6, 35), 1, true);
                return;
            }

            usc.Destroy();
            int loadoutId = GameData.data.NewShipLoadout(null);
            GameData.data.SetShipLoadout(usc.shipData, loadoutId);
            var oldCount = Inventory.instance.newItemCount;
            lastCargoSystem.StoreItem(4, model.id, model.rarity, 1, 0, -1, -1, loadoutId);
            var newCount = Inventory.instance.newItemCount;
            if (newCount - oldCount != 1) {
                Inventory.instance.newItemCount = oldCount + 1;
                Inventory.instance.UpdateNewItemsCount();
            }

            Inventory.instance.RefreshIfOpen(null, false);
            Inventory.instance.DeselectItems();
            ResetButtons();
            Inventory.instance.LoadItems();
        }
    }
}
