using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace ReliableAncientScepter
{
    [RequireComponent(typeof(CampDirector))]
    public class ScepterVoidCampSpawner : MonoBehaviour
    {
        public static float scepterGemSpawnChance => ReliableAncientScepterPlugin.VoidCampGemSpawnChance.Value;

        public InteractableSpawnCard iscScepterGem;

        private CampDirector campDirector;

        private Xoroshiro128Plus rng;

        private bool spawn;

        private void Awake()
        {
            if (NetworkServer.active)
            {
                if (!iscScepterGem)
                {
                    return;
                }

                campDirector = GetComponent<CampDirector>();
                rng = new Xoroshiro128Plus(Run.instance.stageRng.nextUint);

                var range = rng.RangeFloat(0, 100f);
                if (range > scepterGemSpawnChance)
                {
                    spawn = true;
                    campDirector.baseInteractableCredit -= iscScepterGem.directorCreditCost;
                }
            }
        }

        private void Start()
        {
            if (NetworkServer.active)
            {
                SpawnScepterGem();
            }
        }

        private void SpawnScepterGem()
        {
            if(!campDirector || !iscScepterGem)
            {
                return;
            }

            if (!spawn)
            {
                return;
            }

            DirectorPlacementRule placementRule = new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                minDistance = campDirector.campMinimumRadius,
                maxDistance = campDirector.campMaximumRadius,
                position = campDirector.campCenterTransform.position,
                spawnOnTarget = campDirector.campCenterTransform
            };

            GameObject gameObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(iscScepterGem, placementRule, rng));
            if (gameObject)
            {
                PurchaseInteraction component = gameObject.GetComponent<PurchaseInteraction>();
                if (component && component.costType == CostTypeIndex.Money)
                {
                    component.Networkcost = Run.instance.GetDifficultyScaledCost(component.cost);
                }
            }
        }
    }
}
