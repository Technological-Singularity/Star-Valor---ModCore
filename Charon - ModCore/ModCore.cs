//Idea: Force Beam - maybe grapple at range, throw asteroids at people, etc?

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using HarmonyLib;

namespace Charon.StarValor.ModCore {
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    [BepInProcess("Star Valor.exe")]
    public sealed class ModCore : ModCorePlugin {
        const string pluginGuid = "ModCore.ModCore";
        const string pluginName = "StarValor ModCore";
        const string pluginVersion = "0.0.0.0";
        public static ModCorePlugin Instance { get; private set; }
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

        List<Harmony> patchedPlugins { get; set; }
        void Awake() => Instance = this;

        void Start() => InitializeAll();
        
        public void InitializeAll() {
            if (patchedPlugins != null)
                return;

            Log.LogMessage($"{pluginName} initializing");
            patchedPlugins = new List<Harmony>();
            HashSet<Type> uniqueTypes = new HashSet<Type>();
            var patching = AppDomain.CurrentDomain.GetAssemblies().SelectMany(o => o.GetTypes().Where(t => t.IsDefined(typeof(HasPatchesAttribute))));
            Log.LogMessage($"Patching {patching.Count()} types");
            foreach (var type in patching) {
                if (uniqueTypes.Contains(type))
                    continue;
                uniqueTypes.Add(type);
                Log.LogMessage($"  Patch: {type.FullName}");
                patchedPlugins.Add(Harmony.CreateAndPatchAll(type));
            }
            uniqueTypes = null;

            if (pluginsByGuid.Count == 0)
                return;

            Log.LogMessage($"Loading {pluginsByGuid.Count} plugins");
            foreach (var plugin in pluginsByGuid.Values) {
                Log.LogMessage($"  Load: {plugin.Guid}");
                plugin.OnPluginLoad();
            }
            foreach (var plugin in pluginsByGuid.Values) {
                Log.LogMessage($"  LoadLate: {plugin.Guid}");
                plugin.OnPluginLoadLate();
            }
            Log.LogMessage($"Initializing EquipmentDB");

            EquipmentDB.ClearDatabase();
            EquipmentDB.LoadDatabaseForce();

            Log.LogMessage($"{pluginName} done initializing");
        }
        public void UninitializeAll() {
            if (patchedPlugins == null)
                return;

            foreach (var p in patchedPlugins)
                p?.UnpatchSelf();
            patchedPlugins = null;
        }
    }
}
