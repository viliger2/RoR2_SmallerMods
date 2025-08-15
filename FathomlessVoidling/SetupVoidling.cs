using R2API;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Projectile;
using RoR2.Skills;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace FathomlessVoidling
{
    public class SetupVoidling
    {
        protected class SkillParams
        {
            public SkillParams(string name, EntityStates.SerializableEntityStateType activationState)
            {
                this.name = name;
                this.activationState = activationState;
            }

            public string name;
            public string nameToken = "FATHOMLESS_VOIDLING_SKILL_NO_NAME";
            public string descriptionToken = "FATHOMLESS_VOIDLING_SKILL_NO_DESCRIPTION";
            public Sprite icon = null;
            public string activationStateMachine = "Body";
            public EntityStates.SerializableEntityStateType activationState;
            public EntityStates.InterruptPriority interruptPriority = EntityStates.InterruptPriority.Skill;
            public float baseRechargeInterval = 1f;
            public int baseMaxStock = 1;
            public int rechargeStock = 1;
            public int requiredStock = 1;
            public int stockToConsume = 1;
            public bool resetCooldownTimerOnUse = false;
            public bool fullRestockOnAssign = true;
            public bool dontAllowPAstMaxStocks = false;
            public bool beginSkillCooldownOnSkillEnd = false;
            public bool cancelSprintingOnActivation = true;
            public bool forceSprintDuringState = false;
            public bool canceledFromSprinting = false;
            public bool isCombatSkill = true;
            public bool mustKeyPress = false;
        }

        public static void SetupStuff()
        {
            ModifyBodies();
            ModifyMasters();
            SetupProjectiles();

            RegisterEntityStates();
        }

        private static void RegisterEntityStates()
        {
            ContentAddition.AddEntityState<VoidlingEntityStates.Transpose>(out _);

            ContentAddition.AddEntityState<VoidlingEntityStates.Phase1.ChargeRend>(out _);
            ContentAddition.AddEntityState<VoidlingEntityStates.Phase1.Disillusion>(out _);
            ContentAddition.AddEntityState<VoidlingEntityStates.Phase1.Rend>(out _);

            ContentAddition.AddEntityState<VoidlingEntityStates.Phase2.Disillusion>(out _);

            ContentAddition.AddEntityState<VoidlingEntityStates.Phase3.ChargeCrush>(out _);
            ContentAddition.AddEntityState<VoidlingEntityStates.Phase3.Crush>(out _);
            ContentAddition.AddEntityState<VoidlingEntityStates.Phase3.Disillusion>(out _);

            ContentAddition.AddEntityState<VoidEnding.VoidEndingPlayCutscene>(out _);
            ContentAddition.AddEntityState<VoidEnding.VoidEndingSetSceneAndWaitForPlayers>(out _);
            ContentAddition.AddEntityState<VoidEnding.VoidEndingStart>(out _);
        }

        public static void SetupProjectiles()
        {
            var meteor = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Grandparent/GrandparentBoulder.prefab").WaitForCompletion().InstantiateClone("VoidMeteor", true);
            var meteorGhost = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Grandparent/GrandparentBoulderGhost.prefab").WaitForCompletion().InstantiateClone("VoidMeteorGhost", false);

            ProjectileController component = meteor.GetComponent<ProjectileController>();
            component.cannotBeDeleted = true;
            meteor.transform.localScale = new Vector3(1.75f, 1.75f, 1.75f);
            meteorGhost.transform.localScale = new Vector3(2f, 2f, 2f);
            meteorGhost.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().materials = new Material[]
            {
                Addressables.LoadAssetAsync<Material>("RoR2/Base/Grandparent/matGrandparentBoulderProjectile.mat").WaitForCompletion(),
                Addressables.LoadAssetAsync<Material>("RoR2/DLC1/EliteVoid/matEliteVoidOverlay.mat").WaitForCompletion()
            };
            component.ghostPrefab = meteorGhost;

            VoidlingEntityStates.Phase3.Crush.meteorPrefab = meteor;
            ContentAddition.AddProjectile(VoidlingEntityStates.Phase3.Crush.meteorPrefab);

            var voidCrabMissile = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabMissileProjectile.prefab").WaitForCompletion();
            voidCrabMissile.GetComponent<ProjectileSteerTowardTarget>().rotationSpeed = 180f;
        }

        private static void ModifyBodies()
        {
            SkillDef transpose = CreateSkillDef(new SkillParams("FathomlessVoidlingTranspose", new EntityStates.SerializableEntityStateType(typeof(VoidlingEntityStates.Transpose)))
            {
                nameToken = "FATHOMLESS_VOIDLING_TRANSPOSE",
                activationStateMachine = "Body",
                baseMaxStock = 1,
                baseRechargeInterval = ModConfig.utilCD.Value,
                beginSkillCooldownOnSkillEnd = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.Skill,
                isCombatSkill = true,
                mustKeyPress = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1
            });
            SkillFamily transposeSkillFamily = CreateSkillFamily("FathomlessVoidlingTransposeFamily", transpose);
            ContentAddition.AddSkillDef(transpose);
            ContentAddition.AddSkillFamily(transposeSkillFamily);

            var phase1Body = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/MiniVoidRaidCrabBodyPhase1.prefab").WaitForCompletion();
            AdjustStats(phase1Body);
            AdjustP1Skills(phase1Body, transposeSkillFamily);

            var phase2Body = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/MiniVoidRaidCrabBodyPhase2.prefab").WaitForCompletion();
            AdjustStats(phase2Body);
            AdjustP2Skills(phase2Body, transposeSkillFamily);

            var phase3Body = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/MiniVoidRaidCrabBodyPhase3.prefab").WaitForCompletion();
            AdjustStats(phase3Body);
            AdjustP3Skills(phase3Body, transposeSkillFamily);

            void AdjustStats(GameObject gameObject)
            {
                CharacterBody component = gameObject.GetComponent<CharacterBody>();
                component.subtitleNameToken = "FATHOMLESS_VOIDLING_BODY_SUBTITLE";
                component.baseMaxHealth = ModConfig.baseHealth.Value;
                component.levelMaxHealth = ModConfig.levelHealth.Value;
                component.baseDamage = ModConfig.baseDamage.Value;
                component.levelDamage = ModConfig.levelDamage.Value;
                component.baseAttackSpeed = ModConfig.baseAtkSpd.Value;
                component.baseMoveSpeed = ModConfig.baseSpd.Value;
                component.baseAcceleration = ModConfig.acceleration.Value;
                component.baseArmor = ModConfig.baseArmor.Value;
            }
        }

        private static void AdjustP1Skills(GameObject bodyObject, SkillFamily transposeFamily)
        {
            var skillLocator = bodyObject.GetComponent<SkillLocator>();
            skillLocator.primary = null;
            skillLocator.secondary = null;
            skillLocator.utility = null;
            skillLocator.special = null;

            while (bodyObject.TryGetComponent<GenericSkill>(out var genericSkill))
            {
                UnityEngine.Object.DestroyImmediate(genericSkill);
            };

            SkillDef disillusionP1 = CreateSkillDef(new SkillParams("FathomlessVoidlingSkillPrimaryP1", new EntityStates.SerializableEntityStateType(typeof(VoidlingEntityStates.Phase1.Disillusion)))
            {
                nameToken = "FATHOMLESS_VOIDLING_DISILLUSION",
                activationStateMachine = "Weapon",
                baseMaxStock = 1,
                baseRechargeInterval = ModConfig.primCD.Value,
                beginSkillCooldownOnSkillEnd = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.Skill,
                isCombatSkill = true,
                mustKeyPress = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1
            });
            SkillFamily familyPrimary = CreateSkillFamily("FathomlessVoidlingSkillFamilyPrimaryP1", disillusionP1);
            var primary = bodyObject.AddComponent<GenericSkill>();
            primary._skillFamily = familyPrimary;
            skillLocator.primary = primary;

            ContentAddition.AddSkillDef(disillusionP1);
            ContentAddition.AddSkillFamily(familyPrimary);

            SkillDef vacuum = Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC1/VoidRaidCrab/RaidCrabVacuumAttack.asset").WaitForCompletion();
            vacuum.baseRechargeInterval = ModConfig.secCD.Value;
            SkillFamily familySecondary = CreateSkillFamily("FathomlessVoidlingSkillFamilySecondaryP1", vacuum);
            var secondary = bodyObject.AddComponent<GenericSkill>();
            secondary._skillFamily = familySecondary;
            skillLocator.secondary = secondary;

            ContentAddition.AddSkillFamily(familySecondary);

            var utility = bodyObject.AddComponent<GenericSkill>();
            utility._skillFamily = transposeFamily;
            skillLocator.utility = utility;

            SkillDef rend = CreateSkillDef(new SkillParams("FathomlessVoidlingSkillSpecialP1", new EntityStates.SerializableEntityStateType(typeof(VoidlingEntityStates.Phase1.ChargeRend)))
            {
                nameToken = "FATHOMLESS_VOIDLING_REND",
                activationStateMachine = "Body",
                baseMaxStock = 1,
                baseRechargeInterval = ModConfig.specCD.Value,
                beginSkillCooldownOnSkillEnd = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.Skill,
                isCombatSkill = true,
                mustKeyPress = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1
            });
            SkillFamily familySpecial = CreateSkillFamily("FathomlessVoidlingSkillFamilySpecialP1", rend);

            var special = bodyObject.AddComponent<GenericSkill>();
            special._skillFamily = familySpecial;
            skillLocator.special = special;

            ContentAddition.AddSkillDef(rend);
            ContentAddition.AddSkillFamily(familySpecial);
        }

        private static void AdjustP2Skills(GameObject bodyObject, SkillFamily transposeFamily)
        {
            var skillLocator = bodyObject.GetComponent<SkillLocator>();
            skillLocator.primary = null;
            skillLocator.secondary = null;
            skillLocator.utility = null;
            skillLocator.special = null;

            while (bodyObject.TryGetComponent<GenericSkill>(out var genericSkill))
            {
                UnityEngine.Object.DestroyImmediate(genericSkill);
            };

            SkillDef disillusionP2 = CreateSkillDef(new SkillParams("FathomlessVoidlingSkillPrimaryP2", new EntityStates.SerializableEntityStateType(typeof(VoidlingEntityStates.Phase2.Disillusion)))
            {
                nameToken = "FATHOMLESS_VOIDLING_DISILLUSION",
                activationStateMachine = "Weapon",
                baseMaxStock = 1,
                baseRechargeInterval = ModConfig.primCD.Value,
                beginSkillCooldownOnSkillEnd = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.Skill,
                isCombatSkill = true,
                mustKeyPress = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1
            });
            SkillFamily familyPrimary = CreateSkillFamily("FathomlessVoidlingSkillFamilyPrimaryP2", disillusionP2);
            var primary = bodyObject.AddComponent<GenericSkill>();
            primary._skillFamily = familyPrimary;
            skillLocator.primary = primary;

            ContentAddition.AddSkillDef(disillusionP2);
            ContentAddition.AddSkillFamily(familyPrimary);

            SkillDef vacuum = Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC1/VoidRaidCrab/RaidCrabVacuumAttack.asset").WaitForCompletion();
            vacuum.baseRechargeInterval = ModConfig.secCD.Value;
            SkillFamily familySecondary = CreateSkillFamily("FathomlessVoidlingSkillFamilySecondaryP2", vacuum);
            var secondary = bodyObject.AddComponent<GenericSkill>();
            secondary._skillFamily = familySecondary;
            skillLocator.secondary = secondary;

            ContentAddition.AddSkillFamily(familySecondary);

            var utility = bodyObject.AddComponent<GenericSkill>();
            utility._skillFamily = transposeFamily;
            skillLocator.utility = utility;

            SkillDef spin = Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC1/VoidRaidCrab/RaidCrabSpinBeam.asset").WaitForCompletion();
            spin.baseRechargeInterval = ModConfig.specCD.Value;
            SkillFamily familySpecial = CreateSkillFamily("FathomlessVoidlingSkillFamilySpecialP2", spin);

            var special = bodyObject.AddComponent<GenericSkill>();
            special._skillFamily = familySpecial;
            skillLocator.special = special;

            ContentAddition.AddSkillFamily(familySpecial);
        }

        private static void AdjustP3Skills(GameObject bodyObject, SkillFamily transposeFamily)
        {
            var skillLocator = bodyObject.GetComponent<SkillLocator>();
            skillLocator.primary = null;
            skillLocator.secondary = null;
            skillLocator.utility = null;
            skillLocator.special = null;

            while (bodyObject.TryGetComponent<GenericSkill>(out var genericSkill))
            {
                UnityEngine.Object.DestroyImmediate(genericSkill);
            };

            SkillDef disillusionP3 = CreateSkillDef(new SkillParams("FathomlessVoidlingSkillPrimaryP3", new EntityStates.SerializableEntityStateType(typeof(VoidlingEntityStates.Phase3.Disillusion)))
            {
                nameToken = "FATHOMLESS_VOIDLING_DISILLUSION",
                activationStateMachine = "Weapon",
                baseMaxStock = 1,
                baseRechargeInterval = ModConfig.primCD.Value,
                beginSkillCooldownOnSkillEnd = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.Skill,
                isCombatSkill = true,
                mustKeyPress = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1
            });
            SkillFamily familyPrimary = CreateSkillFamily("FathomlessVoidlingSkillFamilyPrimaryP3", disillusionP3);
            var primary = bodyObject.AddComponent<GenericSkill>();
            primary._skillFamily = familyPrimary;
            skillLocator.primary = primary;

            ContentAddition.AddSkillDef(disillusionP3);
            ContentAddition.AddSkillFamily(familyPrimary);

            SkillDef crush = CreateSkillDef(new SkillParams("FathomlessVoidlingSkillSecondaryP3", new EntityStates.SerializableEntityStateType(typeof(VoidlingEntityStates.Phase3.ChargeCrush)))
            {
                nameToken = "FATHOMLESS_VOIDLING_SINGULARITY",
                activationStateMachine = "Body",
                baseMaxStock = 1,
                baseRechargeInterval = ModConfig.secCD.Value,
                beginSkillCooldownOnSkillEnd = true,
                canceledFromSprinting = true,
                cancelSprintingOnActivation = true,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.Skill,
                isCombatSkill = true,
                mustKeyPress = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1
            });
            SkillFamily familySecondary = CreateSkillFamily("FathomlessVoidlingSkillFamilySecondaryP3", crush);
            var secondary = bodyObject.AddComponent<GenericSkill>();
            secondary._skillFamily = familySecondary;
            skillLocator.secondary = secondary;

            ContentAddition.AddSkillDef(crush);
            ContentAddition.AddSkillFamily(familySecondary);

            var utility = bodyObject.AddComponent<GenericSkill>();
            utility._skillFamily = transposeFamily;
            skillLocator.utility = utility;

            SkillDef spinbeam = Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC1/VoidRaidCrab/RaidCrabSpinBeam.asset").WaitForCompletion();
            spinbeam.baseRechargeInterval = ModConfig.specCD.Value;
            SkillFamily familySpecial = CreateSkillFamily("FathomlessVoidlingSkillFamilySpecialP3", spinbeam);

            var special = bodyObject.AddComponent<GenericSkill>();
            special._skillFamily = familySpecial;
            skillLocator.special = special;

            ContentAddition.AddSkillFamily(familySpecial);
        }

        private static void ModifyMasters()
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

        private static SkillFamily CreateSkillFamily(string name, params SkillDef[] skills)
        {
            var skillFamily = ScriptableObject.CreateInstance<SkillFamily>();
            (skillFamily as ScriptableObject).name = name;
            skillFamily.variants = Array.ConvertAll(skills, item => new SkillFamily.Variant
            {
                skillDef = item,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(item.skillNameToken, false, null)
            });
            skillFamily.defaultVariantIndex = 0;

            return skillFamily;
        }

        private static SkillDef CreateSkillDef(SkillParams skillParams)
        {
            var skill = ScriptableObject.CreateInstance<SkillDef>();
            (skill as ScriptableObject).name = skillParams.name;
            skill.skillName = skillParams.name;

            skill.skillNameToken = skillParams.nameToken;
            skill.skillDescriptionToken = skillParams.descriptionToken;
            skill.icon = skillParams.icon;

            skill.activationStateMachineName = skillParams.activationStateMachine;
            skill.activationState = skillParams.activationState;
            skill.interruptPriority = skillParams.interruptPriority;

            skill.baseRechargeInterval = skillParams.baseRechargeInterval;
            skill.baseMaxStock = skillParams.baseMaxStock;
            skill.rechargeStock = skillParams.rechargeStock;
            skill.requiredStock = skillParams.requiredStock;
            skill.stockToConsume = skillParams.stockToConsume;

            skill.resetCooldownTimerOnUse = skillParams.resetCooldownTimerOnUse;
            skill.fullRestockOnAssign = skillParams.fullRestockOnAssign;
            skill.dontAllowPastMaxStocks = skillParams.dontAllowPAstMaxStocks;
            skill.beginSkillCooldownOnSkillEnd = skillParams.beginSkillCooldownOnSkillEnd;

            skill.canceledFromSprinting = skillParams.canceledFromSprinting;
            skill.forceSprintDuringState = skillParams.forceSprintDuringState;
            skill.canceledFromSprinting = skillParams.canceledFromSprinting;

            skill.isCombatSkill = skillParams.isCombatSkill;
            skill.mustKeyPress = skillParams.mustKeyPress;

            return skill;
        }
    }
}
