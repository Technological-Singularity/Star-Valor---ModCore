using System.Collections.Generic;
using UnityEngine;

namespace Charon.StarValor.ModCore {
    public partial class CachedValue {
        public class Debugger : MonoBehaviour {
            protected GameObject updaterGO;
            protected Updater updater;
            protected Transform anchor;
            int dumpCount = 0;

            public bool Enabled {
                get => updater.enabled;
                set => updater.enabled = value;
            }
            protected void Debug() {
                ModCore.Instance.Log.LogWarning(Dump());
            }
            public void Initialize(Transform anchor, float period_ms) {
                this.anchor = anchor;
                updaterGO = new GameObject();
                updaterGO.transform.SetParent(transform);
                updater = updaterGO.AddComponent<Updater>();
                updater.Register(Debug, period_ms);
                updater.enabled = true;
            }
            public string Dump() {
                List<string> lines = new List<string>();
                lines.Add($"Dump {dumpCount++} of effects");
                lines.Add("Dump: " + anchor.name);
                lines.Add("+++");
                if (TryGetControl(anchor, out var control)) {
                    lines.AddRange(control.Dump());
                    lines.Add("---");
                }
                else {
                    lines.Add("No control");
                }
                return string.Join("\n", lines);
            }
        }
    }
}
