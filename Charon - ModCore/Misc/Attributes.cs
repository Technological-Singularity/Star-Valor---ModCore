using System;

namespace Charon.StarValor.ModCore {
    [AttributeUsage(AttributeTargets.Class)]
    public class HasPatchesAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
    public class SerializeAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class RegisterManualAttribute : Attribute { }
}
