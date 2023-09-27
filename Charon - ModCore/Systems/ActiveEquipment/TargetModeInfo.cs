using System;
using UnityEngine;

namespace Charon.StarValor.ModCore {
    public class TargetModeInfo {
        public bool OnlyTargetSelf;
        public bool DefaultTargetSelf;
        public Func<Transform, bool> FilterTargets;

        public TargetModeInfo(bool onlyTargetSelf = true, bool defaultTargetSelf = true, Func<Transform, bool> filterTargets = null) =>
            (OnlyTargetSelf, DefaultTargetSelf, FilterTargets) = (onlyTargetSelf, defaultTargetSelf, filterTargets);
    }
}
