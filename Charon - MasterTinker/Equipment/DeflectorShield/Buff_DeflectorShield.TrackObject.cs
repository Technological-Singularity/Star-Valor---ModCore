using System;
using System.Collections.Generic;
using UnityEngine;

namespace Charon.StarValor.MasterTinker {
    public partial class Buff_DeflectorShield {
        class TrackObject {
            public Collider Collider { get; }
            public Rigidbody RigidBody { get; }
            public float QueuedForce { get; set; } = 0;
            public Vector3 QueuedVector { get; set; } = new Vector3();
            public int Emitters {
                get => _emitterCount;
                set {
                    if (IsNull)
                        return;

                    while (emitterObjects.Count < value) {
                        var gameobj = Instantiate(ObjManager.GetObj("Effects/LineRenderObj"), Collider.transform);
                        var renderer = gameobj.GetComponent<LineRenderer>();
                        renderer.enabled = false;
                        renderer.alignment = LineAlignment.View;

                        renderer.startWidth = 0.2f;
                        renderer.endWidth = 0.2f;
                        renderer.positionCount = 2;
                        //colors.Add((startColor, endColor));

                        renderer.startColor = startColor;
                        renderer.endColor = endColor;
                        emitterObjects.Add((gameobj, renderer));
                    }
                    while (emitterObjects.Count > value) {
                        int last = emitterObjects.Count - 1;
                        var emitter = emitterObjects[last];
                        if (emitter.renderer != null) {
                            emitter.renderer.enabled = false;
                            UnityEngine.Object.Destroy(emitter.gameobj);
                        }
                        emitterObjects.RemoveAt(last);
                    }
                    _emitterCount = value;
                }
            }
            int _emitterCount = 0;

            public void Reset() {
                QueuedForce = 0;
            }

            List<(GameObject gameobj, LineRenderer renderer)> emitterObjects { get; set; } = new List<(GameObject, LineRenderer)>();
            //List<(Color start, Color end)> colors = new List<(Color start, Color end)>();

            Color startColor = new Color(0, 0, 1, 0); //transparent blue
            Color endColor = Color.cyan;
            public Vector3 LastDrawSource { get; private set; }

            public TrackObject(Collider collider, Rigidbody rigidBody) {
                this.Collider = collider;
                this.RigidBody = rigidBody;
            }
            public void Draw(Func<Vector3, Vector3> getClosest, Vector3 start, float startWidth, Vector3 end, float endWidth) {
                LastDrawSource = start;
                Vector3 delta = end - start;
                Vector3 perp_pos = Vector3.Cross(Vector3.up, delta).normalized;
                for (int i = 0; i < emitterObjects.Count; ++i) {
                    Vector3 lineStart = start, lineEnd = end;
                    if (emitterObjects.Count > 1) {
                        lineStart = start + perp_pos * (-startWidth / 2 + startWidth / (Emitters - 1) * i);
                        lineEnd = end + perp_pos * (-endWidth / 2 + endWidth / (Emitters - 1) * i);
                        lineStart = getClosest(lineStart);
                        lineEnd = Collider.ClosestPoint(lineEnd);
                    }
                    var renderer = emitterObjects[i].renderer;
                    renderer.SetPosition(0, lineStart);
                    renderer.SetPosition(1, lineEnd);

                    //renderer.startWidth = 0.1f * startWidth;
                    //renderer.endWidth = 0.1f * endWidth;

                    renderer.enabled = true;
                }
            }
            public void SetAlpha(float start, float end) {
                startColor.a = start;
                endColor.a = end;
                for (int i = 0; i < emitterObjects.Count; ++i) {
                    var renderer = emitterObjects[i].renderer;
                    renderer.startColor = startColor;
                    renderer.endColor = endColor;
                }
            }
            public bool IsNull => Collider == null || RigidBody == null || emitterObjects == null;
            public void Destroy() {
                if (emitterObjects == null)
                    return;
                Emitters = 0;
                emitterObjects = null;
            }
        }
    }
}
