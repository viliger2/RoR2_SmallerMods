using EntityStates;
using EntityStates.NullifierMonster;
using EntityStates.VoidRaidCrab.Weapon;
using RoR2;
using RoR2.Projectile;
using System.Linq;
using UnityEngine;

namespace FathomlessVoidling.VoidlingEntityStates.Phase1
{
    public class Disillusion : BaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            string name = characterBody.name;
            if (!(name == "MiniVoidRaidCrabBodyPhase1(Clone)"))
            {
                if (!(name == "MiniVoidRaidCrabBodyPhase2(Clone)"))
                {
                    if (name == "MiniVoidRaidCrabBodyPhase3(Clone)")
                    {
                        bombPrefab = FathomlessVoidling.bombPrefab3;
                    }
                }
                else
                {
                    bombPrefab = FathomlessVoidling.bombPrefab2;
                }
            }
            else
            {
                bombPrefab = FathomlessVoidling.bombPrefab1;
            }
            fireInterval = duration / 5f;
            fireTimer = 0f;
            stopwatch = 0f;
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
        }

        private void FireBomb(Vector3 point)
        {
            ProjectileManager.instance.FireProjectile(new FireProjectileInfo
            {
                projectilePrefab = bombPrefab,
                position = point,
                rotation = Quaternion.identity,
                owner = gameObject,
                damage = 0f,
                force = 0f,
                crit = characterBody.RollCrit()
            });
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stopwatch += Time.fixedDeltaTime;
            predictor.Update();
            if (stopwatch <= 1.0)
            {
                predictor.GetPredictedTargetPosition(1f, out predictedTargetPosition);
            }
            else
            {
                stopwatch = 0f;
                FireBomb(predictedTargetPosition);
            }
            fireTimer -= Time.fixedDeltaTime;
            if (fireTimer <= 0.0)
            {
                fireTimer += fireInterval;
                EffectManager.SimpleMuzzleFlash(new FireMissiles().muzzleFlashPrefab, gameObject, new FireMissiles().muzzleName, false);
                Util.PlayAttackSpeedSound(new FireMissiles().fireWaveSoundString, gameObject, attackSpeedStat);
                Quaternion quaternion = Util.QuaternionSafeLookRotation(GetAimRay().direction);
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    projectilePrefab = new FireMissiles().projectilePrefab,
                    position = FindModelChild(new FireMissiles().muzzleName).position,
                    owner = gameObject,
                    damage = damageStat * new FireMissiles().damageCoefficient,
                    force = new FireMissiles().force
                };
                for (int i = 0; i < new FireMissiles().numMissilesPerWave; i++)
                {
                    fireProjectileInfo.rotation = quaternion * new FireMissiles().GetRandomRollPitch();
                    fireProjectileInfo.crit = Util.CheckRoll(critStat, characterBody.master);
                    ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                }
                EffectManager.SimpleMuzzleFlash(FirePortalBomb.muzzleflashEffectPrefab, gameObject, new FireMissiles().muzzleName, true);
            }
            if ((double)fixedAge < duration)
            {
                return;
            }
            outer.SetNextStateToMain();
        }

        public static int portalBombCount;

        public static float baseDuration = 4f;

        public static float damageCoefficient;

        public static float procCoefficient;

        public static float randomRadius;

        public static float force;

        public static float minimumDistanceBetweenBombs;

        private GameObject bombPrefab;

        private float duration;

        private float fireTimer;

        private float stopwatch;

        private float fireInterval;

        private Vector3 predictedTargetPosition;

        private Predictor predictor;

        private class Predictor
        {
            public Predictor(Transform bodyTransform)
            {
                this.bodyTransform = bodyTransform;
            }

            public bool hasTargetTransform
            {
                get
                {
                    return targetTransform;
                }
            }

            public bool isPredictionReady
            {
                get
                {
                    return collectedPositions > 2;
                }
            }

            private void PushTargetPosition(Vector3 newTargetPosition)
            {
                targetPosition2 = targetPosition1;
                targetPosition1 = targetPosition0;
                targetPosition0 = newTargetPosition;
                collectedPositions++;
            }

            public void SetTargetTransform(Transform newTargetTransform)
            {
                targetTransform = newTargetTransform;
                targetPosition2 = targetPosition1 = targetPosition0 = newTargetTransform.position;
                collectedPositions = 1;
            }

            public void Update()
            {
                if (!targetTransform)
                {
                    return;
                }
                PushTargetPosition(targetTransform.position);
            }

            public bool GetPredictedTargetPosition(float time, out Vector3 predictedPosition)
            {
                Vector3 vector = targetPosition1 - targetPosition2;
                Vector3 vector2 = targetPosition0 - targetPosition1;
                vector.y = 0f;
                vector2.y = 0f;
                ExtrapolationType extrapolationType = vector == Vector3.zero || vector2 == Vector3.zero ? ExtrapolationType.None : (double)Vector3.Dot(vector.normalized, vector2.normalized) <= 0.980000019073486 ? ExtrapolationType.Polar : ExtrapolationType.Linear;
                float num = 1f / Time.fixedDeltaTime;
                predictedPosition = targetPosition0;
                if (extrapolationType != ExtrapolationType.Linear)
                {
                    if (extrapolationType == ExtrapolationType.Polar)
                    {
                        Vector3 position = bodyTransform.position;
                        Vector3 vector3 = Util.Vector3XZToVector2XY(targetPosition2 - position);
                        Vector3 vector4 = Util.Vector3XZToVector2XY(targetPosition1 - position);
                        Vector3 vector5 = Util.Vector3XZToVector2XY(targetPosition0 - position);
                        float magnitude = vector3.magnitude;
                        float magnitude2 = vector4.magnitude;
                        float magnitude3 = vector5.magnitude;
                        float num2 = Vector2.SignedAngle(vector3, vector4) * num;
                        float num3 = Vector2.SignedAngle(vector4, vector5) * num;
                        float num4 = (float)(((double)magnitude2 - (double)magnitude) * (double)num);
                        double num5 = (double)((magnitude3 - magnitude2) * num);
                        float num6 = (float)(((double)num2 + (double)num3) * 0.5);
                        double num7 = num5;
                        float num8 = (float)(((double)num4 + num7) * 0.5);
                        float num9 = magnitude3 + num8 * time;
                        if ((double)num9 < 0.0)
                        {
                            num9 = 0f;
                        }
                        Vector2 vector6 = Util.RotateVector2(vector5, num6 * time) * (num9 * magnitude3);
                        predictedPosition = position;
                        predictedPosition.x += vector6.x;
                        predictedPosition.z += vector6.y;
                    }
                }
                else
                {
                    predictedPosition = targetPosition0 + vector2 * (time * num);
                }
                return true;
            }

            private Transform bodyTransform;

            private Transform targetTransform;

            private Vector3 targetPosition0;

            private Vector3 targetPosition1;

            private Vector3 targetPosition2;

            private int collectedPositions;

            private enum ExtrapolationType
            {
                None,
                Linear,
                Polar
            }
        }
    }
}
