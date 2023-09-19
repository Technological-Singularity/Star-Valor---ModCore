using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using Charon.StarValor.ModCore;
using HarmonyLib;
using UnityEngine;

namespace Charon.StarValor.Excursion {
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    [BepInProcess("Star Valor.exe")]
    public class Plugin : ModCorePlugin {
        public const string pluginGuid = "starvalor.charon.excursion";
        public const string pluginName = "Charon - Minifix - Excursion";
        public const string pluginVersion = "0.0.0.0";

        public void Awake() {
            Harmony.CreateAndPatchAll(typeof(Plugin));
            Harmony.CreateAndPatchAll(typeof(WorkshopInventoryHandler));
            Harmony.CreateAndPatchAll(typeof(UncontrolledShipCharacter));
        }

        void Start() {
            var player = GameObject.FindGameObjectWithTag("Player");

        }
        void Update() {
            if (!Input.GetKeyDown(KeyCode.F3))
                return;
            TrySwap();
        }
        #region Utility
        public static void Dump(Transform t, string prefix = "--", bool recurse = true) {
            foreach (var o in t.GetComponents<Component>())
                Log.LogMessage(prefix + t.name + " : " + o.name + " " + o.GetType().FullName);
            if (recurse)
                foreach (Transform child in t)
                    Dump(child, prefix + " ", true);
        }
        static void Interrogate(object o, string name, int recursion_levels) {
            foreach (FieldInfo f in o.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
                var value = f.GetValue(o);
                Log.LogMessage(name + "  " + f.Name + " - " + value);
                if (recursion_levels > 0)
                    Interrogate(value, name + "  ", recursion_levels - 1);
            }
        }
        public static List<(Collider, bool)> SetColliders(Transform transform, bool enabled) {
            List<(Collider, bool)> values = new List<(Collider, bool)>();
            void recurse(Transform t) {
                foreach (Collider c in t.GetComponents<Collider>()) {
                    values.Add((c, c.enabled));
                    c.enabled = enabled;
                }
                foreach (Transform child in t)
                    recurse(child);
            }
            recurse(transform);
            return values;
        }
        public static void ResetColliders(List<(Collider, bool)> values) {
            foreach (var (collider, enabled) in values)
                collider.enabled = enabled;
        }
        static List<(Collider, Collider)> IgnoreCollisions(Transform first, Transform second) {
            List<Collider> allFirst = new List<Collider>();
            List<Collider> allSecond = new List<Collider>();
            List<(Collider, Collider)> ignored = new List<(Collider, Collider)>();

            void _get(Transform curTransform, List<Collider> list) {
                foreach (var o in curTransform.GetComponents<Collider>())
                    list.Add(o);
                foreach (Transform child in curTransform)
                    _get(child, list);
            }
            _get(first, allFirst);
            _get(second, allSecond);
            foreach (var a in allFirst)
                foreach (var b in allSecond) {
                    Physics.IgnoreCollision(a, b, true);
                    ignored.Add((a, b));
                }

            return ignored;
        }
        static void ResetCollisions(List<(Collider, Collider)> list) {
            foreach (var (first, second) in list)
                Physics.IgnoreCollision(first, second, false);
        }
        public static List<(GameObject, bool)> SetEnabled(Transform transform, bool enabled) {
            List<(GameObject, bool)> values = new List<(GameObject, bool)>();
            void recurse(Transform t) {
                values.Add((t.gameObject, t.gameObject.activeSelf));
                t.gameObject.SetActive(enabled);
                foreach (Transform child in t)
                    recurse(child);
            }
            recurse(transform);
            return values;
        }
        public static void ResetEnabled(List<(GameObject, bool)> values) {
            foreach (var (gameObj, enabled) in values)
                gameObj.SetActive(enabled);
        }
        //static void SwapPositionRotation(SpaceShip first, SpaceShip second) {
        //    //var oldValues = SetColliders(first.transform, false);
        //    //second.transform.GetChild(2).gameObject.SetActive(false);
        //    //var p = IgnoreCollisions(first.transform, second.transform);
        //    (first.transform.position, second.transform.position) = (second.transform.position, first.transform.position);
        //    (first.transform.rotation, second.transform.rotation) = (second.transform.rotation, first.transform.rotation);            
        //    //ResetCollisions(p);
        //    //ResetColliders(oldValues);
        //    //second.transform.GetChild(2).gameObject.SetActive(true);
        //}
        public static Bounds GetBounds(Transform root, bool localRotation = false) {
            var rot = root.rotation;
            if (localRotation)
                root.rotation = Quaternion.identity;

            var wr = new Bounds(root.transform.position, Vector3.zero);
            foreach (var r in root.GetComponents<Renderer>())
                if (r.bounds.extents != Vector3.zero)
                    wr.Encapsulate(r.bounds);
            foreach (var r in root.GetComponentsInChildren<Renderer>())
                if (r.bounds.extents != Vector3.zero)
                    wr.Encapsulate(r.bounds);

            if (localRotation)
                root.rotation = rot;

            return wr;

        }
        #endregion
        #region Swapping
        //[HarmonyPatch(typeof(HPBarControl), nameof(HPBarControl.SetName))]
        //[HarmonyPostfix]
        //static void HPBarControl_SetName_FixAIDummy(HPBarControl __instance, Text ___textName, Transform ___owner) {
        //    var aic = __instance.GetComponent<AIDummyControl>();
        //    if (aic == null)
        //        return;
        //    ___textName.color = Color.white;
        //    ___textName.text = aic.Char.Name();
        //    ___owner.name = ___textName.text;
        //    ___textName.enabled = true;
        //}

