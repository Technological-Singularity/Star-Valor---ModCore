using UnityEngine;

namespace Charon.StarValor.ModCore {
    public partial class ValueModifier {
        public struct StandardFunction {
            public static readonly StandardFunction Pow = new StandardFunction(0, (float? value, ref float? incoming) => { if (value != null) incoming = Mathf.Pow((incoming ?? 0), value.Value); });
            public static readonly StandardFunction Multiply = new StandardFunction(100, (float? value, ref float? incoming) => { if (value != null) incoming = (incoming ?? 0) * value; });
            public static readonly StandardFunction Add = new StandardFunction(200, (float? value, ref float? incoming) => { if (value != null) incoming = (incoming ?? 0) + value; });
            public static readonly StandardFunction Min = new StandardFunction(1000, (float? value, ref float? incoming) => { if (value != null) incoming = Mathf.Min(incoming ?? float.MaxValue, value.Value); });
            public static readonly StandardFunction Max = new StandardFunction(1000, (float? value, ref float? incoming) => { if (value != null) incoming = Mathf.Max(incoming ?? float.MinValue, value.Value); });

            public int Priority;
            public ModifierFunc Function;
            public StandardFunction(int priority, ModifierFunc func) {
                Priority = priority;
                Function = func;
            }
            public static implicit operator ModifierFunc(StandardFunction a) => a.Function;
            public static implicit operator int(StandardFunction a) => a.Priority;
        }
    }
}
