using EntityStates.GameOver;
using RoR2;
using UnityEngine.Networking;

namespace FathomlessVoidling.VoidEnding
{
    public class VoidEndingSetSceneAndWaitForPlayers : BaseGameOverControllerState
    {
        private SceneDef desiredSceneDef;

        public override void OnEnter()
        {
            base.OnEnter();
            FadeToBlackManager.ForceFullBlack();
            FadeToBlackManager.fadeCount++;
            desiredSceneDef = SceneCatalog.GetSceneDefFromSceneName("voidoutro");
            if (NetworkServer.active)
            {
                Run.instance.AdvanceStage(desiredSceneDef);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (NetworkServer.active && NetworkUser.AllParticipatingNetworkUsersReady() && SceneCatalog.mostRecentSceneDef == desiredSceneDef)
            {
                outer.SetNextState(new VoidEndingPlayCutscene());
            }
        }

        public override void OnExit()
        {
            FadeToBlackManager.fadeCount--;
            base.OnExit();
        }
    }
}
