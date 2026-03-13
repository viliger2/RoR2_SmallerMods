using HG;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static AkMIDIEvent;

namespace BaddiesWithItems
{
    public class ItemAdder
    {
        internal static Dictionary<ItemTier, bool> EnabledItemTierDictionary = new Dictionary<ItemTier, bool>();

        private static Dictionary<ItemIndex, int> itemCaps = new Dictionary<ItemIndex, int>();

        private static HashSet<ItemIndex> blacklistedItems = new HashSet<ItemIndex>();

        private static HashSet<EquipmentIndex> blacklistedEquipment = new HashSet<EquipmentIndex>();

        [SystemInitializer(new Type[] { typeof(ItemCatalog), typeof(EquipmentCatalog), typeof(EliteCatalog) })]
        private static void Init()
        {
            RegenerateBlacklist();
        }

        public static void RegenerateBlacklist()
        {
            blacklistedEquipment.Clear();
            blacklistedItems.Clear();
            itemCaps.Clear();

            List<ItemTag> blacklistedTags = new List<ItemTag>();

            var stringTags = Configuration.ItemBlacklistTags.Value.Split(",");
            foreach (var stringTag in stringTags)
            {
                if (Enum.TryParse(typeof(ItemTag), stringTag, out object result))
                {
                    blacklistedTags.Add((ItemTag)result);
                }
                else
                {
                    Log.Warning($"ItemTag {stringTag} does not exist. Skipping it...");
                }
            }

            var itemBlacklist = new HashSet<string>(Configuration.ItemBlacklist.Value.Split(","));
            foreach (var itemDef in ItemCatalog.allItemDefs)
            {
                if (itemBlacklist.Contains(itemDef.name))
                {
                    blacklistedItems.Add(itemDef.itemIndex);
                    continue;
                }

                foreach (var itemTag in blacklistedTags)
                {
                    if (itemDef.ContainsTag(itemTag))
                    {
                        blacklistedItems.Add(itemDef.itemIndex);
                        break;
                    }
                }
            }

            foreach (var eliteDef in EliteCatalog.eliteDefs)
            {
                if (eliteDef.eliteEquipmentDef)
                {
                    blacklistedEquipment.Add(eliteDef.eliteEquipmentDef.equipmentIndex);
                }
            }

            var equipmentBlacklist = new HashSet<string>(Configuration.EquipmentBlacklist.Value.Split(","));
            foreach (EquipmentIndex item in EquipmentCatalog.allEquipment)
            {
                EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(item);
                if (equipmentDef && equipmentBlacklist.Contains(equipmentDef.name))
                {
                    blacklistedEquipment.Add(item);
                    continue;
                }
            }

            var caps = Configuration.ItemCaps.Value.Split(",");
            foreach(var cap in caps)
            {
                var values = cap.Split(":");
                if(values.Length != 2)
                {
                    Log.Warning($"Cannot parse {cap} for setting item caps - invalid structure.");
                    continue;
                }

                var itemIndex = ItemCatalog.FindItemIndex(values[0]);
                if(itemIndex == ItemIndex.None)
                {
                    continue;
                }

                if(Int32.TryParse(values[1], out var result))
                {
                    if(itemCaps.TryGetValue(itemIndex, out _))
                    {
                        Log.Warning($"Item Caps setting has {itemIndex} listed more than once, skipping all values but first...");
                        continue;
                    }
                    itemCaps.Add(itemIndex, result);
                } else
                {
                    Log.Warning($"Couldn't parse count {values[1]} for {itemIndex} in Item Caps setting.");
                }
            }
#if DEBUG
            Log.Info("Item Caps:");
            foreach(var item in itemCaps)
            {
                Log.Info($"{ItemCatalog.GetItemDef(item.Key).name}x{item.Value}");
            }
            Log.Info("==================================================");
            Log.Info("Item Blacklist:");
            foreach(var item in blacklistedItems)
            {
                Log.Info(ItemCatalog.GetItemDef(item).name);
            }
            Log.Info("==================================================");
            Log.Info("Equipment Blacklist:");
            foreach(var item in blacklistedEquipment)
            {
                Log.Info(EquipmentCatalog.GetEquipmentDef(item).name);
            }
#endif
        }

