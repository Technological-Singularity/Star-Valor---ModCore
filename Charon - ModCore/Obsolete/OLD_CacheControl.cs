//using System.Collections.Generic;
//using UnityEngine;

//namespace StarValor_ModCore {
//    public class CacheControl : MonoBehaviour, ISerializationCallbackReceiver {
//        public delegate bool LoadFunc(int typeId, float value);
//        public delegate bool InvalidateFunc(object sender, int typeId, float oldValue, ref float newValue);

//        class UpdateableValue {
//            int typeId;
//            public float Value { get; private set; }


//            List<LoadFunc> onLoad = null;
//            List<(object sender, InvalidateFunc func)> onInvalidating = null;

//            public UpdateableValue(int typeId, float value) {
//                this.typeId = typeId;
//                Value = value;
//            }
//            public void Load() {
//                if (onLoad == null)
//                    return;
//                foreach (var func in onLoad)
//                    func.Invoke(typeId, Value);
//            }
//            public void OnLoad(LoadFunc func) {
//                if (onLoad == null)
//                    onLoad = new List<LoadFunc>();
//                onLoad.Add(func);
//            }
//            public void OnInvalidate(object sender, InvalidateFunc func) {
//                if (onInvalidating == null)
//                    onInvalidating = new List<(object sender, InvalidateFunc func)>();
//                onInvalidating.Add((sender, func));
//            }
//            public void SetValue(object sender, float newValue) {
//                if (newValue == Value)
//                    return;

//                foreach ((var originator, var func) in onInvalidating) {
//                    if (sender == originator)
//                        continue;
//                    if (!func.Invoke(sender, typeId, Value, ref newValue))
//                        break;
//                }
//                Value = newValue;
//            }
//        }
//        Dictionary<int, UpdateableValue> effectValues = new Dictionary<int, UpdateableValue>();

//        [SerializeField] List<int> _keys;
//        [SerializeField] List<float> _values;

//        public void OnBeforeSerialize() {
//            _keys = new List<int>();
//            _values = new List<float>();
//            foreach (var pair in effectValues) {
//                _keys.Add(pair.Key);
//                _values.Add(pair.Value.Value);
//            }
//        }
//        public void OnAfterDeserialize() {
//            effectValues.Clear();
//            for (int i = 0; i < _keys.Count; ++i)
//                effectValues[_keys[i]] = new UpdateableValue(_keys[i], _values[i]);
//            _keys = null;
//            _values = null;
//        }
//        void Start() {
//            foreach (var o in effectValues.Values)
//                o.Load();
//        }
//        public float Get(int typeId) => effectValues.TryGetValue(typeId, out var wr) ? wr.Value : default;
//        public void Set(object sender, int typeId, float value) => GetOrCreate(typeId).SetValue(sender, value);

//        UpdateableValue GetOrCreate(int typeId) {
//            if (effectValues.TryGetValue(typeId, out var uv))
//                return uv;
//            uv = new UpdateableValue(typeId, default);
//            effectValues.Add(typeId, uv);
//            return uv;
//        }
//        public void OnLoad(int typeId, LoadFunc func) => GetOrCreate(typeId).OnLoad(func);
//        public void OnInvalidate(object sender, int typeId, InvalidateFunc func) => GetOrCreate(typeId).OnInvalidate(sender, func);
//        public string Dump(ModCorePlugin context) {
//            string wr = "";
//            foreach(var kvp in effectValues) {
//                if (IndexSystem.TryGetName(IndexType.Effect, context.Guid, kvp.Key, out var name))
//                    wr += "\n" + name + " : " + kvp.Value.Value;
//            }
//            return wr;
//        }
//    }
//}


//        //Patched version of base version; used here to preserve base version
//        static float GetRarityMod(int rarity, float rarityMod, float effectMod) {
//            float result = 1f;
//            switch (rarity) {
//                case 0:
//                    result = 1f / (1f + 1f * rarityMod * effectMod);
//                    break;
//                case 1:
//                    result = 1f;
//                    break;
//                case 2:
//                    result = 1f + 0.2f * rarityMod * effectMod;
//                    break;
//                case 3:
//                    result = 1f + 0.5f * rarityMod * effectMod;
//                    break;
//                case 4:
//                    result = 1f + 1f * rarityMod * effectMod;
//                    break;
//                case 5:
//                    result = 1f + 1.6f * rarityMod * effectMod;
//                    break;
//            }

//            return result;
//        }

//        public List<Effect> CreateFromType(Type type, float[] values, float effectMultiplier) {
//            List<Effect> wr = new List<Effect>();
//            int i = 0;
//            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(t => t.IsDefined(typeof(EffectAttribute)))) {
//                var attr = (EffectAttribute)field.GetCustomAttribute(typeof(EffectAttribute));
//                int id = context.IndexSystem.Get(IndexType.Effect, attr.Name);
//            }
//            i = 0;
//            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(t => t.IsDefined(typeof(EffectAttribute)))) {
//                var attr = (EffectAttribute)field.GetCustomAttribute(typeof(EffectAttribute));
//                int id = context.IndexSystem.Get(IndexType.Effect, attr.Name);
//                if (!registered.TryGetValue(id, out var effect)) {
//                    effect = new EffectDescriptor() { type = id };
//                    ModCorePlugin.Log.LogWarning($"{attr.Name} was not found in registered effects - returning dummy");
//                }
//                wr.Add(effect.CreateEffect(values[i++], effectMultiplier));

