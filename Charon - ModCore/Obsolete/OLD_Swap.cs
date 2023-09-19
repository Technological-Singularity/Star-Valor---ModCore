//#region Swap method 2

//using Charon.StarValor.Excursion;
//using System.Reflection;
//using UnityEngine.SceneManagement;
//using UnityEngine;

//static MethodInfo __AIControl_Awake = typeof(AIControl).GetMethod("Awake", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
////static MethodInfo __AIControl_Start = typeof(AIControl).GetMethod("Start", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
//static FieldInfo __AIControl_pc = typeof(AIControl).GetField("pc", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
//static FieldInfo __AIControl_Player = typeof(AIControl).GetField("Player", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

//static MethodInfo __Weapon_Load = typeof(Weapon).GetMethod("Load", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

//static FieldInfo __PlayerControl_ss = typeof(PlayerControl).GetField("ss", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
//static MethodInfo __PlayerControl_Start = typeof(PlayerControl).GetMethod("Start", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
//static FieldInfo __PlayerControl_cameraControl = typeof(PlayerControl).GetField("cameraControl", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
//static FieldInfo __PlayerControl_puc = typeof(PlayerControl).GetField("puc", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

//static FieldInfo __SpaceShip_thrusters = typeof(SpaceShip).GetField("thrusters", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
//static MethodInfo __SpaceShip_CalculateShipStats = typeof(SpaceShip).GetMethod("CalculateShipStats", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

//static FieldInfo __SpaceShip_shipModelGO = typeof(SpaceShip).GetField("shipModelGO", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
//static FieldInfo __SpaceShip_pc = typeof(SpaceShip).GetField("pc", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
//static MethodInfo __SpaceShip_GetActiveEquipmentSavedStates = typeof(SpaceShip).GetMethod("GetActiveEquipmentSavedStates", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);


//static MethodInfo __PlayerUIControl_FindPlayer = typeof(PlayerUIControl).GetMethod("FindPlayer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

//static FieldInfo __targetUI_pc = typeof(targetUI).GetField("pc", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

//static FieldInfo __MinimapControl_Player = typeof(MinimapControl).GetField("Player", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

//static FieldInfo __CameraControl_playerTrans = typeof(CameraControl).GetField("playerTrans", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
//static FieldInfo __CameraControl_pc = typeof(CameraControl).GetField("pc", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

//static FieldInfo __HPBarControl_Player = typeof(HPBarControl).GetField("Player", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

//static FieldInfo __PChar_playerSpaceShip = typeof(PChar).GetField("playerSpaceShip", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

//void ResetThrusters(SpaceShip ship) {
//    var modelGO = Instantiate<GameObject>(ObjManager.GetShip("Ship Models/ShipModel" + ship.shipData.shipModelID.ToString("D2")), null);
//    Destroy(((Transform)__SpaceShip_thrusters.GetValue(ship)).gameObject);
//    Transform thrusters = modelGO.transform.Find("Thrusters");
//    thrusters.SetParent((Transform)__SpaceShip_shipModelGO.GetValue(ship), false);
//    __SpaceShip_thrusters.SetValue(ship, thrusters);
//    thrusters.gameObject.SetActive(true);
//    Destroy(modelGO);
//}

////FleetControl.pc, Weapon.pc, PlayerControl.inst
////FriendlyFireSystem.aic, Weapon.aic
//void SwapShips2(SpaceShip ss, SpaceShip target) {
//    //AdjustMinimapIcon
//    //ShowMinimapIcon

//    foreach (var go in FindObjectsOfType<targetUI>())
//        Dump(go.transform, "TARGETUI  ");

//    //dump(Camera.main.transform, "CAMERA ");

//    //var objs = FindObjectsOfType<FollowPosition>();
//    //Log.LogMessage("Found " + objs.Length + " FollowPosition");
//    //foreach (var o in objs)
//    //    dump(o.transform, "FOLLOW ");

//    var pc_old = ss.GetComponent<PlayerControl>();
//    pc_old.enabled = false;
//    PlayerControl.inst = null;