        public static void Hooks()
        {
            SpawnCard.onSpawnedServerGlobal += SpawnCard_onSpawnedServerGlobal;
        }

        private static void SpawnCard_onSpawnedServerGlobal(SpawnCard.SpawnResult result)
        {
            if (!Run.instance)
            {
                return;
            }

            if (!result.spawnedInstance)
            {
                return;
            }

            var characterMaster = result.spawnedInstance.GetComponent<CharacterMaster>();
            if(!characterMaster)
            {
                return;
            }

            if(Run.instance.stageClearCount < Configuration.MinStageCompletion.Value - 1)
            {
                return;
            }

            if((Configuration.OnlyAffectMonsters.Value && characterMaster.teamIndex == TeamIndex.Monster)
                || (!Configuration.OnlyAffectMonsters.Value && characterMaster.teamIndex >= TeamIndex.Monster))
            {
                if (Configuration.CopyPlayerInventory.Value)
                {
                    var playerInstance = PlayerCharacterMasterController.instances[UnityEngine.Random.RandomRangeInt(0, PlayerCharacterMasterController.instances.Count)];
                    if (playerInstance && playerInstance.master && playerInstance.master.inventory)
                    {
                        CopyFromPlayer(characterMaster.inventory, playerInstance.master.inventory);
                    }
                } else
                {
                    GenerateItems(characterMaster.inventory);
                }
            }
        }

        private static void CopyFromPlayer(Inventory targetInventory, Inventory sourceInventory)
        {
            List<ItemIndex> result;
            CollectionPool<ItemIndex, List<ItemIndex>>.DisposableRental disposableRental = CollectionPool<ItemIndex, List<ItemIndex>>.RentCollection(out result);
            try
            {
                sourceInventory.permanentItemStacks.GetNonZeroIndices(result);
                foreach (ItemIndex item in result)
                {
                    if (blacklistedItems.Contains(item))
                    {
                        continue;
                    }

                    var itemDef = ItemCatalog.GetItemDef(item);
                    if (!itemDef)
                    {
                        continue;
                    }

                    if(itemDef.deprecatedTier == ItemTier.NoTier)
                    {
                        continue;
                    }

                    var needToAdd = false;

                    if(!EnabledItemTierDictionary.TryGetValue(itemDef.deprecatedTier, out needToAdd))
                    {
                        // most likely custom tier, just using custom tier config value
                        needToAdd = Configuration.EnableCustomTiers.Value;
                    }

                    if (!needToAdd)
                    {
                        continue;
                    }

                    var itemCount = (int)(sourceInventory.permanentItemStacks.GetStackValue(item) * Configuration.ItemMultiplier.Value);
                    if(itemCaps.TryGetValue(item, out var result2))
                    {
                        itemCount = UnityEngine.Mathf.Min(result2, itemCount);
                    }
#if DEBUG
                    Log.Info($"CopyFromPlayer: Gave {ItemCatalog.GetItemDef(item).name}x{itemCount} to {targetInventory}");
#endif

                    targetInventory.GiveItemPermanent(item, itemCount);
                }
            }
            finally
            {
                disposableRental.Dispose();
            }

            if (Configuration.EnableEquipment.Value)
            {
                for (uint i = 0; i < sourceInventory.GetEquipmentSlotCount(); i++)
                {
                    for (uint k = 0; k < sourceInventory.GetEquipmentSetCount(i); k++)
                    {
                        var equipmentState = sourceInventory.GetEquipment(i, k);
                        if (!equipmentState.Equals(EquipmentState.empty))
                        {
                            if (blacklistedEquipment.Contains(equipmentState.equipmentIndex))
                            {
                                continue;
                            }

                            targetInventory.ResetItemPermanent(RoR2Content.Items.AutoCastEquipment);
                            targetInventory.GiveItemPermanent(RoR2Content.Items.AutoCastEquipment);

                            targetInventory.SetEquipment(equipmentState, i, k);
#if DEBUG
                            Log.Info($"CopyFromPlayer: Gave equipment {EquipmentCatalog.GetEquipmentDef(equipmentState.equipmentIndex).name} to {targetInventory}");
#endif
                        }
                    }
                }

                targetInventory.ResetItemPermanent(RoR2Content.Items.AutoCastEquipment);
                targetInventory.GiveItemPermanent(RoR2Content.Items.AutoCastEquipment);

                targetInventory.CopyEquipmentFrom(sourceInventory, false);
            }
        }

