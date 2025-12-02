using RoR2;
using RoR2.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;

namespace ArtifactOfLimit
{
    public static class ArtifactOfLimitManager
    {
        public static ArtifactDef myArtifact;

        internal static ItemMask newAvailableItems;

        private static ItemMask oldAvailableItems;

        [SystemInitializer(new Type[] { typeof(ArtifactCatalog) })]
        private static void Init()
        {
            if (!ArtifactOfLimitPlugin.isLoaded)
            {
                return;
            }

            RunArtifactManager.onArtifactEnabledGlobal += RunArtifactManager_onArtifactEnabledGlobal;
            RunArtifactManager.onArtifactDisabledGlobal += RunArtifactManager_onArtifactDisabledGlobal;
        }

        private static void RunArtifactManager_onArtifactEnabledGlobal(RunArtifactManager runArtifactManager, ArtifactDef artifactDef)
        {
            if (NetworkServer.active && artifactDef == myArtifact)
            {
                Run.onRunStartGlobal += Run_onRunStartGlobal;
                Run.onRunDestroyGlobal += Run_onRunDestroyGlobal;
            }
        }

        private static void RunArtifactManager_onArtifactDisabledGlobal(RunArtifactManager runArtifactManager, ArtifactDef artifactDef)
        {
            if (artifactDef == myArtifact)
            {
                Run.onRunStartGlobal -= Run_onRunStartGlobal;
                Run.onRunDestroyGlobal -= Run_onRunDestroyGlobal;
            }
        }

        private static void Run_onRunDestroyGlobal(Run run)
        {
            if (!NetworkServer.active)
            {
                return;
            }

            if (oldAvailableItems != null)
            {
                run.availableItems = oldAvailableItems;
            }

            if (newAvailableItems != null)
            {
                ItemMask.Return(newAvailableItems);
            }
        }

        private static void Run_onRunStartGlobal(Run run)
        {
            if (!NetworkServer.active)
            {
                return;
            }

            oldAvailableItems = run.availableItems;
            newAvailableItems = null;

            if (ProperSaveCompat.enabled)
            {
                if (ProperSaveCompat.IsLoading())
                {
                    ProperSaveCompat.LoadItemMask(out newAvailableItems);
                }
                else
                {
                    FillArtifactItemMask(ref newAvailableItems, run);
                }
            }
            else
            {
                FillArtifactItemMask(ref newAvailableItems, run);
            }

            run.availableItems = newAvailableItems;
            run.BuildDropTable();
        }

