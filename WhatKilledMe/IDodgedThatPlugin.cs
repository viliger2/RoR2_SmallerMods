using BepInEx;
using RoR2;
using System;

namespace WhatKilledMe
{
    [BepInPlugin(GUID, ModName, Version)]
    public class IDodgedThatPlugin : BaseUnityPlugin
    {
        public const string Author = "Viliger";
        public const string ModName = "IDodgedThat";
        public const string Version = "1.0.0";
        public const string GUID = "com." + Author + "." + ModName;

        public const string LanguageFolder = "Language";

        private void Awake()
        {
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
            if (damageReport.attackerBody)
            {
                killer = Util.GetBestBodyName(damageReport.attackerBody.gameObject);
            }

            string deathToken = "WHAT_KILLED_ME_NORMAL_DEATH";

            if (damageReport.isFriendlyFire) {
                deathToken = "WHAT_KILLED_ME_FRIENDLY_FIRE";
            } else if(IsDamageVoidFog(damageReport.damageInfo)) {
                deathToken = "WHAT_KILLED_ME_VOID_FOG";
                killer = "";
            }else if (IsDamageVoidDeath(damageReport.damageInfo)) {
                deathToken = "WHAT_KILLED_ME_JAILED";
            } else if(damageReport.isFallDamage)
            {
                deathToken = "WHAT_KILLED_ME_WEAK_ASS_KNEES";
                killer = "";
            }

            string victim = Util.GetBestMasterName(damageReport.victimMaster);

            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = deathToken,
                paramTokens = new string[] { victim, killer, damageReport.damageDealt.ToString("F0"), damageReport.combinedHealthBeforeDamage.ToString("F0") }
            });
         }

        private bool IsDamageVoidFog(DamageInfo damageInfo)
        {
            return damageInfo.damageColorIndex == DamageColorIndex.Void
                && (damageInfo.damageType.damageType & DamageType.BypassArmor) == DamageType.BypassArmor
                && (damageInfo.damageType.damageType & DamageType.BypassBlock) == DamageType.BypassBlock;
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
