using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CertainHallowedConcepts
{
    public class GuaranteedWakeController : MonoBehaviour
    {
        public float chancePerStage => CertainHallowedConcepts.GuaranteedWakePercentPerStage.Value;

        private bool spawnedWake = false;

        private Xoroshiro128Plus rng;

        private void Start()
        {
            if (NetworkServer.active)
            {
                rng = new Xoroshiro128Plus(Run.instance.runRNG.nextUlong);
            }
        }

        private void OnEnable()
        {
            RoR2.SceneDirector.onPostPopulateSceneServer += SceneDirector_onPostPopulateSceneServer;
            RoR2.Run.onLoopBeginServer += Run_onLoopBeginServer;
        }

        private void Run_onLoopBeginServer(Run.LoopBeginArgs obj)
        {
            spawnedWake = false;
#if DEBUG
            Log.Info("Resetting Wake spawn flag on loop.");
#endif
        }

        private void OnDisable()
        {
            RoR2.SceneDirector.onPostPopulateSceneServer -= SceneDirector_onPostPopulateSceneServer;
            RoR2.Run.onLoopBeginServer -= Run_onLoopBeginServer;
        }

        private void SceneDirector_onPostPopulateSceneServer(RoR2.SceneDirector obj)
        {
            if (!RoR2.Run.instance)
            {
                return;
            }

            if (!Run.instance.IsExpansionEnabled(DLC4Content.DLC4))
            {
                return;
            }

            if (!DLC4ProgressionController.instance)
            {
                return;
            }

            if (WakeController.instance)
            {
#if DEBUG
                Log.Info("Not spawning wake since it was spawned by the game.");
#endif
                spawnedWake = true;
            }

            if (spawnedWake)
            {
#if DEBUG
                Log.Info("Not spawning wake since it was already spawned this loop.");
#endif
                return;
            }

            var sceneDef = SceneCatalog.GetSceneDefForCurrentScene();
            var stageOrder = sceneDef.stageOrder;
            if(stageOrder >= 1 && stageOrder <= 4)
            {
                if(rng.nextNormalizedFloat < (stageOrder * chancePerStage) / 100f)
                {
                    DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(DLC4ProgressionController.instance.wakeSpawnCard, new DirectorPlacementRule
                    {
                        placementMode = DirectorPlacementRule.PlacementMode.Random
                    }, rng);
                    spawnedWake = DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
                    if (spawnedWake)
                    {
                        DLC4ProgressionController.instance.hasWakeSpawned = spawnedWake;
                        DLC4ProgressionController.instance.lastSpawnedWakeLoopIndex = Run.instance.loopClearCount;
#if DEBUG
                        Log.Info("Wake has been spawned.");
#endif
                    }
                }
            }
        }
    }
}
