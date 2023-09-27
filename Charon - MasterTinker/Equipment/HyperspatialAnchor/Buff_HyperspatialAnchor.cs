using System;
using System.Collections.Generic;
using Charon.StarValor.ModCore;
using UnityEngine;
using static Charon.StarValor.MasterTinker.Equipment_HyperspatialAnchor.Effects;

namespace Charon.StarValor.MasterTinker {
    public partial class Buff_HyperspatialAnchor : BuffGeneral {
        ValueModifier.Collection data = new ValueModifier.Collection();
        List<(GameObject gameObj, LineRenderer renderer, Vector3 offset)> renderers;

        Color startColor = new Color(0.8f, 0.4f, 0, 0.8f);
        Color endColor = new Color(0, 0, 1, 0);
        static readonly Color anchorColor = new Color(0.8f, 0.4f, 0, 0.2f);

        Buff_HyperspatialAnchor() {
            this.gameObject.name = this.GetType().Name;
        }
        public override void Initialize(SpaceShip ss, Equipment equipment, int rarity, int qnt) {
            base.Initialize(ss, equipment, rarity, qnt);
            var eq = (EquipmentEx)equipment;
            //var effectContext = new EffectContext() { EquipmentRarityMod = equipment.rarityMod, Rarity = rarity, Qnt = qnt };

            List<Type> types = new List<Type>() {
                typeof(Count),
                typeof(Force),
                typeof(Range),
            };
            foreach (var type in types) {
                var vmod = ValueModifier.FromType(type);
                vmod.Modifier = eq.GetEffect(type).value;
                data.Add(type, vmod);
            }                
            data.Link(this.targetSS.transform);
            this.gameObject.AddComponent<CachedValue.Debugger>().Initialize(this.targetSS.transform, 1);
        }
        protected override void OnFixedUpdate() {
            bool allBreak = true;
            foreach (var (source, renderer, _) in renderers) {
                var vect = source.transform.position - this.targetSS.transform.position;
                var magDist = vect.magnitude / data.Get<Range>();
                magDist = magDist > 1.5f ? Mathf.Max(1.5f * (2.5f - magDist), 0) : magDist;

                var vectVel = this.targetSS.Rb.velocity / (12 * Mathf.Sqrt(data.Get<Range>() * Mathf.Ceil(data.Get<Count>())));
                var vectTotal = magDist * vect.normalized - vectVel;

                SetAlpha(renderer, Mathf.Max(0.05f, magDist));
                var point = Utilities.GetRaycastColliderPoint(this.targetSS.gameObject, renderer.transform.position, out _);
                renderer.SetPosition(0, point);

                if (magDist > 0) {
                    allBreak = false;
                    this.targetSS.Rb.AddForce(data.Get<Force>() * vectTotal);
                }
            }
            if (allBreak && this.buffControl.activeEquipment.active)
                this.buffControl.activeEquipment.ActivateDeactivate(false, this.targetSS.transform);
        }
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
            renderers = GenerateAnchors(Mathf.CeilToInt(data.Get<Count>()));
            foreach (var (source, renderer, offset) in renderers) {
                var pos = this.targetSS.Rb.rotation * offset * (data.Get<Range>() / 2) + this.targetSS.transform.position;
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
