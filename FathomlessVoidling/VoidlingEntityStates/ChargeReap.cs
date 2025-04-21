using EntityStates.VoidRaidCrab;
using RoR2;

namespace FathomlessVoidling.VoidlingEntityStates
{
    public class ChargeReap : BaseSpinBeamAttackState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            CreateBeamVFXInstance(SpinBeamWindUp.warningLaserPrefab);
            Util.PlaySound(SpinBeamWindUp.enterSoundString, gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if ((double)fixedAge >= (double)duration && isAuthority)
            {
                outer.SetNextState(new Reap());
            }
            SetHeadYawRevolutions(SpinBeamWindUp.revolutionsCurve.Evaluate(normalizedFixedAge));
        }
    }
}
