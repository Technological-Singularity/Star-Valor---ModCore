using BepInEx;
using System.Reflection;

namespace Charon.StarValor.ModCore {
    public abstract class ModCorePlugin : BaseUnityPlugin {
        public string Name { get; }
        public string Guid { get; }
        public System.Version Version { get; }

        public BepInEx.Logging.ManualLogSource Log { get; private set; }

        public ModCorePlugin() {
            if (!GetType().IsDefined(typeof(BepInPlugin)))
                throw new System.Exception("BepInPlugin attribute must be defined for class " + GetType().FullName);

            var attr = GetType().GetCustomAttribute<BepInPlugin>();
            this.Name = attr.Name;
            this.Guid = attr.GUID;
            this.Version = attr.Version;

            if (!(this is ModCore))
                ModCore.Add(this);

            Log = Logger;
        }

        public virtual void OnPluginLoad() { }
        public virtual void OnPluginLoadLate() { }
        public virtual void OnGameSave() { }
        public virtual void OnGameLoad() { }
    }
}