//            }
//            return wr;
//        }
//        public void Pull(object dst, Equipment src, int rarity) {
//            var type = dst.GetType();
//            int i = 0;
//            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(t => t.IsDefined(typeof(EffectAttribute)))) {
//                var attr = (EffectAttribute)field.GetCustomAttribute(typeof(EffectAttribute));
//                var effect = src.effects[i++];

//                var value = effect.value * CacheSystem2.GetRarityMod(rarity, src.rarityMod, effect.mod);
//                field.SetValue(dst, value);
//            }
//        }
//        public void Pull(object dst, Transform src) {
//            var type = dst.GetType();
//            var control = src.GetComponent<CacheControl>();
//            if (control == null)
//                control = src.gameObject.AddComponent<CacheControl>();

//            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(t => t.IsDefined(typeof(EffectAttribute)))) {
//                var attr = (EffectAttribute)field.GetCustomAttribute(typeof(EffectAttribute));
//                int id = context.IndexSystem.Get(IndexType.Effect, attr.Name);
//                field.SetValue(dst, id);
//            }
//        }
//        public void OnLoad(object dst, Transform src, CacheControl.LoadFunc func = null) {
//            var type = dst.GetType();
//            var control = src.GetComponent<CacheControl>();
//            if (control == null)
//                control = src.gameObject.AddComponent<CacheControl>();

//            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(t => t.IsDefined(typeof(EffectAttribute)))) {
//                var attr = (EffectAttribute)field.GetCustomAttribute(typeof(EffectAttribute));
//                var ufunc = func;
//                if (ufunc == null)
//                    ufunc = (_, value) => { field.SetValue(dst, value); return true; };
//                int id = context.IndexSystem.Get(IndexType.Effect, attr.Name);
//                control.OnLoad(id, ufunc);
//            }
//        }
//        public void OnInvalidate(object dst, Transform src, CacheControl.InvalidateFunc updateFunc = null, Action updateAllFunc = null) {
//            var type = dst.GetType();
//            var control = src.GetComponent<CacheControl>();
//            if (control == null)
//                control = src.gameObject.AddComponent<CacheControl>();

//            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(t => t.IsDefined(typeof(EffectAttribute)))) {
//                var attr = (EffectAttribute)field.GetCustomAttribute(typeof(EffectAttribute));
//                var ufunc = updateFunc;
//                if (ufunc == null) {
//                    bool setFieldValueDefault(object sender, int typeId, float value, ref float newValue) { 
//                        field.SetValue(dst, newValue);
//                        if (updateAllFunc != null)
//                            updateAllFunc.Invoke();
//                        return true;
//                    }
//                    ufunc = setFieldValueDefault;
//                }
//                int id = context.IndexSystem.Get(IndexType.Effect, attr.Name);
//                control.OnInvalidate(dst, id, ufunc);
//            }
//        }
//        public void OnInvalidate(ShipStats dst, Transform src) {
//            var control = src.GetComponent<CacheControl>();
//            if (control == null)
//                control = src.gameObject.AddComponent<CacheControl>();
//            foreach (var field in typeof(ShipStats).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(t => t.FieldType == typeof(float))) {
//                string name = "stats_" + field.Name;

//            }
//        }
//        public void Push(Transform dst, object typeInstance, Equipment src, int rarity, float multiplier = 1, bool cumulative = true) {
//            var type = typeInstance.GetType();
//            var control = dst.GetComponent<CacheControl>();
//            if (control == null)
//                control = dst.gameObject.AddComponent<CacheControl>();

//            int i = 0;
//            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(t => t.IsDefined(typeof(EffectAttribute)))) {
//                var attr = (EffectAttribute)field.GetCustomAttribute(typeof(EffectAttribute));
//                var effect = src.effects[i++];

//                var value = effect.value * CacheSystem2.GetRarityMod(rarity, src.rarityMod, effect.mod);
//                int id = context.IndexSystem.Get(IndexType.Effect, attr.Name);
//                if (cumulative)
//                    control.Set(src, id, control.Get(id) + value * multiplier);
//                else
//                    control.Set(src, id, value * multiplier);
//            }
//        }
//        public void Push(Transform dst, object src, float multiplier = 1, bool cumulative = true) {
//            var type = src.GetType();
//            var control = dst.GetComponent<CacheControl>();
//            if (control == null)
//                control = dst.gameObject.AddComponent<CacheControl>();