//    (target.gameObject.name, ss.gameObject.name) = (ss.gameObject.name, target.gameObject.name);
//    (target.gameObject.tag, ss.gameObject.tag) = (ss.gameObject.tag, target.gameObject.tag);
//    var pc = target.gameObject.AddComponent<PlayerControl>();
//    //MemberwiseCloneTo(pc_n, pc);
//    //__PlayerControl_Start.Invoke(pc_n, null);
//    //pc_n.mercenaries = pc.mercenaries;

//    __PlayerControl_ss.SetValue(pc, target);

//    var aic = target.GetComponent<AIControl>();
//    var aic_n = (AIControl)ss.gameObject.AddComponent(aic.GetType());
//    MemberwiseCloneTo(aic_n, aic);
//    Destroy(aic);

//    ResetThrusters(target);

//    //have ai control override cached values from ship
//    __AIControl_Awake.Invoke(aic_n, null);
//    __AIControl_pc.SetValue(aic_n, pc);

//    UpdateAI(ss, ss.shipData);

//    //fix Player
//    GameManager.instance.Player = target.gameObject;
//    __MinimapControl_Player.SetValue(MinimapControl.instance, target.gameObject);
//    //Camera.main.transform.SetParent(target.transform);

//    //cameras
//    var camera = Camera.main.GetComponent<CameraControl>();//(CameraControl)__PlayerControl_cameraControl.GetValue(pc);
//    __CameraControl_pc.SetValue(camera, null);
//    camera.AdjustCamera(target.shipClass);
//    //__PlayerControl_cameraControl.SetValue(pc_n, camera);
//    __CameraControl_pc.SetValue(camera, pc);
//    __CameraControl_playerTrans.SetValue(camera, target.transform);

//    GameObject.Find("MainCamera").GetComponent<FollowPosition>().target = target.transform;
//    foreach (var hpBar in FindObjectsOfType<HPBarControl>())
//        __HPBarControl_Player.SetValue(hpBar, target.gameObject);

//    //fix PC
//    __PChar_playerSpaceShip.SetValue(null, target);
//    __targetUI_pc.SetValue(GameObject.Find("targetIcon").GetComponent<targetUI>(), pc);
//    __PlayerUIControl_FindPlayer.Invoke(PlayerUIControl.inst, null);
//    FleetControl.instance.pc = pc;
//    __SpaceShip_pc.SetValue(ss, null);
//    __SpaceShip_pc.SetValue(target, pc);
//    foreach (var w in target.weapons)
//        __Weapon_Load.Invoke(w, new object[] { true });
//    foreach (var w in ss.weapons)
//        __Weapon_Load.Invoke(w, new object[] { true });
//    foreach (var _aic in FindObjectsOfType<AIControl>()) {
//        __AIControl_pc.SetValue(_aic, pc);
//        __AIControl_Player.SetValue(_aic, target.gameObject);
//    }


//    //fix AIC
//    ss.ffSys.Setup(aic_n, null, null, false);
//    target.ffSys.Setup(null, pc, null, false);

//    //Log.LogWarning("A");
//    //UpdateShip(target);

//    //Log.LogWarning("B");
//    //UpdateShip(ss);            

//    Destroy(pc_old);
//    pc.SetTarget(ss.transform);

//    void searchPlayerObjects(Transform go) {
//        foreach (var c in go.GetComponents<Component>()) {
//            foreach (var f in c.GetType().GetFields().Where(o => o.GetValue(c) == (object)ss.transform))
//                Plugin.Log.LogMessage("PLAYER TRANSFORM " + c.transform.name + " > " + c.GetType().FullName + " >> " + f.Name);
//            foreach (var f in c.GetType().GetFields().Where(o => o.GetValue(c) == (object)ss.gameObject))
//                Plugin.Log.LogMessage("PLAYER GAMEOBJ " + c.transform.name + " > " + c.GetType().FullName + " >> " + f.Name);
//            foreach (var f in c.GetType().GetFields().Where(o => o.GetValue(c) == (object)ss))
//                Plugin.Log.LogMessage("PLAYER SS " + c.transform.name + " > " + c.GetType().FullName + " >> " + f.Name);
//        }
//        foreach (Transform child in go.transform)
//            searchPlayerObjects(child);
//    }
//    foreach (var go in SceneManager.GetActiveScene().GetRootGameObjects())
//        searchPlayerObjects(go.transform);
//}
//#endregion