using EntityStates;
using EntityStates.VoidRaidCrab.Weapon;
using RoR2;
using UnityEngine;

namespace FathomlessVoidling.VoidlingEntityStates.Phase1
{
    public class ChargeRend : BaseMultiBeamState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            PlayAnimation(animationLayerName, animationStateName, animationPlaybackRateParam, duration);
            ChildLocator modelChildLocator = GetModelChildLocator();
            if (modelChildLocator && chargeEffectPrefab)
            {
                Transform transform = modelChildLocator.FindChild(muzzleName) ?? characterBody.coreTransform;
                if (transform)
                {
                    chargeEffectInstance = Object.Instantiate<GameObject>(chargeEffectPrefab, transform.position, transform.rotation);
                    chargeEffectInstance.transform.parent = transform;
                    ScaleParticleSystemDuration component = chargeEffectInstance.GetComponent<ScaleParticleSystemDuration>();
                    if (component)
                    {
                        component.newDuration = duration;
                    }
                }
            }
            if (!string.IsNullOrEmpty(enterSoundString))
            {
                if (isSoundScaledByAttackSpeed)
                {
                    Util.PlayAttackSpeedSound(enterSoundString, gameObject, attackSpeedStat);
                }
                else
                {
                    Util.PlaySound(enterSoundString, gameObject);
                }
            }
            warningLaserEnabled = true;
        }

        public override void OnExit()
        {
            warningLaserEnabled = false;
            Destroy(chargeEffectInstance);
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!isAuthority || (double)fixedAge < duration)
            {
                return;
            }
            outer.SetNextState(new Rend());
        }

        public override void Update()
        {
            base.Update();
            UpdateWarningLaser();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Vehicle;
        }

        private bool warningLaserEnabled
        {
            get
            {
                return warningLaserVfxInstance;
            }
            set
            {
                if (value == warningLaserEnabled)
                {
                    return;
                }
                if (!value)
                {
                    Destroy(warningLaserVfxInstance);
                    warningLaserVfxInstance = null;
                    warningLaserVfxInstanceRayAttackIndicator = null;
                    return;
                }
                if (!warningLaserVfxPrefab)
                {
                    return;
                }
                warningLaserVfxInstance = Object.Instantiate<GameObject>(warningLaserVfxPrefab);
                warningLaserVfxInstanceRayAttackIndicator = warningLaserVfxInstance.GetComponent<RayAttackIndicator>();
                UpdateWarningLaser();
            }
        }

        private void UpdateWarningLaser()
        {
            if (!warningLaserVfxInstanceRayAttackIndicator)
            {
                return;
            }
            warningLaserVfxInstanceRayAttackIndicator.attackRange = beamMaxDistance;
            Ray ray;
            Vector3 vector;
            CalcBeamPath(out ray, out vector);
            warningLaserVfxInstanceRayAttackIndicator.attackRay = ray;
        }

        [SerializeField]
        public float baseDuration = new ChargeMultiBeam().baseDuration;

        [SerializeField]
        public GameObject chargeEffectPrefab = new ChargeMultiBeam().chargeEffectPrefab;

        [SerializeField]
        public GameObject warningLaserVfxPrefab = new ChargeMultiBeam().warningLaserVfxPrefab;

        [SerializeField]
        public new string muzzleName = new ChargeMultiBeam().muzzleName;

        [SerializeField]
        public string enterSoundString = new ChargeMultiBeam().enterSoundString;

        [SerializeField]
        public bool isSoundScaledByAttackSpeed = new ChargeMultiBeam().isSoundScaledByAttackSpeed;

        [SerializeField]
        public string animationLayerName = new ChargeMultiBeam().animationLayerName;

        [SerializeField]
        public string animationStateName = new ChargeMultiBeam().animationStateName;

        [SerializeField]
        public string animationPlaybackRateParam = new ChargeMultiBeam().animationPlaybackRateParam;

        private float duration;

        private GameObject chargeEffectInstance;

        private GameObject warningLaserVfxInstance;

        private RayAttackIndicator warningLaserVfxInstanceRayAttackIndicator;
    }
}
