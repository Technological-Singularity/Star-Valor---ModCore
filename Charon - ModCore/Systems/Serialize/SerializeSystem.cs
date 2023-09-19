using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using HarmonyLib;
using UnityEngine;
using BepInEx;

namespace Charon.StarValor.ModCore {
    [HasPatches]
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    [BepInProcess("Star Valor.exe")]
    public class SerializeSystem : ModCorePlugin {
        public const string pluginGuid = "starvalor.charon.modcore.serialize_system";
        public const string pluginName = "Charon - Modcore - Serialize System";
        public const string pluginVersion = "0.0.0.0";

        #region Patches
        [HarmonyPatch(typeof(GameData), nameof(GameData.SaveGame))]
        [HarmonyPostfix]
        public static void SaveSaveGame(GameData __instance, bool __result) {
            if (!__result)
                return;
            Loader.ForEach((context) => context.OnGameSave());
            Instance.SerializeAll();
        }
        [HarmonyPatch(typeof(GameData), nameof(GameData.LoadGame))]
        [HarmonyPostfix]
        public static void LoadGame(bool __result) {
            if (!__result)
                return;
            Loader.ForEach((context) => context.OnGameLoad());
            Instance.DeserializeAll();
        }
        [HarmonyPatch(typeof(GameData), nameof(GameData.LoadGameOld))]
        [HarmonyPostfix]
        public static void LoadGameOld(bool __result) {
            if (!__result)
                return;
            //bug? should use file name for backup, but source doesn't
            Loader.ForEach((context) => context.OnGameLoad());
            Instance.DeserializeAll();
        }
        #endregion

        public static SerializeSystem Instance { get; private set; }
        void Awake()  => Instance = this;

        List<ISerializable> serializables = new List<ISerializable>();
        public void Add(ISerializable obj) => serializables.Add(obj);

        void SerializeAll() {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream fileStream = null;
            bool success = true;

            lock (GameData.threadSaveLock) {
                try {
                    string tempIndexSavePath = GameData.tempSaveGamePath + ".index";
                    fileStream = File.Create(tempIndexSavePath);
                    binaryFormatter.Serialize(fileStream, IndexSystem.Instance.GetSerialization());
                }
                catch (Exception ex) {
                    success = false;
                    Debug.LogError($"ERROR trying to save INDEX FILE. Exception: {ex}");
                }
                finally {
                    if (fileStream != null)
                        fileStream.Close();
                }

                Instance.serializables.ForEach((obj) => {
                    if (!success)
                        return;

                    var extraData = obj.GetSerialization();
                    if (extraData == null)
                        return;

                    string tempSaveGamePath = GameData.tempSaveGamePath + "." + obj.Guid;
                    try {
                        fileStream = File.Create(tempSaveGamePath);
                        binaryFormatter.Serialize(fileStream, extraData);
                    }
                    catch (Exception ex) {
                        success = false;
                        Debug.LogError($"ERROR trying to save GUID FILE for {obj.Guid}. Exception: {ex}");
                    }
                    finally {
                        if (fileStream != null)
                            fileStream.Close();
                    }
                });
                if (!success) {
                    Debug.LogError("Could NOT save game.");
                    foreach (var filename in Directory.GetFiles(Application.dataPath + GameData.saveFolderName, GameData.tempSaveGamePath + ".*"))
                        File.Delete(filename);
                    return;
                }

                string searchPattern = Path.GetFileName(GameData.fullSaveGameBackupPath);
                foreach (var filename in Directory.GetFiles(Application.dataPath + GameData.saveFolderName, searchPattern + ".*"))
                    File.Delete(filename);

                searchPattern = Path.GetFileName(GameData.fullSaveGamePath);
                foreach (var filename in Directory.GetFiles(Application.dataPath + GameData.saveFolderName, searchPattern + ".*")) {
                    var filename_short = Path.GetFileName(filename);
                    if (filename_short.Length <= searchPattern.Length)
                        continue;
                    string guid = Path.GetFileName(filename_short).Substring(searchPattern.Length + 1);
                    string fullSaveGameBackupPath = Path.GetFullPath(GameData.fullSaveGameBackupPath) + "." + guid;
                    var filename_long = Path.GetFullPath(filename);
                    File.Move(filename_long, fullSaveGameBackupPath);
                }

                searchPattern = Path.GetFileName(GameData.tempSaveGamePath);
                foreach (var filename in Directory.GetFiles(Application.dataPath + GameData.saveFolderName, searchPattern + ".*")) {
                    var filename_short = Path.GetFileName(filename);
                    if (filename_short.Length <= searchPattern.Length)
                        continue;
                    string guid = Path.GetFileName(filename_short).Substring(searchPattern.Length + 1);
                    string fullSaveGamePath = Path.GetFullPath(GameData.fullSaveGamePath) + "." + guid;
                    var filename_long = Path.GetFullPath(filename);
                    File.Move(filename_long, fullSaveGamePath);
                }
            }
        }
        void DeserializeAll() {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream fileStream = null;

            Loader.ForEach((context) => context.OnGameSave());
            
            serializables.ForEach((context) => {
                string fullSaveGamePath = GameData.fullSaveGamePath + "." + context.Guid;
                if (File.Exists(fullSaveGamePath)) {
                    fileStream = File.Open(fullSaveGamePath, FileMode.Open);
                    var serialization = binaryFormatter.Deserialize(fileStream);
                    fileStream.Close();
                    context.Deserialize(true,serialization);
                }
                else {
                    context.Deserialize(false, null);
                }
            });
        }
    }
}
