using EntityStates.GameOver;
using UnityEngine.Networking;

namespace FathomlessVoidling.VoidEnding
{
    public class VoidEndingStart : BaseGameOverControllerState
    {
        public static float duration = 3f;

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (NetworkServer.active && base.fixedAge >= duration)
            {
                outer.SetNextState(new VoidEndingSetSceneAndWaitForPlayers());
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}
