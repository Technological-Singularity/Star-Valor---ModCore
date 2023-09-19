using System.Collections.Generic;
using Charon.StarValor.ModCore;
using Charon.StarValor.ModCore.Systems.Buff;
using UnityEngine;

namespace Charon.StarValor.MasterTinker {
    public partial class Buff_HyperspatialAnchor : BuffGeneral {
        Equipment_HyperspatialAnchor.Effects data = new Equipment_HyperspatialAnchor.Effects();
        List<(GameObject gameObj, LineRenderer renderer, Vector3 offset)> renderers;
        static List<(GameObject, LineRenderer, Vector3)> GenerateAnchors(int count) {
            var wr = new List<(GameObject, LineRenderer, Vector3)>();
            void addAnchor(Vector3 position) {
                var anchorGO = Instantiate(ObjManager.GetObj("Effects/LineRenderObj"), null);
                var renderer = anchorGO.GetComponent<LineRenderer>();
                wr.Add((anchorGO, renderer, position));

                renderer.enabled = false;
                renderer.alignment = LineAlignment.View;

                renderer.startWidth = 2f;
                renderer.endWidth = 0.4f;
                renderer.positionCount = 2;
            }

            if (count == 1) {
                addAnchor(Vector3.zero);
            }
            else {
                float delta = 360.0f / count;
                Vector3 pos = Vector3.forward;
                Quaternion rot = Quaternion.Euler(0, delta, 0);
                for (int i = 0; i < count; ++i) {
                    addAnchor(pos);
                    pos = rot * pos;
                }
            }
            return wr;
        }

        Color startColor = new Color(0.8f, 0.4f, 0, 0.8f);
        Color endColor = new Color(0, 0, 1, 0);
        static readonly Color anchorColor = new Color(0.8f, 0.4f, 0, 0.2f);

        Buff_HyperspatialAnchor() {
            this.gameObject.name = this.GetType().Name;
            this.gameObject.AddComponent<CachedValue.Debugger>();
        }
        protected override void OnInitialize(Equipment equipment, int rarity, int qnt) {
            base.Initialize(equipment, rarity, qnt);
            data.LoadEquipment(equipment, rarity, qnt);
            data.Link(this.targetSS.transform, this.gameObject.transform);
        }
        protected override void OnFixedUpdate() {
            bool allBreak = true;
            foreach (var (source, renderer, _) in renderers) {
                var vect = source.transform.position - this.targetSS.transform.position;
                var magDist = vect.magnitude / data.range;
                magDist = magDist > 1.5f ? Mathf.Max(1.5f * (2.5f - magDist), 0) : magDist;

                var vectVel = this.targetSS.rb.velocity / (12 * Mathf.Sqrt(data.range * Mathf.Ceil(data.count)));
                var vectTotal = magDist * vect.normalized - vectVel;

                SetAlpha(renderer, Mathf.Max(0.05f, magDist));
                var point = Utilities.GetRaycastColliderPoint(this.targetSS.gameObject, renderer.transform.position, out _);
                renderer.SetPosition(0, point);

                if (magDist > 0) {
                    allBreak = false;
                    this.targetSS.rb.AddForce(data.force * vectTotal);
                }
            }
            if (allBreak && this.buffControl.activeEquipment.active)
                this.buffControl.activeEquipment.ActivateDeactivate(false, this.targetSS.transform);
        }
        public void SetAlpha(LineRenderer renderer, float alpha) {
            var newStart = startColor;
            var newEnd = endColor;
            newStart.a = alpha;
            newEnd.a = 0;
            renderer.startColor = newStart;
            renderer.endColor = newEnd;
        }
        protected override void Begin() {
            base.Begin();
            data.Enabled = true;
            data.Relink();
            renderers = GenerateAnchors(Mathf.CeilToInt(data.count));
            foreach (var (source, renderer, offset) in renderers) {
                var pos = this.targetSS.rb.rotation * offset * (data.range / 2) + this.targetSS.transform.position;
                source.transform.position = pos;
                renderer.SetPosition(1, pos);
                renderer.enabled = true;
                renderer.startColor = startColor;
                renderer.endColor = endColor;
            }
        }
        protected override void End() {
            if (enabled) {
                foreach (var (go, renderer, _) in renderers) {
                    renderer.enabled = false;
                    Destroy(go);
                }
                renderers.Clear();
                data.Unlink();
                data.Enabled = false;
            }
            base.End();
        }
        void OnDestroy() {
            data.Unlink();
            if (renderers != null) {
                foreach (var (go, renderer, _) in renderers) {
                    renderer.enabled = false;
                    Destroy(go);
                }
                renderers = null;
            }
        }
    }
}
