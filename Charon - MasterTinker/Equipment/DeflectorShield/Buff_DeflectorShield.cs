using System;
using System.Collections.Generic;
using Charon.StarValor.ModCore;
using Charon.StarValor.ModCore.Systems.Buff;
using UnityEngine;

namespace Charon.StarValor.MasterTinker {
    public partial class Buff_DeflectorShield : BuffGeneral {
        const float min_cutoff = 0.05f;
        float detectionRange = 0;
        int totalEmitters = 0;
        float hardnessDenom = 1;
        float hardnessEffective = 0;
        Equipment_DeflectorShield.Effects data = new Equipment_DeflectorShield.Effects();
        LinkedList<TrackObject> trackedOrder { get; set; } = new LinkedList<TrackObject>();
        Dictionary<Rigidbody, TrackObject> trackedUnique { get; } = new Dictionary<Rigidbody, TrackObject>();
        Buff_DeflectorShield() {
            this.gameObject.name = this.GetType().Name;
            this.gameObject.AddComponent<CachedValue.Debugger>();
        }
        protected override void OnInitialize(Equipment equipment, int rarity, int qnt) {
            base.Initialize(equipment, rarity, qnt);
            data.LoadEquipment(equipment, rarity, qnt);
            data.Link(this.targetSS.transform, this.gameObject.transform, this.targetSS.stats, this.targetSS);

            data.test.Modifier = 100;
            data.test2.Modifier = 100;
        }

        //List<Vector3> emitterPositions = new List<Vector3>();
        //void RecalculateEmitterPositions() {
        //	void visualize(Vector3 start, Vector3 end) {
        //		var gameobj = Instantiate(ObjManager.GetObj("Effects/LineRenderObj"), this.targetSS.transform);
        //		var renderer = gameobj.GetComponent<LineRenderer>();
        //		renderer.enabled = false;
        //		renderer.alignment = LineAlignment.View;

        //		renderer.startWidth = 0.5f;
        //		renderer.endWidth = 0.5f;
        //		renderer.positionCount = 2;

        //		renderer.startColor = Color.red;
        //		renderer.endColor = Color.red;
        //		renderer.SetPosition(0, start);
        //		renderer.SetPosition(1, end);
        //		renderer.enabled = true;
        //	}

        //	emitterPositions.Clear();
        //	const float radius = 300;
        //	var delta = 360f / totalEmitters;
        //	var rotationDelta = Quaternion.Euler(0, delta, 0);
        //	var rotation = this.targetSS.transform.forward;
        //	if (totalEmitters % 2 == 0)
        //		rotation = Quaternion.Euler(0, delta / 2, 0) * rotation;
        //	for(int i = 0; i < totalEmitters; ++i) {
        //		var pos = radius * rotation + this.targetSS.transform.position;
        //		var visualStart = pos;

        //		pos = GetClosestColliderPoint(this.targetSS.gameObject, pos, out _);
        //		var visualEnd = pos;

        //		pos -= this.targetSS.transform.position;
        //		emitterPositions.Add(pos);

        //		visualize(visualStart, visualEnd);

