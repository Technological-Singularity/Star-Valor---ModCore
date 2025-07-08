using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Charon.StarValor.ModCore {
    public interface IIndexable {
        Guid Guid { get; set; }
        QualifiedName QualifiedName { get; set; }
        bool UseQualifiedName { get; }
        bool UniqueType { get; }
        int RefCount { get; set; }
    }
    public static class IIndexable_Extensions {
        public static bool Ref(this IIndexable inst, Guid? staticGuid) {
            if (inst.RefCount == 0)
                IndexSystem.Instance.AllocateTypeInstance(inst, staticGuid);
            ++inst.RefCount;
            return inst.RefCount == 1;
        }
        public static bool Deref(this IIndexable inst) {
            if (--inst.RefCount > 0)
                return false;            
            IndexSystem.Instance.DeallocateTypeInstance(inst);
            return true;
        }
    }
}
