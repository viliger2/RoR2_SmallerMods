using BepInEx;
using BepInEx.Configuration;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace CelestialPortalAfterBosses
{
    [BepInPlugin(GUID, ModName, Version)]
    public class CelestialPortalAfterBossesPlugin : BaseUnityPlugin
    {
        public const string Author = "viliger";
        public const string ModName = "CelestialPortalAfterBosses";
        public const string Version = "1.0.1";
        public const string GUID = "com." + Author + "." + ModName;

        public static ConfigEntry<bool> SpawnAfterFalseSon;
        public static ConfigEntry<bool> SpawnAfterSHeart;

        private void Awake()
        {
            SpawnAfterFalseSon = Config.Bind("Celestial Portal", "Spawn After False Son", true, "Spawns Celestial Portal after defeating False Son post loop.");
            SpawnAfterSHeart = Config.Bind("Celestial Portal", "Spawn After Solus Heart", true, "Spawns Celestial Portal after defeating Solus Heart post loop.");

            if (SpawnAfterFalseSon.Value)
            {
                On.EntityStates.ShrineRebirth.RebirthOrPortalChoice.OnEnter += RebirthOrPortalChoice_OnEnter;
            }
            if (SpawnAfterSHeart.Value)
            {
                On.RoR2.SolusWebMissionController.SpawnExitPortal += SolusWebMissionController_SpawnExitPortal;
            }
        }

        private void SolusWebMissionController_SpawnExitPortal(On.RoR2.SolusWebMissionController.orig_SpawnExitPortal orig, RoR2.SolusWebMissionController self)
        {
            orig(self);
            OpenMSPortal(new Vector3(-40.161f, -226.2f, 20.511f), new Vector3(0f, 270f, 0f));
        }

        private void RebirthOrPortalChoice_OnEnter(On.EntityStates.ShrineRebirth.RebirthOrPortalChoice.orig_OnEnter orig, EntityStates.ShrineRebirth.RebirthOrPortalChoice self)
        {
            orig(self);
            OpenMSPortal(new Vector3(108.8621f, 147.3f, -64.8503f), Vector3.zero);
        }

        private void OpenMSPortal(Vector3 position, Vector3 rotation)
        {
            if (!NetworkServer.active)
            {
                return;
            }

            if (!Run.instance || Run.instance.loopClearCount == 0)
            {
                return;
            }

            var handle = Addressables.LoadAssetAsync<InteractableSpawnCard>(RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_PortalMS.iscMSPortal_asset);
            if (handle.IsValid())
            {
                handle.Completed += (result) =>
                {
                    var card = result.Result;
                    if (DirectorCore.instance)
                    {
                        var obj = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(card, new DirectorPlacementRule()
                        {
                            placementMode = DirectorPlacementRule.PlacementMode.DirectWithoutRandomRotation,
                            position = position,
                            rotation = Quaternion.Euler(rotation)
                        },
                            Run.instance.stageRng)
                        );
                        if (obj)
                        {
                            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                            {
                                baseToken = "PORTAL_MS_OPEN"
                            });
                        }
                    }
                    Addressables.Release(handle);
                };
            }
        }

    }
}