        private static void GenerateItems(Inventory inventory)
        {
            int stageClearCount = Run.instance.stageClearCount;

            int numberOfItems = 0;

            switch (Configuration.ItemGenerationCount.Value)
            {
                case Configuration.ItemGenerationType.Average:
                default:
                    int totalCount = 0;
                    foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances)
                    {
                        if(player && player.master && player.master.inventory)
                        {
                            totalCount += player.master.inventory.permanentItemStacks.GetTotalItemStacks();
                        }
                    }
                    numberOfItems = totalCount / PlayerCharacterMasterController.instances.Count;
                    break;
                case Configuration.ItemGenerationType.MaxOnePlayer:
                    foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances)
                    {
                        if (player && player.master && player.master.inventory)
                        {
                            numberOfItems = UnityEngine.Mathf.Max(numberOfItems, player.master.inventory.permanentItemStacks.GetTotalItemStacks());
                        }
                    }
                    break;
                case Configuration.ItemGenerationType.Total:
                    foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances)
                    {
                        if (player && player.master && player.master.inventory)
                        {
                            numberOfItems += player.master.inventory.permanentItemStacks.GetTotalItemStacks();
                        }
                    }
                    break;
            }

            numberOfItems = (int)UnityEngine.Mathf.Pow(stageClearCount, 2) + numberOfItems;

            List<(ItemIndex, int)> list = new List<(ItemIndex, int)>();

            int generatedItems = 0;
            GenerateItems(Configuration.EnableTier1.Value, Run.instance.availableTier1DropList, Configuration.Tier1GenChance.Value, Configuration.Tier1SSCPower.Value, Configuration.Tier1GenCap.Value);
            GenerateItems(Configuration.EnableTier2.Value, Run.instance.availableTier2DropList, Configuration.Tier2GenChance.Value, Configuration.Tier2SSCPower.Value, Configuration.Tier2GenCap.Value);
            GenerateItems(Configuration.EnableTier3.Value, Run.instance.availableTier3DropList, Configuration.Tier3GenChance.Value, Configuration.Tier3SSCPower.Value, Configuration.Tier3GenCap.Value);
            GenerateItems(Configuration.EnableTierLunar.Value, Run.instance.availableLunarItemDropList, Configuration.TierLunarGenChance.Value, Configuration.TierLunarSSCPower.Value, Configuration.TierLunarGenCap.Value);
            GenerateItems(Configuration.EnableTierBoss.Value, Run.instance.availableBossDropList, Configuration.TierBossGenChance.Value, Configuration.TierBossSSCPower.Value, Configuration.TierBossGenCap.Value);
            GenerateItems(Configuration.EnableTierVoid.Value, Run.instance.availableVoidTier1DropList, Configuration.TierVoidGenChance.Value, Configuration.TierVoidSSCPower.Value, Configuration.TierVoidGenCap.Value);
            GenerateItems(Configuration.EnableTierVoid.Value, Run.instance.availableVoidTier2DropList, Configuration.TierVoidGenChance.Value, Configuration.TierVoidSSCPower.Value, Configuration.TierVoidGenCap.Value);
            GenerateItems(Configuration.EnableTierVoid.Value, Run.instance.availableVoidTier3DropList, Configuration.TierVoidGenChance.Value, Configuration.TierVoidSSCPower.Value, Configuration.TierVoidGenCap.Value);
            GenerateItems(Configuration.EnableTierVoid.Value, Run.instance.availableVoidBossDropList, Configuration.TierVoidGenChance.Value, Configuration.TierVoidSSCPower.Value, Configuration.TierVoidGenCap.Value);
            GenerateItems(Configuration.EnableTierFood.Value, Run.instance.availableFoodTierDropList, Configuration.TierFoodGenChance.Value, Configuration.TierFoodSSCPower.Value, Configuration.TierFoodGenCap.Value);

            if (Configuration.EnableEquipment.Value)
            {
                if (Util.CheckRoll(Configuration.EquipmentGenChance.Value))
                {
                    int iteration = 0;
                    while(iteration < 10)
                    {
                        var pickupIndex = Run.instance.availableEquipmentDropList[Run.instance.spawnRng.RangeInt(0, Run.instance.availableEquipmentDropList.Count)];
                        var pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
                        if(pickupDef == null)
                        {
                            iteration++;
                            continue;
                        }

                        var equipmentIndex = pickupDef.equipmentIndex;
                        if(equipmentIndex == EquipmentIndex.None)
                        {
                            iteration++;
                            continue;
                        }

                        if (blacklistedEquipment.Contains(equipmentIndex))
                        {
                            iteration++;
                            continue;
                        }

                        inventory.ResetItemPermanent(RoR2Content.Items.AutoCastEquipment);
                        inventory.GiveItemPermanent(RoR2Content.Items.AutoCastEquipment);

                        inventory.SetEquipmentIndex(equipmentIndex, true);
#if DEBUG
                        Log.Info($"GenerateItems: Gave equipment {EquipmentCatalog.GetEquipmentDef(equipmentIndex).name} to {inventory}");
#endif
                        break;
                    }
                }
            }

            list.RemoveAll(item => blacklistedItems.Contains(item.Item1));

            foreach(var item in list)
            {
#if DEBUG
                Log.Info($"GenerateItems: Gave {ItemCatalog.GetItemDef(item.Item1).name}x{item.Item2} to {inventory}");
#endif
                inventory.GiveItemPermanent(item.Item1, item.Item2);
            }

            void GenerateItems(bool needToGenerate, List<PickupIndex> itemList, float genChance, float sscPower, float genCap)
            {
                if (needToGenerate)
                {
                    foreach (var pickupIndex in itemList)
                    {
                        if (generatedItems >= numberOfItems)
                        {
                            break;
                        };

                        var pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
                        if (pickupDef == null)
                        {
                            continue;
                        };

                        var itemIndex = pickupDef.itemIndex;
                        if (itemIndex == ItemIndex.None)
                        {
                            continue;
                        }

                        if (itemIndex == RoR2.RoR2Content.Items.AutoCastEquipment.itemIndex)
                        {
                            continue;
                        }
                        if (Util.CheckRoll(genChance))
                        {
                            var amountToGive = Run.instance.spawnRng.RangeInt(0, (int)(Math.Pow(stageClearCount, sscPower) * genCap) + 1);
                            if (generatedItems + amountToGive > numberOfItems)
                            {
                                amountToGive = numberOfItems - generatedItems;
                            }
                            generatedItems += amountToGive;

                            var actuallyGivenItems = (int)(amountToGive * Configuration.ItemMultiplier.Value);
                            if(itemCaps.TryGetValue(itemIndex, out var result))
                            {
                                actuallyGivenItems = UnityEngine.Mathf.Min(result, actuallyGivenItems);
                            }
                            list.Add((itemIndex, actuallyGivenItems));
                        }
                    }
                }
            }
        }
    }
}
