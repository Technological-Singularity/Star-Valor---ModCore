using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

namespace Charon.StarValor.ModCore {
    [HasPatches]
    public abstract class AIDummyControl : AIControl {
        static MethodInfo __AIControl_Awake = typeof(AIControl).GetMethod("Awake", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyPatch(typeof(AIControl), nameof(AIControl.RequestDialog))]
        [HarmonyPrefix]
        static bool AIControl_RequestDialog_Override(Transform requester, ref bool __result, AIControl __instance) {
            if (__instance is AIDummyControl aidc) {
                __result = aidc.RequestDialog(requester);
                return false;
            }
            return true;
        }
        public void SetLabelName(Text text, Transform owner = null) {
            var model = Char.shipData.GetShipModelData();
            var formattedName = "<size=14>";
            switch (model.rarity) {
                case 0:
                    formattedName += ColorSys.rarity0;
                    break;
                case 1:
                    formattedName += ColorSys.rarity1;
                    break;
                case 2:
                    formattedName += ColorSys.rarity2;
                    break;
                case 3:
                    formattedName += ColorSys.rarity3;
                    break;
                case 4:
                    formattedName += ColorSys.rarity4;
                    break;
                default:
                    break;
            }
            var charname = Char.Name();
            if (owner != null)
                owner.transform.name = charname;
            formattedName = GameOptions.GetColorblindTier(model.rarity) + formattedName;
            formattedName += charname + "</color></size>";
            text.text = formattedName;
        }
        protected virtual void Awake() => __AIControl_Awake.Invoke(this, null);
        protected override void Start() { }
        protected virtual void Update() { }
        protected virtual void FixedUpdate() { }
        protected virtual void OnDestroy() { }
        protected virtual void LateUpdate() { }
        public virtual void ResetControl() { }
        protected virtual void OnCollisionEnter(Collision collision) { }
        public bool RequestDialogue(Transform requester) {
            if (hailing)
                return false;
            hailing = true;
            return OnRequestDialogue(requester);
        }
        protected virtual bool OnRequestDialogue(Transform requester) { 
            hailing = false; 
            return false; 
        }

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
    }
}