//            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(t => t.IsDefined(typeof(EffectAttribute)))) {
//                var attr = (EffectAttribute)field.GetCustomAttribute(typeof(EffectAttribute));
//                var value = (float)field.GetValue(src);
//                int id = context.IndexSystem.Get(IndexType.Effect, attr.Name);
//                if (cumulative)
//                    control.Set(src, id, control.Get(id) + value * multiplier);
//                else
//                    control.Set(src, id, value * multiplier);
//            }
//        }
//        public string Dump(object src) {
//            var type = src.GetType();
//            string wr = $"\n[ Type: {src.GetType().Name} ]";
//            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(t => t.IsDefined(typeof(EffectAttribute)));
//            wr += $"\nFound {fields.Count()} fields";
//            foreach (var field in fields) {
//                var attr = (EffectAttribute)field.GetCustomAttribute(typeof(EffectAttribute));
//                var value = (float)field.GetValue(src);
//                int id = context.IndexSystem.Get(IndexType.Effect, attr.Name);
//                wr += "\n" + attr.Name + " : " + field.GetValue(src);
//            }
//            return wr;
//        }
//    }
//}


//public static class Equipment_DeflectorShieldz {
//	readonly static Layer[] targets_asteroid = new Layer[] { Layer.Asteroid };
//	readonly static Layer[] targets_civilian = new Layer[] { Layer.Asteroid, Layer.Collectible, Layer.Object, Layer.Spaceship, Layer.Station };
//	readonly static Layer[] targets_combat = new Layer[] { Layer.Default, Layer.SmallObject, Layer.Missiles };
//	readonly static Layer[] targets_all = new Layer[] { Layer.Asteroid, Layer.Collectible, Layer.Object, Layer.Spaceship, Layer.Station, Layer.Default, Layer.SmallObject, Layer.Missiles };

//	static Equipment InitializeEquipment(string name, string typeName, float effectMultiplier, ShipClassLevel minClass, int space, float energy, int techLevel, float rarityCostMod = 1, float rarityMod = 1, Layer[] targets = null, float force = 0, float range = 0, float hardness = 0, float emitters = 0, float repulsion = 0, float deflection = 0, float dispersion = 0, string description = null) {
//		var eq = ScriptableObject.CreateInstance<Equipment>();
//		eq.name = typeName;
//		eq.id = Core.Context.IndexSystem.Set(IndexType.Equipment, typeName);
//		eq.activated = true;
//		eq.activeEquipmentIndex = Core.Context.IndexSystem.Get(IndexType.ActiveEffect, typeof(AE_DeflectorShield).FullName);
//		eq.defaultKey = KeyCode.X;
//		eq.description = description + $"\n\nDefault key: '{Enum.GetName(typeof(KeyCode), eq.defaultKey)}'";
//		eq.dropLevel = DropLevel.Normal;
//		eq.effects = null;// Core.Context.EffectSystem.GetEffects<Buff_DeflectorShield.Data>(effectMultiplier, Extensions.GetLayerMask(targets), force, range, hardness, emitters, repulsion, deflection, dispersion);
//		eq.energyCost = energy;
//		eq.equipName = name;
//		eq.minShipClass = minClass;
//		eq.rarityCostMod = rarityCostMod;
//		eq.rarityMod = rarityMod;
//		eq.space = space;
//		eq.sprite = EquipmentDB.GetEquipmentByIndex(0).sprite;
//		eq.techLevel = techLevel;
//		eq.type = EquipmentType.Utility;
//		eq.uniqueReplacement = true;
//		return eq;
//	}

//	public static List<Equipment> GetEquipment() {
//		//List<Equipment> wr = new List<Equipment>();
//		//string[] sizes = new string[] { "", "Large ", "Capital " };
//		//string[] targeting = new string[] { "Multiplex ", "Asteroid ", "Civilian ", "Combat " };
//		//string[] names = new string[] { "Vectored", "Repulsor", "Dispersion", "Deflector" };



//		return new List<Equipment> {
//			//Repulse
//			InitializeEquipment("Multiplex Repulsor Array",             typeName: "array_repulsor_0",               effectMultiplier: 0,        minClass: ShipClassLevel.Shuttle,       space: 0,   energy: 1,  techLevel: 0,   targets: targets_all,       force: 90,      range: 6,   hardness: 120,  emitters: 1,		repulsion: 1.00f, deflection: 0.00f, dispersion: 0.00f, description: "Protects the ship from collisions by vigorously repulsing approaching obstacles and hostile objects."),
//			InitializeEquipment("Large Multiplex Repulsor Array",       typeName: "array_repulsor_1",               effectMultiplier: 0.5f,     minClass: ShipClassLevel.Corvette,      space: 0,   energy: 1,  techLevel: 0,   targets: targets_all,       force: 120,     range: 25,  hardness: 120,  emitters: 2,		repulsion: 1.00f, deflection: 0.00f, dispersion: 0.00f, description: "Protects the ship from collisions by vigorously repulsing approaching obstacles and hostile objects."),
//			InitializeEquipment("Capital Multiplex Repulsor Array",     typeName: "array_repulsor_2",               effectMultiplier: 1f,       minClass: ShipClassLevel.Cruiser,       space: 0,   energy: 1,  techLevel: 0,   targets: targets_all,       force: 160,     range: 45,  hardness: 120,  emitters: 3,		repulsion: 1.00f, deflection: 0.00f, dispersion: 0.00f, description: "Protects the ship from collisions by vigorously repulsing approaching obstacles and hostile objects."),