        private static void FillArtifactItemMask(ref ItemMask newAvailableItems, Run run)
        {
            newAvailableItems = ItemMask.Rent();

            var whitelistOfItems = Config.Whitelist.Value.Split(",", StringSplitOptions.RemoveEmptyEntries);
            foreach ( var item in whitelistOfItems)
            {
                var itemIndex = ItemCatalog.FindItemIndex(item);
                if(itemIndex == ItemIndex.None)
                {
                    continue;
                }

                var pickupIndex = PickupCatalog.FindPickupIndex(itemIndex);
                if(pickupIndex == PickupIndex.none)
                {
                    continue;
                }
                if(run.availableTier1DropList.Contains(pickupIndex)
                    || run.availableTier2DropList.Contains(pickupIndex)
                    || run.availableTier3DropList.Contains(pickupIndex))
                {
                    newAvailableItems.Add(itemIndex);
                }
            }

            GenerateItemPool(
                ref newAvailableItems,
                run.runRNG,
                run.availableTier1DropList,
                Config.Tier1ItemCount.Value,
                Config.Tier1DamageTag.Value,
                Config.Tier1HealingTag.Value,
                Config.Tier1UtilityTag.Value,
                run.availableVoidTier1DropList
            );
            GenerateItemPool(
                ref newAvailableItems,
                run.runRNG,
                run.availableTier2DropList,
                Config.Tier2ItemCount.Value,
                Config.Tier2DamageTag.Value,
                Config.Tier2HealingTag.Value,
                Config.Tier2UtilityTag.Value,
                run.availableVoidTier2DropList
            );
            GenerateItemPool(
                ref newAvailableItems,
                run.runRNG,
                run.availableTier3DropList,
                Config.Tier3ItemCount.Value,
                Config.Tier3DamageTag.Value,
                Config.Tier3HealingTag.Value,
                Config.Tier3UtilityTag.Value,
                run.availableVoidTier3DropList
            );

            if (Config.AffectVoidItems.Value)
            {
                var voidItemCount = ItemCountInMask(newAvailableItems, run.availableVoidTier1DropList)
                    + ItemCountInMask(newAvailableItems, run.availableVoidTier2DropList)
                    + ItemCountInMask(newAvailableItems, run.availableVoidTier3DropList);
                if (voidItemCount == 0)
                {
                    for (int i = 0; i < Config.PityVoidItemCount.Value; i++)
                    {
                        var item = GetRandomVoidItem(run);
                        var repeatCheck = 0;
                        while (newAvailableItems.Contains(item) && repeatCheck < 5)
                        {
                            item = GetRandomVoidItem(run);
                        }
                        if (newAvailableItems.Contains(item))
                        {
                            continue;
                        }

                        newAvailableItems.Add(item);
                    }
                }
            }
            else
            {
                AddAllItemsToItemMask(ref newAvailableItems, run.availableVoidTier1DropList);
                AddAllItemsToItemMask(ref newAvailableItems, run.availableVoidTier2DropList);
                AddAllItemsToItemMask(ref newAvailableItems, run.availableVoidTier3DropList);
            }

            AddAllItemsToItemMask(ref newAvailableItems, run.availableLunarItemDropList);
            AddAllItemsToItemMask(ref newAvailableItems, run.availableBossDropList);
            AddAllItemsToItemMask(ref newAvailableItems, run.availableVoidBossDropList);
            AddAllItemsToItemMask(ref newAvailableItems, run.availableFoodTierDropList);
            AddAllItemsToItemMask(ref newAvailableItems, run.availablePowerShapeItemsDropList);

            if (Config.PrintItemList.Value) 
            {
                Log.Info("Artifact Of Limit Item List: ");
                for(int i = 0; i < newAvailableItems.array.Length; i++)
                {
                    if (newAvailableItems.array[i])
                    {
                        Log.Info(ItemCatalog.GetItemDef((ItemIndex)i));
                    }
                }
            }

            void AddAllItemsToItemMask(ref ItemMask itemMask, List<PickupIndex> itemList)
            {
                foreach (var item in itemList)
                {
                    var pickupDef = PickupCatalog.GetPickupDef(item);
                    if (pickupDef.itemIndex != ItemIndex.None)
                    {
                        itemMask.Add(pickupDef.itemIndex);
                    }
                }
            }

            int ItemCountInMask(ItemMask itemMask, List<PickupIndex> itemList)
            {
                int count = 0;
                foreach (var item in itemList)
                {
                    var pickupDef = PickupCatalog.GetPickupDef(item);
                    if (pickupDef.itemIndex != ItemIndex.None)
                    {
                        if (itemMask.Contains(pickupDef.itemIndex))
                        {
                            count++;
                        }
                    }
                }

                return count;
            }

            ItemIndex GetRandomVoidItem(RoR2.Run run)
            {
                PickupIndex pickupIndex;
                switch (run.runRNG.RangeInt(0, 3))
                {
                    case 0:
                    default:
                        pickupIndex = run.availableVoidTier1DropList[run.runRNG.RangeInt(0, run.availableVoidTier1DropList.Count)];
                        break;
                    case 1:
                        pickupIndex = run.availableVoidTier2DropList[run.runRNG.RangeInt(0, run.availableVoidTier2DropList.Count)];
                        break;
                    case 2:
                        pickupIndex = run.availableVoidTier2DropList[run.runRNG.RangeInt(0, run.availableVoidTier3DropList.Count)];
                        break;
                }

                return PickupCatalog.GetPickupDef(pickupIndex).itemIndex;
            }
        }

