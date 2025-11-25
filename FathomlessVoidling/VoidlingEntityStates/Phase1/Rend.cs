using EntityStates;
using EntityStates.VoidRaidCrab.Weapon;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static UnityEngine.Random;

namespace FathomlessVoidling.VoidlingEntityStates.Phase1
{
    // TODO: add DamageSource
    public class Rend : BaseMultiBeamState
    {
        public static GameObject tracerEffectPrefab = Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC1_VoidRaidCrab.TracerVoidRaidCrabTripleBeamSmall_prefab).WaitForCompletion();

        public static GameObject explosionEffectPrefab = Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC1_VoidRaidCrab.VoidRaidCrabTripleBeamExplosion_prefab).WaitForCompletion();

        public static GameObject projectilePrefab = Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC1_VoidRaidCrab.VoidRaidCrabMultiBeamDotZone_prefab).WaitForCompletion();

        public static float projectileDamageCoefficient = 2f;

        public static float baseDuration = 4f;

        public static float missileSpawnFrequency = 6f;

        public static string animationLayerName = "Gesture";

        public static string animationStateName = "FireMultiBeamFinale";

        public static string animationPlaybackRateParam = "MultiBeam.playbackRate";

        public static float blastRadius = 6f;

        public static float blastDamageCoefficient = 1f;

        public static Vector3 blastBonusForce = new Vector3(0f, 100f, 0f);

        public static float blastForceMagnitude = 3000f;

        private float duration;

        private float missileStopwatch;

        private float missleSpawnTimer;

        private BlastAttack blobBlastAttack;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            missleSpawnTimer = 1f / missileSpawnFrequency;
            if (isAuthority)
            {
                blobBlastAttack = SetupBlastAttack();
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            missileStopwatch += Time.fixedDeltaTime;
            if (missileStopwatch >= missleSpawnTimer)
            {
                FireBlob();

                missileStopwatch -= missleSpawnTimer;
            }

            if (fixedAge > duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        private BlastAttack SetupBlastAttack()
        {
            return new BlastAttack()
            {
                attacker = gameObject,
                inflictor = gameObject,
                teamIndex = TeamComponent.GetObjectTeam(gameObject),
                baseDamage = damageStat * (blastDamageCoefficient / 2f),
                baseForce = blastForceMagnitude,
                radius = blastRadius,
                falloffModel = BlastAttack.FalloffModel.SweetSpot,
                bonusForce = blastBonusForce,
                damageType = DamageType.Generic
            };
        }

        private void FireBlob()
        {
            PlayAnimation(animationLayerName, animationStateName, animationPlaybackRateParam, duration);
            Util.PlaySound(new FireMultiBeamSmall().enterSoundString, gameObject);

            if (isAuthority)
            {
                CalcBeamPath(out var ray, out var vector);
                vector += new Vector3(Range(-15f, 15f), Range(-10f, 10f), Range(-15f, 15f));

                Transform modelTransform = GetModelTransform();
                if (modelTransform)
                {
                    ChildLocator component = modelTransform.GetComponent<ChildLocator>();
                    if (component)
                    {
                        int num = component.FindChildIndex(BaseMultiBeamState.muzzleName);
                        if (tracerEffectPrefab)
                        {
                            EffectData effectData = new EffectData
                            {
                                origin = vector,
                                start = ray.origin,
                                scale = blastRadius
                            };
                            effectData.SetChildLocatorTransformReference(gameObject, num);
                            EffectManager.SpawnEffect(tracerEffectPrefab, effectData, true);
                            EffectManager.SpawnEffect(explosionEffectPrefab, effectData, true);
                        }
                    }
                }

                blobBlastAttack.position = vector;
                blobBlastAttack.crit = RollCrit();
                blobBlastAttack.Fire();

                ProjectileManager.instance.FireProjectile(new FireProjectileInfo
                {
                    projectilePrefab = projectilePrefab,
                    position = vector,
                    owner = gameObject,
                    damage = damageStat * (projectileDamageCoefficient / 6f),
                    crit = RollCrit()
                });
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
