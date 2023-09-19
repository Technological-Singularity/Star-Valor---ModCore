using HarmonyLib;
using UnityEngine;
using Charon.StarValor.ModCore;

namespace Charon.StarValor.Excursion {
    [HasPatches]
    public class UncontrollShipControl : AIDummyControl {
        [HarmonyPatch(typeof(Entity), nameof(Entity.ApplyImpactDamage))]
        [HarmonyPrefix]
        static bool Entity_ApplyImpactDamage_DontHurtCollectible(float dmg, TCritical crit, DamageType dmgType, Vector3 point, Transform dmgDealer, WeaponImpact impact, Vector3 lookPosition, Entity __instance) {
            if (dmgDealer.GetComponent<PlayerControl>() == null)
                return true;

            var cs = dmgDealer.GetComponent<CargoSystem>();
            if (cs == null)
                return true;

            var control = __instance.GetComponent<UncontrollShipControl>();
            if (control == null)
                return true;

            return !control.TryBeCollected(dmgDealer);
        }

        bool detected;
        GameObject minimapIcon;
        bool collectInvalid = false;
        Transform queuedCollectionAttempt;
        //GameObject _labelBar;
        //Text text;
        //GameObject LabelBar {
        //    get {
        //        if (_labelBar == null) {
        //            var canvas = GameObject.FindGameObjectWithTag("MainCanvas").transform.GetChild(0);
        //            _labelBar = Instantiate(ObjManager.GetObj("LabelBar"), canvas);
        //            var hpb = _labelBar.GetComponent<HPBarControl>();
        //            hpb.verticalSpace = 20f;
        //            hpb.owner = transform;
        //            hpb.PositionBar();
        //            text = _labelBar.transform.GetChild(0).Find("TextName").GetComponent<Text>();
        //            Refresh();
        //        }
        //        return _labelBar;
        //    }
        //}
        public void ResizeMinimapIcon() {
            var model = Char.shipData.GetShipModelData();
            minimapIcon.transform.localScale = new Vector3(15f + model.sizeScale, 15f + model.sizeScale, 1f + model.sizeScale);
        }
        public override void ResetControl() {
            detected = false;
            count = 0;

            GetComponent<SpaceShip>().ShowThrusterFX(0);
        }
        protected override void Start() {
            if (minimapIcon != null)
                return;

            base.Start();
            var basis = ItemDB.GetItem(5).gameObj;
            var minimapGO = basis.transform.Find("MinimapIcon").gameObject;
            minimapIcon = Instantiate(minimapGO, transform);

            //_labelBar = LabelBar;
            ResetControl();
        }
        protected override void Update() {
            this.count -= Time.deltaTime;
            if (count > 0)
                return;
            count = 0.25f;

            var range = PChar.LootScannerRange(true);
            bool visible = (Vector3.SqrMagnitude(Player.transform.position - transform.position) <= range * range);
            ToggleVisibility(visible);
        }
        protected override void OnDestroy() {
            Destroy(minimapIcon);
            //if (_labelBar != null)
            //    Destroy(_labelBar);
        }
        void ToggleVisibility(bool enabled) {
            minimapIcon.SetActive(enabled);
            //if (LabelBar != null)
            //    LabelBar.SetActive(enabled);
            if (enabled && !detected) {
                detected = true;
                ShowDetectionWarning();
            }
        }
        void ShowDetectionWarning() {
            var model = transform.GetComponent<SpaceShip>().shipData.GetShipModelData();
            if (GameOptions.PlayLootWarningSound(4, model.id))
                SoundSys.PlaySound(27, true);
            SideInfo.AddMsg(Lang.Get(6, 102, Char.Name()));
        }
        protected override void OnCollisionEnter(Collision collision) {
            var collider = collision.collider;
            Transform target = collider.transform;
            if (collider.CompareTag("Collider"))
                target = collider.GetComponent<ColliderControl>().ownerEntity.transform;
            queuedCollectionAttempt = target;
        }
        protected override void LateUpdate() {
            if (queuedCollectionAttempt != null) {
                var target = queuedCollectionAttempt;
                queuedCollectionAttempt = null;
                if (!TryBeCollected(target))
                    TryCollect(target);
            }
        }
        public bool TryBeCollected(Transform collector) {
            if (collectInvalid)
                return false;

            var usc = (UncontrolledShipCharacter)Char;
            var model = usc.shipData.GetShipModelData();

            //Try station first
            if (collector.CompareTag("Station")) {
                var dock = collector.GetComponentInChildren<DockingControl>();
                if (dock != null && PChar.GetRepRank(dock.station.factionIndex) >= 0) {
                    SoundSys.PlaySound(11, true);
                    SideInfo.AddMsg(Lang.Get(6, 9, Char.shipData.GetShipModelData().modelName));

                    usc.Destroy();
                    int loadoutId = GameData.data.NewShipLoadout(null);
                    GameData.data.SetShipLoadout(usc.shipData, loadoutId);
                    PlayerControl.inst.GetComponent<CargoSystem>().StoreItem(4, model.id, model.rarity, 1, 0, -1, dock.station.id, loadoutId);
                    Destroy(gameObject);
                    collectInvalid = true;
                    return true;
                }
            }

            var collectorSS = collector.GetComponent<SpaceShip>();
            if (collectorSS == null)
                return false;

            //Try hangar space
            var fleetSpace = model.fleetSpaceOcupied;

            if ((int)model.shipClass <= WorkshopInventoryHandler.GetMaxWorkshopSize(collector) && fleetSpace <= WorkshopInventoryHandler.GetUsedWorkshopSpace(collector)) {
                if (collector == PlayerControl.inst.transform) {
                    SoundSys.PlaySound(11, true);
                    SideInfo.AddMsg(model.modelName + " received (stored in workshop)");

                    usc.Move(LocationSystem.Instance.GetLocation(LocationType.Player, 0));
                    ++Inventory.instance.newFleetCount;
                    Destroy(gameObject);
                    collectInvalid = true;
                    return true;
                }
            }

            //Try cargo space
            if (model.spaceOcupied <= collectorSS.cs.FreeSpace(false)) {
                if (collector == PlayerControl.inst.transform) {
                    SoundSys.PlaySound(11, true);
                    SideInfo.AddMsg(model.modelName + " received (stored in cargo)");

                    usc.Destroy();
                    int loadoutId = GameData.data.NewShipLoadout(null);
                    GameData.data.SetShipLoadout(usc.shipData, loadoutId);
                    var oldCount = Inventory.instance.newItemCount;
                    collectorSS.cs.StoreItem(4, model.id, model.rarity, 1, 0, -1, -1, loadoutId);
                    var newCount = Inventory.instance.newItemCount;
                    if (newCount - oldCount != 1) {
                        Inventory.instance.newItemCount = oldCount + 1;
                        Inventory.instance.UpdateNewItemsCount();
                    }
                    if (Inventory.instance.isOpen) {
                        Inventory.instance.RefreshIfOpen(null, false);
                        Inventory.instance.DeselectItems();
                        WorkshopInventoryHandler.Instance.ResetButtons();
                        Inventory.instance.LoadItems();
                    }

                    Destroy(gameObject);
                    collectInvalid = true;
                    return true;
                }
            }
            return false;
        }
        bool TryCollect(Transform collectee) {
            var usc = (UncontrolledShipCharacter)Char;
            var targetSS = collectee.GetComponent<SpaceShip>();
            if (targetSS == null)
                return false;

            var model = targetSS.shipData.GetShipModelData();

            var fleetSpace = model.fleetSpaceOcupied;
            if ((int)model.shipClass <= WorkshopInventoryHandler.GetMaxWorkshopSize(transform) && fleetSpace <= WorkshopInventoryHandler.GetUsedWorkshopSpace(transform)) {
                if (collectee == PlayerControl.inst.transform) {
                    SoundSys.PlaySound(11, true);
                    SideInfo.AddMsg(model.modelName + " received (stored in workshop)");

                    var del = collectee.gameObject.AddComponent<Delegator>();
                    del.OnStart = () => {
                        //Plugin.PlayerBecomeShip(ss, out _);
                        Plugin.SwapPlayerShip(ss, true);
                        usc.Move(LocationSystem.Instance.GetLocation(LocationType.Player, 0));
                        Inventory.instance.LoadItems();
                        ShipEnhancement.inst.RefreshScreen();
                        return true;
                    };

                    ++Inventory.instance.newFleetCount;
                    Destroy(gameObject);
                    collectInvalid = true;
                    return true;
                }
            }

            //Try cargo space
            if (model.spaceOcupied <= ss.cs.FreeSpace(false)) {
                if (collectee == PlayerControl.inst.transform) {
                    SoundSys.PlaySound(11, true);
                    SideInfo.AddMsg(model.modelName + " received (stored in cargo)");

                    var del = collectee.gameObject.AddComponent<Delegator>();
                    del.OnStart = () => {
                        //Plugin.PlayerBecomeShip(ss, out _);
                        Plugin.SwapPlayerShip(ss, true);
                        usc.Destroy();
                        if (Inventory.instance.isOpen) {
                            Inventory.instance.RefreshIfOpen(null, false);
                            Inventory.instance.DeselectItems();
                            WorkshopInventoryHandler.Instance.ResetButtons();
                        }
                        Inventory.instance.LoadItems();
                        ShipEnhancement.inst.RefreshScreen();
                        return true;
                    };


                    int loadoutId = GameData.data.NewShipLoadout(null);
                    GameData.data.SetShipLoadout(targetSS.shipData, loadoutId);
                    var oldCount = Inventory.instance.newItemCount;
                    ss.cs.StoreItem(4, model.id, model.rarity, 1, 0, -1, -1, loadoutId);
                    var newCount = Inventory.instance.newItemCount;
                    if (newCount - oldCount != 1) {
                        Inventory.instance.newItemCount = oldCount + 1;
                        Inventory.instance.UpdateNewItemsCount();
                    }
                    Destroy(gameObject);
                    collectInvalid = true;
                    return true;
                }
            }
            return false;
        }
        protected override bool OnRequestDialogue(Transform requester) {
            var time = 0.1f;
            Invoke("AcceptDialogRequest", time);
            Invoke("IgnoreDialogRequest", time);
            return false;
        }
        private void AcceptDialogRequest() {
            this.hailing = false;
            GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("Dialog").GetComponent<NPCDialog>().Open(this);
            GameObject.FindGameObjectWithTag("InfoPanel").GetComponent<InfoPanelControl>().HideText(false);
        }
        private void IgnoreDialogRequest() {
            this.hailing = false;
            GameObject.FindGameObjectWithTag("InfoPanel").GetComponent<InfoPanelControl>().ShowWarning("No response.", 2, false);
        }
        #region Overrides
        public override void BroadcastSignal(string msg, Transform attacker) { }
        protected override bool CanFireWeaponType(TWeapon weap) => false;
        public override float CheckCredits() => 0;
        public override void ConfigureAI() { }
        public override void Die(float playerDmgPerc) { }
        protected override void ForgetDestination() { }
        public override void ForgetTarget(bool checkTargetLastPosition) { }
        protected override bool IgnoreSpaceshipObstacles() => true;
        public override bool PayCost(float value, PaymentType paymentType) => false;
        protected override void SetActions() { }
        public override void SetNewTarget(Transform newTarget, bool startedFight) { }
        protected override void TravelToDestination(float maxSpeed) { }
        protected override void VerifyTargetStatus() { }
        #endregion
    }
}
