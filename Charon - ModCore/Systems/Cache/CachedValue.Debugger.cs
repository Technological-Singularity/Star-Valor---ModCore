using System.Collections.Generic;
using UnityEngine;

namespace Charon.StarValor.ModCore {
    public partial class CachedValue {
        public class Debugger : MonoBehaviour {
            protected GameObject updaterGO;
            protected Updater updater;
            int dumpCount = 0;
            protected void Debug() {
                ModCorePlugin.Log.LogWarning($"Dump {dumpCount++} of effects" + Dump());
            }
            public Debugger() {
                updaterGO = new GameObject();
                updaterGO.transform.SetParent(transform);

                updater = updaterGO.AddComponent<Updater>();
                updater.Register(Debug, 1.0f);
                updater.enabled = true;
            }
            public string Dump() {
                List<string> lines = new List<string>();
                lines.Add("Dump: " + transform.name);
                if (TryGetControl(transform, out var control)) {
                    lines.AddRange(control.Dump());
                    lines.Add("---");
                }
                return string.Join("\n", lines);
            }
        }
    }
}