//			InitializeEquipment("Asteroid Repulsor Array",              typeName: "array_repulsor_asteroid_0",      effectMultiplier: 0,        minClass: ShipClassLevel.Shuttle,       space: 0,   energy: 1,  techLevel: 0,   targets: targets_asteroid,  force: 90,      range: 6,   hardness: 120,  emitters: 0.75f,	repulsion: 0.80f, deflection: 0.20f, dispersion: 0.00f, description: "Protects the ship from damage by vigorously repulsing approaching asteroids."),
//			InitializeEquipment("Large Asteroid Repulsor Array",        typeName: "array_repulsor_asteroid_1",      effectMultiplier: 0.5f,     minClass: ShipClassLevel.Corvette,      space: 0,   energy: 1,  techLevel: 0,   targets: targets_asteroid,  force: 300,     range: 25,  hardness: 120,  emitters: 0.75f,	repulsion: 0.80f, deflection: 0.20f, dispersion: 0.00f, description: "Protects the ship from damage by vigorously repulsing approaching asteroids."),
//			InitializeEquipment("Capital Asteroid Repulsor Array",      typeName: "array_repulsor_asteroid_2",      effectMultiplier: 1f,       minClass: ShipClassLevel.Cruiser,       space: 0,   energy: 1,  techLevel: 0,   targets: targets_asteroid,  force: 520,     range: 45,  hardness: 120,  emitters: 0.75f,	repulsion: 0.80f, deflection: 0.20f, dispersion: 0.00f, description: "Protects the ship from damage by vigorously repulsing approaching asteroids."),

//			InitializeEquipment("Civilian Repulsor Array",              typeName: "array_repulsor_civilian_0",      effectMultiplier: 0,        minClass: ShipClassLevel.Shuttle,       space: 0,   energy: 1,  techLevel: 0,   targets: targets_civilian,  force: 90,      range: 6,   hardness: 120,  emitters: 0.75f,	repulsion: 0.80f, deflection: 0.20f, dispersion: 0.00f, description: "Protects the ship from collisions by vigorously repulsing approaching obstacles."),
//			InitializeEquipment("Large Civilian Repulsor Array",        typeName: "array_repulsor_civilian_1",      effectMultiplier: 0.5f,     minClass: ShipClassLevel.Corvette,      space: 0,   energy: 1,  techLevel: 0,   targets: targets_civilian,  force: 300,     range: 25,  hardness: 120,  emitters: 0.75f,	repulsion: 0.80f, deflection: 0.20f, dispersion: 0.00f, description: "Protects the ship from collisions by vigorously repulsing approaching obstacles."),
//			InitializeEquipment("Capital Civilian Repulsor Array",      typeName: "array_repulsor_civilian_2",      effectMultiplier: 1f,       minClass: ShipClassLevel.Cruiser,       space: 0,   energy: 1,  techLevel: 0,   targets: targets_civilian,  force: 520,     range: 45,  hardness: 120,  emitters: 0.75f,	repulsion: 0.80f, deflection: 0.20f, dispersion: 0.00f, description: "Protects the ship from collisions by vigorously repulsing approaching obstacles."),

//			InitializeEquipment("Combat Repulsor Array",                typeName: "array_repulsor_combat_0",        effectMultiplier: 0,        minClass: ShipClassLevel.Shuttle,       space: 0,   energy: 1,  techLevel: 0,   targets: targets_combat,    force: 90,      range: 6,   hardness: 120,  emitters: 0.75f,	repulsion: 0.80f, deflection: 0.20f, dispersion: 0.00f, description: "Protects the ship from damage by vigorously repulsing projectiles and drones."),
//			InitializeEquipment("Large Combat Repulsor Array",          typeName: "array_repulsor_combat_1",        effectMultiplier: 0.5f,     minClass: ShipClassLevel.Corvette,      space: 0,   energy: 1,  techLevel: 0,   targets: targets_combat,    force: 300,     range: 25,  hardness: 120,  emitters: 0.75f,	repulsion: 0.80f, deflection: 0.20f, dispersion: 0.00f, description: "Protects the ship from damage by vigorously repulsing projectiles and drones."),
//			InitializeEquipment("Capital Combat Repulsor Array",        typeName: "array_repulsor_combat_2",        effectMultiplier: 1f,       minClass: ShipClassLevel.Cruiser,       space: 0,   energy: 1,  techLevel: 0,   targets: targets_combat,    force: 520,     range: 45,  hardness: 120,  emitters: 0.75f,	repulsion: 0.80f, deflection: 0.20f, dispersion: 0.00f, description: "Protects the ship from damage by vigorously repulsing projectiles and drones."),

