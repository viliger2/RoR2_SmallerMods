using BepInEx;
using BepInEx.Configuration;
using RoR2.ContentManagement;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace RepurposedCraterBoss
{
    [BepInPlugin(GUID, Name, Version)]
    public class RepurposedCraterBossPlugin : BaseUnityPlugin
    {
        public const string Author = "viliger";
        public const string Name = nameof(RepurposedCraterBossPlugin);
        public const string Version = "1.0.0";
        public const string GUID = Author + "." + Name;

        public const string LanguageFolder = "Language";

        public static ConfigEntry<int> CamerasToDestroy;
        public static ConfigEntry<int> CamerasToSpawn;
        public static ConfigEntry<bool> UseAWU;

        private void Awake()
        {
            Log.Init(Logger);

            ContentManager.collectContentPackProviders += ContentManager_collectContentPackProviders;
            RoR2.Language.collectLanguageRootFolders += Language_collectLanguageRootFolders;

            RoR2.Stage.onStageStartGlobal += Stage_onStageStartGlobal;

            CamerasToDestroy = Config.Bind("Repurposed Crater Boss", "Cameras To Destroy", 6, "Number of cameras to destroy to spawn the boss.");
            CamerasToSpawn = Config.Bind("Repurposed Crater Boss", "Cameras To Spawn", 12, "Number of cameras that will be spawned on the stage.");
            UseAWU = Config.Bind("Repurposed Crater Boss", "Spawn AWU", false, "Spawns AWU instead of Alloy Hunter.");
        }

        private void Language_collectLanguageRootFolders(System.Collections.Generic.List<string> folders)
        {
            folders.Add(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(base.Info.Location), "Language"));
        }

        private void ContentManager_collectContentPackProviders(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(new ContentProvider());
        }

        private void Stage_onStageStartGlobal(RoR2.Stage stage)
        {
            if(stage.sceneDef.cachedName == "repurposedcrater")
            {
                // 1. deleting chests while keeping golden trees active
                var goldChestsGroup = GameObject.Find("HOLDER: Toggle Groups/GROUP_ GoldChest");
                if (!goldChestsGroup)
                {
                    Log.Warning("Couldn't find \"HOLDER: Toggle Groups/GROUP_ GoldChest\" on repurposedcrater. Not doing anything...");
                    return;
                }

                for(int i = 0; i < goldChestsGroup.transform.childCount; i++)
                {
                    var chestHolder = goldChestsGroup.transform.GetChild(i);
                    chestHolder.gameObject.SetActive(true); // enabling golden things for visuals
                    var chest = chestHolder.transform.Find("GoldChest");
                    if (chest)
                    {
                        UnityEngine.Object.Destroy(chest.gameObject);
                    }
                }

                // 2. spawning combat encounter prefab and cameras
                if (NetworkServer.active)
                {
                    var encounter = UnityEngine.Object.Instantiate(ContentProvider.alloyHunterBossEncounterObject);
                    NetworkServer.Spawn(encounter);
                }
            }
        }
    }
}
