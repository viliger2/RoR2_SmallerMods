using EntityStates;
using EntityStates.VoidRaidCrab.Weapon;
using RoR2;
using UnityEngine;

namespace FathomlessVoidling.VoidlingEntityStates.Phase3
{
    public class ChargeCrush : BaseState
    {
        private float duration;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            PlayAnimation("Body", "SuckEnter", "Suck.playbackRate", 3.3f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!isAuthority || (double)fixedAge < (double)duration)
            {
                return;
            }
            Util.PlaySound(new FireMultiBeamSmall().enterSoundString, gameObject);
            EffectManager.SimpleMuzzleFlash(new FireMissiles().muzzleFlashPrefab, gameObject, BaseMultiBeamState.muzzleName, false);
            PlayAnimation("Body", "SuckExit", "Suck.playbackRate", 3.3f);
            outer.SetNextState(new Crush());
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        [SerializeField]
        public float baseDuration = 3.3f;
    }
}
