﻿using BepInEx;
using RoR2.Skills;
using RoR2;
using System;
using BepInEx.Configuration;

namespace WhatKilledMe
{
    [BepInPlugin(GUID, ModName, Version)]
    public class IDodgedThatPlugin : BaseUnityPlugin
    {
        public const string Author = "Viliger";
        public const string ModName = "IDodgedThat";
        public const string Version = "1.0.1";
        public const string GUID = "com." + Author + "." + ModName;

        public const string LanguageFolder = "Language";

        public static ConfigEntry<bool> UseSkillDefNames;

        private void Awake()
        {
            UseSkillDefNames = Config.Bind("Death Log", "Output SkillDef Names", true, "Outputs SkillDef name if nameToken is null or empty for that skillDef.");

            RoR2.GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            RoR2.Language.collectLanguageRootFolders += Language_collectLanguageRootFolders;
        }


        private void GlobalEventManager_onCharacterDeathGlobal(RoR2.DamageReport damageReport)
        {
            if(damageReport.victimTeamIndex != RoR2.TeamIndex.Player)
            {
                return;
            }

            if(!damageReport.victimBody || !damageReport.victimBody.isPlayerControlled)
            {
                return;
            }

            string killer = Language.GetString("WHAT_KILLED_ME_SOMETHING");
            string skill = "";
            if (damageReport.attackerBody)
            {
                killer = Util.GetBestBodyName(damageReport.attackerBody.gameObject);
                skill = string.Format(Language.GetString("WHAT_KILLED_ME_DAMAGE_SOURCE"), AttemptToGetSkill(damageReport));
            }

            string deathToken = "WHAT_KILLED_ME_NORMAL_DEATH";

            if (damageReport.isFriendlyFire) {
                deathToken = "WHAT_KILLED_ME_FRIENDLY_FIRE";
            } else if(IsDamageVoidFog(damageReport.damageInfo)) {
                deathToken = "WHAT_KILLED_ME_VOID_FOG";
                killer = "";
                skill = "";
            }
            else if (IsDamageVoidDeath(damageReport.damageInfo)) {
                deathToken = "WHAT_KILLED_ME_JAILED";
                skill = "";
            } else if(damageReport.isFallDamage)
            {
                deathToken = "WHAT_KILLED_ME_WEAK_ASS_KNEES";
                killer = "";
                skill = "";
            }

            string victim = Util.GetBestMasterName(damageReport.victimMaster);

            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = deathToken,
                paramTokens = new string[] { victim, killer, damageReport.damageDealt.ToString("F0"), damageReport.combinedHealthBeforeDamage.ToString("F0"), skill }
            });
         }

        private bool IsDamageVoidFog(DamageInfo damageInfo)
        {
            return damageInfo.damageColorIndex == DamageColorIndex.Void
                && (damageInfo.damageType.damageType & DamageType.BypassArmor) == DamageType.BypassArmor
                && (damageInfo.damageType.damageType & DamageType.BypassBlock) == DamageType.BypassBlock;
        }

        // what an amazing piece of code
        private string AttemptToGetSkill(DamageReport damageReport)
        {
            var attacker = damageReport.attackerBody;

            if (!attacker || attacker.skillLocator)
            {
                return "";
            }

            if (damageReport.damageInfo.damageType.damageSource.HasFlag(DamageSource.Primary))
            {
                return GetSkillString(attacker.skillLocator.primary);
            }
            else if (damageReport.damageInfo.damageType.damageSource.HasFlag(DamageSource.Secondary))
            {
                return GetSkillString(attacker.skillLocator.secondary);
            }
            else if (damageReport.damageInfo.damageType.damageSource.HasFlag(DamageSource.Utility))
            {
                return GetSkillString(attacker.skillLocator.utility);
            }
            else if (damageReport.damageInfo.damageType.damageSource.HasFlag(DamageSource.Special))
            {
                return GetSkillString(attacker.skillLocator.special);
            }

            return "";
        }

        private string GetSkillString(GenericSkill skill)
        {
            if (skill)
            {
                SkillDef skillDef;
                if (skill.currentSkillOverride >= 0)
                {
                    skillDef = skill.skillOverrides[skill.currentSkillOverride].skillDef;
                } else
                {
                    skillDef = skill.skillDef;
                }
                if (skillDef)
                {
                    if (!string.IsNullOrEmpty(skillDef.skillNameToken))
                    {
                        return Language.GetString(skillDef.skillNameToken);
                    } else if (UseSkillDefNames.Value)
                    {
                        return skillDef.skillName;
                    }
                }
            }

            return "";
        }

        private bool IsDamageVoidDeath(DamageInfo damageInfo)
        {
            return (damageInfo.damageType.damageType & DamageType.VoidDeath) == DamageType.VoidDeath;
        }

        private void Language_collectLanguageRootFolders(System.Collections.Generic.List<string> folders)
        {
            folders.Add(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(base.Info.Location), LanguageFolder));
        }
    }
}
