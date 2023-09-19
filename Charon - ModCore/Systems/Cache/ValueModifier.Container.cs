using System;
using System.Collections.Generic;

namespace Charon.StarValor.ModCore {
    public partial class ValueModifier {
        public abstract class List : List<(ValueModifier modifier, int loadIndex)> {
            public static List GetDefault<T>() where T : List => (T)Activator.CreateInstance(typeof(T));
            public bool Enabled {
                get => _enabled;
                set {
                    _enabled = value;
                    foreach (var (modifier, _) in this)
                        modifier.Enabled = _enabled;
                }
            }
            bool _enabled = false;

            public void Add(ValueModifier modifier, int linkIndex = 0) => Add((modifier, linkIndex));
            public void Link(params object[] links) {
                foreach (var (modifier, _) in this)
                    modifier.Link(links);
            }
            public void Unlink() {
                foreach (var (modifier, _) in this)
                    modifier.Unlink();
            }
            public void Relink() {
                foreach (var (modifier, _) in this)
                    modifier.Relink();
            }
        }
    }
}
