using BepInEx;
using JetBrains.Annotations;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ArtifactofTheKing
{
    [BepInPlugin(GUID, ModName, Version)]
    [BepInDependency(R2API.ContentManagement.R2APIContentManager.PluginGUID)]
    public class ArtifactOfTheKing : BaseUnityPlugin
    {
        public const string Author = "Original by Blobface, ported to SoTS by viliger";
        public const string ModName = "Artifact of the King";
        public const string Version = "1.2.6";
        public const string GUID = "com.Blobface.ArtifactKing";

        public static ArtifactDef King;

        private static GameObject Mithrix;

        private static GameObject BrotherGlassBody;

        private static bool hasFiredWeaponSlam = false; // why
        private static bool hasFiredSkyLeapFirst = false; // now this is OUR shitcode
        private static bool hasFiredSkyLeapSecond = false;
        private static bool isEnabled = false;

        private void Awake()
        {
            KingConfiguration.PopulateConfig(Config);

            AssetBundle bundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Info.Location), "assetbundles", "artifactoftheking"));

            Mithrix = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherBody.prefab").WaitForCompletion();

            King = ScriptableObject.CreateInstance<ArtifactDef>();
            King.cachedName = "ArtifactOfKing_Blobface";
            (King as ScriptableObject).name = King.cachedName;
            King.nameToken = "ARTIFACT_OF_THE_KING_NAME";
            King.descriptionToken = "ARTIFACT_OF_THE_KING_DESCRIPTION";
            King.smallIconSelectedSprite = bundle.LoadAsset<Sprite>("Assets/ArtifactOfTheKing/headon.png");
            King.smallIconDeselectedSprite = bundle.LoadAsset<Sprite>("Assets/ArtifactOfTheKing/headoff.png");

            R2API.ContentAddition.AddArtifactDef(King);

            RoR2.Language.collectLanguageRootFolders += Language_collectLanguageRootFolders;
            RoR2.RunArtifactManager.onArtifactEnabledGlobal += RunArtifactManager_onArtifactEnabledGlobal;
            RoR2.RunArtifactManager.onArtifactDisabledGlobal += RunArtifactManager_onArtifactDisabledGlobal;

            ModifyBrotherGlass();
        }

        private void ModifyBrotherGlass()
        {
            BrotherGlassBody = Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Junk_BrotherGlass.BrotherGlassBody_prefab).WaitForCompletion();
            var mdlBrother = BrotherGlassBody.transform.Find("ModelBase/mdlBrother");
            var mesh = Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_Brother.mdlBrother_fbx).WaitForCompletion();

            if (mdlBrother)
            {
                if (mdlBrother.TryGetComponent<ModelSkinController>(out var modelSkinController))
                {
                    UnityEngine.Object.DestroyImmediate(modelSkinController);
                }
                if (mdlBrother.TryGetComponent<Animator>(out var animator))
                {
                    animator.runtimeAnimatorController = Addressables.LoadAssetAsync<RuntimeAnimatorController>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_Brother.animBrother_controller).WaitForCompletion();
                    animator.avatar = mesh.GetComponent<Animator>().avatar;
                }
            }

            var eye = BrotherGlassBody.transform.Find("ModelBase/mdlBrother/BrotherArmature/ROOT/base/stomach/chest/neck/head/eyeball/BrotherEye");
            if (eye)
            {
                eye.GetComponent<MeshFilter>().mesh = mesh.transform.Find("BrotherArmature/ROOT/base/stomach/chest/neck/head/eyeball/BrotherEye").GetComponent<MeshFilter>().mesh;
                eye.GetComponent<MeshRenderer>().material = Addressables.LoadAssetAsync<Material>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_Brother.matBrotherEye_mat).WaitForCompletion();
            }

            var bodyMesh = BrotherGlassBody.transform.Find("ModelBase/mdlBrother/BrotherBodyMesh");
            if (bodyMesh)
            {
                bodyMesh.GetComponent<SkinnedMeshRenderer>().sharedMesh = mesh.transform.Find("BrotherBodyMesh").GetComponent<SkinnedMeshRenderer>().sharedMesh;
            }

            var hammerConcrete = BrotherGlassBody.transform.Find("ModelBase/mdlBrother/BrotherHammerConcrete");
            if (hammerConcrete)
            {
                hammerConcrete.GetComponent<SkinnedMeshRenderer>().sharedMesh = mesh.transform.Find("BrotherHammerConcrete").GetComponent<SkinnedMeshRenderer>().sharedMesh;
            }

            var hammerStib = BrotherGlassBody.transform.Find("ModelBase/mdlBrother/BrotherHammerConcrete/BrotherHammerStib");
            if (hammerStib)
            {
                var hammerStibSMR = hammerStib.GetComponent<SkinnedMeshRenderer>();
                hammerStibSMR.sharedMesh = mesh.transform.Find("BrotherHammerConcrete/BrotherHammerStib").GetComponent<SkinnedMeshRenderer>().sharedMesh;
                hammerStibSMR.material = Addressables.LoadAssetAsync<Material>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_Brother.matBrotherStib_mat).WaitForCompletion();
            }

            var stibPieces = BrotherGlassBody.transform.Find("ModelBase/mdlBrother/BrotherStibPieces");
            if (stibPieces)
            {
                var hammerPiecesSMR = stibPieces.GetComponent<SkinnedMeshRenderer>();
                hammerPiecesSMR.sharedMesh = mesh.transform.Find("BrotherStibPieces").GetComponent<SkinnedMeshRenderer>().sharedMesh;
                hammerPiecesSMR.material = Addressables.LoadAssetAsync<Material>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_Brother.matBrotherStib_mat).WaitForCompletion();
            }
        }

        private void RunArtifactManager_onArtifactEnabledGlobal([NotNull] RunArtifactManager runArtifactManager, [NotNull] ArtifactDef artifactDef)
        {
            if (artifactDef == King && !isEnabled)
            {
                isEnabled = true;
                Logger.LogMessage("Initializing modded stats");
                AddHooks();
                AdjustSkills();
                AdjustStats();
            }
        }

        private void RunArtifactManager_onArtifactDisabledGlobal([NotNull] RunArtifactManager runArtifactManager, [NotNull] ArtifactDef artifactDef)
        {
            if (artifactDef == King && isEnabled)
            {
                isEnabled = false;
                Logger.LogMessage("Reverting to vanilla stats");
                RemoveHooks();
                RevertSkills();
                RevertStats();
            }
        }

        private void AddHooks()
        {
            On.RoR2.HealthComponent.ServerFixedUpdate += HealthComponent_ServerFixedUpdate;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            On.EntityStates.BrotherMonster.SlideIntroState.OnEnter += SlideIntroState_OnEnter;
            On.EntityStates.BrotherMonster.SprintBash.OnEnter += SprintBash_OnEnter;
            On.EntityStates.BrotherMonster.WeaponSlam.OnEnter += WeaponSlam_OnEnter;
            On.EntityStates.BrotherMonster.WeaponSlam.FixedUpdate += WeaponSlam_FixedUpdate;
            On.EntityStates.BrotherMonster.Weapon.FireLunarShards.OnEnter += FireLunarShards_OnEnter;

            // this never worked
            //On.EntityStates.BrotherMonster.ExitSkyLeap.OnEnter += ExitSkyLeap_OnEnter;
            // so we write our own shitcode instead
            if (KingConfiguration.EnableFixes.Value)
            {
                On.EntityStates.BrotherMonster.ExitSkyLeap.OnEnter += ExitSkyLeap_OnEnter;
                On.EntityStates.BrotherMonster.ExitSkyLeap.FixedUpdate += ExitSkyLeap_FixedUpdate;
            }
        }

        private void RemoveHooks()
        {
            On.RoR2.HealthComponent.ServerFixedUpdate -= HealthComponent_ServerFixedUpdate;
            On.RoR2.HealthComponent.TakeDamage -= HealthComponent_TakeDamage;
            On.EntityStates.BrotherMonster.SlideIntroState.OnEnter -= SlideIntroState_OnEnter;
            On.EntityStates.BrotherMonster.SprintBash.OnEnter -= SprintBash_OnEnter;
            On.EntityStates.BrotherMonster.WeaponSlam.OnEnter -= WeaponSlam_OnEnter;
            On.EntityStates.BrotherMonster.WeaponSlam.FixedUpdate -= WeaponSlam_FixedUpdate;
            On.EntityStates.BrotherMonster.Weapon.FireLunarShards.OnEnter -= FireLunarShards_OnEnter;

            // this never worked
            //On.EntityStates.BrotherMonster.ExitSkyLeap.OnEnter += ExitSkyLeap_OnEnter;
            // so we write our own shitcode instead
            //if (KingConfiguration.EnableFixes.Value)
            //{
            On.EntityStates.BrotherMonster.ExitSkyLeap.OnEnter -= ExitSkyLeap_OnEnter;
            On.EntityStates.BrotherMonster.ExitSkyLeap.FixedUpdate -= ExitSkyLeap_FixedUpdate;
            //}
        }

        private void ExitSkyLeap_FixedUpdate(On.EntityStates.BrotherMonster.ExitSkyLeap.orig_FixedUpdate orig, EntityStates.BrotherMonster.ExitSkyLeap self)
        {
            if (self.isAuthority)
            {
                if (self.fixedAge > 0.45f * self.duration && !hasFiredSkyLeapFirst)
                {
                    hasFiredSkyLeapFirst = true;
                    self.FireRingAuthority();
                }
                if (self.fixedAge > 0.9f * self.duration && !hasFiredSkyLeapSecond)
                {
                    hasFiredSkyLeapSecond = true;
                    self.FireRingAuthority();
                }
            }

            orig(self);
        }

        private void ExitSkyLeap_OnEnter(On.EntityStates.BrotherMonster.ExitSkyLeap.orig_OnEnter orig, EntityStates.BrotherMonster.ExitSkyLeap self)
        {
            if (self.isAuthority)
            {
                hasFiredSkyLeapFirst = false;
                hasFiredSkyLeapSecond = false;
            }

            orig(self);
        }

        private void FireLunarShards_OnEnter(On.EntityStates.BrotherMonster.Weapon.FireLunarShards.orig_OnEnter orig, EntityStates.BrotherMonster.Weapon.FireLunarShards self)
        {
            if (self.isAuthority)
            {
                if (!(self is EntityStates.BrotherMonster.Weapon.FireLunarShardsHurt) && KingConfiguration.AdditionalShards.Value)
                {
                    FireAdditionalShards();
                }

                if (self is EntityStates.BrotherMonster.Weapon.FireLunarShardsHurt && KingConfiguration.AdditionalShardsHurt.Value)
                {
                    FireAdditionalShards();
                }
            }

            orig(self);

            void FireAdditionalShards()
            {
                var aimRay = self.GetAimRay();
                var shardsMuzzleTransform = self.FindModelChild(EntityStates.BrotherMonster.Weapon.FireLunarShards.muzzleString);
                if (shardsMuzzleTransform)
                {
                    aimRay.origin = shardsMuzzleTransform.position;
                }
                var projectileInfo = new FireProjectileInfo()
                {
                    position = aimRay.origin,
                    rotation = Quaternion.LookRotation(aimRay.direction),
                    crit = self.RollCrit(),
                    damage = self.damageStat * self.damageCoefficient,
                    owner = self.gameObject,
                    force = 0f,
                    projectilePrefab = EntityStates.BrotherMonster.Weapon.FireLunarShards.projectilePrefab
                };
                for (int i = 0; i < KingConfiguration.LunarShardAdd.Value; i++)
                {
                    ProjectileManager.instance.FireProjectile(projectileInfo);
                    aimRay.direction = Util.ApplySpread(aimRay.direction, 0f, self.maxSpread * (1f + 0.45f * i), self.spreadYawScale * (1f + 0.45f * i), self.spreadPitchScale * (1f + 0.45f * i), 0f, 0f);
                    projectileInfo.rotation = Quaternion.LookRotation(aimRay.direction);
                }
            }
        }

        private void WeaponSlam_FixedUpdate(On.EntityStates.BrotherMonster.WeaponSlam.orig_FixedUpdate orig, EntityStates.BrotherMonster.WeaponSlam self)
        {
            if (self.isAuthority)
            {
                if (self.hasDoneBlastAttack)
                {
                    if (self.modelTransform && !hasFiredWeaponSlam)
                    {
                        hasFiredWeaponSlam = true;
                        float angle = 360f / KingConfiguration.SlamOrbCount.Value;
                        var projectVector = Vector3.ProjectOnPlane(self.inputBank.aimDirection, Vector3.up);
                        var muzzleTransform = self.FindModelChild(EntityStates.BrotherMonster.WeaponSlam.muzzleString);
                        for (int i = 0; i < KingConfiguration.SlamOrbCount.Value; i++)
                        {
                            var vector = Quaternion.AngleAxis(angle * i, Vector3.up) * projectVector;
                            ProjectileManager.instance.FireProjectile(
                                EntityStates.BrotherMonster.FistSlam.waveProjectilePrefab,
                                muzzleTransform.position,
                                Util.QuaternionSafeLookRotation(vector),
                                self.gameObject,
                                self.damageStat * EntityStates.BrotherMonster.FistSlam.waveProjectileDamageCoefficient,
                                EntityStates.BrotherMonster.FistSlam.waveProjectileForce,
                                self.RollCrit(),
                                DamageColorIndex.Default);
                        }
                    }
                }
            }
            orig(self);
        }

        private void WeaponSlam_OnEnter(On.EntityStates.BrotherMonster.WeaponSlam.orig_OnEnter orig, EntityStates.BrotherMonster.WeaponSlam self)
        {
            if (self.isAuthority)
            {
                hasFiredWeaponSlam = false;
            }

            orig(self);
        }

        private void SprintBash_OnEnter(On.EntityStates.BrotherMonster.SprintBash.orig_OnEnter orig, EntityStates.BrotherMonster.SprintBash self)
        {
            if (self.isAuthority)
            {
                var aimRay = self.GetAimRay();
                for (int i = 0; i < KingConfiguration.SecondaryFan.Value; i++)
                {
                    var directionWithSpread = Util.ApplySpread(aimRay.direction, 0f, 0f, 1f, 0f, i * 5f, 0f);
                    ProjectileManager.instance.FireProjectile(
                        EntityStates.BrotherMonster.Weapon.FireLunarShards.projectilePrefab,
                        aimRay.origin,
                        Util.QuaternionSafeLookRotation(directionWithSpread),
                        self.gameObject,
                        self.damageStat * 0.1f / 12f,
                        0f,
                        self.RollCrit(),
                        DamageColorIndex.Default);
                    directionWithSpread = Util.ApplySpread(aimRay.direction, 0f, 0f, 1f, 0f, -i * 5f, 0f);
                    ProjectileManager.instance.FireProjectile(
                        EntityStates.BrotherMonster.Weapon.FireLunarShards.projectilePrefab,
                        aimRay.origin,
                        Util.QuaternionSafeLookRotation(directionWithSpread),
                        self.gameObject,
                        self.damageStat * 0.1f / 12f,
                        0f,
                        self.RollCrit(),
                        DamageColorIndex.Default);
                }
            }

            orig(self);
        }

        private void SlideIntroState_OnEnter(On.EntityStates.BrotherMonster.SlideIntroState.orig_OnEnter orig, EntityStates.BrotherMonster.SlideIntroState self)
        {
            if (self.isAuthority)
            {
                var aimRay = self.GetAimRay();
                for (int i = 0; i < KingConfiguration.UtilityShotgun.Value; i++)
                {
                    ProjectileManager.instance.FireProjectile(
                        EntityStates.BrotherMonster.Weapon.FireLunarShards.projectilePrefab,
                        aimRay.origin,
                        Quaternion.LookRotation(aimRay.direction),
                        self.gameObject,
                        self.damageStat * 0.05f / 12f,
                        0f,
                        self.RollCrit(),
                        DamageColorIndex.Default);
                    aimRay.direction = Util.ApplySpread(aimRay.direction, 0f, 4f, 4f, 4f, 0f, 0f);
                }
            }

            orig(self);
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            orig(self, damageInfo);

            // stupid hack
            if (self && self.name.StartsWith("Brother"))
            {
                float num = damageInfo.damage / self.fullCombinedHealth * 100f * 50f * self.itemCounts.adaptiveArmor;
                self.adaptiveArmorValue = Mathf.Min(self.adaptiveArmorValue + num, 900f);
            }
        }

        private void HealthComponent_ServerFixedUpdate(On.RoR2.HealthComponent.orig_ServerFixedUpdate orig, HealthComponent self, float deltaTime)
        {
            orig(self, deltaTime);

            // stupid hack
            if (self && self.name.StartsWith("Brother"))
            {
                self.adaptiveArmorValue = Mathf.Max(0, self.adaptiveArmorValue - 100f * deltaTime);
            }
        }

        private void AdjustSkills()
        {
            if (Mithrix)
            {
                SkillLocator skillLocator = Mithrix.GetComponent<SkillLocator>();
                if (skillLocator == null)
                {
                    return;
                }

                ModifySkill(skillLocator.primary.skillFamily.variants[0].skillDef, KingConfiguration.PrimCD.Value, KingConfiguration.PrimStocks.Value);
                ModifySkill(skillLocator.secondary.skillFamily.variants[0].skillDef, KingConfiguration.SecCD.Value, KingConfiguration.SecStocks.Value);
                ModifySkill(skillLocator.utility.skillFamily.variants[0].skillDef, KingConfiguration.UtilCD.Value, KingConfiguration.UtilStocks.Value);
                ModifySkill(skillLocator.special.skillFamily.variants[0].skillDef, KingConfiguration.SpecialCD.Value, KingConfiguration.SpecialStocks.Value);
            }
        }

        private void AdjustStats()
        {
            if (Mithrix)
            {
                var characterBody = Mithrix.GetComponent<CharacterBody>();

                characterBody.baseMaxHealth = KingConfiguration.basehealth.Value;
                characterBody.levelMaxHealth = KingConfiguration.levelhealth.Value;
                characterBody.baseAttackSpeed = KingConfiguration.baseattackspeed.Value;
                characterBody.baseMoveSpeed = KingConfiguration.basespeed.Value;
                characterBody.baseAcceleration = KingConfiguration.acceleration.Value;
                characterBody.baseJumpPower = KingConfiguration.jumpingpower.Value;
                characterBody.baseArmor = KingConfiguration.basearmor.Value;
                characterBody.baseDamage = KingConfiguration.basedamage.Value;
                characterBody.levelDamage = KingConfiguration.leveldamage.Value;

                var characterDirection = Mithrix.GetComponent<CharacterDirection>();
                characterDirection.turnSpeed = KingConfiguration.turningspeed.Value;

                var characterMotor = Mithrix.GetComponent<CharacterMotor>();
                characterMotor.mass = KingConfiguration.mass.Value;
                characterMotor.airControl = KingConfiguration.aircontrol.Value;
                characterMotor.jumpCount = KingConfiguration.jumpcount.Value;

                var projectileSteerTowardsTarget = EntityStates.BrotherMonster.Weapon.FireLunarShards.projectilePrefab.GetComponent<ProjectileSteerTowardTarget>();
                projectileSteerTowardsTarget.rotationSpeed = KingConfiguration.ShardHoming.Value;

                var projectileDirectionTargetFinder = EntityStates.BrotherMonster.Weapon.FireLunarShards.projectilePrefab.GetComponent<ProjectileDirectionalTargetFinder>();
                projectileDirectionTargetFinder.lookRange = KingConfiguration.ShardRange.Value;
                projectileDirectionTargetFinder.lookCone = KingConfiguration.ShardCone.Value;
                projectileDirectionTargetFinder.allowTargetLoss = true;

                EntityStates.BrotherMonster.WeaponSlam.duration = 3.5f / KingConfiguration.baseattackspeed.Value;
                EntityStates.BrotherMonster.HoldSkyLeap.duration = KingConfiguration.JumpPause.Value;
                EntityStates.BrotherMonster.ExitSkyLeap.cloneCount = KingConfiguration.clonecount.Value;
                EntityStates.BrotherMonster.ExitSkyLeap.cloneDuration = KingConfiguration.cloneduration.Value;
                EntityStates.BrotherMonster.ExitSkyLeap.recastChance = KingConfiguration.JumpRecast.Value;
                EntityStates.BrotherMonster.UltChannelState.waveProjectileCount = KingConfiguration.UltimateWaves.Value;
                EntityStates.BrotherMonster.UltChannelState.maxDuration = KingConfiguration.UltimateDuration.Value;
                EntityStates.BrotherMonster.UltChannelState.totalWaves = KingConfiguration.UltimateCount.Value;
            }
        }

        private void RevertStats()
        {
            if (Mithrix)
            {
                var characterBody = Mithrix.GetComponent<CharacterBody>();

                characterBody.baseMaxHealth = 1000f;
                characterBody.levelMaxHealth = 300f;
                characterBody.baseAttackSpeed = 1f;
                characterBody.baseMoveSpeed = 15f;
                characterBody.baseAcceleration = 45f;
                characterBody.baseJumpPower = 25f;
                characterBody.baseArmor = 20f;
                characterBody.baseDamage = 16f;
                characterBody.levelDamage = 3.2f;

                var characterDirection = Mithrix.GetComponent<CharacterDirection>();
                characterDirection.turnSpeed = 270f;

                var characterMotor = Mithrix.GetComponent<CharacterMotor>();
                characterMotor.mass = 900f;
                characterMotor.airControl = 0.25f;
                characterMotor.jumpCount = 0;

                var projectileSteerTowardsTarget = EntityStates.BrotherMonster.Weapon.FireLunarShards.projectilePrefab.GetComponent<ProjectileSteerTowardTarget>();
                projectileSteerTowardsTarget.rotationSpeed = 20f;

                var projectileDirectionTargetFinder = EntityStates.BrotherMonster.Weapon.FireLunarShards.projectilePrefab.GetComponent<ProjectileDirectionalTargetFinder>();
                projectileDirectionTargetFinder.lookRange = 80f;
                projectileDirectionTargetFinder.lookCone = 90f;
                projectileDirectionTargetFinder.allowTargetLoss = false;

                EntityStates.BrotherMonster.WeaponSlam.duration = 4f;
                EntityStates.BrotherMonster.HoldSkyLeap.duration = 3f;
                EntityStates.BrotherMonster.ExitSkyLeap.cloneCount = 0;
                EntityStates.BrotherMonster.ExitSkyLeap.cloneDuration = 0;
                EntityStates.BrotherMonster.ExitSkyLeap.recastChance = 0;
                EntityStates.BrotherMonster.UltChannelState.waveProjectileCount = 9;
                EntityStates.BrotherMonster.UltChannelState.maxDuration = 8f;
                EntityStates.BrotherMonster.UltChannelState.totalWaves = 4;
            }
        }

        private void RevertSkills()
        {
            if (Mithrix)
            {
                SkillLocator skillLocator = Mithrix.GetComponent<SkillLocator>();
                if (skillLocator == null)
                {
                    return;
                }

                ModifySkill(skillLocator.primary.skillFamily.variants[0].skillDef, 4f, 1);
                ModifySkill(skillLocator.secondary.skillFamily.variants[0].skillDef, 5f, 1);
                ModifySkill(skillLocator.utility.skillFamily.variants[0].skillDef, 3f, 2);
                ModifySkill(skillLocator.special.skillFamily.variants[0].skillDef, 30f, 5);
            }
        }

        private void ModifySkill(SkillDef skillDef, float newRecharge, int newStock)
        {
            skillDef.baseRechargeInterval = newRecharge;
            skillDef.baseMaxStock = newStock;
        }

        private void Language_collectLanguageRootFolders(System.Collections.Generic.List<string> folders)
        {
            folders.Add(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(base.Info.Location), "Language"));
        }
    }
}