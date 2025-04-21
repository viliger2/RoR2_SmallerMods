using EntityStates.VoidRaidCrab;
using RoR2;
using RoR2.Audio;
using UnityEngine;

namespace FathomlessVoidling.VoidlingEntityStates
{
    public class ReapLaser : BaseSpinBeamAttackState
    {
        public override void OnEnter()
        {
            if (count > 3)
            {
                outer.SetNextStateToMain();
            }
            duration = baseDuration / attackSpeedStat;
            aimRay = GetAimRay();
            aimRay.direction = targetPosition - aimRay.origin;
            beamVfxInstance = Object.Instantiate<GameObject>(SpinBeamAttack.beamVfxPrefab, aimRay.origin, Quaternion.LookRotation(aimRay.direction));
            loopPtr = LoopSoundManager.PlaySoundLoopLocal(gameObject, SpinBeamAttack.loopSound);
            Util.PlaySound(SpinBeamAttack.enterSoundString, gameObject);
        }

        public override void FixedUpdate()
        {
            if (isAuthority)
            {
                if (beamTickTimer <= 0.0)
                {
                    beamTickTimer += 1f / SpinBeamAttack.beamTickFrequency;
                    FireBeamBulletAuthority();
                }
                beamTickTimer -= Time.fixedDeltaTime;
            }
        }

        public override void OnExit()
        {
            LoopSoundManager.StopSoundLoopLocal(loopPtr);
            VfxKillBehavior.KillVfxObject(beamVfxInstance);
            beamVfxInstance = null;
            outer.SetNextState(new Reap());
        }

        private void FireBeamBulletAuthority()
        {
            new BulletAttack
            {
                origin = aimRay.origin,
                aimVector = aimRay.direction,
                minSpread = 0f,
                maxSpread = 0f,
                maxDistance = 400f,
                hitMask = LayerIndex.CommonMasks.bullet,
                stopperMask = 0,
                bulletCount = 1U,
                radius = SpinBeamAttack.beamRadius,
                smartCollision = false,
                queryTriggerInteraction = QueryTriggerInteraction.Ignore,
                procCoefficient = 1f,
                procChainMask = default,
                owner = gameObject,
                weapon = gameObject,
                damage = SpinBeamAttack.beamDpsCoefficient * damageStat / SpinBeamAttack.beamTickFrequency,
                damageColorIndex = DamageColorIndex.Default,
                damageType = DamageType.Generic,
                falloffModel = BulletAttack.FalloffModel.None,
                force = 0f,
                hitEffectPrefab = SpinBeamAttack.beamImpactEffectPrefab,
                tracerEffectPrefab = null,
                isCrit = false,
                HitEffectNormal = false
            }.Fire();
        }

        public int count;

        public Vector3 targetPosition;

        private Ray aimRay;

        private new float baseDuration = 2f;

        private new float duration;

        private float beamTickTimer;

        private LoopSoundManager.SoundLoopPtr loopPtr;
    }
}
