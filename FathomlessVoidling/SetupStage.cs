using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace FathomlessVoidling
{
    public class SetupStage
    {
        public static SpawnCard locusPortalCard;

        public static void SetupStuff()
        {
            SceneDef voidRaid = Addressables.LoadAssetAsync<SceneDef>("RoR2/DLC1/voidraid/voidraid.asset").WaitForCompletion();
            voidRaid.blockOrbitalSkills = false;

            if (!ModConfig.enableVoidFog.Value)
            {
                On.RoR2.VoidStageMissionController.RequestFog += VoidStageMissionController_RequestFog;
            }

            if (ModConfig.enableAltMoon.Value)
            {
                locusPortalCard = Addressables.LoadAssetAsync<SpawnCard>("RoR2/DLC1/PortalVoid/iscVoidPortal.asset").WaitForCompletion();

                RoR2.Stage.onServerStageBegin += Stage_onServerStageBegin;
                On.RoR2.TeleporterInteraction.AttemptToSpawnAllEligiblePortals += TeleporterInteraction_AttemptToSpawnAllEligiblePortals1;
            }
        }

        private static VoidStageMissionController.FogRequest VoidStageMissionController_RequestFog(On.RoR2.VoidStageMissionController.orig_RequestFog orig, VoidStageMissionController self, IZone zone)
        {
            return null;
        }

        private static void TeleporterInteraction_AttemptToSpawnAllEligiblePortals1(On.RoR2.TeleporterInteraction.orig_AttemptToSpawnAllEligiblePortals orig, TeleporterInteraction self)
        {
            if (self.beginContextString.Contains("LUNAR"))
            {
                List<PortalSpawner> list = self.portalSpawners.ToList<PortalSpawner>();
                PortalSpawner portalSpawner = list.Find((PortalSpawner x) => x.portalSpawnCard == locusPortalCard);
                if (portalSpawner != null)
                {
                    list.Remove(portalSpawner);
                    self.portalSpawners = list.ToArray();
                }
                if (!NetworkServer.active)
                {
                    return;
                }

                DirectorCore instance = DirectorCore.instance;
                DirectorPlacementRule directorPlacementRule = new DirectorPlacementRule
                {
                    minDistance = 10f,
                    maxDistance = 40f,
                    placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                    position = self.transform.position,
                    spawnOnTarget = self.transform
                };
                DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(locusPortalCard, directorPlacementRule, self.rng);
                GameObject gameObject = instance.TrySpawnObject(directorSpawnRequest);
                if (gameObject)
                {
                    NetworkServer.Spawn(gameObject);
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                    {
                        baseToken = "PORTAL_VOID_OPEN"
                    });
                }
            }
            orig.Invoke(self);
        }

        private static void Stage_onServerStageBegin(Stage stage)
        {
            if (stage.sceneDef.cachedName == "voidstage")
            {
                var handle1 = Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_LunarCauldrons_LunarCauldron_RedToWhite.Variant_prefab);
                if (handle1.IsValid())
                {
                    handle1.Completed += (result) =>
                    {
                        if (result.IsDone && result.Result)
                        {
                            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(result.Result, new Vector3(-142.67f, 29.94f, 242.74f), Quaternion.identity);
                            gameObject.transform.eulerAngles = new Vector3(0f, 66f, 0f);
                            NetworkServer.Spawn(gameObject);
                        }
                        Addressables.Release(handle1);
                    };
                }

                var handle2 = Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_LunarCauldrons_LunarCauldron_GreenToRed.Variant_prefab);
                if (handle2.IsValid())
                {
                    handle2.Completed += (result) =>
                    {
                        if (result.IsDone && result.Result)
                        {
                            GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(result.Result, new Vector3(-136.76f, 29.94f, 246.51f), Quaternion.identity);
                            gameObject2.transform.eulerAngles = new Vector3(0f, 66f, 0f);
                            NetworkServer.Spawn(gameObject2);
                            GameObject gameObject3 = UnityEngine.Object.Instantiate<GameObject>(result.Result, new Vector3(-149.74f, 29.93f, 239.7f), Quaternion.identity);
                            gameObject3.transform.eulerAngles = new Vector3(0f, 66f, 0f);
                            NetworkServer.Spawn(gameObject3);
                        }
                        Addressables.Release(handle2);
                    };
                }

                var handle3 = Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_LunarCauldrons_LunarCauldron.WhiteToGreen_prefab);
                if (handle3.IsValid())
                {
                    handle3.Completed += (result) =>
                    {
                        if (result.IsDone && result.Result)
                        {
                            GameObject gameObject4 = UnityEngine.Object.Instantiate<GameObject>(result.Result, new Vector3(-157.41f, 29.97f, 237.12f), Quaternion.identity);
                            gameObject4.transform.eulerAngles = new Vector3(0f, 66f, 0f);
                            NetworkServer.Spawn(gameObject4);
                            GameObject gameObject5 = UnityEngine.Object.Instantiate<GameObject>(result.Result, new Vector3(-126.63f, 29.93f, 249.1f), Quaternion.identity);
                            gameObject5.transform.eulerAngles = new Vector3(0f, 66f, 0f);
                            NetworkServer.Spawn(gameObject5);
                        }
                        Addressables.Release(handle2);
                    };
                }
            }
        }
    }
}