//			//Vector/Slip
//			InitializeEquipment("Multiplex Vectored Array",				typeName: "array_vectored_0",				effectMultiplier: 0,        minClass: ShipClassLevel.Shuttle,       space: 0,   energy: 1,  techLevel: 0,   targets: targets_all,       force: 50,      range: 14,  hardness: 80,   emitters: 2,        repulsion: 0.00f, deflection: 1.00f, dispersion: 0.00f, description: "Protects the ship from collisions by deflecting approaching obstacles and hostile objects."),
//			InitializeEquipment("Large Multiplex Vectored Array",		typeName: "array_vectored_1",				effectMultiplier: 0.5f,     minClass: ShipClassLevel.Corvette,      space: 0,   energy: 1,  techLevel: 0,   targets: targets_all,       force: 70,		range: 48,  hardness: 80,   emitters: 4,		repulsion: 0.00f, deflection: 1.00f, dispersion: 0.00f, description: "Protects the ship from collisions by deflecting approaching obstacles and hostile objects."),
//			InitializeEquipment("Capital Multiplex Vectored Array",		typeName: "array_vectored_2",				effectMultiplier: 1f,       minClass: ShipClassLevel.Cruiser,       space: 0,   energy: 1,  techLevel: 0,   targets: targets_all,		force: 90,		range: 96,  hardness: 80,   emitters: 6,		repulsion: 0.00f, deflection: 1.00f, dispersion: 0.00f, description: "Protects the ship from collisions by deflecting approaching obstacles and hostile objects."),

//			InitializeEquipment("Asteroid Vectored Array",				typeName: "array_vectored_asteroid_0",		effectMultiplier: 0,        minClass: ShipClassLevel.Shuttle,       space: 0,   energy: 1,  techLevel: 0,   targets: targets_asteroid,  force: 50,      range: 14,  hardness: 80,   emitters: 2,		repulsion: 0.00f, deflection: 1.00f, dispersion: 0.00f, description: "Protects the ship from damage by deflecting approaching asteroids."),
//			InitializeEquipment("Large Asteroid Vectored Array",		typeName: "array_vectored_asteroid_1",		effectMultiplier: 0.5f,     minClass: ShipClassLevel.Corvette,      space: 0,   energy: 1,  techLevel: 0,   targets: targets_asteroid,  force: 180,     range: 48,  hardness: 80,   emitters: 2,		repulsion: 0.00f, deflection: 1.00f, dispersion: 0.00f, description: "Protects the ship from damage by deflecting approaching asteroids."),
//			InitializeEquipment("Capital Asteroid Vectored Array",		typeName: "array_vectored_asteroid_2",		effectMultiplier: 1f,       minClass: ShipClassLevel.Cruiser,       space: 0,   energy: 1,  techLevel: 0,   targets: targets_asteroid,  force: 270,     range: 96,  hardness: 80,   emitters: 3,		repulsion: 0.00f, deflection: 1.00f, dispersion: 0.00f, description: "Protects the ship from damage by deflecting approaching asteroids."),

//			InitializeEquipment("Civilian Vectored Array",				typeName: "array_vectored_civilian_0",		effectMultiplier: 0,        minClass: ShipClassLevel.Shuttle,       space: 0,   energy: 1,  techLevel: 0,   targets: targets_civilian,  force: 50,      range: 14,  hardness: 80,   emitters: 1,		repulsion: 0.00f, deflection: 1.00f, dispersion: 0.00f, description: "Protects the ship from damage by deflecting approaching obstacles."),
//			InitializeEquipment("Large Civilian Vectored Array",		typeName: "array_vectored_civilian_1",		effectMultiplier: 0.5f,     minClass: ShipClassLevel.Corvette,      space: 0,   energy: 1,  techLevel: 0,   targets: targets_civilian,  force: 180,     range: 48,  hardness: 80,   emitters: 2,		repulsion: 0.00f, deflection: 1.00f, dispersion: 0.00f, description: "Protects the ship from damage by deflecting approaching obstacles."),
//			InitializeEquipment("Capital Civilian Vectored Array",		typeName: "array_vectored_civilian_2",		effectMultiplier: 1f,       minClass: ShipClassLevel.Cruiser,       space: 0,   energy: 1,  techLevel: 0,   targets: targets_civilian,  force: 270,     range: 96,  hardness: 80,   emitters: 3,		repulsion: 0.00f, deflection: 1.00f, dispersion: 0.00f, description: "Protects the ship from damage by deflecting approaching obstacles."),

