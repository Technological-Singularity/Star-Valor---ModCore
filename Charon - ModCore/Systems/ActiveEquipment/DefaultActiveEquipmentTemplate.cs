//using System.Collections.Generic;
//using UnityEngine;

//namespace Charon.StarValor.ModCore {
//    class DefaultActiveEquipmentTemplate : ActiveEquipmentExTemplate {
//        public static void Register(Equipment equipment) {
//            if (!equipment.activated || registered.ContainsKey(equipment.activeEquipmentIndex))
//                return;
//            var aeid = equipment.activeEquipmentIndex;
//            ModCorePlugin.Log.LogMessage($"Registering {aeid} as ActiveEquipmentEx [{aeid}]");
//            registered.Add(aeid, new DefaultActiveEquipmentTemplate(equipment));
//        }
//        static Dictionary<int, DefaultActiveEquipmentTemplate> registered = new Dictionary<int, DefaultActiveEquipmentTemplate>();

//        //workaround to nonexistent lookup
//        static ActiveEquipment GetActiveEquipment(Equipment equipment) {
//            GameObject go = new GameObject();
//            var ss = go.AddComponent<SpaceShip>();
//            var ae = ActiveEquipment.AddActivatedEquipment(equipment, ss, KeyCode.None, 1, 1);
//            Object.Destroy(ss);
//            Object.DestroyImmediate(go, false);
//            return ae;
//        }
//        DefaultActiveEquipmentTemplate(Equipment equipment) {
//            IndexSystem.Instance.AllocateTypeInstance(this, equipment.activeEquipmentIndex);
//        }
//        public override void OnApplying(IIndexableInstance instance) {
//            base.OnApplying(instance);
//        }
//        public override void OnRemoving(IIndexableInstance instance) {
//            base.OnRemoving(instance);
//        }
//        protected override void OnInitialization(ActiveEquipmentEx sender) {

//            base.OnInitialization(sender);
//        }
//        protected override bool OnActivate(ActiveEquipmentEx sender, bool shiftPressed, Transform target) {
//            return base.OnActivate(sender, shiftPressed, target);
//        }
//        protected override (Transform target, bool result) OnActivateDeactivate(ActiveEquipmentEx sender, bool shiftPressed, Transform target) {
//            return base.OnActivateDeactivate(sender, shiftPressed, target);
//        }

//    }
//}