        //		rotation = rotationDelta * rotation; //quaternion rotation
        //	}
        //      }
        //Vector3 GetClosestEmitter(Vector3 point) {
        //if (emitterPositions.Count == 0)
        //	return new Vector3();
        //float dist = float.MaxValue;
        //Vector3 closest = new Vector3();
        //         foreach(var p in emitterPositions) {
        //	var pointCandidate = this.targetSS.transform.rotation * p + this.targetSS.transform.position;
        //	var distCandidate = (p - point).sqrMagnitude;
        //	if (distCandidate < dist) {
        //		dist = distCandidate;
        //		closest = pointCandidate;
        //             }
        //         }
        //return closest;
        //}
        protected override void OnUpdate() {
            if (!this.active || this.targetSS.transform is null)
                return;

            hardnessEffective = data.hardness / 20 - 7;
            hardnessDenom = Mathf.Exp(hardnessEffective) - 1;
            totalEmitters = Mathf.CeilToInt(data.emitters);
            detectionRange = data.range + 40;

            try {
                for (var node = trackedOrder.First; node != null;) {
                    var thisNode = node;
                    node = node.Next;
                    var obj = thisNode.Value;
                    if (obj == null || obj.IsNull || Vector3.SqrMagnitude(this.targetSS.transform.position - obj.Collider.transform.position) > detectionRange * detectionRange) {
                        trackedOrder.Remove(thisNode);
                        trackedUnique.Remove(obj.RigidBody);
                        obj.Destroy();
                    }
                }
            }
            catch {
                Plugin.Log.LogError("CAUGHT 0");
            }
            //if (totalEmitters != emitterPositions.Count)
            //	RecalculateEmitterPositions();

            void tryAddCollider(Collider collider) {
                try {
                    var transform = collider.transform;
                    var rigidBody = collider.attachedRigidbody;
                    if (collider.transform.CompareTag("Collider"))
                        rigidBody = collider.transform.GetComponent<ColliderControl>().ownerEntity.rb;
                    if (rigidBody == null || rigidBody == this.targetSS.rb || trackedUnique.ContainsKey(rigidBody) || collider.CompareTag("Communication"))
                        return;
                    var tracker = new TrackObject(collider, rigidBody);
                    trackedUnique.Add(rigidBody, tracker);
                    trackedOrder.AddLast(tracker);
                }
                catch {
                    Plugin.Log.LogError("CAUGHT 1");
                }
            }

            var targetMask = Mathf.RoundToInt(data.targets);
            foreach (var collider in Physics.OverlapSphere(this.targetSS.transform.position, detectionRange, targetMask, QueryTriggerInteraction.UseGlobal))
                tryAddCollider(collider);
            foreach (var collider in Physics.OverlapSphere(this.targetSS.transform.position, detectionRange, targetMask, QueryTriggerInteraction.Collide))
                tryAddCollider(collider);
            foreach (var collider in Physics.OverlapSphere(this.targetSS.transform.position, detectionRange, targetMask, QueryTriggerInteraction.Ignore))
                tryAddCollider(collider);

            Plugin.Log.LogWarning($"Found {trackedOrder.Count} colliders within {detectionRange} range");
            //foreach (var o in tracked)
            //    Core.Log.LogWarning(o.RigidBody.tag);
        }
        protected override void OnFixedUpdate() {
            if (trackedOrder.Count == 0)
                return;

            float getExponentialScale(float ratio) {
                if (float.IsNaN(ratio)) ratio = 0;
                if (ratio > 1) ratio = 1;
                if (ratio < 0) ratio = 0;
                return Mathf.Abs(hardnessDenom) < float.Epsilon ? ratio : (Mathf.Exp(ratio * hardnessEffective) - 1) / hardnessDenom;
            }

            bool runUpdate = false;
            foreach (var trackObj in trackedOrder)
                trackObj.Reset();

            LinkedListNode<TrackObject> node = trackedOrder.First;
            //totalEmitters = int.MaxValue;           //debug
            var emittersRemaining = totalEmitters;
            for (int i = 0; emittersRemaining > 0 && i < trackedOrder.Count; ++i) {
                var target = node.Value;
                node = node.Next;

                if (target.IsNull) {
                    target.Destroy();
                    continue;
                }
                runUpdate = true;

                (var collider, var rigidBody) = (target.Collider, target.RigidBody);
                Collider ownerClosestCollider;
                Vector3 closestTargetPos, closestOwnerPos;
                if (collider.transform.tag == "Collectible") {
                    closestTargetPos = collider.GetComponent<Collectible>().transform.position;
                    closestOwnerPos = Utilities.GetClosestColliderPoint(this.targetSS.gameObject, closestTargetPos, out ownerClosestCollider);
                    //closestOwnerPos = thisCollider.ClosestPoint(closestTargetPos);
                }
                else if (collider.transform.tag == "Projectile") {
                    closestTargetPos = collider.GetComponent<ProjectileControl>().transform.position;
                    closestOwnerPos = Utilities.GetClosestColliderPoint(this.targetSS.gameObject, closestTargetPos, out ownerClosestCollider);
                }
                else {
                    closestTargetPos = collider.ClosestPoint(this.targetSS.transform.position);
                    closestOwnerPos = Utilities.GetClosestColliderPoint(this.targetSS.gameObject, closestTargetPos, out ownerClosestCollider);
                    //closestOwnerPos = thisCollider.ClosestPoint(closestTargetPos);
                    closestTargetPos = collider.ClosestPoint(closestOwnerPos);
                }

                //if (target.AlreadyAffecting) {
                //	target.Draw(ownerClosestCollider, closestOwnerPos, ownerClosestCollider.bounds.size.magnitude, closestTargetPos, target.Collider.bounds.size.magnitude);
                //	continue;
                //}

                var forceVector = closestTargetPos - closestOwnerPos;
                var normForceVector = Vector3.Normalize(forceVector);
                var rangeEffective = Vector3.Magnitude(forceVector);

                var relVelocity = this.targetSS.rb.velocity - rigidBody.velocity;
                var normalSpeed = Vector3.Dot(relVelocity, normForceVector);
                var relSpeed = Vector3.Magnitude(relVelocity);

                float repulseScale = 0, deflectScale = 0, disperseScale = 0;
                if (data.mag_repulse != 0) {
                    var modifier = Mathf.Min(Mathf.Abs(data.mag_repulse), 1) * 0.7f + 0.3f;
                    var repulseRange = rangeEffective / modifier;

                    var repulseSpeed = normalSpeed + 50;
                    if (repulseRange <= data.range && repulseSpeed > 0) {
                        var speedScale = getExponentialScale(repulseSpeed / 20);
                        var distScale = 1 - getExponentialScale(repulseRange / data.range);

                        repulseScale = distScale * speedScale * data.mag_repulse * modifier;
                    }
                }

                if ((data.mag_vector != 0 || data.mag_disperse != 0) && rangeEffective <= data.range && normalSpeed > 0) {
                    var speedScale = getExponentialScale(normalSpeed / 20);
                    var distScale = 1;// 1 - getExponentialScale(rangeEffective / data.range);

                    deflectScale = speedScale * distScale * data.mag_vector;
                    disperseScale = speedScale * distScale * data.mag_disperse;
                }

                float ars = Math.Abs(repulseScale), adefs = Mathf.Abs(deflectScale), adisp = Mathf.Abs(disperseScale);
                if (ars <= float.Epsilon && adefs <= float.Epsilon && adisp <= float.Epsilon || ars + adefs + adisp < min_cutoff) {
                    target.Emitters = 0;
                    continue;
                }

                Vector3 repulsion = new Vector3(), dispersion = new Vector3(), deflection = new Vector3();
                Vector3 normVelVector = relVelocity.normalized;
                Vector3 normAwayVector = (closestTargetPos - this.targetSS.transform.position).normalized;

                Vector3 normSideVector = (normAwayVector * Vector3.Dot(normAwayVector, normVelVector) - normVelVector).normalized;
                if (Mathf.Abs(repulseScale) > float.Epsilon)
                    repulsion = repulseScale * normAwayVector;
                if (Mathf.Abs(deflectScale) > float.Epsilon)
                    deflection = deflectScale * normSideVector;
                if (Mathf.Abs(disperseScale) > float.Epsilon)
                    dispersion = disperseScale * normVelVector;
                target.QueuedVector = repulsion + deflection + dispersion;
                target.QueuedForce = target.QueuedVector.magnitude;
                target.QueuedVector = target.QueuedVector.normalized;

                //if (rigidBody.transform.tag == "Projectile") {
                //	var mult = 6 * rigidBody.mass * Math.Min(4, rigidBody.velocity.sqrMagnitude / 1600);
                //	var sign = Math.Sign(target.QueuedForce);
                //	var scalar = Mathf.Max(0, Mathf.Pow(Mathf.Abs(target.QueuedForce), 0.25f));
                //	target.QueuedForce = sign * scalar * mult;
                //}


                var thisSize = ownerClosestCollider.bounds.size.magnitude;
                var targetSize = target.Collider.bounds.size.magnitude;

                if (target.QueuedForce <= 0.05f) {
                    target.Emitters = 0;
                }
                else if (target.RigidBody.tag == "Projectile" || target.RigidBody.tag == "Collectible")
                    target.Emitters = 1;
                else {
                    target.Emitters = Math.Min((int)(targetSize / 4) + 1, emittersRemaining);
                }
                emittersRemaining -= target.Emitters;
                target.Draw(ownerClosestCollider.ClosestPoint, closestOwnerPos, thisSize, closestTargetPos, targetSize);
            }

            if (!runUpdate)
                return;

            node = trackedOrder.First;
            for (int i = 0; i < trackedOrder.Count; ++i) {
                var target = node.Value;
                var thisNode = node;
                node = node.Next;

                if (target.IsNull)
                    continue;
                if (target.Emitters == 0) {
                    trackedOrder.Remove(thisNode);
                    trackedOrder.AddLast(thisNode);
                    continue;
                }
                //if (trackObject.AlreadyAffecting) {
                //	trackObject.SetAlpha(0, Math.Min(Mathf.Abs(trackObject.QueuedForce), 1));
                //	continue;
                //}

                var rigidBody = target.RigidBody;
                bool isStationary = rigidBody.transform.GetComponent<Station>() != null || rigidBody.CompareTag("Communication");
                float massMultiplier;
                switch (rigidBody.transform.tag) {
                    case "Asteroid": massMultiplier = 4; break;
                    case "Communication":
                    case "Station": massMultiplier = 1000; break;
                    case "Drone": massMultiplier = 0.02f; break;
                    case "Projectile": massMultiplier = 0.001f; break;
                    case "Collectible": massMultiplier = 0.1f; break;
                    default: massMultiplier = 1; break;
                }
                var targetMass = rigidBody.mass * massMultiplier;
                if (targetMass <= 0)
                    targetMass = .01f;

                var reducedMass = targetMass * this.targetSS.rb.mass / (targetMass + this.targetSS.rb.mass);
                var normForceVector = target.QueuedVector * Mathf.Sqrt(target.Emitters) * data.force * target.QueuedForce;

                var forceAppl = -normForceVector;
                var moment = target.LastDrawSource - this.targetSS.transform.position;
                var normMoment = moment.normalized;
                var torque = Vector3.Cross(moment, forceAppl) / 5;//(forceAppl - push).magnitude * moment.magnitude * Vector3.up;
                this.targetSS.rb.AddForce(forceAppl * reducedMass / this.targetSS.rb.mass, ForceMode.Force);
                this.targetSS.rb.AddTorque(torque, ForceMode.Force);

                if (!isStationary) {
                    rigidBody.AddForce(normForceVector * reducedMass / targetMass, ForceMode.Force);
                    if (rigidBody.transform.tag == "Projectile" && rigidBody.velocity.sqrMagnitude > 0.001f)
                        rigidBody.transform.rotation = Quaternion.LookRotation(rigidBody.velocity);
                }
                //trackObject.SetAlpha(0, Math.Min(Mathf.Abs(trackObject.QueuedForce), 1));
                target.SetAlpha(0, target.QueuedForce * 0.9f + 0.1f);
                if (target.QueuedForce > trackedOrder.First.Value.QueuedForce) {
                    trackedOrder.Remove(thisNode);
                    trackedOrder.AddFirst(thisNode);
                    continue;
                }
            }
        }
        protected override void Begin() {
            base.Begin();
            data.Enabled = true;
            data.Relink();
        }
        protected override void End() {
            if (this.enabled) {
                foreach (var tracker in trackedOrder)
                    tracker.Destroy();
                trackedOrder.Clear();
                trackedUnique.Clear();
                data.Unlink();
                data.Enabled = false;
            }
            base.End();
        }
    }
}