//			InitializeEquipment("Combat Vectored Array",				typeName: "array_vectored_combat_0",		effectMultiplier: 0,        minClass: ShipClassLevel.Shuttle,       space: 0,   energy: 1,  techLevel: 0,   targets: targets_combat,    force: 50,      range: 14,  hardness: 80,   emitters: 1,		repulsion: 0.00f, deflection: 1.00f, dispersion: 0.00f, description: "Protects the ship from damage by deflecting projectiles and drones."),
//			InitializeEquipment("Large Combat Vectored Array",			typeName: "array_vectored_combat_1",		effectMultiplier: 0.5f,     minClass: ShipClassLevel.Corvette,      space: 0,   energy: 1,  techLevel: 0,   targets: targets_combat,    force: 180,     range: 48,  hardness: 80,   emitters: 2,		repulsion: 0.00f, deflection: 1.00f, dispersion: 0.00f, description: "Protects the ship from damage by deflecting projectiles and drones."),
//			InitializeEquipment("Capital Combat Vectored Array",		typeName: "array_vectored_combat_2",		effectMultiplier: 1f,       minClass: ShipClassLevel.Cruiser,       space: 0,   energy: 1,  techLevel: 0,   targets: targets_combat,    force: 270,     range: 96,  hardness: 80,   emitters: 3,		repulsion: 0.00f, deflection: 1.00f, dispersion: 0.00f, description: "Protects the ship from damage by deflecting projectiles and drones."),

//			//Dispersion
//			InitializeEquipment("Multiplex Dispersion Array",			typeName: "array_dispersion_0",				effectMultiplier: 0,        minClass: ShipClassLevel.Shuttle,       space: 0,   energy: 1,  techLevel: 0,   targets: targets_all,       force: 50,      range: 17,  hardness: 80,	emitters: 2,		repulsion: 0.02f, deflection: 0.10f, dispersion: 0.90f, description: "Protects the ship from collisions by slowing approaching obstacles."),
//			InitializeEquipment("Large Multiplex Dispersion Array",		typeName: "array_dispersion_1",				effectMultiplier: 0.5f,     minClass: ShipClassLevel.Corvette,      space: 0,   energy: 1,  techLevel: 0,   targets: targets_all,       force: 180,		range: 60,  hardness: 80,	emitters: 4,		repulsion: 0.02f, deflection: 0.10f, dispersion: 0.90f, description: "Protects the ship from collisions by slowing approaching obstacles."),
//			InitializeEquipment("Capital Multiplex Dispersion Array",	typeName: "array_dispersion_2",				effectMultiplier: 1f,       minClass: ShipClassLevel.Cruiser,       space: 0,   energy: 1,  techLevel: 0,   targets: targets_all,       force: 270,     range: 115, hardness: 80,	emitters: 6,		repulsion: 0.02f, deflection: 0.10f, dispersion: 0.90f, description: "Protects the ship from collisions by slowing approaching obstacles."),

//			InitializeEquipment("Asteroid Dispersion Array",			typeName: "array_dispersion_asteroid_0",	effectMultiplier: 0,        minClass: ShipClassLevel.Shuttle,       space: 0,   energy: 1,  techLevel: 0,   targets: targets_asteroid,  force: 50,      range: 17,  hardness: 80,	emitters: 1,		repulsion: 0.02f, deflection: 0.10f, dispersion: 0.90f, description: "Protects the ship from damage by slowing approaching asteroids."),
//			InitializeEquipment("Large Asteroid Dispersion Array",		typeName: "array_dispersion_asteroid_1",	effectMultiplier: 0.5f,     minClass: ShipClassLevel.Corvette,      space: 0,   energy: 1,  techLevel: 0,   targets: targets_asteroid,  force: 180,		range: 48,  hardness: 80,	emitters: 2,		repulsion: 0.02f, deflection: 0.10f, dispersion: 0.90f, description: "Protects the ship from damage by slowing approaching asteroids."),
//			InitializeEquipment("Capital Asteroid Dispersion Array",	typeName: "array_dispersion_asteroid_2",	effectMultiplier: 1f,       minClass: ShipClassLevel.Cruiser,       space: 0,   energy: 1,  techLevel: 0,   targets: targets_asteroid,  force: 270,     range: 96,  hardness: 80,	emitters: 3,		repulsion: 0.02f, deflection: 0.10f, dispersion: 0.90f, description: "Protects the ship from damage by slowing approaching asteroids."),

//			InitializeEquipment("Civilian Dispersion Array",			typeName: "array_dispersion_civilian_0",	effectMultiplier: 0,        minClass: ShipClassLevel.Shuttle,       space: 0,   energy: 1,  techLevel: 0,   targets: targets_civilian,  force: 50,      range: 17,  hardness: 80,	emitters: 1,		repulsion: 0.02f, deflection: 0.10f, dispersion: 0.90f, description: "Protects the ship from collisions by slowing approaching obstacles."),
//			InitializeEquipment("Large Civilian Dispersion Array",		typeName: "array_dispersion_civilian_1",	effectMultiplier: 0.5f,     minClass: ShipClassLevel.Corvette,      space: 0,   energy: 1,  techLevel: 0,   targets: targets_civilian,  force: 180,		range: 48,  hardness: 80,	emitters: 2,		repulsion: 0.02f, deflection: 0.10f, dispersion: 0.90f, description: "Protects the ship from collisions by slowing approaching obstacles."),
//			InitializeEquipment("Capital Civilian Dispersion Array",	typeName: "array_dispersion_civilian_2",	effectMultiplier: 1f,       minClass: ShipClassLevel.Cruiser,       space: 0,   energy: 1,  techLevel: 0,   targets: targets_civilian,  force: 270,     range: 96,  hardness: 80,	emitters: 3,		repulsion: 0.02f, deflection: 0.10f, dispersion: 0.90f, description: "Protects the ship from collisions by slowing approaching obstacles."),

