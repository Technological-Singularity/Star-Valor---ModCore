using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using HarmonyLib;

namespace Charon.StarValor.ModCore {
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    [BepInProcess("Star Valor.exe")]
    public sealed class Loader : BaseUnityPlugin {
        const string pluginGuid = "modcore.loader";
        const string pluginName = "Star Valor Mod Core";
        const string pluginVersion = "0.0.0.0";
        #region Lookup
        static readonly Dictionary<string, ModCorePlugin> pluginsByGuid = new Dictionary<string, ModCorePlugin>();
        public static bool HasGuid(string guid) => pluginsByGuid.ContainsKey(guid);
        public static ModCorePlugin GetPluginByGuid(string guid) => pluginsByGuid[guid];
        public static bool TryGetPluginByGuid(string guid, out ModCorePlugin plugin) => pluginsByGuid.TryGetValue(guid, out plugin);
        public static void ForEach(Action<ModCorePlugin> action) {
            foreach (var kvp in pluginsByGuid)
                action(kvp.Value);
        }
        #endregion
        public static void Add(ModCorePlugin plugin) {
            if (plugin.Guid is null)
                throw new Exception(plugin.GetType().FullName + " does not appear to be instantiated (ensure base constructor was executed)");
            
            if (pluginsByGuid.ContainsKey(plugin.Guid))
                throw new ArgumentException($"Guid {plugin.Guid} already exists", "plugin");

            pluginsByGuid[plugin.Guid] = plugin;
        }

        public Loader() => ModCorePlugin.Log = Logger;
        List<Harmony> patches;

        void Start() => InitializeAll();
        
        public void InitializeAll() {
            if (patches != null)
                return;
            
            patches = new List<Harmony>();
            HashSet<Type> uniqueTypes = new HashSet<Type>();
            foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(o => o.GetTypes().Where(t => t.IsDefined(typeof(HasPatchesAttribute))))) {
                if (uniqueTypes.Contains(type))
                    continue;
                uniqueTypes.Add(type);
                Logger.LogMessage($"Patch: {type.FullName}");
                patches.Add(Harmony.CreateAndPatchAll(type));
            }
            uniqueTypes = null;

            if (pluginsByGuid.Count == 0)
                return;

            ModCorePlugin.Log.LogMessage($"Loading {pluginsByGuid.Count} plugins");
            foreach (var plugin in pluginsByGuid.Values) {
                Logger.LogMessage($"First pass: {plugin.Guid}");
                plugin.OnPluginLoad();
            }
            foreach (var plugin in pluginsByGuid.Values) {
                Logger.LogMessage($"Late pass: {plugin.Guid}");
                plugin.OnPluginLoadLate();
            }
            ModCorePlugin.Log.LogMessage($"Done loading");
        }
        public void UninitializeAll() {
            if (patches == null)
                return;

            foreach (var p in patches)
                p?.UnpatchSelf();
            patches = null;
        }
    }
}
