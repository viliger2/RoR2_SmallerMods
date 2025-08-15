using EntityStates;
using EntityStates.NullifierMonster;
using RoR2;
using RoR2.Projectile;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace FathomlessVoidling.VoidlingEntityStates
{
    public abstract class BaseDisillusion : BaseState
    {
        internal abstract GameObject bombPrefab { get; }

        public static GameObject missilesMuzzleFlashPrefab = Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_DLC1_VoidRaidCrab.VoidRaidCrabMuzzleflashEyeMissiles_prefab).WaitForCompletion();

        public static string missilesMuzzleName = "EyeProjectileCenter";

        public static GameObject missilesProjectilePrefab = Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_DLC1_VoidRaidCrab.VoidRaidCrabMissileProjectile_prefab).WaitForCompletion();

        public static float missilesDamageCoefficient = 0.3f;

        public static float missilesForce = 100f;

        public static int missilesNumMissilesPerWave = 6;

        public static float missilesMinSpreadDegrees = 0f;

        public static float missilesRangeSpreadDegrees = 20f;

        public static float baseDuration = 4f;

        public static float fireFrequency = 5f;

        public static float bombFireTimer = 1f;

        private float duration;

        private float fireInterval;

        private float fireStopwatch;

        private Predictor predictor;

        private float bombStopwatch;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            fireInterval = duration / fireFrequency;

            if (isAuthority)
            {
                BullseyeSearch bullseyeSearch = new BullseyeSearch();
                bullseyeSearch.teamMaskFilter = TeamMask.allButNeutral;
                if (teamComponent)
                {
                    bullseyeSearch.teamMaskFilter.RemoveTeam(teamComponent.teamIndex);
                }
                bullseyeSearch.maxDistanceFilter = 1000f;
                bullseyeSearch.maxAngleFilter = 360f;
                Ray aimRay = GetAimRay();
                bullseyeSearch.searchOrigin = aimRay.origin;
                bullseyeSearch.searchDirection = aimRay.direction;
                bullseyeSearch.filterByLoS = false;
                bullseyeSearch.sortMode = BullseyeSearch.SortMode.DistanceAndAngle;
                bullseyeSearch.RefreshCandidates();
                HurtBox hurtBox = bullseyeSearch.GetResults().FirstOrDefault();
                if (!hurtBox)
                {
                    return;
                }
                predictor = new Predictor(transform);
                predictor.SetTargetTransform(hurtBox.transform);
                FireMissileAuthority();
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority)
            {
                predictor.Update();
                bombStopwatch += Time.fixedDeltaTime;
                if (bombStopwatch >= bombFireTimer)
                {
                    predictor.GetPredictedTargetPosition(1f, out var position);
                    ProjectileManager.instance.FireProjectile(new FireProjectileInfo
                    {
                        projectilePrefab = bombPrefab,
                        position = position,
                        rotation = Quaternion.identity,
                        owner = gameObject,
                        damage = 0f,
                        force = 0f,
                        crit = characterBody.RollCrit()
                    });
                    bombStopwatch -= bombFireTimer;
                }

                fireStopwatch += Time.fixedDeltaTime;
                if (fireStopwatch >= fireInterval)
                {
                    FireMissileAuthority();
                    fireStopwatch -= fireInterval;
                }
            }

            if (fixedAge > duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        private void FireMissileAuthority()
        {
            EffectManager.SimpleMuzzleFlash(missilesMuzzleFlashPrefab, gameObject, missilesMuzzleName, true);
            Quaternion quaternion = Util.QuaternionSafeLookRotation(GetAimRay().direction);
            FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
            {
                projectilePrefab = missilesProjectilePrefab,
                position = FindModelChild(missilesMuzzleName).position,
                owner = gameObject,
                damage = damageStat * missilesDamageCoefficient,
                force = missilesForce
            };
            for (int i = 0; i < missilesNumMissilesPerWave; i++)
            {
                fireProjectileInfo.rotation = quaternion * GetRandomRollPitch();
                fireProjectileInfo.crit = Util.CheckRoll(critStat, characterBody.master);
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
            EffectManager.SimpleMuzzleFlash(FirePortalBomb.muzzleflashEffectPrefab, gameObject, missilesMuzzleName, true);
        }

        protected Quaternion GetRandomRollPitch()
        {
            Quaternion quaternion = Quaternion.AngleAxis(UnityEngine.Random.Range(0, 360), Vector3.forward);
            Quaternion quaternion2 = Quaternion.AngleAxis(missilesMinSpreadDegrees + UnityEngine.Random.Range(0f, missilesRangeSpreadDegrees), Vector3.left);
            return quaternion * quaternion2;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