//			InitializeEquipment("Combat Dispersion Array",				typeName: "array_dispersion_combat_0",		effectMultiplier: 0,        minClass: ShipClassLevel.Shuttle,       space: 0,   energy: 1,  techLevel: 0,   targets: targets_combat,    force: 50,      range: 17,  hardness: 80,	emitters: 1,		repulsion: 0.02f, deflection: 0.10f, dispersion: 0.90f, description: "Protects the ship from damage by slowing approaching projectiles and drones."),
//			InitializeEquipment("Large Combat Dispersion Array",		typeName: "array_dispersion_combat_1",		effectMultiplier: 0.5f,     minClass: ShipClassLevel.Corvette,      space: 0,   energy: 1,  techLevel: 0,   targets: targets_combat,    force: 180,		range: 48,  hardness: 80,	emitters: 2,		repulsion: 0.02f, deflection: 0.10f, dispersion: 0.90f, description: "Protects the ship from damage by slowing approaching projectiles and drones."),
//			InitializeEquipment("Capital Combat Dispersion Array",		typeName: "array_dispersion_combat_2",		effectMultiplier: 1f,       minClass: ShipClassLevel.Cruiser,       space: 0,   energy: 1,  techLevel: 0,   targets: targets_combat,    force: 270,     range: 96,  hardness: 80,	emitters: 3,		repulsion: 0.02f, deflection: 0.10f, dispersion: 0.90f, description: "Protects the ship from damage by slowing approaching projectiles and drones."),

//			//Deflector
//			InitializeEquipment("Multiplex Deflector Array",			typeName: "array_deflector_0",				effectMultiplier: 0,        minClass: ShipClassLevel.Shuttle,       space: 0,   energy: 1,  techLevel: 0,   targets: targets_all,       force: 25,      range: 14,  hardness: 80,	emitters: 2,		repulsion: 0.20f, deflection: 1.00f, dispersion: 1.00f, description: "Protects the ship from collisions by deflecting, repulsing, and slowing approaching obstacles."),
//			InitializeEquipment("Large Multiplex Deflector Array",		typeName: "array_deflector_1",				effectMultiplier: 0.5f,     minClass: ShipClassLevel.Corvette,      space: 0,   energy: 1,  techLevel: 0,   targets: targets_all,       force: 90,		range: 48,  hardness: 80,	emitters: 4,		repulsion: 0.20f, deflection: 1.00f, dispersion: 1.00f, description: "Protects the ship from collisions by deflecting, repulsing, and slowing approaching obstacles."),
//			InitializeEquipment("Capital Multiplex Deflector Array",	typeName: "array_deflector_2",				effectMultiplier: 1f,       minClass: ShipClassLevel.Cruiser,       space: 0,   energy: 1,  techLevel: 0,   targets: targets_all,       force: 135,     range: 96,  hardness: 80,	emitters: 6,		repulsion: 0.20f, deflection: 1.00f, dispersion: 1.00f, description: "Protects the ship from collisions by deflecting, repulsing, and slowing approaching obstacles."),

//			InitializeEquipment("Asteroid Deflector Array",				typeName: "array_deflector_asteroid_0",		effectMultiplier: 0,        minClass: ShipClassLevel.Shuttle,       space: 0,   energy: 1,  techLevel: 0,   targets: targets_asteroid,  force: 25,      range: 14,  hardness: 80,	emitters: 1,		repulsion: 0.20f, deflection: 1.00f, dispersion: 1.00f, description: "Protects the ship from damage by deflecting, repulsing, and slowing approaching asteroids."),
//			InitializeEquipment("Large Asteroid Deflector Array",		typeName: "array_deflector_asteroid_1",		effectMultiplier: 0.5f,     minClass: ShipClassLevel.Corvette,      space: 0,   energy: 1,  techLevel: 0,   targets: targets_asteroid,  force: 90,		range: 48,  hardness: 80,	emitters: 2,		repulsion: 0.20f, deflection: 1.00f, dispersion: 1.00f, description: "Protects the ship from damage by deflecting, repulsing, and slowing approaching asteroids."),
//			InitializeEquipment("Capital Asteroid Deflector Array",		typeName: "array_deflector_asteroid_2",		effectMultiplier: 1f,       minClass: ShipClassLevel.Cruiser,       space: 0,   energy: 1,  techLevel: 0,   targets: targets_asteroid,  force: 135,     range: 96,  hardness: 80,	emitters: 3,		repulsion: 0.20f, deflection: 1.00f, dispersion: 1.00f, description: "Protects the ship from damage by deflecting, repulsing, and slowing approaching asteroids."),

