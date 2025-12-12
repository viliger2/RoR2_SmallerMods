using R2API;
using RepurposedCraterBoss.ModdedEntityStates.BossEncounter;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace RepurposedCraterBoss
{
    public class ContentProvider : IContentPackProvider
    {
        public static GameObject alloyHunterBossEncounterObject;

        public string identifier => RepurposedCraterBossPlugin.GUID + "." + nameof(ContentPack);

        private readonly ContentPack _contentPack = new ContentPack();

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(_contentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            _contentPack.identifier = identifier;

            string assetBundleFolderPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(ContentProvider).Assembly.Location), "AssetBundles");
            AssetBundle assetbundle = null;
            yield return LoadAssetBundle(System.IO.Path.Combine(assetBundleFolderPath, "alloyhunterboss"), args.progressReceiver, (resultAssetBundle) => assetbundle = resultAssetBundle);

            alloyHunterBossEncounterObject = CreateAlloyHunterBossEncounter(assetbundle.LoadAsset<GameObject>("Assets/AlloyHunterCamera/AlloyHunterBossEncounter.prefab"));
            Listening.cameraPrefab = CreateAlloyCamera(assetbundle.LoadAsset<GameObject>("Assets/AlloyHunterCamera/AlloyCamera.prefab"));

            _contentPack.bodyPrefabs.Add(new GameObject[] { Listening.cameraPrefab });
            _contentPack.networkedObjectPrefabs.Add(new GameObject[] { alloyHunterBossEncounterObject });
            _contentPack.entityStateTypes.Add(new Type[] { typeof(Listening), typeof(ModdedEntityStates.AlloyCamera.DeathState) });

            yield break;
        }

        private IEnumerator LoadAssetBundle(string assetBundleFullPath, IProgress<float> progress, Action<AssetBundle> onAssetBundleLoaded)
        {
            var assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(assetBundleFullPath);
            while (!assetBundleCreateRequest.isDone)
            {
                progress.Report(assetBundleCreateRequest.progress);
                yield return null;
            }

            onAssetBundleLoaded(assetBundleCreateRequest.assetBundle);

            yield break;
        }

        private GameObject CreateAlloyHunterBossEncounter(GameObject encounterPrefab)
        {
            var explicitSpawnPosition = encounterPrefab.transform.Find("ExplicitSpawnPosition");
            explicitSpawnPosition.position = new Vector3(-101.760002f, 22.5f, -44.3800011f);

            var explicitDropPosition = encounterPrefab.transform.Find("ExplicitDropPosition");
            explicitDropPosition.position = new Vector3(-101.760002f, 7.30000019f, -44.3800011f);

            encounterPrefab.GetOrAddComponent<NetworkIdentity>();

            var combatSquad = encounterPrefab.GetOrAddComponent<CombatSquad>();
            combatSquad.grantBonusHealthInMultiplayer = true;

            var spawnCard = RepurposedCraterBossPlugin.UseAWU.Value
                ? Addressables.LoadAssetAsync<CharacterSpawnCard>(RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_RoboBallBoss.cscSuperRoboBallBoss_asset).WaitForCompletion()
                : Addressables.LoadAssetAsync<CharacterSpawnCard>(RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_DLC3_VultureHunter.cscVultureHunter_asset).WaitForCompletion();

            var scriptedEncounter = encounterPrefab.GetOrAddComponent<ScriptedCombatEncounter>();
            scriptedEncounter.randomizeSeed = false;
            scriptedEncounter.seed = 111220251533;
            scriptedEncounter.teamIndex = TeamIndex.Monster;
            scriptedEncounter.spawns = new ScriptedCombatEncounter.SpawnInfo[]
            {
                new ScriptedCombatEncounter.SpawnInfo()
                {
                    spawnCard = spawnCard,
                    explicitSpawnPosition = explicitSpawnPosition.transform,
                    cullChance = 0
                }
            };
            scriptedEncounter.spawnOnStart = false;
            scriptedEncounter.grantUniqueBonusScaling = true;

            var bossGroup = encounterPrefab.GetOrAddComponent<BossGroup>();
            bossGroup.bossDropChance = 0;
            bossGroup.dropPosition = explicitDropPosition.transform;
            bossGroup.dropTable = Addressables.LoadAssetAsync<BasicPickupDropTable>(RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_Common.dtTier3Item_asset).WaitForCompletion();
            bossGroup.scaleRewardsByPlayerCount = true;
            bossGroup.shouldDisplayHealthBarOnHud = true;
            bossGroup.shouldCreateObjectiveEntry = true;

            var esm = encounterPrefab.GetOrAddComponent<EntityStateMachine>();
            esm.initialStateType = new EntityStates.SerializableEntityStateType(typeof(Listening));
            esm.mainStateType = new EntityStates.SerializableEntityStateType(typeof(Listening));

            encounterPrefab.RegisterNetworkPrefab();
            return encounterPrefab;
        }

        private GameObject CreateAlloyCamera(GameObject prefab)
        {
            var modelTransform = prefab.transform.Find("ModelBase/mdlCamera");
            var modelBaseTransform = prefab.transform.Find("ModelBase");

            var hurtBox = prefab.transform.Find("ModelBase/mdlCamera/Hurtbox");
            var hurtBoxComp = hurtBox.gameObject.AddComponent<HurtBox>();
            hurtBoxComp.isBullseye = true;

            prefab.AddComponent<NetworkIdentity>();

            var pseudoMotor = prefab.GetOrAddComponent<PseudoCharacterMotor>();
            pseudoMotor.mass = 100;
            pseudoMotor.needsIncreasedLandingHeight = true;

            prefab.GetOrAddComponent<SkillLocator>();

            prefab.GetOrAddComponent<TeamComponent>().teamIndex = TeamIndex.Neutral;

            var characterBody = prefab.GetOrAddComponent<CharacterBody>();
            characterBody.baseNameToken = "VILIGER_ALLOY_CAMERA";
            characterBody.bodyFlags = CharacterBody.BodyFlags.HasBackstabImmunity | CharacterBody.BodyFlags.Masterless;
            characterBody.baseMaxHealth = 1f;

            var healthComponent = prefab.GetOrAddComponent<HealthComponent>();
            healthComponent.dontShowHealthbar = true;
            hurtBoxComp.healthComponent = healthComponent;

            var esm = prefab.GetOrAddComponent<EntityStateMachine>();
            esm.initialStateType = new EntityStates.SerializableEntityStateType(typeof(EntityStates.Idle));
            esm.mainStateType = new EntityStates.SerializableEntityStateType(typeof(EntityStates.Idle));

            var deathBehavior = prefab.GetOrAddComponent<CharacterDeathBehavior>();
            deathBehavior.deathStateMachine = esm;
            deathBehavior.deathState = new EntityStates.SerializableEntityStateType(typeof(ModdedEntityStates.AlloyCamera.DeathState));

            var networkStateMachine = prefab.GetOrAddComponent<NetworkStateMachine>();
            networkStateMachine.stateMachines = new EntityStateMachine[] { esm };

            var modelLocator = prefab.GetOrAddComponent<ModelLocator>();
            modelLocator.modelTransform = modelTransform;
            modelLocator.modelBaseTransform = modelBaseTransform;
            modelLocator.dontDetatchFromParent = true;
            modelLocator.noCorpse = true;
            modelLocator.dontReleaseModelOnDeath = true;

            var modelGameObject = modelTransform.gameObject;

            var hurtBoxGroup = modelGameObject.GetOrAddComponent<HurtBoxGroup>();
            hurtBoxGroup.hurtBoxes = new HurtBox[] { hurtBoxComp };
            hurtBoxGroup.mainHurtBox = hurtBoxComp;
            hurtBoxComp.hurtBoxGroup = hurtBoxGroup;

            var charModel = modelGameObject.GetOrAddComponent<CharacterModel>();
            charModel.body = characterBody;

            prefab.transform.Find("ModelBase/mdlCamera/SolusCamera").gameObject.GetComponent<MeshRenderer>().material = Addressables.LoadAssetAsync<Material>(RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_Common_TrimSheets.matTrimSheetAlien1BossEmission_mat).WaitForCompletion();

            prefab.RegisterNetworkPrefab();
            return prefab;
        }
    }
}
