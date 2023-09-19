using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace Charon.StarValor.AggressiveProjectiles {

    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    [BepInProcess("Star Valor.exe")]
    public partial class Plugin : BaseUnityPlugin {
        public const string pluginGuid = "starvalor.charon.aggressive_projectiles";
        public const string pluginName = "Charon - Minifix - Aggressive Projectiles";
        public const string pluginVersion = "0.0.0.0";

        //static MethodInfo weapon_turret_ClearLOF = typeof(WeaponTurret).GetMethod("ClearLOF", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        //static bool WeaponTurret_ClearLOF(WeaponTurret instance, bool toTargetOnly) => (bool)weapon_turret_ClearLOF.Invoke(instance, new object[] { toTargetOnly });
        //static MethodInfo weapon_turret_CanFireAgainst = typeof(WeaponTurret).GetMethod("CanFireAgainst", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        //static float WeaponTurret_CanFireAgainst(WeaponTurret instance, Transform targetTrans) => (float)weapon_turret_CanFireAgainst.Invoke(instance, new object[] { targetTrans });

        static MethodInfo ai_control_ClearLOF = typeof(AIControl).GetMethod("ClearLOF", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        static bool AIControl_ClearLOF(AIControl instance, bool toTargetOnly) => (bool)ai_control_ClearLOF.Invoke(instance, new object[] { toTargetOnly });

        public static BepInEx.Logging.ManualLogSource Log;

        public void Awake() {
            Log = Logger;
            Harmony.CreateAndPatchAll(typeof(Plugin));
        }

        #region Helper functions
        public static List<(Transform transform, int layer)> SetLayers(Transform transform, int layerMask, int newLayer) {
            transform = GetSSTransform(transform);
            var list = new List<(Transform, int)>();
            void _setLayers(Transform curTransform) {
                var thisLayer = curTransform.gameObject.layer;
                if ((layerMask >> thisLayer & 1) == 1) {
                    list.Add((curTransform, thisLayer));
                    curTransform.gameObject.layer = newLayer;
                }
                foreach (Transform child in curTransform)
                    _setLayers(child);
            }
            _setLayers(transform);
            return list;
        }
        public static void ResetLayers(List<(Transform, int)> list) {
            foreach (var (transform, layer) in list)
                transform.gameObject.layer = layer;
        }
        public static bool InvisibleRaycast(Transform ignore, Vector3 source, Vector3 direction, float range, int layerMask, out RaycastHit hitInfo) {
            var oldLayers = SetLayers(ignore, layerMask, 2);
            var wasHit = Physics.Raycast(source, direction, out hitInfo, range, layerMask, QueryTriggerInteraction.Ignore);
            ResetLayers(oldLayers);
            return wasHit;
        }
        public static RaycastHit[] InvisibleRaycastAll(Transform ignore, Vector3 source, Vector3 direction, float range, int layerMask) {
            var oldLayers = SetLayers(ignore, layerMask, 2);
            var hits = Physics.RaycastAll(source, direction, range, layerMask, QueryTriggerInteraction.Ignore);
            ResetLayers(oldLayers);
            return hits;
        }
        public static bool InvisibleRaycastSelective(Transform ignore, Transform target, Vector3 source, Vector3 direction, int layerMask, out RaycastHit hitInfo, float range = Mathf.Infinity) {
            var hits = Physics.RaycastAll(source, direction, range, layerMask, QueryTriggerInteraction.Ignore);
            float dst = float.MaxValue;
            hitInfo = default;
            foreach (var hit in hits) {
                if (GetSSTransform(hit.transform) == GetSSTransform(target.transform) && hit.distance < dst) {
                    dst = hit.distance;
                    hitInfo = hit;
                }
            }
            return dst != float.MaxValue;
        }
        public static Transform GetSSTransform(Transform transform) {
            var tr = transform;
            for (int i = 0; i < 5; ++i) {
                if (tr.GetComponent<SpaceShip>() != null || tr.parent == null)
                    return tr;
                tr = tr.parent;
            }
            return transform;
        }
        public static bool GetClosestColliderPosition(Vector3 source, Transform transform, out Vector3 position) {
            transform = GetSSTransform(transform);

            void recurseChildren(Transform parent, ref float closestDist, ref Vector3 closestPoint) {
                var collider = parent.GetComponent<Collider>();
                if (collider != null) {
                    Vector3 candidate = collider.ClosestPoint(source);
                    float candidateDist = (candidate - source).sqrMagnitude;
                    if (candidateDist < closestDist) {
                        closestPoint = candidate;
                        closestDist = candidateDist;
                    }
                }
                foreach (Transform child in parent)
                    recurseChildren(child, ref closestDist, ref closestPoint);
            }

            Vector3 hitPos = Vector3.zero;
            float hitRange = float.MaxValue;
            recurseChildren(transform, ref hitRange, ref hitPos);
            position = hitPos;
            return position != Vector3.zero;
        }
        #endregion
        #region Battle computer
        [HarmonyPatch(typeof(PlayerControl), "ShowAimObject")]
        [HarmonyPrefix]
        public static bool PlayerControl_ShowAimObject(PlayerControl __instance, SpaceShip ___ss) {
            var control = __instance.GetComponent<AimObjectControl>();
            if (/*__instance.target != null &&*/ ___ss.stats.hasAimObj) {
                if (control == null)
                    control = __instance.gameObject.AddComponent<AimObjectControl>();
                if (!control.enabled || ___ss != control.SpaceShip /*|| control.Target != __instance.target*/) {
                    if (___ss != control.SpaceShip)
                        control.Clear();
                    control.Initialize(__instance, ___ss/*, __instance.target*/);
                    control.enabled = true;
                }
            }
            else if (control != null) {
                control.enabled = false;
            }
            return false;
        }
        #endregion
        #region Missiles
        [HarmonyPatch(typeof(ProjectileControl), "FixedUpdate")]
        [HarmonyPrefix]
        public static bool Projectile_FixedUpdate_Tracking(ProjectileControl __instance) {
            if (__instance.homing)
                return false;
            return true;
        }
        #endregion
        #region AI control
        [HarmonyPatch(typeof(AIControl), "AimAtTarget")]
        [HarmonyPrefix]
        public static bool AIControl_AimAtTarget(ref bool __result, AIControl __instance, GameObject ___aimTarget, float ___aimErrorX, float ___aimErrorZ, bool ___firingBeamWeapon, Rigidbody ___rb, SpaceShip ___ss, ref Quaternion ___targetRotation) {
            if (__instance.target == null) {
                __result = false;
                return false;
            }
            if (__instance.target != null && __instance.target.position == __instance.transform.position) {
                __result = true;
                return false;
            }

            Vector3 prediction;
            var error = new Vector3(___aimErrorX, 0, ___aimErrorZ);

            if (___firingBeamWeapon) {
                prediction = __instance.target.position + error;
            }
            else {
                var targetPredictor = __instance.target.GetComponent<TargetPredictor>();
                if (targetPredictor == null) {
                    targetPredictor = __instance.target.gameObject.AddComponent<TargetPredictor>();
                    targetPredictor.enabled = true;
                }

                var thisPredictor = __instance.GetComponent<TargetPredictor>();
                if (thisPredictor == null) {
                    thisPredictor = __instance.gameObject.AddComponent<TargetPredictor>();
                    thisPredictor.enabled = true;
                }

                var (targetPos, targetVel, _) = targetPredictor.State;
                prediction = thisPredictor.Predict_OneShot(targetPos + error, targetVel, __instance.currWeaponSpeed, out _);
                if (prediction == Vector3.zero) {
                    var d = (thisPredictor.State.pos - targetPos).magnitude / __instance.currWeaponSpeed;
                    prediction = __instance.target.position + d * (__instance.target.GetComponent<Rigidbody>().velocity - targetVel);
                }
                else {
                    prediction = targetPos + Vector3.Dot(targetPos - thisPredictor.State.pos, prediction) * prediction;
                }
            }

            var relP = ___ss.transform.position - prediction;
            var hits = Physics.RaycastAll(___ss.transform.position, relP, 2 * relP.magnitude, 1 << __instance.target.gameObject.layer, QueryTriggerInteraction.Ignore);
            bool rayHit = false;
            foreach (var hit in hits) {
                if (__instance.target == hit.transform || (hit.transform.CompareTag("Collider") && __instance.target == hit.transform.GetComponent<ColliderControl>().ownerEntity.transform)) {
                    prediction = hit.point;
                    rayHit = true;
                    break;
                }
            }

            ___aimTarget.transform.position = prediction;
            ___targetRotation = Quaternion.LookRotation(prediction - __instance.transform.position);
            ___ss.Turn(___targetRotation);

            var angleDeltaActual = Quaternion.Angle(__instance.transform.rotation, ___targetRotation);
            __result = (rayHit || angleDeltaActual <= 20f) && AIControl_ClearLOF(__instance, angleDeltaActual > 10f);

            return false;
        }
        #endregion

        //const int targetLayerMask = (1 << 8) | (1 << 9) | (1 << 10) | (1 << 13) | (1 << 14) | (1 << 16); //these are the objects that can occlude a shot

        //[HarmonyPatch(typeof(WeaponTurret), "FindTarget")]
        //[HarmonyPrefix]
        //public static void FindTarget(Transform ___parentShipTrans, Transform ___tf, ref List<ScanObject> objs, bool smallObject) {
        //    //This fix was designed to fix the Taurus laser targeting - it needs to be fixed so it doesn't stop e.g. firing at an asteroid behind another asteroid

        //    ////filter the initial list so that only objects that are currently in LOF can actualy be targeted

        //    //var oldLayers = SetLayers(___parentShipTrans, layerMask, 2); //ignore raycast layer

        //    //var newList = new List<ScanObject>();
        //    //foreach (var o in objs) {
        //    //    if (o == null || o.trans == null)
        //    //        continue;
        //    //    if (o.trans.CompareTag("Projectile")) {
        //    //        newList.Add(o);
        //    //        continue;
        //    //    }
        //    //    var relP = o.trans.position - ___tf.position;
        //    //    var wasHit = Physics.Raycast(___tf.position, relP.normalized, out var hitInfo, 2 * relP.magnitude, layerMask, QueryTriggerInteraction.Ignore);
        //    //    if (wasHit && hitInfo.transform == o.trans)
        //    //        newList.Add(o);
        //    //}

        //    //ResetLayers(oldLayers);

        //    //objs = newList;
        //}

        //[HarmonyPatch(typeof(WeaponTurret), "ClearLOF")]
        //[HarmonyPrefix]
        //public static bool WeaponTurret_ClearLOF_Optimized(ref bool __result, bool toTargetOnly, Transform ___tf, float ___desiredDistance, WeaponTurret __instance) {            

        //    var oldLayers = SetLayers(___tf, targetLayerMask, 2);
        //    var relPos = toTargetOnly ? __instance.target.position - ___tf.position : ___tf.forward;
        //    var wasHit = Physics.Raycast(___tf.position, relPos, out var hitInfo, ___desiredDistance, targetLayerMask, QueryTriggerInteraction.Ignore);

        //    ResetLayers(oldLayers);

        //    if (wasHit) {
        //        var transform = hitInfo.transform;
        //        if (transform.CompareTag("Collider"))
        //            transform = transform.GetComponent<ColliderControl>().ownerEntity.transform;
        //        __result = transform == __instance.target;
        //    }
        //    else {
        //        __result = !toTargetOnly;
        //    }
        //    return false;
        //}

        [HarmonyPatch(typeof(WeaponTurret), "FindTarget")]
        [HarmonyPrefix]
        public static void WeaponTurret_FindTarget(WeaponTurret __instance, bool smallObject, ref bool __result, ref List<ScanObject> objs, SpaceShip ___ss) {
            var extraData = ___ss.GetComponent<WeaponSlotExtraData>();
            if (extraData == null) {
                extraData = ___ss.gameObject.AddComponent<WeaponSlotExtraData>();
                extraData.Initialize(___ss);
            }
            var turret = extraData[__instance.turretIndex];
            __result = turret.SetClosestTarget(objs, smallObject);
        }

        [HarmonyPatch(typeof(WeaponTurret), "AimAtTarget")]
        [HarmonyPrefix]
        public static bool WeaponTurret_AimAtTarget(WeaponTurret __instance, ref bool __result, SpaceShip ___ss, GameObject ___aimTarget, float ___aimErrorX) {
            var extraData = ___ss.GetComponent<WeaponSlotExtraData>();
            if (extraData == null) {
                extraData = ___ss.gameObject.AddComponent<WeaponSlotExtraData>();
                extraData.Initialize(___ss);
            }
            var turret = extraData[__instance.turretIndex];
            __result = turret.AimAt(__instance.target, ___aimTarget.transform, ___aimErrorX);
            return false;
        }


        //[HarmonyPatch(typeof(WeaponTurret), "AimAtTarget")]
        //[HarmonyPrefix]
        //public static bool WeaponTurret_AimAtTarget(ref bool __result, WeaponTurret __instance, GameObject ___aimTarget, float ___aimErrorX, float ___aimErrorZ, bool ___firingBeamWeapon, Rigidbody ___rb, SpaceShip ___ss) {
        //    if (__instance.target == null) {
        //        __result = false;
        //        return false;
        //    }
        //    if (__instance.target != null && __instance.target.position == __instance.transform.position) {
        //        __result = true;
        //        return false;
        //    }

        //    Vector3 prediction;
        //    var error = new Vector3(___aimErrorX, 0, ___aimErrorZ);

        //    if (___firingBeamWeapon) {
        //        prediction = __instance.target.position + error;
        //    }
        //    else {
        //        var targetPredictor = __instance.target.GetComponent<TargetPredictor>();
        //        if (targetPredictor == null) {
        //            targetPredictor = __instance.target.gameObject.AddComponent<TargetPredictor>();
        //            targetPredictor.enabled = true;
        //        }

        //        var thisPredictor = __instance.transform.parent.parent.GetComponent<TargetPredictor>();
        //        if (thisPredictor == null) {
        //            thisPredictor = __instance.transform.parent.parent.gameObject.AddComponent<TargetPredictor>();
        //            thisPredictor.enabled = true;
        //        }

        //        var (_, parentVel, _) = thisPredictor.State;
        //        var pos = __instance.transform.position;
        //        prediction = targetPredictor.Predict_OneShot(pos + error, parentVel, __instance.currWeaponSpeed, out _);
        //        if (prediction == Vector3.zero) {
        //            var d = (targetPredictor.State.pos - pos).magnitude / __instance.currWeaponSpeed;
        //            prediction = __instance.target.position + d * (targetPredictor.State.vel - parentVel)  + error;
        //        }
        //        else {
        //            prediction = pos + Vector3.Dot(targetPredictor.State.pos - pos, prediction) * prediction;
        //        }
        //    }
        //    ___aimTarget.transform.position = prediction;
        //    var newRotationTarget = Quaternion.LookRotation(prediction - __instance.transform.position);
        //    var maxDegreesDelta = Time.deltaTime * 10f * __instance.turnSpeed;

        //    __instance.transform.rotation = Quaternion.RotateTowards(__instance.transform.rotation, newRotationTarget, maxDegreesDelta);
        //    var angleDeltaActual = Quaternion.Angle(__instance.transform.rotation, newRotationTarget);

        //    var clearLOF = WeaponTurret_ClearLOF(__instance, angleDeltaActual > 10f);
        //    __result = (angleDeltaActual <= 20f || WeaponTurret_CanFireAgainst(__instance, __instance.target.transform) >= 0) && clearLOF;

        //    return false;
        //}

        [HarmonyPatch(typeof(SpaceShip), nameof(SpaceShip.UpdateWeaponTurretStats))]
        [HarmonyPrefix]
        public static void SpaceShip_UpdateWeaponTurretStats_AppendModule(SpaceShip __instance) {
            var extraData = __instance.GetComponent<WeaponSlotExtraData>();
            if (extraData == null) {
                extraData = __instance.gameObject.AddComponent<WeaponSlotExtraData>();
                extraData.Initialize(__instance);
            }
            extraData.Refresh();
        }

        //[HarmonyPatch(typeof(WeaponTurret), "CanFireAgainst")]
        //[HarmonyPrefix]
        //public static void WeaponTurret_CanFireAgainst_FixRange(WeaponTurret __instance, Transform targetTrans, ref float ___desiredDistance, ref float __state, SpaceShip ___ss) {
        //    __state = ___desiredDistance;
        //    if (targetTrans.CompareTag("Projectile")) {
        //        var turretSlots = ___ss.GetComponent<WeaponSlotExtraData>();
        //        if (turretSlots == null) {
        //            turretSlots = ___ss.gameObject.AddComponent<WeaponSlotExtraData>();
        //            turretSlots.Initialize(___ss);
        //            turretSlots.Refresh();
        //        }
        //        ___desiredDistance = turretSlots[__instance.turretIndex].GetEffectiveRange(WeaponCategory.PointDefense, targetTrans, ___ss.rb.position, ___ss.rb.velocity);
        //    }
        //}

        //[HarmonyPatch(typeof(WeaponTurret), "CanFireAgainst")]
        //[HarmonyPostfix]
        //public static void WeaponTurret_CanFireAgainst_FixRangeCleanup(Transform targetTrans, WeaponTurret __instance, ref float __result, ref float ___desiredDistance, float __state, SpaceShip ___ss) {
        //    if (__result > 0)
        //        return;

        //    //TO DO: set desire distance properly, according to weapon debuffs. where is the correct range value calculated?

        //    var turretSlots = ___ss.GetComponent<WeaponSlotExtraData>();
        //    if (turretSlots == null) {
        //        turretSlots = ___ss.gameObject.AddComponent<WeaponSlotExtraData>();
        //        turretSlots.Initialize(___ss);
        //        turretSlots.Refresh();
        //    }

        //    var oldLayers = SetLayers(___ss.transform, targetLayerMask, 2);

        //    float found = float.MaxValue;

        //    foreach(var barrel in turretSlots[__instance.turretIndex].Barrels) {
        //        if (barrel == null)
        //            continue;

        //        var wasHit = Physics.Raycast(barrel.position, barrel.forward, out var hitInfo, ___desiredDistance, targetLayerMask, QueryTriggerInteraction.Ignore);

        //        if (wasHit && hitInfo.transform == targetTrans) {
        //            var dst = (barrel.position - hitInfo.point).magnitude;
        //            if (dst < found)
        //                found = dst;
        //        }
        //    }

        //    if (found != float.MaxValue)
        //        __result = found;

        //    ResetLayers(oldLayers);
        //    ___desiredDistance = __state;
        //}

        static void Weapon_Fire_Projectile(Transform target, bool buttonDown,
            Weapon instance, SpaceShip ss, Transform mainParent, bool isDrone, bool loaded,
            TWeapon wRef, float chargeTime, Transform weaponSlot,
            MuzzleFlash muzzleFlash, MuzzleFlash[] extraMuzzleFlash, Color muzzleFlashColor, float flashSize,
            float delayTime, TCritical critical,
            WeaponStatsModifier mods, GameObject projectileRef, Transform gunTip,
            Drone drone, Rigidbody rbShip, float damage,
            AudioSource audioS, float audioMod, AudioClip audioToPlay,
            int explodeBoostChance, float explodeBoost,
            Vector3 size, float sizeMod,
            WeaponImpact impact, int projSpeed, int range, float chargedDamageBoost,
            Transform[] extraBarrels, float delayPortion, bool alternateExtraFire,
            ref sbyte burstCount, ref float chargedFireCount, ref float currCoolDown
        ) {
            if (!isDrone && ss.energyMmt.valueMod(0) == 0f)
                return;

            if (!loaded || weaponSlot == null)
                instance.Load(true);

            if (delayTime > 0f && currCoolDown <= 0f && buttonDown)
                currCoolDown += wRef.rateOfFire * delayTime;

            if (chargeTime > 0f && !ChargedWeaponPass(instance))
                return;

            if (currCoolDown <= 0f && PayCost(instance)) {
                var dmgMod = 1f;
                var tempCritical = critical;
                if (!isDrone) {
                    dmgMod = ss.DamageMod((int)wRef.type) * mods.DamageMod((int)wRef.type);
                    if (chargedFireCount > 0f) {
                        dmgMod *= chargedDamageBoost;
                    }
                    tempCritical.chance += mods.criticalChanceBonus;
                    tempCritical.dmgBonus += mods.criticalDamageBonus;
                    if (ss.fluxChargeSys.charges > 0)
                        tempCritical = ss.stats.ApplyFluxCriticalBonuses(tempCritical);
                }

                void FireBarrel(MuzzleFlash _muzzleFlash, Transform _barrelTip) {
                    if (muzzleFlashColor != Color.black)
                        _muzzleFlash.FlashFire(muzzleFlashColor, flashSize, true);

                    GameObject gameObject = Instantiate(projectileRef, _barrelTip.position, _barrelTip.rotation);
                    var projControl = gameObject.GetComponent<ProjectileControl>();
                    projControl.target = target;
                    if (isDrone || wRef.energyCost == 0f)
                        projControl.damage = damage;
                    else
                        projControl.damage = damage * ss.energyMmt.valueMod(0);

                    if (!isDrone) {
                        projControl.damage *= dmgMod;
                        projControl.SetFFSystem(ss.ffSys);
                    }
                    else {
                        projControl.SetFFSystem(drone.ffSys);
                    }
                    audioS.PlayOneShot(audioToPlay, SoundSys.SFXvolume * (isDrone ? 0.3f : 1f) * audioMod);
                    if (explodeBoostChance > 0 && Random.Range(1, 101) <= explodeBoostChance) {
                        projControl.aoe = wRef.aoe * (1f + explodeBoost);
                        projControl.transform.localScale = size * 2f * sizeMod;
                        SoundSys.PlaySound(22, true);
                    }
                    else {
                        projControl.aoe = wRef.aoe;
                        projControl.transform.localScale = size * sizeMod;
                    }
                    projControl.critical = tempCritical;
                    projControl.impact = impact;
                    projControl.speed = (float)projSpeed;
                    if (wRef.timedFuse) {
                        projControl.timeToDestroy = GetDistanceToAimPoint(instance) / (float)projSpeed;
                        projControl.explodeOnDestroy = true;
                    }
                    else {
                        projControl.timeToDestroy = ((projSpeed != 0) ? ((float)range / (float)projSpeed) : 0f);
                        projControl.explodeOnDestroy = wRef.explodeOnMaxRange;
                    }
                    projControl.damageType = wRef.damageType;
                    projControl.owner = mainParent;
                    projControl.canHitProjectiles = wRef.canHitProjectiles;
                    projControl.piercing = wRef.piercing;
                    var projRB = gameObject.GetComponent<Rigidbody>();
                    if (wRef.compType == WeaponCompType.MineObject) {
                        projControl.timeToDestroy = 240f;
                        projRB.velocity = Vector3.zero;
                    }
                    else if (!isDrone && ss.stats.weaponStabilized) {
                        projRB.velocity = ss.ForwardVelocity();
                    }
                    else {
                        projRB.velocity = rbShip.velocity;
                    }
                    if (projControl.homing) {
                        var control = projControl.gameObject.AddComponent<ProjectileHoming>();
                        control.Initialize(mainParent, projRB, ss, instance, target, projSpeed, projControl.turnSpeed * 15); //15 is from original code
                        control.enabled = true;
                    }
                }

                IEnumerator<object> FireDelayed(MuzzleFlash _muzzleFlash, Transform _barrel, float delay) {
                    if (delay > 0)
                        yield return new WaitForSeconds(delay);
                    FireBarrel(_muzzleFlash, _barrel);
                    yield break;
                };

                if (extraBarrels != null && extraBarrels.Length > 0) {
                    float delay = alternateExtraFire ? delayPortion : 0;
                    for (int i = 0; i < extraBarrels.Length; ++i)
                        instance.StartCoroutine(FireDelayed(extraMuzzleFlash[i], extraBarrels[i], delay * (i + 1)));
                }
                FireBarrel(muzzleFlash, gunTip);

                if (wRef.burst == 0) {
                    currCoolDown = wRef.rateOfFire;
                }
                else {
                    burstCount += 1;
                    if (burstCount == wRef.burst + 1) {
                        currCoolDown = wRef.rateOfFire;
                        burstCount = 0;
                    }
                    else {
                        currCoolDown = wRef.shortCooldown;
                    }
                }
                if (chargeTime > 0f && burstCount == 0 && wRef.rateOfFire > chargedFireCount)
                    chargedFireCount = 0.001f;
            }
        }

        static MethodInfo _ChargedWeaponPass = typeof(Weapon).GetMethod("ChargedWeaponPass", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        static bool ChargedWeaponPass(Weapon w) => (bool)_ChargedWeaponPass.Invoke(w, null);
        static MethodInfo _PayCost = typeof(Weapon).GetMethod("PayCost", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        static bool PayCost(Weapon w) => (bool)_PayCost.Invoke(w, null);
        static MethodInfo _GetDistanceToAimPoint = typeof(Weapon).GetMethod("GetDistanceToAimPoint", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        static float GetDistanceToAimPoint(Weapon w) => (float)_GetDistanceToAimPoint.Invoke(w, null);

        [HarmonyPatch(typeof(Weapon), nameof(Weapon.Fire))]
        [HarmonyPrefix]
        public static bool Weapon_Fire_ProjectileFix(Transform target, bool buttonDown,
            Weapon __instance, SpaceShip ___ss, Transform ___mainParent, bool ___isDrone, bool ___loaded,
            TWeapon ___wRef, float ___chargeTime, Transform ___weaponSlot,
            MuzzleFlash ___muzzleFlash, MuzzleFlash[] ___extraMuzzleFlash, Color ___muzzleFlashColor, float ___flashSize,
            float ___delayTime, TCritical ___critical,
            WeaponStatsModifier ___mods, GameObject ___projectileRef, Transform ___gunTip,
            Drone ___drone, Rigidbody ___rbShip, float ___damage,
            AudioSource ___audioS, float ___audioMod, AudioClip ___audioToPlay,
            int ___explodeBoostChance, float ___explodeBoost,
            Vector3 ___size, float ___sizeMod,
            WeaponImpact ___impact, int ___projSpeed, int ___range, float ___chargedDamageBoost,
            Transform[] ___extraBarrels, float ___delayPortion, bool ___alternateExtraFire,
            ref sbyte ___burstCount, ref float ___chargedFireCount, ref float ___currCoolDown
        ) {
            Weapon_Fire_Projectile(target, buttonDown,
                __instance, ___ss, ___mainParent, ___isDrone, ___loaded,
                ___wRef, ___chargeTime, ___weaponSlot,
                ___muzzleFlash, ___extraMuzzleFlash, ___muzzleFlashColor, ___flashSize,
                ___delayTime, ___critical,
                ___mods, ___projectileRef, ___gunTip, ___drone,
                ___rbShip, ___damage, ___audioS, ___audioMod, ___audioToPlay,
                ___explodeBoostChance, ___explodeBoost, ___size, ___sizeMod,
                ___impact, ___projSpeed, ___range, ___chargedDamageBoost,
                ___extraBarrels, ___delayPortion, ___alternateExtraFire,
                ref ___burstCount, ref ___chargedFireCount, ref ___currCoolDown
                );

            return false;
        }
    }
}