//			InitializeEquipment("Civilian Deflector Array",				typeName: "array_deflector_civilian_0",		effectMultiplier: 0,        minClass: ShipClassLevel.Shuttle,       space: 0,   energy: 1,  techLevel: 0,   targets: targets_civilian,  force: 25,      range: 14,  hardness: 80,	emitters: 1,		repulsion: 0.20f, deflection: 1.00f, dispersion: 1.00f, description: "Protects the ship from collisions by deflecting, repulsing, and slowing approaching obstacles."),
//			InitializeEquipment("Large Civilian Deflector Array",		typeName: "array_deflector_civilian_1",		effectMultiplier: 0.5f,     minClass: ShipClassLevel.Corvette,      space: 0,   energy: 1,  techLevel: 0,   targets: targets_civilian,  force: 90,		range: 48,  hardness: 80,	emitters: 2,		repulsion: 0.20f, deflection: 1.00f, dispersion: 1.00f, description: "Protects the ship from collisions by deflecting, repulsing, and slowing approaching obstacles."),
//			InitializeEquipment("Capital Civilian Deflector Array",		typeName: "array_deflector_civilian_2",		effectMultiplier: 1f,       minClass: ShipClassLevel.Cruiser,       space: 0,   energy: 1,  techLevel: 0,   targets: targets_civilian,  force: 135,     range: 96,  hardness: 80,	emitters: 3,		repulsion: 0.20f, deflection: 1.00f, dispersion: 1.00f, description: "Protects the ship from collisions by deflecting, repulsing, and slowing approaching obstacles."),

//			InitializeEquipment("Combat Deflector Array",				typeName: "array_deflector_combat_0",		effectMultiplier: 0,        minClass: ShipClassLevel.Shuttle,       space: 0,   energy: 1,  techLevel: 0,   targets: targets_combat,    force: 25,      range: 14,  hardness: 80,	emitters: 1,		repulsion: 0.20f, deflection: 1.00f, dispersion: 1.00f, description: "Protects the ship from damage by deflecting, repulsing, and slowing approaching projectiles and drones."),
//			InitializeEquipment("Large Combat Deflector Array",			typeName: "array_deflector_combat_1",		effectMultiplier: 0.5f,     minClass: ShipClassLevel.Corvette,      space: 0,   energy: 1,  techLevel: 0,   targets: targets_combat,    force: 90,		range: 48,  hardness: 80,	emitters: 2,		repulsion: 0.20f, deflection: 1.00f, dispersion: 1.00f, description: "Protects the ship from damage by deflecting, repulsing, and slowing approaching projectiles and drones."),
//			InitializeEquipment("Capital Combat Deflector Array",		typeName: "array_deflector_combat_2",		effectMultiplier: 1f,       minClass: ShipClassLevel.Cruiser,       space: 0,   energy: 1,  techLevel: 0,   targets: targets_combat,    force: 135,     range: 96,  hardness: 80,	emitters: 3,		repulsion: 0.20f, deflection: 1.00f, dispersion: 1.00f, description: "Protects the ship from damage by deflecting, repulsing, and slowing approaching projectiles and drones."),
//		};

//	}
//static Equipment Create(int size, int id) {
//	var wr = new Equipment() {
//		id = Core.GetIndex(id),

//		activated = false,
//		activeEquipmentIndex = Core.GetActiveIndex(typeof(AE_DeflectorShield)),
//		//buff
//		//crafting materials //make list; use auto-generated for testing
//		defaultKey = KeyCode.G,
//		description = $"Test Description (size: {size})",
//		dropLevel = DropLevel.Normal, //boss/elite loot, etc
//		effects = EffectAttribute.CreateEffects(typeof(AE_DeflectorShield), effectValues[size]), //determines uniquness, stores effectiveness information,
//		//enableChangeKey //whether or not the key can be changed?
//		energyCost = energyCosts[size],
//		//energyCostPerShipClass //bool, modifies energy cost by ship class (determine how)
//		equipName = names[size] + "Deflector Shield",
//		//lootChance //chance of being added to loot pool in percent
//		//massChange //percentage mass change, positive or negative
//		minShipClass = minShipClass[size],
//		rarityCostMod = 1, //energy cost modifier for rarity
//		//rarityMod
//		//refName
//		//repReq
//		//requiredItemId //if >= 0, requires another item of specific id - for ammunition
//		//requiredQnt //how many of requiredItemId to consume
//		//sellChance //loot and trade?
//		//sortPower //use default unless required otherwise
//		space = space[size],
//		//spawnInArena //unknown
//		//sprite - need to create or reference art
//		techLevel = techLevel[size],
//		type = EquipmentType.Utility,
//		uniqueReplacement = true, //replace other unique equipment already installed
//	};

//	return wr;
//      }
//}