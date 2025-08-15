using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace FathomlessVoidling.VoidlingEntityStates.Phase3
{
    public class ChargeCrush : BaseState
    {
        public static float baseDuration = 3.5f;

        public static GameObject missilesMuzzleFlashPrefab = Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_DLC1_VoidRaidCrab.VoidRaidCrabMuzzleflashEyeMissiles_prefab).WaitForCompletion();

        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            PlayAnimation("Body", "SuckEnter", "Suck.playbackRate", duration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge > duration && isAuthority)
            {
                outer.SetNextState(new Crush());
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            Util.PlaySound("Play_voidRaid_snipe_shoot", gameObject);
            EffectManager.SimpleMuzzleFlash(missilesMuzzleFlashPrefab, gameObject, "EyeProjectileCenter", false);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
