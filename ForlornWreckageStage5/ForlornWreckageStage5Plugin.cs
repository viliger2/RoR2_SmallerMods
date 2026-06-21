using BepInEx;
using LOP;
using R2API;
using R2API.ContentManagement;
using RoR2;
using System.Collections.Generic;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace ForlornWreckageStage5
{
    [BepInPlugin(GUID, ModName, Version)]
    [BepInDependency("com.Winslow.WS_SpaceStage1", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(DirectorAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(R2APIContentManager.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(LocationsOfPrecipitation.GUID, BepInDependency.DependencyFlags.HardDependency)]
    public class ForlornWreckageStage5Plugin : BaseUnityPlugin
    {
        public const string Author = "viliger";
        public const string ModName = "ForlornWreckageStage5";
        public const string Version = "1.0.0";
        public const string GUID = "com." + Author + "." + ModName;

        private const string FORLORN_WRECKAGE_SCENE_NAME = "forgottenwreckage_ws";

        private static GameObject artifactPortalPrefab;

        private void Awake()
        {
            RoR2Application.onLoadFinished += ModifyForlornWreckage;
            On.RoR2.SceneDirector.Start += SceneDirector_Start;

            CreateArtifactThing();

            AddMonsterToStage(RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_Parent.cscParent_asset, DirectorAPI.MonsterCategory.Minibosses);
            AddMonsterToStage(RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_LemurianBruiser.cscLemurianBruiser_asset, DirectorAPI.MonsterCategory.Minibosses);
            AddMonsterToStage(RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_Grandparent.cscGrandparent_asset, DirectorAPI.MonsterCategory.Champions);

            var directorCard = new DirectorCard()
            {
                spawnCardReference = new AssetReferenceT<SpawnCard>(RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_TripleShopLarge.iscTripleShopLarge_asset),
                selectionWeight = 40, // directly copied from skymeadow
                spawnDistance = DirectorCore.MonsterSpawnDistance.Standard
            };
            DirectorAPI.DirectorCardHolder directorCardHolder = new DirectorAPI.DirectorCardHolder
            {
                Card = directorCard,
                InteractableCategory = DirectorAPI.InteractableCategory.Chests
            };
            DirectorAPI.Helpers.AddNewInteractableToStage(directorCardHolder, DirectorAPI.Stage.Custom, FORLORN_WRECKAGE_SCENE_NAME);
            DirectorAPI.Helpers.RemoveExistingInteractableFromStage(DirectorAPI.Helpers.InteractableNames.TripleShop, DirectorAPI.Stage.Custom, FORLORN_WRECKAGE_SCENE_NAME);
        }

        private void CreateJumpPad(Vector3 position, Vector3 rotation, Vector3 targetPosition, Vector3 jumpVelocity, float time)
        {
            var jumpPad = new GameObject();
            jumpPad.name = "FW_ArtifactPortalJumpPad";

            var target = new GameObject();
            target.transform.parent = jumpPad.transform;
            target.transform.localPosition = targetPosition;

            var geyserPrefab = jumpPad.AddComponent<InstantiateGeyserPrefab>();
            geyserPrefab.geyserType = InstantiateGeyserPrefab.GeyserType.Default;

            var jumpVolume = jumpPad.GetComponent<JumpVolume>();
            jumpVolume.jumpVelocity = jumpVelocity;
            jumpVolume.time = time;
            jumpVolume.jumpSoundString = "Play_env_geyser_launch";
            jumpVolume.targetElevationTransform = target.transform;
            jumpVolume.onJump = new UnityEngine.Events.UnityEvent<CharacterBody>();

            jumpPad.transform.position = position;
            jumpPad.transform.rotation = Quaternion.Euler(rotation);
            geyserPrefab.Refresh();
        }

        private void CreateArtifactThing()
        {
            artifactPortalPrefab = new GameObject();
            Object.DontDestroyOnLoad(artifactPortalPrefab);
            artifactPortalPrefab.name = "FW_ArtifactPortalPrefab";

            artifactPortalPrefab.AddComponent<NetworkIdentity>();
            var instantiateComponent = artifactPortalPrefab.AddComponent<InstantiateArtifactPortal>();

            var buttonList = new List<Transform>
            {
                AddButton(artifactPortalPrefab, new Vector3(5.09000015f, -19.5699997f, 20.9699993f), Vector3.zero),
                AddButton(artifactPortalPrefab, new Vector3(2f, -19.5699997f, 16.75f), Vector3.zero),
                AddButton(artifactPortalPrefab, new Vector3(-1.23000002f, -19.5699997f, 12.46f), Vector3.zero),
                AddButton(artifactPortalPrefab, new Vector3(0.949976802f, -19.5699997f, 23.9400082f), Vector3.zero),
                AddButton(artifactPortalPrefab, new Vector3(-2.0999999f, -19.5699997f, 19.7900009f), Vector3.zero),
                AddButton(artifactPortalPrefab, new Vector3(-5.41002321f, -19.5699997f, 15.6100082f), Vector3.zero),
                AddButton(artifactPortalPrefab, new Vector3(-3.0999999f, -19.5699997f, 26.7299995f), Vector3.zero),
                AddButton(artifactPortalPrefab, new Vector3(-5.90000629f, -19.5699997f, 22.8800106f), Vector3.zero),
                AddButton(artifactPortalPrefab, new Vector3(-9.36000633f, -19.5699997f, 18.6800117f), Vector3.zero)
            };

            instantiateComponent.artifactButtons = buttonList.ToArray();
            instantiateComponent.portalLocation = AddButton(artifactPortalPrefab, new Vector3(36.9399986f, -8.72000027f, -18.5f), new Vector3(0, 126.947906f, 0));
            instantiateComponent.laptopLocation = AddButton(artifactPortalPrefab, new Vector3(-10.2799997f, -19.9599991f, 10.0699997f), Vector3.zero);

            artifactPortalPrefab.RegisterNetworkPrefab();

            ContentAddition.AddNetworkedObject(artifactPortalPrefab);

            Transform AddButton(GameObject parent, Vector3 position, Vector3 rotation)
            {
                var button = new GameObject();
                button.transform.parent = parent.transform;
                button.transform.localPosition = position;
                button.transform.rotation = Quaternion.Euler(rotation);

                return button.transform;
            }
        }

        private void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            if (SceneInfo.instance && SceneInfo.instance.sceneDef && SceneInfo.instance.sceneDef.baseSceneName == FORLORN_WRECKAGE_SCENE_NAME)
            {
                self.teleporterSpawnCard = Addressables.LoadAssetAsync<InteractableSpawnCard>(RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_Teleporters.iscLunarTeleporter_asset).WaitForCompletion();

                var csi = SceneInfo.instance.GetComponent<ClassicStageInfo>();
                if (csi)
                {
                    csi.sceneDirectorInteractibleCredits = 520;
                }

                CreateJumpPad(new Vector3(153.320007f, 270.01001f, -405.059998f), new Vector3(270, 0, 0), new Vector3(-11.5799999f, 40.4300003f, 53.5299988f), new Vector3(-5.03478336f, 57.7739105f, -17.5782585f), 2.3f);
                CreateJumpPad(new Vector3(148.820007f, 323.459991f, -457.399994f), new Vector3(270, 0, 0), new Vector3(-13.6099997f, 21.7299995f, 48.4099998f), new Vector3(-5.91739178f, 55.5478287f, -9.44783115f), 2.3f);
                CreateJumpPad(new Vector3(146.899994f, 371.970001f, -475.179993f), new Vector3(270, 0, 0), new Vector3(114.599998f, -44.7999992f, 61.2770004f), new Vector3(38.2000008f, 65.4256668f, 14.9333296f), 3f);

                if (NetworkServer.active && artifactPortalPrefab)
                {
                    var prefab = UnityEngine.Object.Instantiate(artifactPortalPrefab, new Vector3(312.423401f, 430.787842f, -385.854156f), Quaternion.identity);
                    NetworkServer.Spawn(prefab);
                }
            }

            orig(self);
        }

        private void AddMonsterToStage(string cardGuid, DirectorAPI.MonsterCategory category)
        {
            var directorCard = new DirectorCard()
            {
                spawnCardReference = new AssetReferenceT<SpawnCard>(cardGuid),
                selectionWeight = 1,
                spawnDistance = DirectorCore.MonsterSpawnDistance.Standard,
                preventOverhead = true
            };
            DirectorAPI.DirectorCardHolder directorCardHolder = new DirectorAPI.DirectorCardHolder
            {
                Card = directorCard,
                MonsterCategory = category,
            };
            DirectorAPI.Helpers.AddNewMonsterToStage(directorCardHolder, false, DirectorAPI.Stage.Custom, FORLORN_WRECKAGE_SCENE_NAME);
        }

        private void ModifyForlornWreckage()
        {
            var fwSceneDef = SceneCatalog.FindSceneDef(FORLORN_WRECKAGE_SCENE_NAME);
            if (!fwSceneDef)
            {
                return;
            }

            var loopSgStage3 = Addressables.LoadAssetAsync<SceneCollection>(RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_SceneGroups.loopSgStage3_asset).WaitForCompletion();
            var sgStage3 = Addressables.LoadAssetAsync<SceneCollection>(RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_SceneGroups.sgStage3_asset).WaitForCompletion();

            RemoveStageFromSceneCollection(ref loopSgStage3, fwSceneDef);
            RemoveStageFromSceneCollection(ref sgStage3, fwSceneDef);

            var loopSgStage5 = Addressables.LoadAssetAsync<SceneCollection>(RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_SceneGroups.loopSgStage5_asset).WaitForCompletion();
            var sgStage5 = Addressables.LoadAssetAsync<SceneCollection>(RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_SceneGroups.sgStage5_asset).WaitForCompletion();

            AppendStageToSceneCollection(ref loopSgStage5, fwSceneDef);
            AppendStageToSceneCollection(ref sgStage5, fwSceneDef);

            var loopSgStage1 = Addressables.LoadAssetAsync<SceneCollection>(RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_SceneGroups.loopSgStage1_asset).WaitForCompletion();
            var sgStage1 = Addressables.LoadAssetAsync<SceneCollection>(RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_SceneGroups.sgStage1_asset).WaitForCompletion();

            fwSceneDef.destinationsGroup = sgStage1;
            fwSceneDef.loopedDestinationsGroup = loopSgStage1;

            void AppendStageToSceneCollection(ref SceneCollection sceneCollection, SceneDef sceneDef)
            {
                HG.ArrayUtils.ArrayAppend(ref sceneCollection._sceneEntries, new SceneCollection.SceneEntry { sceneDef = sceneDef, weightMinusOne = 0 });
            }

            void RemoveStageFromSceneCollection(ref SceneCollection sceneCollection, SceneDef sceneDef)
            {
                for (int i = 0; i < sceneCollection._sceneEntries.Length; i++)
                {
                    if (sceneCollection._sceneEntries[i].sceneDef == sceneDef)
                    {
                        HG.ArrayUtils.ArrayRemoveAtAndResize(ref sceneCollection._sceneEntries, i);
                        break;
                    }
                }
            }
        }

    }
}