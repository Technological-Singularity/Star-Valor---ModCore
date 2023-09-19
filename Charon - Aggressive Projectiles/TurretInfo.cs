using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Charon.StarValor.AggressiveProjectiles {
    public class TurretInfo {
        enum FFStatus : int {
            None = 0,
            Enemy = 1,
            Neutral = 2,
            Friendly = 3,
        }
        /// <summary>
        /// Weapon category, ordered from low to high by priority (highest priority = first)
        /// </summary>
        enum WeaponCategory : int {
            PointDefense = 0,
            RepairBeam = 1,
            Normal = 2,
            Beam = 3,
            Invalid = int.MaxValue,
        }

        const int targetLayerMask = 1 << 8 | 1 << 9 | 1 << 10 | 1 << 13 | 1 << 14 | 1 << 16;
        const float thresholdAimOK = 20; //degrees, used to determine if aim is close enough to fire
        const float rotationSpeedMultiplier = 10;
        static FieldInfo WeaponTurret_get_archLimit = typeof(WeaponTurret).GetField("archLimit", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        static Quaternion errorQuaternion = Quaternion.Euler(0, 90, 0);

        Transform transform => weaponSlot.transform;
        Transform weaponSlot;
        WeaponTurret turret;
        Transform[] barrels;
        Func<Transform> gunTipGetter;
        Func<Transform[]> extraBarrelsGetter;
        Quaternion baseRotation;
        float archLimit;
        int lastRaycastFrame = -1;
        int slotIndex;
        SpaceShip ss;
        Dictionary<WeaponCategory, (float range, float speed, float lifetime)> projectileInfo = new Dictionary<WeaponCategory, (float, float, float)>();
        List<(Transform barrel, Transform hitTarget, FFStatus status)> raycastHits = new List<(Transform barrel, Transform hitTarget, FFStatus status)>();
        WeaponCategory currentCategory;

        public TurretInfo(SpaceShip ss, WeaponTurret turret, int slotIndex) {
            this.ss = ss;
            this.slotIndex = slotIndex;
            weaponSlot = ss.weaponSlots.GetChild(slotIndex);
            this.turret = turret;
            if (turret == null) {
                baseRotation = Quaternion.identity;
                archLimit = 0;
                gunTipGetter = () => ss.weaponSlots.GetChild(slotIndex);
                extraBarrelsGetter = () => null;
                barrels = new Transform[1];
            }
            else {
                baseRotation = Quaternion.Euler(0, turret.baseDegreeRaw, 0);
                archLimit = (float)WeaponTurret_get_archLimit.GetValue(turret);
                var isGunTip = weaponSlot.Find("GunTip") != null;
                var isTurret = weaponSlot.GetComponent<WeaponTurret>() != null;

                barrels = new Transform[turret == null ? 1 : 1 + (turret.extraBarrels?.Length ?? 0)];
                if (isGunTip)
                    gunTipGetter = () => ss.weaponSlots.GetChild(slotIndex).Find("GunTip");
                else
                    gunTipGetter = () => ss.weaponSlots.GetChild(slotIndex);

                if (isTurret)
                    extraBarrelsGetter = () => ss.weaponSlots.GetChild(slotIndex).GetComponent<WeaponTurret>().extraBarrels;
                else
                    extraBarrelsGetter = () => null;
            }
        }
        public Transform[] Barrels {
            get {
                if (barrels[0] != null)
                    return barrels;

                barrels[0] = gunTipGetter();
                if (extraBarrelsGetter != null) {
                    int idx = 0;
                    foreach (var o in extraBarrelsGetter())
                        barrels[++idx] = o;
                }

                return barrels;
            }
        }
        List<(Transform barrel, Transform hitTarget, FFStatus status)> UpdateRaycasts(float range) {
            if (Time.frameCount == lastRaycastFrame)
                return raycastHits;

            lastRaycastFrame = Time.frameCount;
            raycastHits.Clear();
            var oldLayers = Plugin.SetLayers(ss.transform, targetLayerMask, 2);
            foreach (var barrel in barrels) {
                if (barrel == null)
                    continue;

                var wasHit = Physics.Raycast(barrel.position, barrel.transform.forward, out var hitInfo, range, targetLayerMask, QueryTriggerInteraction.Ignore);
                if (wasHit) {
                    var hitTransform = hitInfo.transform;
                    if (hitTransform.CompareTag("Collider"))
                        hitTransform = hitTransform.GetComponent<ColliderControl>().ownerEntity.transform;
                    var hitStatus = GetFFStatus(hitTransform);
                    raycastHits.Add((barrel, hitTransform, hitStatus));
                }
            }
            Plugin.ResetLayers(oldLayers);
            return raycastHits;
        }
        Vector3 GetError(Transform target, float mag) => errorQuaternion * (target.position - transform.position).normalized * mag;
        WeaponCategory GetCurrentCategory() {
            foreach (WeaponCategory category in Enum.GetValues(typeof(WeaponCategory)))
                if (projectileInfo.ContainsKey(category))
                    return category;
            return WeaponCategory.Invalid;
        }
        public float GetFireableRange(Transform target) {
            var hit = TryHit(target, out var predictedDirection, out var distance);
            if (!hit)
                return -1f;

            if (turret?.type == WeaponTurretType.Rotating)
                return distance;

            float angle = Quaternion.Angle(Quaternion.LookRotation(predictedDirection), ss.transform.rotation * baseRotation);

            if (Mathf.Abs(angle) <= archLimit + 0.1f)
                return distance;

            return -1f;
        }
        bool TryHit(Transform target, out Vector3 predictedDirection, out float distance) {
            if (target == null) {
                predictedDirection = default;
                distance = default;
                return false;
            }

            var range = GetEffectiveRange(target, transform.position, out predictedDirection, out _);
            if (float.IsNaN(range)) {
                distance = default;
                return false;
            }

            var wasHit = Plugin.InvisibleRaycastSelective(transform, target, transform.position, predictedDirection, targetLayerMask, out var hitInfo, range: range);
            distance = hitInfo.distance;
            return wasHit;
        }
        public bool SetClosestTarget(List<ScanObject> objs, bool smallObject) {
            if (objs == null || objs.Count == 0 || turret == null)
                return false;

            if (currentCategory == WeaponCategory.Invalid)
                Refresh();

            var target = objs
                .Where(o => o != null)
                .Select(o => (transform: o.trans, distance: GetFireableRange(o.trans)))
                .Where(o => o.distance >= 0)
                .Aggregate((transform: (Transform)null, distance: float.MaxValue), (highest, current) => current.distance < highest.distance ? current : highest)
                .transform;

            if (target == null)
                return false;
            if (smallObject)
                return turret.SetTargetSmallObject(target);
            return turret.SetTarget(target);
        }
        public void Refresh() {
            projectileInfo.Clear();

            var weapons = ss.weapons.Where(o => o?.weaponSlotIndex == slotIndex);
            foreach (var weapon in weapons) {
                if (weapon.wRef.canHitProjectiles)
                    Update(WeaponCategory.PointDefense, weapon);
                if (weapon.wRef.damageType == DamageType.Repair)
                    Update(WeaponCategory.RepairBeam, weapon);
                else {
                    Update(WeaponCategory.Normal, weapon);
                    if (weapon.wRef.compType == WeaponCompType.BeamWeaponObject)
                        Update(WeaponCategory.Beam, weapon);
                }
            }
            currentCategory = GetCurrentCategory();
        }
        void Update(WeaponCategory type, Weapon weapon) {
            //to do: correct for mines
            //to do: correct for turret/ship buffs (if needed?)

            var range = weapon.range + weapon.wRef.aoe;
            var speed = weapon.wRef.compType == WeaponCompType.BeamWeaponObject ? float.PositiveInfinity : weapon.projSpeed;
            var lifetime = weapon.wRef.compType == WeaponCompType.BeamWeaponObject ? float.PositiveInfinity : weapon.range / weapon.projSpeed;

            if (!projectileInfo.TryGetValue(type, out var pair) || range > pair.range)
                projectileInfo[type] = (range, speed, lifetime);
        }
        bool Get(WeaponCategory type, out float range, out float speed, out float lifetime) {
            if (projectileInfo.TryGetValue(type, out var pair)) {
                (range, speed, lifetime) = pair;
                return true;
            }
            (range, speed, lifetime) = (default, default, default);
            return false;
        }
        public float GetEffectiveRange(Transform target, Vector3 sourcePosition, out Vector3 predictedDirection, out float predictedTime) {
            if (!Get(currentCategory, out var range, out var speed, out var lifetime)) {
                predictedDirection = Vector3.zero;
                predictedTime = float.NaN;
                return float.NaN;
            }

            if (float.IsInfinity(speed)) {
                predictedDirection = (target.position - sourcePosition).normalized;
                predictedTime = 0;
                return range;
            }

            var targetPredictor = target.GetComponent<TargetPredictor>();
            if (targetPredictor == null) {
                targetPredictor = target.gameObject.AddComponent<TargetPredictor>();
                targetPredictor.enabled = true;
            }

            var thisPredictor = ss.GetComponent<TargetPredictor>();
            if (thisPredictor == null) {
                thisPredictor = ss.gameObject.AddComponent<TargetPredictor>();
                thisPredictor.enabled = true;
            }

            predictedDirection = targetPredictor.Predict_OneShot(sourcePosition, thisPredictor.State.vel, speed, out predictedTime);
            var effectiveVelocity = thisPredictor.State.vel + predictedDirection * speed;
            return effectiveVelocity.magnitude * lifetime;
        }
        FFStatus GetFFStatus(Transform target) {
            if (target == null)
                return FFStatus.None;

            var targetSS = target.GetComponent<SpaceShip>();
            if (targetSS != null) {
                if (ss.ffSys.TargetIsFriendly(targetSS.ffSys))
                    return FFStatus.Friendly;
                if (ss.ffSys.TargetIsEnemy(targetSS.ffSys))
                    return FFStatus.Enemy;
            }
            return FFStatus.Neutral;
        }
        public bool CheckFireOK(Transform target, float range, bool requireHit) {
            bool isClear = true;
            bool someHit = false;
            var targetStatus = GetFFStatus(target);
            foreach (var (_, hitTransform, hitStatus) in UpdateRaycasts(range)) {
                someHit = true;
                if (hitStatus > targetStatus)
                    isClear = false;
            }
            return isClear && (someHit || !requireHit);
        }
        public bool AimAt(Transform target, Transform aimTarget, float errorMag) {
            var hasInfo = Get(currentCategory, out _, out float speed, out _);
            if (!hasInfo)
                Refresh();

            Vector3 prediction;
            Quaternion newRotationTarget;
            if (float.IsInfinity(speed) || float.IsNaN(speed)) {
                prediction = target.position + GetError(target, errorMag);
                aimTarget.transform.position = prediction;
                newRotationTarget = Quaternion.LookRotation(prediction - weaponSlot.position);
            }
            else {
                var error = GetError(target, errorMag);

                var targetPredictor = target.GetComponent<TargetPredictor>();
                if (targetPredictor == null) {
                    targetPredictor = target.gameObject.AddComponent<TargetPredictor>();
                    targetPredictor.enabled = true;
                }

                var thisPredictor = transform.parent.parent.GetComponent<TargetPredictor>();
                if (thisPredictor == null) {
                    thisPredictor = transform.parent.parent.gameObject.AddComponent<TargetPredictor>();
                    thisPredictor.enabled = true;
                }

                GetEffectiveRange(target, transform.position, out prediction, out var time);

                if (prediction == Vector3.zero && speed > 0) {
                    var d = (targetPredictor.State.pos - transform.position).magnitude / speed;
                    prediction = targetPredictor.State.pos + d * (targetPredictor.State.vel - thisPredictor.State.vel) + error;
                    newRotationTarget = Quaternion.LookRotation(prediction - transform.position);
                    aimTarget.transform.position = prediction;
                }
                else if (prediction == Vector3.zero) {
                    newRotationTarget = transform.rotation;
                    aimTarget.transform.position = transform.position; ;
                }
                else {
                    newRotationTarget = Quaternion.LookRotation(prediction);
                    aimTarget.transform.position = transform.position + prediction * time;
                }
            }
            var maxDegreesDelta = Time.deltaTime * rotationSpeedMultiplier * turret.turnSpeed;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, newRotationTarget, maxDegreesDelta);
            var angleDeltaActual = Quaternion.Angle(transform.rotation, newRotationTarget);

            return angleDeltaActual <= thresholdAimOK/* && CheckFireOK(target, range, false)*/;
        }
    }
}

//The following functions should be updated to have altered range based on the velocity of the target:
//WeaponTurret.CanFireAt
//AIControl.FireAllWeapons

