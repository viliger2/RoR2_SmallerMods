using EntityStates;
using EntityStates.VoidRaidCrab.Weapon;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace FathomlessVoidling.VoidlingEntityStates.Phase1
{
    public class ChargeRend : ChargeMultiBeam
    {
        public static float staticBaseDuration = 1f;

        public static GameObject staticChargeEffectPrefab = Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_DLC1_VoidRaidCrab.VoidRaidCrabTripleBeamChargeUp_prefab).WaitForCompletion();

        public static GameObject staticWarningLaserVfxPrefab = Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_DLC1_VoidRaidCrab.MultiBeamRayIndicator_prefab).WaitForCompletion();

        public static string staticMuzzleName = "EyeProjectileCenter";

        public static string staticEnterSoundString = "Play_voidRaid_snipe_chargeUp";

        public static bool staticIsSoundScaledByAttackSpeed = false;

        public static string staticAnimationLayerName = "Gesture";

        public static string staticAnimationStateName = "ChargeMultiBeam";

        public static string staticAnimationPlaybackRateParam = "MultiBeam.playbackRate";

        public override void OnEnter()
        {
            baseDuration = staticBaseDuration;
            chargeEffectPrefab = staticChargeEffectPrefab;
            warningLaserVfxPrefab = staticWarningLaserVfxPrefab;
            muzzleName = staticMuzzleName;
            enterSoundString = staticEnterSoundString;
            isSoundScaledByAttackSpeed = staticIsSoundScaledByAttackSpeed;
            animationLayerName = staticAnimationLayerName;
            animationStateName = staticAnimationStateName;
            animationPlaybackRateParam = staticAnimationPlaybackRateParam;
            base.OnEnter();
        }

        public override void FixedUpdate()
        {
            fixedAge += GetDeltaTime();
            if (isAuthority && fixedAge > duration)
            {
                outer.SetNextState(new Rend());
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
