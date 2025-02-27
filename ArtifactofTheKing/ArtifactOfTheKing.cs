using BepInEx;
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
        public const string Version = "1.2.1";
        public const string GUID = "com.Blobface.ArtifactKing";

        public static ArtifactDef King;

        private static GameObject Mithrix;

        private static bool hasFiredWeaponSlam = false; // why
        private static bool hasFiredSkyLeapFirst = false; // now this is OUR shitcode
        private static bool hasFiredSkyLeapSecond = false;

        private void Awake()
        {
            KingConfiguration.PopulateConfig(Config);

            AssetBundle bundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Info.Location), "assetbundles", "artifactoftheking"));

            Mithrix = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherBody.prefab").WaitForCompletion();

            King = ScriptableObject.CreateInstance<ArtifactDef>();
            (King as ScriptableObject).name = "ArtifactOfKing_Blobface";
            King.nameToken = "ARTIFACT_OF_THE_KING_NAME";
            King.descriptionToken = "ARTIFACT_OF_THE_KING_DESCRIPTION";
            King.smallIconSelectedSprite = bundle.LoadAsset<Sprite>("Assets/ArtifactOfTheKing/headon.png");
            King.smallIconDeselectedSprite = bundle.LoadAsset<Sprite>("Assets/ArtifactOfTheKing/headoff.png");

            R2API.ContentAddition.AddArtifactDef(King);

            RoR2.Language.collectLanguageRootFolders += Language_collectLanguageRootFolders;

            On.RoR2.Run.Start += Run_Start;
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

        private void ExitSkyLeap_FixedUpdate(On.EntityStates.BrotherMonster.ExitSkyLeap.orig_FixedUpdate orig, EntityStates.BrotherMonster.ExitSkyLeap self)
        {
            if (RunArtifactManager.instance.IsArtifactEnabled(King) && self.isAuthority)
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
            if (RunArtifactManager.instance.IsArtifactEnabled(King) && self.isAuthority)
            {
                hasFiredSkyLeapFirst = false;
                hasFiredSkyLeapSecond = false;
            }
            orig(self);
        }

        private void FireLunarShards_OnEnter(On.EntityStates.BrotherMonster.Weapon.FireLunarShards.orig_OnEnter orig, EntityStates.BrotherMonster.Weapon.FireLunarShards self)
        {
            if (RunArtifactManager.instance.IsArtifactEnabled(King) && self.isAuthority && self is EntityStates.BrotherMonster.Weapon.FireLunarShardsHurt)
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
            orig(self);
        }

        private void WeaponSlam_FixedUpdate(On.EntityStates.BrotherMonster.WeaponSlam.orig_FixedUpdate orig, EntityStates.BrotherMonster.WeaponSlam self)
        {
            if (RunArtifactManager.instance.IsArtifactEnabled(King) && self.isAuthority)
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
            if (RunArtifactManager.instance.IsArtifactEnabled(King) && self.isAuthority)
            {
                hasFiredWeaponSlam = false;
            }
            orig(self);
        }

        private void SprintBash_OnEnter(On.EntityStates.BrotherMonster.SprintBash.orig_OnEnter orig, EntityStates.BrotherMonster.SprintBash self)
        {
            if (RunArtifactManager.instance.IsArtifactEnabled(King) && self.isAuthority)
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
            if (RunArtifactManager.instance.IsArtifactEnabled(King) && self.isAuthority)
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
            if (RunArtifactManager.instance.IsArtifactEnabled(King))
            {
                float num = damageInfo.damage / self.fullCombinedHealth * 100f * 50f * self.itemCounts.adaptiveArmor;
                self.adaptiveArmorValue = Mathf.Min(self.adaptiveArmorValue + num, 900f);
            }
        }

        private void HealthComponent_ServerFixedUpdate(On.RoR2.HealthComponent.orig_ServerFixedUpdate orig, HealthComponent self, float deltaTime)
        {
            orig(self, deltaTime);
            if (RunArtifactManager.instance.IsArtifactEnabled(King))
            {
                self.adaptiveArmorValue = Mathf.Max(0, self.adaptiveArmorValue - 100f * deltaTime);
            }
        }

        private void Run_Start(On.RoR2.Run.orig_Start orig, Run self)
        {
            if (RunArtifactManager.instance.IsArtifactEnabled(King))
            {
                Logger.LogMessage("Initializing modded stats");
                AdjustSkills();
                AdjustStats();
            }
            else
            {
                Logger.LogMessage("Reverting to vanilla stats");
                RevertSkills();
                RevertStats();
            }
            orig(self);
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