using System.Reflection;

namespace Charon.StarValor.ModCore {
    public abstract class ModCorePlugin {
        public static string BaseGameGuid => null;
        public static BepInEx.Logging.ManualLogSource Log { get; set; } = null;

        protected ModCorePlugin() {
            if (!GetType().IsDefined(typeof(BepInEx.BepInPlugin)))
                throw new System.Exception("BepInPlugin attribute must be defined for class " + GetType().FullName);

            var attr = GetType().GetCustomAttribute<BepInEx.BepInPlugin>();
            this.Name = attr.Name;
            this.Guid = attr.GUID;
            this.Version = attr.Version;
            
            Loader.Add(this);
        }

        public string Name { get; }
        public string Guid { get; }
        public System.Version Version { get; }

        public static QualifiedName Qualify(string guid, string name) => GetQualifiedName(guid, name);
        public static QualifiedName GetQualifiedName(string guid, string name) => new QualifiedName(guid, name);
        public QualifiedName Qualify(string name) => Qualify(Guid, name);
        public QualifiedName GetQualifiedName(string name) => new QualifiedName(this.Guid, name);

        public virtual void OnPluginLoad() { }
        public virtual void OnPluginLoadLate() { }
        public virtual void OnGameSave() { }
        public virtual void OnGameLoad() { }
    }
}