        //[HarmonyPatch(typeof(AIControl), nameof(AIControl.ConfigureAI))]
        //[HarmonyPrefix]
        //static bool AIControl_ConfigureAI_Dummy(AICharacter ___Char) {
        //    if (___Char is AIDummyCharacter)
        //        return false;
        //    return true;
        //}
        //[HarmonyPatch(typeof(AIControl), "Update")]
        //[HarmonyPrefix]
        //static bool AIControl_Update_Dummy(AICharacter ___Char) {
        //    if (___Char is AIDummyCharacter)
        //        return false;
        //    return true;
        //}

        void TrySwap() {
            var playerShip = GameObject.FindGameObjectWithTag("Player");
            var ss = playerShip.GetComponent<SpaceShip>();
            var pc = playerShip.GetComponent<PlayerControl>();
            var target = pc.target;

            if (target == null)
                return;

            if (TryReplaceDriftingObject(target, out var shipGO)) {
                target = shipGO.transform;
                PlayerControl.inst.target = target;
                PlayerUIControl.inst.UpdateTargetInfo();
            }

            var ship = target.GetComponent<SpaceShip>();
            if (ship != null) {
                SwapShips(ss, ship, true);
                return;
            }
        }
        bool TryReplaceDriftingObject(Transform transform, out GameObject instance) {
            var drift = transform.GetComponent<DriftingObjectControl>();
            if (drift == null || drift.driftObj == null || drift.driftObj.itemType != 4) {
                instance = default;
                return false;
            }

            var rb = drift.GetComponent<Rigidbody>();

            var newLoadoutId = drift.driftObj.shipLoadoutID;
            if (newLoadoutId == -1) {
                var ci = new CargoItem() { itemID = drift.driftObj.itemID };
                newLoadoutId = GameData.data.NewShipLoadout(ci);
            }
            var newData = GameData.data.GetShipLoadout(newLoadoutId);
            GameData.data.DeleteShipLoadout(newLoadoutId);
            var dummyCaptain = new UncontrolledShipCharacter(newData);

            dummyCaptain.Move(LocationSystem.Instance.GetLocation(LocationType.Sector, GameData.data.currentSectorIndex));

            var physics = (rb.position, rb.rotation, rb.velocity, rb.angularVelocity);
            drift.RemoveFromMemory();
            UnityEngine.Object.Destroy(drift.transform.gameObject);

            instance = dummyCaptain.Spawn(true);
            instance.SetActive(true);
            rb = instance.GetComponent<Rigidbody>();
            (instance.transform.position, instance.transform.rotation, rb.velocity, rb.angularVelocity) = physics;
            return true;

            //var collectible = target.GetComponent<Collectible>();
            //collectible.itemID = oldData.shipModelID;
            //collectible.rarity = oldModel.rarity;
            //driftObj.itemID = oldData.shipModelID;
            //driftObj.rarity = oldModel.rarity;
            //AdjustSpaceshipType(collectible);
            //collectible.AdjustName();
        }
        static FieldInfo __PlayerControl_puc = typeof(PlayerControl).GetField("puc", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        static FieldInfo __SpaceShip_shipModelGO = typeof(SpaceShip).GetField("shipModelGO", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        static void MemberwiseCloneTo<T>(T dst, T src) {
            foreach (var field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                field.SetValue(dst, field.GetValue(src));
        }
        static List<(FieldInfo, object)> SaveValues<T>(T src, bool valueTypesOnly) {
            List<(FieldInfo, object)> values = new List<(FieldInfo, object)>();
            foreach (var field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                if (!valueTypesOnly || field.FieldType.IsValueType)
                    values.Add((field, field.GetValue(src)));
            return values;
        }
        static void LoadValues<T>(T dst, List<(FieldInfo, object)> values) {
            foreach (var (field, value) in values)
                field.SetValue(dst, value);
        }
        #endregion
        #region Swap Method
        [HarmonyPatch(typeof(AE_Spotlight), MethodType.Constructor, new Type[] { typeof(SpaceShip) })]
        [HarmonyPostfix]
        static void AE_Spotlight__ctor_SaveState(SpaceShip ss, ref bool ___saveState) {
            ___saveState = true;
        }

        [HarmonyPatch(typeof(HPBarControl), nameof(HPBarControl.SetName))]
        [HarmonyPrefix]
        static bool HPBarControl_SetName_AIDummy(Transform ___owner, HPBarControl __instance, UnityEngine.UI.Text ___textName) {
            var aic = ___owner.GetComponent<AIDummyControl>();
            if (aic != null) {
                //aic.ResizeMinimapIcon();
                aic.SetLabelName(___textName, owner: ___owner);
                return false;
            }
            return true;
        }
        public static void SwapPlayerShip(SpaceShip target, bool activate) => SwapShips(PlayerControl.inst.GetSpaceShip, target, activate);
        static void SaveAEStates(SpaceShip ship) {
            if (ship.activeEquips != null)
                foreach (var o in ship.activeEquips.Where(o => o.active))
                    o.SaveState();
        }
        static void SaveShipData(SpaceShip spaceShip) {
            if (spaceShip.GetComponent<PlayerControl>()) {
                GameData.data.spaceShipData = spaceShip.shipData;
                GameData.data.credits = spaceShip.cs.credits;
            }
            else {
                var aic = spaceShip.GetComponent<AIControl>();
                aic.Char.shipData = spaceShip.shipData;
                aic.Char.credits = spaceShip.cs.credits;
            }
            spaceShip.shipData.cargo = spaceShip.cs.cargo;
            spaceShip.shipData.HPstatus = spaceShip.currHP;
            spaceShip.shipData.HPbase = spaceShip.baseHP;
            spaceShip.shipData.energyStatus = spaceShip.stats.currEnergy;
            spaceShip.shipData.energyBase = spaceShip.stats.baseEnergy;
            spaceShip.shipData.shieldStatus = spaceShip.stats.currShield / spaceShip.energyMmt.valueMod(1);
            spaceShip.shipData.shieldBase = spaceShip.stats.baseShield;
            spaceShip.shipData.energyLevels = spaceShip.energyMmt.level;
            if (spaceShip.fluxChargeSys != null)
                spaceShip.shipData.powerChargeCount = spaceShip.fluxChargeSys.GetCharges();
        }
        static void LoadShipData(SpaceShip spaceShip) {
            if (spaceShip.GetComponent<PlayerControl>()) {
                spaceShip.shipData = GameData.data.spaceShipData;
                spaceShip.cs.credits = GameData.data.credits;
            }
            else {
                var aic = spaceShip.GetComponent<AIControl>();
                spaceShip.shipData = aic.Char.shipData;
                spaceShip.cs.credits = aic.Char.credits;
                if (aic is AIDummyControl usc)
                    ((UncontrolledShipCharacter)usc.Char).Refresh();
            }
            spaceShip.cs.cargo = spaceShip.shipData.cargo;
            spaceShip.currHP = spaceShip.shipData.HPstatus;
            spaceShip.baseHP = spaceShip.shipData.HPbase;
            spaceShip.stats.currEnergy = spaceShip.shipData.energyStatus;
            spaceShip.stats.baseEnergy = spaceShip.shipData.energyBase;
            spaceShip.stats.currShield = spaceShip.shipData.shieldStatus * spaceShip.energyMmt.valueMod(1);
            spaceShip.stats.baseShield = spaceShip.shipData.shieldBase;
            spaceShip.energyMmt.level = spaceShip.shipData.energyLevels;
            if (spaceShip.fluxChargeSys != null)
                spaceShip.fluxChargeSys.SetCharges(spaceShip.shipData.powerChargeCount);
        }

        static void SwapShipData(SpaceShip first, SpaceShip second) {
            (first.shipData, second.shipData) = (second.shipData, first.shipData); //swap shipdata -> cargos are swapped in shipdata, not in ss
            (first.shipData.cargo, second.shipData.cargo) = (second.shipData.cargo, first.shipData.cargo); //swap cargos back -> cargos now normal in shipdata and ss
            SwapStationInventory(first, second); //swap non-station inventory on shipdata and ss
            (first.attackDrones, second.attackDrones) = SwapLists(first, first.attackDrones, second, second.attackDrones, (go, owner) => go.GetComponent<Drone>().owner = owner.transform);
            (first.repairDrones, second.repairDrones) = SwapLists(first, first.repairDrones, second, second.repairDrones, (go, owner) => go.GetComponent<Drone>().owner = owner.transform);
            (first.changeActiveEquipment, second.changeActiveEquipment) = (true, true);

            SpaceShipData _first, _second;
            if (first.GetComponent<PlayerControl>())
                _first = GameData.data.spaceShipData;
            else
                _first = first.GetComponent<AIControl>().Char.shipData;

            if (second.GetComponent<PlayerControl>())
                _second = GameData.data.spaceShipData;
            else
                _second = second.GetComponent<AIControl>().Char.shipData;

            if (first.GetComponent<PlayerControl>())
                GameData.data.spaceShipData = _second;
            else
                first.GetComponent<AIControl>().Char.shipData = _second;

            if (second.GetComponent<PlayerControl>())
                GameData.data.spaceShipData = _first;
            else
                second.GetComponent<AIControl>().Char.shipData = _first;
        }
        static void SwapShips(SpaceShip ss, SpaceShip target, bool activate) {
            //Prepare
            ss.gameObject.SetActive(true);
            target.gameObject.SetActive(true);
            ss.gameObject.SetActive(false);
            target.gameObject.SetActive(false);

            SaveAEStates(ss);
            SaveAEStates(target);

            SaveShipData(ss);
            SaveShipData(target);

            var oldSS = SaveValues(ss, true);
            var oldTarget = SaveValues(target, true);


            var ssVelocities = (ss.GetComponent<Rigidbody>().velocity, ss.GetComponent<Rigidbody>().angularVelocity);
            var targetVelocities = (target.GetComponent<Rigidbody>().velocity, target.GetComponent<Rigidbody>().angularVelocity);


            SwapShipData(ss, target);

            LoadValues(ss, oldTarget);
            LoadValues(target, oldSS);

            var ssMgo = (Transform)__SpaceShip_shipModelGO.GetValue(ss);
            var targetMgo = (Transform)__SpaceShip_shipModelGO.GetValue(target);

            (ss.transform.position, target.transform.position) = (target.transform.position, ss.transform.position);

            (ss.transform.rotation, target.transform.rotation) = (target.transform.rotation, ss.transform.rotation);
            var (ssZ, targetZ) = (Quaternion.Euler(0, 0, ssMgo.transform.rotation.eulerAngles.z - targetMgo.transform.rotation.eulerAngles.z), Quaternion.Euler(0, 0, targetMgo.transform.rotation.eulerAngles.z - ssMgo.transform.rotation.eulerAngles.z));
            ssMgo.transform.rotation *= targetZ;
            targetMgo.transform.rotation *= ssZ;

            ss.changeActiveEquipment = true;
            target.changeActiveEquipment = true;

            if (activate) {
                ss.gameObject.SetActive(true);
                target.gameObject.SetActive(true);
            }

            UpdateShip(ss);
            UpdateShip(target);

            void AddDelegate(SpaceShip ship, (Vector3, Vector3) velocities) {
                var del = ship.gameObject.AddComponent<Delegator>();
                del.OnStart = () => {
                    LoadShipData(ship);
                    ship.VerifyShipCargoAndEquipment();

                    ship.CallUpdateBar();
                    var aidc = ship.GetComponent<AIDummyControl>();
                    if (aidc != null)
                        aidc.ResetControl();
                    if (ship.shipData.isPlayerCharacter)
                        GameObject.FindGameObjectWithTag("SecondaryCamera").GetComponent<MinimapControl>().UpdateMap(true, false);
                    var rb = ship.GetComponent<Rigidbody>();
                    (rb.velocity, rb.angularVelocity) = velocities;
                    ship.CallUpdateBar();
                    return true;
                };
            }

            AddDelegate(ss, targetVelocities);
            AddDelegate(target, ssVelocities);
            PlayerControl.inst.SetTarget(target.transform);
        }
        public static void PlayerBecomeShip(SpaceShip target, out AICharacter aichar) {
            //Prepare
            SpaceShip ss = PlayerControl.inst.GetSpaceShip;
            aichar = target.GetComponent<AIControl>().Char;

            target.gameObject.SetActive(false);
            SaveAEStates(ss);
            SaveAEStates(target);

            SaveShipData(ss);
            SaveShipData(target);

            var oldTarget = SaveValues(target, true);
            var targetVelocities = (target.GetComponent<Rigidbody>().velocity, target.GetComponent<Rigidbody>().angularVelocity);

            SwapShipData(ss, target);
            LoadValues(ss, oldTarget);

            var ssMgo = (Transform)__SpaceShip_shipModelGO.GetValue(ss);
            var targetMgo = (Transform)__SpaceShip_shipModelGO.GetValue(target);

            (ss.transform.position, target.transform.position) = (target.transform.position, ss.transform.position);
            (ss.transform.rotation, target.transform.rotation) = (target.transform.rotation, ss.transform.rotation);
            var (ssZ, targetZ) = (Quaternion.Euler(0, 0, ssMgo.transform.rotation.eulerAngles.z - targetMgo.transform.rotation.eulerAngles.z), Quaternion.Euler(0, 0, targetMgo.transform.rotation.eulerAngles.z - ssMgo.transform.rotation.eulerAngles.z));
            ssMgo.transform.rotation *= targetZ;
            targetMgo.transform.rotation *= ssZ;
            ss.changeActiveEquipment = true;

            UpdateShip(ss);
            UnityEngine.Object.Destroy(target.gameObject);

            void AddDelegate(SpaceShip ship, (Vector3, Vector3) velocities) {
                var del = ship.gameObject.AddComponent<Delegator>();
                del.OnStart = () => {
                    ship.CallUpdateBar();
                    var aidc = ship.GetComponent<AIDummyControl>();
                    if (aidc != null)
                        aidc.ResetControl();
                    if (ship.shipData.isPlayerCharacter) {
                        GameObject.FindGameObjectWithTag("SecondaryCamera").GetComponent<MinimapControl>().UpdateMap(true, false);
                    }
                    var rb = ship.GetComponent<Rigidbody>();
                    (rb.velocity, rb.angularVelocity) = velocities;
                    return false;
                };
                del.OnUpdate = () => {
                    return false;
                };
                del.OnLateUpdate = () => {
                    LoadShipData(ship);
                    ship.CallUpdateBar();
                    return true;
                };
            }
            AddDelegate(ss, targetVelocities);
            PlayerControl.inst.SetTarget(target.transform);
        }
        //static FieldInfo inventory = typeof(ShipInfo).GetField("inventory", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        //static Inventory inventory_get(ShipInfo obj) => (Inventory)inventory.GetValue(obj);

        static void SwapStationInventory(SpaceShip first, SpaceShip second) {
            var firstData = first.shipData;
            var secondData = second.shipData;

            var newFirst = firstData.cargo.Where(o => o.stockStationID != -1).ToList();
            newFirst.AddRange(secondData.cargo.Where(o => o.stockStationID == -1));

            var newSecond = secondData.cargo.Where(o => o.stockStationID != -1).ToList();
            newSecond.AddRange(firstData.cargo.Where(o => o.stockStationID == -1));

            firstData.cargo = newFirst;
            first.cs.cargo = newFirst;

            secondData.cargo = newSecond;
            second.cs.cargo = newSecond;
        }
        static (List<T> first, List<T> second) SwapLists<T, U>(U argFirst, List<T> first, U argSecond, List<T> second, Action<T, U> func) {
            if (first != null)
                foreach (var o in first)
                    func(o, argSecond);
            if (second != null)
                foreach (var o in second)
                    func(o, argFirst);
            return (second, first);
        }
        static void UpdateShip(SpaceShip ship) {
            var pc = ship.GetComponent<PlayerControl>();
            ship.shipData.isPlayerCharacter = pc != null;

            var tr = ship.transform.Find("Weapons");
            int childCount = tr.childCount;
            for (int i = 0; i < childCount; i++)
                UnityEngine.Object.DestroyImmediate(tr.GetChild(0).gameObject);

            if (pc == null) {
                ship.ValidateSpaceShipData(2);
                ship.GetShipModel();
                ship.GetComponent<AIControl>().InstallWeapons();
            }
            else {
                ship.ValidateSpaceShipData(1);
                ship.GetShipModel();
                pc.InstallWeapons();
                ((PlayerUIControl)__PlayerControl_puc.GetValue(pc)).UpdateUI();
            }
            ship.UpdateWeaponTurretStats();
        }
        #endregion
    }
    class Delegator : MonoBehaviour {
        public Func<bool> OnStart;
        public Func<bool> OnUpdate;
        public Func<bool> OnFixedUpdate;
        public Func<bool> OnLateUpdate;
        public Func<bool> OnDestroyed;
        void Start() {
            if (OnStart?.Invoke() ?? false)
                Destroy(this);
        }
        void Update() {
            if (OnUpdate?.Invoke() ?? false)
                Destroy(this);
        }
        void FixedUpdate() {
            if (OnFixedUpdate?.Invoke() ?? false)
                Destroy(this);
        }
        void LateUpdate() {
            if (OnLateUpdate?.Invoke() ?? false)
                Destroy(this);
        }
        void OnDestroy() {
            if (this.enabled) {
                this.enabled = false;
                OnDestroyed?.Invoke();
            }
        }
    }
}
