using EntityStates;
using EntityStates.GrandParentBoss;
using EntityStates.VoidRaidCrab.Weapon;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace FathomlessVoidling.VoidlingEntityStates.Phase3
{
    public class Crush : BaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            missileStopwatch -= missileSpawnDelay;
            Transform modelTransform = GetModelTransform();
            if (!modelTransform)
            {
                return;
            }
            childLocator = modelTransform.GetComponent<ChildLocator>();
            if (!childLocator)
            {
                return;
            }
            childLocator.FindChild(muzzleString);
        }

        private void FireBlob(Ray projectileRay, float bonusPitch, float bonusYaw)
        {
            EffectManager.SpawnEffect(FathomlessVoidling.portal, new EffectData
            {
                origin = projectileRay.origin,
                rotation = Util.QuaternionSafeLookRotation(projectileRay.direction)
            }, false);
            FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
            {
                position = projectileRay.origin,
                rotation = Util.QuaternionSafeLookRotation(projectileRay.direction),
                crit = Util.CheckRoll(critStat, characterBody.master),
                damage = damageStat * (new FireSecondaryProjectile().damageCoefficient * 2f),
                owner = gameObject,
                force = new FireSecondaryProjectile().force * 2f,
                speedOverride = 100f,
                projectilePrefab = FathomlessVoidling.meteor
            };
            ProjectileManager.instance.FireProjectile(fireProjectileInfo);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stopwatch += Time.fixedDeltaTime;
            missileStopwatch += Time.fixedDeltaTime;
            if (missileStopwatch >= 1.0 / missileSpawnFrequency)
            {
                missileStopwatch -= 1f / missileSpawnFrequency;
                Transform transform = childLocator.FindChild(muzzleString);
                if (transform)
                {
                    Ray aimRay = GetAimRay();
                    Ray ray = default;
                    ray.direction = aimRay.direction;
                    float num = 1000f;
                    float num2 = Random.Range(-100f, 100f);
                    float num3 = Random.Range(50f, 75f);
                    float num4 = Random.Range(-100f, 100f);
                    Vector3 vector = new Vector3(num2, num3, num4);
                    Vector3 vector2 = transform.position + vector;
                    ray.origin = vector2;
                    RaycastHit raycastHit;
                    if (Physics.Raycast(aimRay, out raycastHit, num, LayerIndex.world.mask))
                    {
                        ray.direction = raycastHit.point - ray.origin;
                    }
                    FireBlob(ray, 0f, 0f);
                }
                if (stopwatch < (double)baseDuration || !isAuthority)
                {
                    return;
                }
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        private float stopwatch;

        private float missileStopwatch;

        public static float baseDuration = 6f;

        public static string muzzleString = BaseMultiBeamState.muzzleName;

        public static float missileSpawnFrequency = 6f;

        public static float missileSpawnDelay = 0f;

        public static float damageCoefficient;

        public static float maxSpread = 1f;

        public static GameObject projectilePrefab;

        public static GameObject muzzleflashPrefab;

        private ChildLocator childLocator;
    }
}