        private static void GenerateItemPool(ref ItemMask newAvailableItems, Xoroshiro128Plus runRng, List<PickupIndex> dropList, int totalItemCount, float damagePercent, float healingPercent, float utilityPercent, List<PickupIndex> voidItemPool)
        {
            List<ItemIndex> voidItemPoolItem = voidItemPool.ConvertAll(item => PickupCatalog.GetPickupDef(item).itemIndex);

            var totalPercent = damagePercent + healingPercent + utilityPercent;

            int damageItemCount = (int)(totalItemCount * (damagePercent / totalPercent));
            int healingItemCount = (int)(totalItemCount * (healingPercent / totalPercent));
            int utilityItemCount = (int)(totalItemCount * (utilityPercent / totalPercent));

            if (totalItemCount != damageItemCount + healingItemCount + utilityItemCount)
            {
                var missingItemCount = totalItemCount - (damageItemCount + healingItemCount + utilityItemCount);
                while (missingItemCount > 0)
                {
                    switch (runRng.RangeInt(0, 3))
                    {
                        case 0:
                        default:
                            damageItemCount++;
                            break;
                        case 1:
                            utilityItemCount++;
                            break;
                        case 2:
                            healingItemCount++;
                            break;
                    }
                    missingItemCount--;
                }
            }

            List<ItemIndex> damageItems = dropList.Where(item => SelectItemWithTag(item, ItemTag.Damage)).ToList().ConvertAll(item => PickupCatalog.GetPickupDef(item).itemIndex);
            List<ItemIndex> utilityItems = dropList.Where(item => SelectItemWithTag(item, ItemTag.Utility)).ToList().ConvertAll(item => PickupCatalog.GetPickupDef(item).itemIndex);
            List<ItemIndex> healingItems = dropList.Where(item => SelectItemWithTag(item, ItemTag.Healing)).ToList().ConvertAll(item => PickupCatalog.GetPickupDef(item).itemIndex);

            var addedDamageItemsCount = AddItemsToItemMask(ref newAvailableItems, ref damageItems, damageItemCount, voidItemPoolItem);
            var addedUtilityItemsCount = AddItemsToItemMask(ref newAvailableItems, ref utilityItems, utilityItemCount, voidItemPoolItem);
            var addedHealingItemsCount = AddItemsToItemMask(ref newAvailableItems, ref healingItems, healingItemCount, voidItemPoolItem);

            var remainingItems = new List<ItemIndex>(damageItems.Count + utilityItems.Count + healingItems.Count);
            remainingItems.AddRange(damageItems);
            remainingItems.AddRange(utilityItems);
            remainingItems.AddRange(healingItems);

            if (addedDamageItemsCount + addedUtilityItemsCount + addedHealingItemsCount != totalItemCount)
            {
                var stillNeedToAdd = totalItemCount - (addedDamageItemsCount + addedUtilityItemsCount + addedHealingItemsCount);
                var rerollCount = 0;
                while (stillNeedToAdd > 0 && rerollCount < 10)
                {
                    var pickUpIndex = remainingItems[runRng.RangeInt(0, remainingItems.Count)];
                    if (!newAvailableItems.Contains(pickUpIndex))
                    {
                        newAvailableItems.Add(pickUpIndex);
                        stillNeedToAdd--;
                    }
                    rerollCount++;
                }
            }

            int AddItemsToItemMask(ref ItemMask itemMask, ref List<ItemIndex> items, int itemCount, List<ItemIndex> voidItemPool)
            {
                var addedItemCount = 0;
                if (items.Count <= itemCount)
                {
                    foreach (var item in items)
                    {
                        itemMask.Add(item);
                        addedItemCount++;
                    }
                    items.Clear();
                }
                else
                {
                    for (int i = 0; i < itemCount; i++)
                    {
                        var item = items[runRng.RangeInt(0, items.Count)];
                        int repeatCheck = 0;
                        while (itemMask.Contains(item) && repeatCheck < 5)
                        {
                            item = items[runRng.RangeInt(0, items.Count)];
                            repeatCheck++;
                        }

                        if (itemMask.Contains(item))
                        {
                            continue;
                        }

                        itemMask.Add(item);
                        items.Remove(item);

                        addedItemCount++;
                        var itemDef = ItemCatalog.GetItemDef(item);


                        if (Config.AffectVoidItems.Value)
                        {
                            for (int k = 0; k < ContagiousItemManager.transformationInfos.Length; k++)
                            {
                                if (ContagiousItemManager.transformationInfos[k].originalItem != item)
                                {
                                    continue;
                                }

                                if (!voidItemPool.Contains(ContagiousItemManager.transformationInfos[k].transformedItem))
                                {
                                    continue;
                                }

                                itemMask.Add(ContagiousItemManager.transformationInfos[k].transformedItem);
                                break;
                            }
                        }
                    }
                }
                return addedItemCount;
            }

            bool SelectItemWithTag(PickupIndex item, ItemTag tag)
            {
                var pickupDef = PickupCatalog.GetPickupDef(item);
                if (pickupDef.itemIndex != ItemIndex.None)
                {
                    ItemDef itemDef = ItemCatalog.GetItemDef(pickupDef.itemIndex);
                    return Array.IndexOf(itemDef.tags, tag) != -1;
                }
                return false;
            }
        }
    }
}
