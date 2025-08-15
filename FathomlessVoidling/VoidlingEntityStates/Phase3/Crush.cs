using EntityStates;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace FathomlessVoidling.VoidlingEntityStates.Phase3
{
    public class Crush : BaseState
    {
        public static GameObject meteorPrefab;

        public static GameObject portalPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabSpawnEffect.prefab").WaitForCompletion();

        public static float duration = 6f;

        public static float missileSpawnFrequency = 6f;

        public static string muzzleString = "EyeProjectileCenter";

        public static float damageCoefficient = 2f;

        public static float force = 10000f;

        private float missileStopwatch;

        private float missileTimer;

        public override void OnEnter()
        {
            base.OnEnter();
            PlayAnimation("Body", "SuckExit", "Suck.playbackRate", 3.3f);
            missileTimer = 1f / missileSpawnFrequency;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority)
            {
                missileStopwatch += Time.fixedDeltaTime;
                if (missileStopwatch >= missileTimer)
                {
                    Transform transform = FindModelChild(muzzleString);
                    if (transform)
                    {
                        Ray aimRay = GetAimRay();
                        Ray ray = default;
                        ray.direction = aimRay.direction;
                        float num = 1000f;
                        float num2 = UnityEngine.Random.Range(-100f, 100f);
                        float num3 = UnityEngine.Random.Range(50f, 75f);
                        float num4 = UnityEngine.Random.Range(-100f, 100f);
                        Vector3 vector = new Vector3(num2, num3, num4);
                        Vector3 vector2 = transform.position + vector;
                        ray.origin = vector2;
                        RaycastHit raycastHit;
                        if (Physics.Raycast(aimRay, out raycastHit, num, LayerIndex.world.mask))
                        {
                            ray.direction = raycastHit.point - ray.origin;
                        }
                        FireBlobAuthority(ray);
                    }
                    missileStopwatch -= missileTimer;
                }
            }

            if (fixedAge > duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        private void FireBlobAuthority(Ray projectileRay)
        {
            EffectManager.SpawnEffect(portalPrefab, new EffectData
            {
                origin = projectileRay.origin,
                rotation = Util.QuaternionSafeLookRotation(projectileRay.direction)
            }, true);
            FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
            {
                position = projectileRay.origin,
                rotation = Util.QuaternionSafeLookRotation(projectileRay.direction),
                crit = Util.CheckRoll(critStat, characterBody.master),
                damage = damageStat * damageCoefficient,
                owner = gameObject,
                force = force,
                speedOverride = 100f,
                projectilePrefab = meteorPrefab
            };
            ProjectileManager.instance.FireProjectile(fireProjectileInfo);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
