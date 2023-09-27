//using Charon.StarValor.ModCore.Systems.EffectSystem;
//using System.Collections.Generic;
//using UnityEngine;

//////To do: bind fields from spaceship stats to effects, create some binding method for weapons/other objects

//namespace Charon.StarValor.ModCore {
//    [HasPatches]
//    public partial class EffectSystem {
//        static Dictionary<string, EffectDescriptor> registeredNames = new Dictionary<string, EffectDescriptor>();
//        public static bool HasRegistered(int id) {
//            if (IndexSystem.TryGetQualifiedName(IndexType.Effect, id, out var name))
//                return registeredNames.ContainsKey(name);
//            return false;
//        }
//        public static bool TryGetRegistered(string name, out EffectDescriptor value) => registeredNames.TryGetValue(name, out value);
//        public static bool TryGetRegistered(int id, out EffectDescriptor value) {
//            if (IndexSystem.TryGetQualifiedName(IndexType.Effect, id, out var name))
//                return registeredNames.TryGetValue(name, out value);
//            value = null;
//            return false;
//        }

//        PluginContext context;
//        public EffectSystem(ModCorePlugin plugin) {
//            this.context = plugin;
//        }
//        public List<EffectEx> GetEffects<T>(params float[] values) where T : EffectContainer {
//            var type = typeof(T);
//            List<EffectEx> wr = new List<EffectEx>();
//            var container = EffectContainer.GetDefault<T>();
//            int effectIdx = 0;
//            container.ForEach(o => {
//                int id = context.IndexSystem.Get(IndexType.Effect, o.Name);
//                if (!registeredNames.TryGetValue(o.Name, out var effect)) {
//                    effect = new EffectDescriptor() { type = id };
//                    ModCorePlugin.Log.LogWarning($"{o.Name} was not found in registered effects - returning dummy");
//                }
//                wr.Add(effect.CreateEffect(values[effectIdx++]));
//            });
//            return wr;
//        }
//        static int RegisterQualified(string qualifiedName, UniqueLevel uniqueLevel = UniqueLevel.None, float rarityMod = 0, string description = null) {
//            IndexSystem.TrySetQualified(IndexType.Effect, qualifiedName, out var id);
//            EffectDescriptor effect = new EffectDescriptor() {
//                type = id,
//                description = description,
//                rarityMod = rarityMod,
//                uniqueLevel = (int)uniqueLevel,
//            };
//            ModCorePlugin.Log.LogWarning($"Registered effect {id} as {qualifiedName}");
//            registeredNames[qualifiedName] = effect;
//            return id;
//        }
//        public static int Register(string pluginGuid, string name, UniqueLevel uniqueLevel = UniqueLevel.None, float rarityMod = 0, string description = null) => RegisterQualified(ModCorePlugin.Qualify(pluginGuid, name), uniqueLevel, rarityMod, description);
//        public int Register(string name, UniqueLevel uniqueLevel = UniqueLevel.None, float rarityMod = 0, string description = null, bool invertRarityMod = false) => Register(context.Guid, name, uniqueLevel, (invertRarityMod ? -1 : 1) * Mathf.Abs(rarityMod), description);
//    }
//}
