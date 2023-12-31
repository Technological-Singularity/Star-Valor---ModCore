﻿using UnityEngine;

namespace Charon.StarValor.AggressiveProjectiles {
    public class TargetPredictor : MonoBehaviour {
        public Transform Target { get; private set; }
        (Vector3 pos, Vector3 vel, Vector3 accel) targetState;
        (Vector3 pos, Vector3 vel, Vector3 accel) rawState;
        protected const int iir_factor = 5;
        const float missile_time_estimate = 2f;

        public (Vector3 pos, Vector3 vel, Vector3 accel) State => targetState;
        void Awake() {
            Target = this.transform;
            var rb = Target.GetComponent<Rigidbody>();
            if (rb == null)
                targetState = (Target.position, Vector3.zero, Vector3.zero);
            else
                targetState = (rb.position, rb.velocity, Vector3.zero);
            rawState = targetState;
        }

        protected virtual void FixedUpdate() {
            UpdateTargetData();
        }
        private void UpdateTargetData() {
            if (Target == null)
                return;
            var newPos = Target.position;
            var newVel = (newPos - rawState.pos) / Time.deltaTime;
            var newAccel = (newVel - rawState.vel) / Time.deltaTime;

            rawState = (newPos, newVel, newAccel);

            newVel = (newVel + (iir_factor - 1) * targetState.vel) / iir_factor;
            newAccel = (newAccel + (iir_factor - 1) * targetState.accel) / iir_factor;

            targetState = (newPos, newVel, newAccel);
        }

        public Vector3 GetInterceptPosition(Vector3 sourcePosition, Vector3 sourceVelocity, Vector3 predictedDirection, float projectileSpeed, float time) {
            return sourcePosition + (sourceVelocity + predictedDirection * projectileSpeed) * time;
        }

        /// <summary>
        /// Gives a normalized vector from sourcePosition that is the direction that a projectile should aim in order to hit the target.
        /// </summary>
        /// <param name="sourcePosition"></param>
        /// <param name="sourceVelocity"></param>
        /// <param name="projectileSpeed"></param>
        /// <param name="time">Time to intercept</param>
        /// <returns></returns>
        public Vector3 Predict_OneShot(Vector3 sourcePosition, Vector3 sourceVelocity, float projectileSpeed, out float time) {
            if (float.IsNaN(projectileSpeed) || float.IsInfinity(projectileSpeed)) {
                time = 0;
                return (State.pos - sourcePosition).normalized;
            }

            var relPosition = targetState.pos - sourcePosition;
            var relVelocity = targetState.vel - sourceVelocity;

            var projectileSpeedSq = projectileSpeed * projectileSpeed;
            var targetSpeedSq = relVelocity.sqrMagnitude;

            float towardMag = Vector3.Dot(relPosition, relVelocity);
            float relDistSq = relPosition.sqrMagnitude;
            float inner = towardMag * towardMag + relDistSq * (projectileSpeedSq - targetSpeedSq);

            if (inner <= 0) { //projectile too slow, no solution
                //Plugin.Log.LogWarning("NS " + towardMag + " " + relDistSq + " " + targetSpeedSq + " " + projectileSpeedSq + " " + relVelocity + " " + sourceVelocity);
                time = float.NaN;
                return Vector3.zero;
            }

            float sqrt = Mathf.Sqrt(inner);
            float invTimeN = Mathf.Abs((-towardMag - sqrt) / relDistSq); //check to make sure Abs is okay here
            float invTimeP = Mathf.Abs((-towardMag + sqrt) / relDistSq);

            if (invTimeP < invTimeN)
                invTimeP = invTimeN;
            //if (invTimeP < 0)
            //    invTimeP *= -1;

            Vector3 intercept = (invTimeP * relPosition + relVelocity).normalized;

            //Vector3 position = targetState.pos + targetState.vel / invTimeP;
            //Vector3 velVector = targetState.vel;
            //if (velVector.magnitude < 1)
            //    velVector = Target.forward;
            //velVector.Normalize();
            //Vector3 intersectPos;

            //if (Mathf.Abs(Vector3.Dot(velVector, intercept)) > 0.95f)
            //    intersectPos = position;
            //else
            //    intersectPos = GetInterceptPoint(sourcePosition, intercept, targetState.pos, velVector);
            //intersectPos = (intersectPos + sourcePosition) / 2;

            time = 1 / invTimeP;
            return intercept;


            //var relPosition = targetPosition - sourcePosition;
            //var relDistance = relPosition.magnitude;
            //var relTowardDirection = relPosition.normalized;

            //var relVelocity = targetState.vel - sourceVelocity;
            //var relAcceleration = Vector3.zero;// targetState.accel - sourceAcceleration;

            //var projectileVelocity = relVelocity + projectileSpeed * relTowardDirection;
            //var relTowardSpeed = Vector3.Dot(projectileVelocity, relTowardDirection);

            //float expectedTime;
            //if (relTowardSpeed <= 0) //projectile will never hit                
            //    expectedTime = Time.deltaTime;
            //else
            //    expectedTime = relDistance / relTowardSpeed;

            //Plugin.Log.LogWarning(expectedTime);

            //return targetPosition + relVelocity * expectedTime + relAcceleration / 2 * expectedTime * expectedTime;
        }
        public Vector3 Predict_SelfPropelled(Vector3 sourcePosition, Vector3 sourceVelocity, float sourceAccel) {
            var intercept = Predict_OneShot(sourcePosition, sourceVelocity, sourceAccel * missile_time_estimate, out _);
            if (intercept == Vector3.zero) {
                intercept = targetState.pos - sourcePosition + (targetState.vel - sourceVelocity) * missile_time_estimate;
                intercept.Normalize();
            }
            if (intercept == Vector3.zero) {
                intercept = (targetState.pos - sourcePosition).normalized;
            }
            return intercept;

            //var relPosition = targetState.pos - sourcePosition;
            //var relDistance = relPosition.magnitude;
            //var relTowardDirection = relPosition.normalized;

            //var relVelocity = targetRB.velocity - sourceVelocity;
            //var relTowardSpeed = Vector3.Dot(relVelocity, relTowardDirection);
            //var relCrossSpeed = (relVelocity - relTowardSpeed * relTowardDirection).magnitude;

            //var timeToward = quadraticLowestPositive(-sourceAccel, relTowardSpeed, relDistance);
            //var timeCross = relCrossSpeed / Mathf.Abs(sourceAccel);
            //expectedTime = Mathf.Sqrt(timeToward * timeToward + timeCross * timeCross);
            //return targetRB.position + targetState.vel * expectedTime + targetState.accel / 2 * expectedTime * expectedTime;
        }
    }
}
