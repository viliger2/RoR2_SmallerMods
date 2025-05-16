using BepInEx;
using EntityStates;
using EntityStates.VoidRaidCrab.Weapon;
using FathomlessVoidling.VoidlingEntityStates.Phase1;
using FathomlessVoidling.VoidlingEntityStates.Phase3;
using R2API;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Projectile;
using RoR2.Skills;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

// TODO:
// 1. Unfuck skills, whatever the hell the hook is doing is not good, you should just modify the prefab,
// but that means creating skill families for every single body and then creating skilldefs for that, modifying AI, modifying bodies and skill locators...
// 2. Add language file, since voidling title is currently hardcoded as a string intead of a token
// 3. go through every skill state and check for networking, add authority checks, etc.
// 4. remove the asset list at the bottom, there is no need to keep it like that (outside of maybe projectile clones).
// 5. write propper contentpack instead of relying on ContentAddition
// 6. maybe implement void cutscene? not like its difficult, its just like 3 states.
namespace FathomlessVoidling
{
    [BepInDependency(R2API.ContentManagement.R2APIContentManager.PluginGUID)]
    [BepInDependency(R2API.PrefabAPI.PluginGUID)]
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(GUID, ModName, Version)]
    public class FathomlessVoidling : BaseUnityPlugin
    {
        public const string ModName = "FathomlessVoidling";
        public const string Version = "0.9.4";
        public const string GUID = "com.Nuxlar.FathomlessVoidling";

        public void Awake()
        {
            ModConfig.InitConfig(base.Config);
            ModifyBodies();
            ModifyMasters();
            SetupProjectiles();
            AddContent();

            On.RoR2.CharacterMaster.OnBodyStart += CharacterMaster_OnBodyStart;
            RoR2.Stage.onServerStageBegin += Stage_onServerStageBegin;
            On.RoR2.VoidStageMissionController.RequestFog += VoidStageMissionController_RequestFog;
            On.RoR2.TeleporterInteraction.AttemptToSpawnAllEligiblePortals += TeleporterInteraction_AttemptToSpawnAllEligiblePortals1;
            if (ModConfig.Phase2Vacuum.Value)
            {
                On.EntityStates.VoidRaidCrab.BaseVacuumAttackState.OnEnter += BaseVacuumAttackState_OnEnter;
            }
            FathomlessVoidling.voidRaid.blockOrbitalSkills = false;
        }

        // I am not rewriting skills for 3 bodies, fuck this
        private void CharacterMaster_OnBodyStart(On.RoR2.CharacterMaster.orig_OnBodyStart orig, CharacterMaster self, CharacterBody body)
        {
            orig(self, body);
            if (body.name == "MiniVoidRaidCrabBodyPhase1(Clone)")
            {
                AdjustPhase1Skills(body);
            }
            if (body.name == "MiniVoidRaidCrabBodyPhase2(Clone)")
            {
                AdjustPhase2Skills(body);
            }
            if (body.name == "MiniVoidRaidCrabBodyPhase3(Clone)")
            {
                AdjustPhase3Skills(body);
            }
        }

        private void ModifyMasters()
        {
            var OOBItemDef = Addressables.LoadAssetAsync<ItemDef>("RoR2/Base/TeleportWhenOob/TeleportWhenOob.asset").WaitForCompletion();

            var phase1Master = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/MiniVoidRaidCrabMasterPhase1.prefab").WaitForCompletion();
            ModifyAIStates(phase1Master);
            AddTeleportWhenOOB(phase1Master, OOBItemDef);

            var phase2Master = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/MiniVoidRaidCrabMasterPhase2.prefab").WaitForCompletion();
            ModifyAIStates(phase2Master);
            AddTeleportWhenOOB(phase2Master, OOBItemDef);

            var phase3Master = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/MiniVoidRaidCrabMasterPhase3.prefab").WaitForCompletion();
            ModifyAIStates(phase3Master);
            AddTeleportWhenOOB(phase3Master, OOBItemDef);

            void AddTeleportWhenOOB(GameObject master, ItemDef oobItem)
            {
                var pickups = master.GetComponent<GivePickupsOnStart>();
                HG.ArrayUtils.ArrayAppend(ref pickups.itemDefInfos, new RoR2.GivePickupsOnStart.ItemDefInfo
                {
                    count = 1,
                    dontExceedCount = true,
                    itemDef = oobItem
                });
            }

            void ModifyAIStates(GameObject master)
            {
                (from x in master.GetComponents<AISkillDriver>()
                 where x.skillSlot == SkillSlot.Primary
                 select x).First<AISkillDriver>();
                AISkillDriver aiskillDriver = (from x in master.GetComponents<AISkillDriver>()
                                               where x.skillSlot == SkillSlot.Secondary
                                               select x).First<AISkillDriver>();
                AISkillDriver aiskillDriver2 = (from x in master.GetComponents<AISkillDriver>()
                                                where x.skillSlot == SkillSlot.Utility
                                                select x).First<AISkillDriver>();
                AISkillDriver aiskillDriver3 = (from x in master.GetComponents<AISkillDriver>()
                                                where x.skillSlot == SkillSlot.Special
                                                select x).First<AISkillDriver>();
                aiskillDriver.movementType = AISkillDriver.MovementType.Stop;
                aiskillDriver.maxUserHealthFraction = 0.9f;
                aiskillDriver.minUserHealthFraction = float.NegativeInfinity;
                aiskillDriver.requiredSkill = null;
                aiskillDriver.maxDistance = 200f;
                aiskillDriver2.maxUserHealthFraction = float.PositiveInfinity;
                aiskillDriver2.minUserHealthFraction = float.NegativeInfinity;
                aiskillDriver2.requiredSkill = null;
                aiskillDriver2.maxDistance = 400f;
                aiskillDriver2.minDistance = 0f;
                aiskillDriver3.maxUserHealthFraction = 0.8f;
                aiskillDriver3.minUserHealthFraction = float.NegativeInfinity;
                aiskillDriver3.requiredSkill = null;
                aiskillDriver3.maxDistance = 400f;
                aiskillDriver3.minDistance = 0f;
            }
        }

        private void ModifyBodies()
        {
            var skillsWhatever = new Skills();

            var phase1Body = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/MiniVoidRaidCrabBodyPhase1.prefab").WaitForCompletion();
            AdjustPhase1Stats(phase1Body);
            skillsWhatever.CreatePrimary(0, phase1Body);
            skillsWhatever.CreateSecondary(0, phase1Body);
            skillsWhatever.CreateUtility(0, phase1Body);
            skillsWhatever.CreateSpecial(0, phase1Body);
            //AdjustPhase1Skills(phase1Body.GetComponent<CharacterBody>());

            var phase2Body = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/MiniVoidRaidCrabBodyPhase2.prefab").WaitForCompletion();
            //AdjustPhase2Skills(phase2Body.GetComponent<CharacterBody>());
            AdjustPhase2Stats(phase2Body);
            skillsWhatever.CreatePrimary(1, phase2Body);
            skillsWhatever.CreateSecondary(1, phase2Body);
            skillsWhatever.CreateUtility(1, phase2Body);
            skillsWhatever.CreateSpecial(1, phase2Body);

            var phase3Body = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/MiniVoidRaidCrabBodyPhase3.prefab").WaitForCompletion();
            //AdjustPhase3Skills(phase3Body.GetComponent<CharacterBody>());
            AdjustPhase3Stats(phase3Body);
            skillsWhatever.CreatePrimary(2, phase3Body);
            skillsWhatever.CreateSecondary(2, phase3Body);
            skillsWhatever.CreateUtility(2, phase3Body);
            skillsWhatever.CreateSpecial(2, phase3Body);
        }

        private void BaseVacuumAttackState_OnEnter(On.EntityStates.VoidRaidCrab.BaseVacuumAttackState.orig_OnEnter orig, EntityStates.VoidRaidCrab.BaseVacuumAttackState self)
        {
            orig.Invoke(self);
            if (self.characterBody.name == "MiniVoidRaidCrabBodyPhase2(Clone)")
            {
                Transform transform = VoidRaidGauntletController.instance.currentDonut.root.transform.Find("HOLDER: Skybox+PP/ReflectionProbe, Center");
                if (transform)
                {
                    self.vacuumOrigin = transform;
                }
            }
        }

        private void TeleporterInteraction_AttemptToSpawnAllEligiblePortals1(On.RoR2.TeleporterInteraction.orig_AttemptToSpawnAllEligiblePortals orig, TeleporterInteraction self)
        {
            if (self.beginContextString.Contains("LUNAR") && ModConfig.enableAltMoon.Value)
            {
                List<PortalSpawner> list = self.portalSpawners.ToList<PortalSpawner>();
                PortalSpawner portalSpawner = list.Find((PortalSpawner x) => x.portalSpawnCard == FathomlessVoidling.locusPortalCard);
                if (portalSpawner != null)
                {
                    list.Remove(portalSpawner);
                    self.portalSpawners = list.ToArray();
                }
                this.SpawnLocusPortal(self.transform, self.rng);
            }
            orig.Invoke(self);
        }

        private VoidStageMissionController.FogRequest VoidStageMissionController_RequestFog(On.RoR2.VoidStageMissionController.orig_RequestFog orig, VoidStageMissionController self, IZone zone)
        {
            if (ModConfig.enableVoidFog.Value)
            {
                return orig.Invoke(self, zone);
            }
            return null;
        }

        private void Stage_onServerStageBegin(Stage stage)
        {
            if (stage.sceneDef.cachedName == "voidstage" && ModConfig.enableAltMoon.Value)
            {
                this.SpawnLocusCauldrons();
            }
        }

        private void SpawnLocusCauldrons()
        {
            GameObject gameObject = Object.Instantiate<GameObject>(FathomlessVoidling.r2wCauldron, new Vector3(-142.67f, 29.94f, 242.74f), Quaternion.identity);
            gameObject.transform.eulerAngles = new Vector3(0f, 66f, 0f);
            NetworkServer.Spawn(gameObject);
            GameObject gameObject2 = Object.Instantiate<GameObject>(FathomlessVoidling.g2rCauldron, new Vector3(-136.76f, 29.94f, 246.51f), Quaternion.identity);
            gameObject2.transform.eulerAngles = new Vector3(0f, 66f, 0f);
            NetworkServer.Spawn(gameObject2);
            GameObject gameObject3 = Object.Instantiate<GameObject>(FathomlessVoidling.g2rCauldron, new Vector3(-149.74f, 29.93f, 239.7f), Quaternion.identity);
            gameObject3.transform.eulerAngles = new Vector3(0f, 66f, 0f);
            NetworkServer.Spawn(gameObject3);
            GameObject gameObject4 = Object.Instantiate<GameObject>(FathomlessVoidling.w2gCauldron, new Vector3(-157.41f, 29.97f, 237.12f), Quaternion.identity);
            gameObject4.transform.eulerAngles = new Vector3(0f, 66f, 0f);
            NetworkServer.Spawn(gameObject4);
            GameObject gameObject5 = Object.Instantiate<GameObject>(FathomlessVoidling.w2gCauldron, new Vector3(-126.63f, 29.93f, 249.1f), Quaternion.identity);
            gameObject5.transform.eulerAngles = new Vector3(0f, 66f, 0f);
            NetworkServer.Spawn(gameObject5);
        }

        private void SpawnLocusPortal(Transform transform, Xoroshiro128Plus rng)
        {
            if (!NetworkServer.active)
            {
                return;
            }

            DirectorCore instance = DirectorCore.instance;
            DirectorPlacementRule directorPlacementRule = new DirectorPlacementRule();
            directorPlacementRule.minDistance = 10f;
            directorPlacementRule.maxDistance = 40f;
            directorPlacementRule.placementMode = DirectorPlacementRule.PlacementMode.Approximate;
            directorPlacementRule.position = transform.position;
            directorPlacementRule.spawnOnTarget = transform;
            DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(FathomlessVoidling.locusPortalCard, directorPlacementRule, rng);
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

        private void AddContent()
        {
            ContentAddition.AddEntityState<ChargeRend>(out _);
            ContentAddition.AddEntityState<Disillusion>(out _);
            ContentAddition.AddEntityState<Rend>(out _);
            ContentAddition.AddEntityState<Transpose>(out _);

            ContentAddition.AddEntityState<ChargeCrush>(out _);
            ContentAddition.AddEntityState<Crush>(out _);

            //ContentAddition.AddEntityState<Reap>(out _); // Reap is unused

            ContentAddition.AddProjectile(FathomlessVoidling.meteor);
        }

        private void SetupProjectiles()
        {
            base.Logger.LogInfo("Setting Up Projectiles");
            ProjectileController component = FathomlessVoidling.meteor.GetComponent<ProjectileController>();
            component.cannotBeDeleted = true;
            FathomlessVoidling.meteor.transform.localScale = new Vector3(1.75f, 1.75f, 1.75f);
            FathomlessVoidling.meteorGhost.transform.localScale = new Vector3(2f, 2f, 2f);
            FathomlessVoidling.meteorGhost.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>()
                .materials = new Material[]
            {
                FathomlessVoidling.boulderMat,
                FathomlessVoidling.voidAffixMat
            };
            component.ghost = FathomlessVoidling.meteorGhost.GetComponent<ProjectileGhostController>();
            component.ghostPrefab = FathomlessVoidling.meteorGhost;

            var voidCrabMissile = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabMissileProjectile.prefab").WaitForCompletion();
            voidCrabMissile.GetComponent<ProjectileSteerTowardTarget>().rotationSpeed = 180f;

            base.Logger.LogInfo("Finished Setting Up Projectiles");
        }

        private void AdjustPhase1Stats(GameObject gameObject)
        {
            base.Logger.LogInfo("Adjusting P1 Stats");
            CharacterBody component = gameObject.GetComponent<CharacterBody>();
            component.subtitleNameToken = "Augur of the Abyss";
            component.baseMaxHealth = ModConfig.baseHealth.Value;
            component.levelMaxHealth = ModConfig.levelHealth.Value;
            component.baseDamage = ModConfig.baseDamage.Value;
            component.levelDamage = ModConfig.levelDamage.Value;
            component.baseAttackSpeed = ModConfig.baseAtkSpd.Value;
            component.baseMoveSpeed = ModConfig.baseSpd.Value;
            component.baseAcceleration = ModConfig.acceleration.Value;
            component.baseArmor = ModConfig.baseArmor.Value;
            base.Logger.LogInfo("Finished Adjusting P1 Stats");
        }

        private void AdjustPhase2Stats(GameObject gameObject)
        {
            base.Logger.LogInfo("Adjusting P2 Stats");
            CharacterBody component = gameObject.GetComponent<CharacterBody>();
            component.subtitleNameToken = "Augur of the Abyss";
            component.baseMaxHealth = ModConfig.baseHealth.Value;
            component.levelMaxHealth = ModConfig.levelHealth.Value;
            component.baseDamage = ModConfig.baseDamage.Value;
            component.levelDamage = ModConfig.levelDamage.Value;
            component.baseAttackSpeed = ModConfig.baseAtkSpd.Value;
            component.baseMoveSpeed = ModConfig.baseSpd.Value;
            component.baseAcceleration = ModConfig.acceleration.Value;
            component.baseArmor = ModConfig.baseArmor.Value;
            base.Logger.LogInfo("Finished Adjusting P2 Stats");
        }

        private void AdjustPhase3Stats(GameObject gameObject)
        {
            base.Logger.LogInfo("Adjusting P3 Stats");
            CharacterBody component = gameObject.GetComponent<CharacterBody>();
            component.subtitleNameToken = "Augur of the Abyss";
            component.baseMaxHealth = ModConfig.baseHealth.Value;
            component.levelMaxHealth = ModConfig.levelHealth.Value;
            component.baseDamage = ModConfig.baseDamage.Value;
            component.levelDamage = ModConfig.levelDamage.Value;
            component.baseAttackSpeed = ModConfig.baseAtkSpd.Value;
            component.baseMoveSpeed = ModConfig.baseSpd.Value;
            component.baseAcceleration = ModConfig.acceleration.Value;
            component.baseArmor = ModConfig.baseArmor.Value;
            base.Logger.LogInfo("Finished Adjusting P3 Stats");
        }

        private void AdjustPhase1Skills(CharacterBody body)
        {
            base.Logger.LogInfo("Adjusting P1 Skills");
            SkillLocator skillLocator = body.skillLocator;
            SkillDef skillDef = skillLocator.primary.skillFamily.variants[0].skillDef;
            SkillDef skillDef2 = skillLocator.secondary.skillFamily.variants[0].skillDef;
            SkillDef skillDef3 = skillLocator.utility.skillFamily.variants[0].skillDef;
            SkillDef skillDef4 = skillLocator.special.skillFamily.variants[0].skillDef;
            skillDef.activationState = new SerializableEntityStateType(typeof(Disillusion));
            skillDef2.activationState = new SerializableEntityStateType(typeof(EntityStates.VoidRaidCrab.VacuumEnter));
            skillDef3.activationState = new SerializableEntityStateType(typeof(Transpose));
            skillDef4.activationState = new SerializableEntityStateType(typeof(ChargeRend));
            base.Logger.LogInfo("Finished Adjusting P1 Skills");
        }

        private void AdjustPhase2Skills(CharacterBody body)
        {
            base.Logger.LogInfo("Adjusting P2 Skills");
            SkillLocator skillLocator = body.skillLocator;
            SkillDef skillDef = skillLocator.primary.skillFamily.variants[0].skillDef;
            SkillDef skillDef2 = skillLocator.secondary.skillFamily.variants[0].skillDef;
            SkillDef skillDef3 = skillLocator.utility.skillFamily.variants[0].skillDef;
            SkillDef skillDef4 = skillLocator.special.skillFamily.variants[0].skillDef;
            skillDef.activationState = new SerializableEntityStateType(typeof(Disillusion));
            skillDef2.activationState = new SerializableEntityStateType(typeof(EntityStates.VoidRaidCrab.VacuumEnter));
            skillDef3.activationState = new SerializableEntityStateType(typeof(Transpose));
            skillDef4.activationState = new SerializableEntityStateType(typeof(EntityStates.VoidRaidCrab.SpinBeamEnter));
            base.Logger.LogInfo("Finished Adjusting P2 Skills");
        }

        private void AdjustPhase3Skills(CharacterBody body)
        {
            base.Logger.LogInfo("Adjusting P3 Skills");
            SkillLocator skillLocator = body.skillLocator;
            SkillDef skillDef = skillLocator.primary.skillFamily.variants[0].skillDef;
            SkillDef skillDef2 = skillLocator.secondary.skillFamily.variants[0].skillDef;
            SkillDef skillDef3 = skillLocator.utility.skillFamily.variants[0].skillDef;
            SkillDef skillDef4 = skillLocator.special.skillFamily.variants[0].skillDef;
            skillDef.activationState = new SerializableEntityStateType(typeof(Disillusion));
            skillDef2.activationState = new SerializableEntityStateType(typeof(ChargeCrush));
            skillDef3.activationState = new SerializableEntityStateType(typeof(Transpose));
            skillDef4.activationState = new SerializableEntityStateType(typeof(EntityStates.VoidRaidCrab.SpinBeamEnter));
            base.Logger.LogInfo("Finished Adjusting P3 Skills");
        }


        public static GameObject meteor = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Grandparent/GrandparentBoulder.prefab").WaitForCompletion(), "VoidMeteor");

        private static GameObject meteorGhost = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Grandparent/GrandparentBoulderGhost.prefab").WaitForCompletion(), "VoidMeteorGhost");

        public static GameObject portal = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabSpawnEffect.prefab").WaitForCompletion();

        public static GameObject bombPrefab3 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidJailer/VoidJailerDeathBombProjectile.prefab").WaitForCompletion();

        public static GameObject bombPrefab2 = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Nullifier/NullifierDeathBombProjectile.prefab").WaitForCompletion();

        public static GameObject bombPrefab1 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathBombProjectile.prefab").WaitForCompletion();

        private static Material boulderMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/Grandparent/matGrandparentBoulderProjectile.mat").WaitForCompletion();

        private static Material voidAffixMat = Addressables.LoadAssetAsync<Material>("RoR2/DLC1/EliteVoid/matEliteVoidOverlay.mat").WaitForCompletion();

        public static GameObject deathBombPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathBombExplosion.prefab").WaitForCompletion();

        public static GameObject spawnEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabSpawnEffect.prefab").WaitForCompletion();

        public static GameObject spinBeamVFX = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabSpinBeamVFX.prefab").WaitForCompletion();

        public static GameObject r2wCauldron = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarCauldrons/LunarCauldron, RedToWhite Variant.prefab").WaitForCompletion();

        public static GameObject g2rCauldron = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarCauldrons/LunarCauldron, GreenToRed Variant.prefab").WaitForCompletion();

        public static GameObject w2gCauldron = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarCauldrons/LunarCauldron, WhiteToGreen.prefab").WaitForCompletion();

        public static SpawnCard locusPortalCard = Addressables.LoadAssetAsync<SpawnCard>("RoR2/DLC1/PortalVoid/iscVoidPortal.asset").WaitForCompletion();

        public static SceneDef voidRaid = Addressables.LoadAssetAsync<SceneDef>("RoR2/DLC1/voidraid/voidraid.asset").WaitForCompletion();
    }
}
