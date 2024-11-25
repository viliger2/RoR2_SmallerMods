using BepInEx;
using HarmonyLib;
using RoR2;
using RoR2.EntitlementManagement;
using System;

namespace ContentUnlocker
{
    [BepInPlugin("com.RandyPitchford.ContentUnlocker", "ContentUnlocker", "1.0")]
    public class ContentUnlocker : BaseUnityPlugin
    {
        private void Awake()
        {
            var harmony = new Harmony("com.RandyPitchford.ContentUnlocker");
            harmony.PatchAll();
            Logger.LogMessage("It's like seven, seven and a half...");
        }

        [HarmonyPatch(typeof(RoR2.EntitlementManagement.BaseUserEntitlementTracker<RoR2.LocalUser>), nameof(RoR2.EntitlementManagement.BaseUserEntitlementTracker<RoR2.LocalUser>.UserHasEntitlement))]
        class UserHasEntitlementPatch
        {
            static bool Prefix(ref bool __result)
            {
                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(RoR2.EntitlementManagement.BaseUserEntitlementTracker<RoR2.LocalUser>), nameof(RoR2.EntitlementManagement.BaseUserEntitlementTracker<RoR2.LocalUser>.AnyUserHasEntitlement))]
        class AnyUserHasEntitlementPatch
        {
            static bool Prefix(ref bool __result)
            {
                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(RoR2.EntitlementManagement.EgsEntitlementResolver), nameof(RoR2.EntitlementManagement.EgsEntitlementResolver.CheckLocalUserHasEntitlement))]
        class EgsEntitlementResolverPatch
        {
            static bool Prefix(ref bool __result)
            {
                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(RoR2.EntitlementManagement.EntitlementAbstractions), nameof(RoR2.EntitlementManagement.EntitlementAbstractions.VerifyLocalSteamUser))]
        class VerifyLocalSteamUserPatch
        {
            static bool Prefix(ref bool __result)
            {
                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(RoR2.EntitlementManagement.EntitlementAbstractions), nameof(RoR2.EntitlementManagement.EntitlementAbstractions.VerifyRemoteUser))]
        class VerifyRemoteUserPatch
        {
            static bool Prefix(ref bool __result)
            {
                __result = true;
                return false;
            }
        }
    }
}
