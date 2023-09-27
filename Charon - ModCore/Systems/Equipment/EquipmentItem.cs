using System;
using System.Collections.Generic;

namespace Charon.StarValor.ModCore {
    public abstract class EquipmentItem : EquipmentExTemplate {
        List<Type> effectTypes = new List<Type>();
        protected void AddEffect<T>(EquipmentEx eq) where T : EffectExTemplate {
            effectTypes.Add(typeof(T));
            eq.AddEffect<T>();
        }
        public override void OnRemoving(IIndexableInstance instance) {
            var eq = (EquipmentEx)instance;
            foreach (var ef in effectTypes)
                eq.RemoveEffect(ef);
        }
    }
}
