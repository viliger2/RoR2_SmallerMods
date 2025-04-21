using EntityStates;
using EntityStates.Huntress;
using RoR2;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace FathomlessVoidling.VoidlingEntityStates
{
    public class Reap : BaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            counter++;
            predictiveLaserIndicator = Object.Instantiate<GameObject>(areaIndicatorPrefab);
            predictiveLaserIndicator.transform.localScale = new Vector3(ArrowRain.arrowRainRadius, ArrowRain.arrowRainRadius, ArrowRain.arrowRainRadius);
            duration = baseDuration / attackSpeedStat;
            stopwatch = 0f;
            BullseyeSearch bullseyeSearch = new BullseyeSearch();
            bullseyeSearch.teamMaskFilter = TeamMask.allButNeutral;
            if (teamComponent)
            {
                bullseyeSearch.teamMaskFilter.RemoveTeam(teamComponent.teamIndex);
            }
            bullseyeSearch.maxDistanceFilter = 500f;
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

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stopwatch += Time.fixedDeltaTime;
            predictor.Update();
            if (stopwatch <= 1.0)
            {
                predictor.GetPredictedTargetPosition(1f, out predictedTargetPosition);
                predictiveLaserIndicator.transform.position = predictedTargetPosition;
            }
            else
            {
                stopwatch = 0f;
                predictiveLaserIndicator.transform.position = new Vector3(0f, 0f, 0f);
                outer.SetNextState(new ReapLaser
                {
                    count = counter
                });
            }
            if ((double)fixedAge < duration)
            {
                return;
            }
            outer.SetNextStateToMain();
        }

        public static int portalBombCount;

        public static float baseDuration = 4f;

        private float duration;

        private float stopwatch;

        private Vector3 predictedTargetPosition;

        private GameObject predictiveLaserIndicator;

        private int counter;

        private Predictor predictor;

        private static GameObject areaIndicatorPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Huntress/HuntressArrowRainIndicator.prefab").WaitForCompletion();

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
