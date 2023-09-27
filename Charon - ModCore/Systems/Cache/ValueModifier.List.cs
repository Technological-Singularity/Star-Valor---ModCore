using System;
using System.Collections.Generic;

namespace Charon.StarValor.ModCore {
    public partial class ValueModifier {
        public class Collection : Dictionary<object, ValueModifier> {
            public bool Enabled {
                get => _enabled;
                set {
                    _enabled = value;
                    foreach (var modifier in this.Values)
                        modifier.Enabled = _enabled;
                }
            }
            bool _enabled = false;

            public ValueModifier Add<T>(ValueModifier modifier) { 
                Add(typeof(T), modifier); 
                return modifier; 
            }

            public ValueModifier Get(object key) => this[key];
            public ValueModifier Get<T>() => this[typeof(TAmmo)];

            public void Link(object anchor) {
                foreach (var modifier in this.Values)
                    modifier.Link(anchor);
            }
            public void Unlink() {
                foreach (var modifier in this.Values)
                    modifier.Unlink();
            }
            public void Relink() {
                foreach (var modifier in this.Values)
                    modifier.Relink();
            }
        }
    }
}
