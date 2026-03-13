using HG;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace BaddiesWithItems
{
    public static class ItemDropper
    {
        internal static Dictionary<ItemTier, float> ItemTierDropWeightDictionary = new Dictionary<ItemTier, float>();

        public static void Hooks()
        {
            if (Configuration.EnableItemDrop.Value)
            {
                GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            }
        }

        private static void GlobalEventManager_onCharacterDeathGlobal(DamageReport damageReport)
        {
            if (!damageReport.victim || !damageReport.victimBody || !damageReport.victimMaster || !damageReport.victimMaster.inventory)
            {
                return;
            }

            var victimMaster = damageReport.victimMaster;
            if(!((Configuration.OnlyAffectMonsters.Value && victimMaster.teamIndex == TeamIndex.Monster)
                || (!Configuration.OnlyAffectMonsters.Value && victimMaster.teamIndex >= TeamIndex.Monster)))
            {
                return;
            }

            if (!Util.CheckRoll(Configuration.DropChance.Value))
            {
                return;
            }

            WeightedSelection<ItemIndex> weightedSelection = new WeightedSelection<ItemIndex>();

            List<ItemIndex> result;
            CollectionPool<ItemIndex, List<ItemIndex>>.DisposableRental disposableRental = CollectionPool<ItemIndex, List<ItemIndex>>.RentCollection(out result);
            try
            {
                victimMaster.inventory.permanentItemStacks.GetNonZeroIndices(result);
                foreach (ItemIndex item in result)
                {
                    var itemDef = ItemCatalog.GetItemDef(item);
                    if (!itemDef)
                    {
                        continue;
                    }

                    if(itemDef.deprecatedTier == ItemTier.NoTier)
                    {
                        continue;
                    }

                    bool tierAvailable = false;
                    if(!ItemAdder.EnabledItemTierDictionary.TryGetValue(itemDef.deprecatedTier, out tierAvailable))
                    {
                        tierAvailable = Configuration.EnableCustomTiers.Value;
                    }

                    if (!tierAvailable)
                    {
                        return;
                    }

                    float dropChance = 0.01f;
                    if(!ItemTierDropWeightDictionary.TryGetValue(itemDef.deprecatedTier, out dropChance))
                    {
                        // if couldn't get value from 
                        dropChance = Configuration.TierCustomDropWeight.Value;
                    }

                    weightedSelection.AddChoice(item, dropChance);
                }
            }
            finally
            {
                disposableRental.Dispose();
            }

            if(weightedSelection.Count == 0)
            {
                return;
            }

            var vector = damageReport.victim.gameObject.transform.position;
            var quaternion = damageReport.victim.gameObject.transform.rotation;
            InputBankTest inputBankTest = damageReport.victimBody.inputBank;

            Ray ray = (inputBankTest ? inputBankTest.GetAimRay() : new Ray(vector, quaternion * Vector3.forward));

            var itemToDrop = weightedSelection.Evaluate(Run.instance.treasureRng.nextNormalizedFloat);
            PickupDropletController.CreatePickupDroplet(new UniquePickup(PickupCatalog.FindPickupIndex(itemToDrop)), vector + Vector3.up * 1.5f, Vector3.up * 20f + ray.direction * 2f, false);
        }
    }
}
