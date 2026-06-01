using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace BasicChest
{
    public class CustomExplicitDropTable : PickupDropTable
    {
        public string[] entries = Array.Empty<string>();

        public float tier1Weight = 0.8f;

        public float tier2Weight = 0.2f;

        public float tier3Weight = 0.01f;

        public float everythingElseWeight = 0.01f;

        private readonly WeightedSelection<UniquePickup> weightedSelection = new WeightedSelection<UniquePickup>();

        public override void Regenerate(Run run)
        {
            GenerateWeightedSelection(run);
        }

        private void GenerateWeightedSelection(Run run)
        {
            weightedSelection.Clear();
            for (int i = 0; i < entries.Length; i++)
            {
                var itemIndex = ItemCatalog.FindItemIndex(entries[i]);
                if (itemIndex != ItemIndex.None)
                {
                    var itemDef = ItemCatalog.GetItemDef(itemIndex);

                    var pickupIndex = PickupCatalog.FindPickupIndex(itemIndex);
                    if (pickupIndex != PickupIndex.none && run.IsPickupAvailable(pickupIndex))
                    {
                        weightedSelection.AddChoice(new UniquePickup(pickupIndex), GetTierWeight(itemDef.tier));
                    }
                }
            }
        }

        private float GetTierWeight(ItemTier tier)
        {
            switch (tier) 
            {
                case ItemTier.Tier1:
                    return tier1Weight;
                case ItemTier.Tier2:
                    return tier2Weight;
                case ItemTier.Tier3:
                    return tier3Weight;
                default:
                    return everythingElseWeight;
            }
        }

        public override UniquePickup GeneratePickupPreReplacement(Xoroshiro128Plus rng)
        {
            return PickupDropTable.GeneratePickupFromWeightedSelection(rng, weightedSelection);
        }

        public override void GenerateDistinctPickupsPreReplacement(List<UniquePickup> dest, int desiredCount, Xoroshiro128Plus rng)
        {
            PickupDropTable.GenerateDistinctFromWeightedSelection(dest, desiredCount, rng, weightedSelection);
        }

        public override int GetPickupCount()
        {
            return weightedSelection.Count;
        }
    }
}
