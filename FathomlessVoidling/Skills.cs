using EntityStates;
using EntityStates.VoidRaidCrab;
using FathomlessVoidling.VoidlingEntityStates.Phase1;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace FathomlessVoidling
{
    public class Skills
    {
        public void CreatePrimary(int phase, GameObject gameObject)
        {
            SkillLocator component = gameObject.GetComponent<SkillLocator>();
            GenericSkill genericSkill = gameObject.GetComponents<GenericSkill>()[0];
            genericSkill.skillName = "Disillusion";
            SkillFamily skillFamily = ScriptableObject.CreateInstance<SkillFamily>();
            (skillFamily as ScriptableObject).name = "MiniVoidRaidCrabBodyBase" + "DisillusionFamily";
            skillFamily.variants = new SkillFamily.Variant[1];
            genericSkill._skillFamily = skillFamily;
            SkillDef skillDef = ScriptableObject.CreateInstance<SkillDef>();
            (skillDef as ScriptableObject).name = "FathomlessVoidlingSkillPrimary" + phase;
            skillDef.activationState = new SerializableEntityStateType(typeof(Disillusion));
            skillDef.skillNameToken = "Disillusion";
            skillDef.activationStateMachineName = "Weapon";
            skillDef.baseMaxStock = 1;
            skillDef.baseRechargeInterval = (float)ModConfig.primCD.Value;
            skillDef.beginSkillCooldownOnSkillEnd = true;
            skillDef.canceledFromSprinting = false;
            skillDef.cancelSprintingOnActivation = false;
            skillDef.fullRestockOnAssign = true;
            skillDef.interruptPriority = InterruptPriority.PrioritySkill;
            skillDef.isCombatSkill = true;
            skillDef.mustKeyPress = false;
            skillDef.rechargeStock = 1;
            skillDef.requiredStock = 1;
            skillDef.stockToConsume = 1;
            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = skillDef
            };
            ContentAddition.AddSkillDef(skillDef);
            ContentAddition.AddSkillFamily(skillFamily);
            component.primary = genericSkill;
        }

        public void CreateSecondary(int phase, GameObject gameObject)
        {
            SkillLocator component = gameObject.GetComponent<SkillLocator>();
            GenericSkill genericSkill = gameObject.GetComponents<GenericSkill>()[1];
            genericSkill.skillName = "Singularity";
            SkillFamily skillFamily = ScriptableObject.CreateInstance<SkillFamily>();
            (skillFamily as ScriptableObject).name = "MiniVoidRaidCrabBodyBase" + "SingularityFamily";
            skillFamily.variants = new SkillFamily.Variant[1];
            genericSkill._skillFamily = skillFamily;
            SkillDef skillDef = ScriptableObject.CreateInstance<SkillDef>();
            (skillDef as ScriptableObject).name = "FathomlessVoidlingSkillSecondary" + phase;
            skillDef.activationState = new SerializableEntityStateType(typeof(VacuumEnter));
            skillDef.skillNameToken = "Singularity";
            skillDef.activationStateMachineName = "Body";
            skillDef.baseMaxStock = 1;
            skillDef.baseRechargeInterval = (float)ModConfig.secCD.Value;
            skillDef.beginSkillCooldownOnSkillEnd = true;
            skillDef.canceledFromSprinting = true;
            skillDef.cancelSprintingOnActivation = true;
            skillDef.fullRestockOnAssign = true;
            skillDef.interruptPriority = InterruptPriority.Pain;
            skillDef.isCombatSkill = true;
            skillDef.mustKeyPress = false;
            skillDef.rechargeStock = 1;
            skillDef.requiredStock = 1;
            skillDef.stockToConsume = 1;
            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = skillDef
            };
            ContentAddition.AddSkillDef(skillDef);
            ContentAddition.AddSkillFamily(skillFamily);
            component.secondary = genericSkill;
        }

        public void CreateUtility(int phase, GameObject gameObject)
        {
            SkillLocator component = gameObject.GetComponent<SkillLocator>();
            GenericSkill genericSkill = ((phase == 0) ? gameObject.AddComponent<GenericSkill>() : gameObject.GetComponents<GenericSkill>()[2]);
            genericSkill.skillName = "Transpose";
            SkillFamily skillFamily = ScriptableObject.CreateInstance<SkillFamily>();
            (skillFamily as ScriptableObject).name = "MiniVoidRaidCrabBodyBase" + "TransposeFamily";
            skillFamily.variants = new SkillFamily.Variant[1];
            genericSkill._skillFamily = skillFamily;
            SkillDef skillDef = ScriptableObject.CreateInstance<SkillDef>();
            (skillDef as ScriptableObject).name = "FathomlessVoidlingSkillUtility" + phase;
            skillDef.activationState = new SerializableEntityStateType(typeof(Transpose));
            skillDef.skillNameToken = "Transpose";
            skillDef.activationStateMachineName = "Body";
            skillDef.baseMaxStock = 1;
            skillDef.baseRechargeInterval = (float)ModConfig.utilCD.Value;
            skillDef.beginSkillCooldownOnSkillEnd = true;
            skillDef.canceledFromSprinting = false;
            skillDef.cancelSprintingOnActivation = false;
            skillDef.fullRestockOnAssign = true;
            skillDef.interruptPriority = InterruptPriority.Skill;
            skillDef.isCombatSkill = true;
            skillDef.mustKeyPress = false;
            skillDef.rechargeStock = 1;
            skillDef.requiredStock = 1;
            skillDef.stockToConsume = 1;
            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = skillDef
            };
            ContentAddition.AddSkillDef(skillDef);
            ContentAddition.AddSkillFamily(skillFamily);
            component.utility = genericSkill;
        }

        public void CreateSpecial(int phase, GameObject gameObject)
        {
            SkillLocator component = gameObject.GetComponent<SkillLocator>();
            GenericSkill genericSkill = ((phase == 0) ? gameObject.GetComponents<GenericSkill>()[2] : gameObject.GetComponents<GenericSkill>()[3]);
            genericSkill.skillName = "OmegaBeam";
            SkillFamily skillFamily = ScriptableObject.CreateInstance<SkillFamily>();
            (skillFamily as ScriptableObject).name = "MiniVoidRaidCrabBodyBase" + "OmegaBeamFamily";
            skillFamily.variants = new SkillFamily.Variant[1];
            genericSkill._skillFamily = skillFamily;
            SkillDef skillDef = ScriptableObject.CreateInstance<SkillDef>();
            (skillDef as ScriptableObject).name = "FathomlessVoidlingSkillSpecial" + phase;
            skillDef.activationState = new SerializableEntityStateType(typeof(Transpose));
            skillDef.skillNameToken = "OmegaBeam";
            skillDef.activationStateMachineName = "Body";
            skillDef.baseMaxStock = 1;
            skillDef.baseRechargeInterval = (float)ModConfig.specCD.Value;
            skillDef.beginSkillCooldownOnSkillEnd = true;
            skillDef.canceledFromSprinting = false;
            skillDef.cancelSprintingOnActivation = false;
            skillDef.fullRestockOnAssign = true;
            skillDef.interruptPriority = InterruptPriority.PrioritySkill;
            skillDef.isCombatSkill = true;
            skillDef.mustKeyPress = false;
            skillDef.rechargeStock = 1;
            skillDef.requiredStock = 1;
            skillDef.stockToConsume = 1;
            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = skillDef
            };
            ContentAddition.AddSkillDef(skillDef);
            ContentAddition.AddSkillFamily(skillFamily);
            component.special = genericSkill;
        }
    }
}
