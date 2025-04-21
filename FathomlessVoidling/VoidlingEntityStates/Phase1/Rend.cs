using EntityStates;
using EntityStates.VoidRaidCrab.Weapon;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace FathomlessVoidling.VoidlingEntityStates.Phase1
{
    public class Rend : BaseState
    {
        private Transform muzzleTransform;

        public override void OnEnter()
        {
            base.OnEnter();
            missileStopwatch -= missileSpawnDelay;
            duration = baseDuration / attackSpeedStat;
            muzzleTransform = FindModelChild(muzzleName);
        }

        private void FireBlob()
        {
            PlayAnimation(animationLayerName, animationStateName, animationPlaybackRateParam, duration);
            Util.PlaySound(new FireMultiBeamSmall().enterSoundString, gameObject);
            Ray ray;
            Vector3 vector;
            CalcBeamPath(out ray, out vector);
            vector += new Vector3(Random.Range(-15f, 15f), Random.Range(-10f, 10f), Random.Range(-15f, 15f));
            new BlastAttack
            {
                attacker = gameObject,
                inflictor = gameObject,
                teamIndex = TeamComponent.GetObjectTeam(gameObject),
                baseDamage = damageStat * (blastDamageCoefficient / 2f),
                baseForce = blastForceMagnitude,
                position = vector,
                radius = blastRadius,
                falloffModel = BlastAttack.FalloffModel.SweetSpot,
                bonusForce = blastBonusForce,
                damageType = DamageType.Generic
            }.Fire();
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
            OnFireBeam(ray.origin, vector);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stopwatch += Time.fixedDeltaTime;
            missileStopwatch += Time.fixedDeltaTime;
            if (missileStopwatch >= 1.0 / missileSpawnFrequency)
            {
                missileStopwatch -= 1f / missileSpawnFrequency;
                FireBlob();
                if (stopwatch < (double)baseDuration || !isAuthority)
                {
                    return;
                }
                outer.SetNextStateToMain();
            }
        }

        public void OnFireBeam(Vector3 beamStart, Vector3 beamEnd)
        {
            ProjectileManager.instance.FireProjectile(new FireProjectileInfo
            {
                projectilePrefab = new FireMultiBeamFinale().projectilePrefab,
                position = beamEnd + Vector3.up * new FireMultiBeamFinale().projectileVerticalSpawnOffset,
                owner = gameObject,
                damage = damageStat * (new FireMultiBeamFinale().projectileDamageCoefficient / 6f),
                crit = Util.CheckRoll(critStat, characterBody.master)
            });
        }

        public void CalcBeamPath(out Ray beamRay, out Vector3 beamEndPos)
        {
            Ray aimRay = GetAimRay();
            float num = float.PositiveInfinity;
            RaycastHit[] array = Physics.RaycastAll(aimRay, BaseMultiBeamState.beamMaxDistance, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.Ignore);
            Transform root = GetModelTransform().root;
            for (int i = 0; i < array.Length; i++)
            {
                ref RaycastHit ptr = ref array[i];
                float distance = ptr.distance;
                if ((double)distance < (double)num && ptr.collider.transform.root != root)
                {
                    num = distance;
                }
            }
            float num2 = Mathf.Min(num, BaseMultiBeamState.beamMaxDistance);
            beamEndPos = aimRay.GetPoint(num2);
            Vector3 position = muzzleTransform.position;
            beamRay = new Ray(position, beamEndPos - position);
        }

        private float stopwatch;

        private float missileStopwatch;

        public float baseDuration = 4f;

        public static string muzzleString = BaseMultiBeamState.muzzleName;

        public static float missileSpawnFrequency = 6f;

        public static float missileSpawnDelay = 0f;

        public static float damageCoefficient;

        public static float maxSpread = 1f;

        public static float blastDamageCoefficient = 1f;

        public static float blastForceMagnitude = 3000f;

        public static float blastRadius = 6f;

        public static Vector3 blastBonusForce = new Vector3(0f, 100f, 0f);

        public static string muzzleName = "EyeProjectileCenter";

        public static string animationLayerName = "Gesture";

        public static string animationStateName = "FireMultiBeamFinale";

        public static string animationPlaybackRateParam = "MultiBeam.playbackRate";

        public static GameObject tracerEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/TracerVoidRaidCrabTripleBeamSmall.prefab").WaitForCompletion();

        public static GameObject explosionEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabTripleBeamExplosion.prefab").WaitForCompletion();

        public static GameObject muzzleEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabTripleBeamMuzzleflash.prefab").WaitForCompletion();

        private float duration;
    }
}
