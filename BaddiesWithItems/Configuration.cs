using BepInEx.Configuration;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace BaddiesWithItems
{
    public static class Configuration
    {
        public enum ItemGenerationType
        {
            Average,
            MaxOnePlayer,
            Total
        }

        public static ConfigEntry<string> ItemBlacklist;
        public static ConfigEntry<string> ItemBlacklistTags;
        public static ConfigEntry<string> EquipmentBlacklist;

        public static ConfigEntry<string> ItemCaps;

        public static ConfigEntry<int> MinStageCompletion;
        public static ConfigEntry<bool> OnlyAffectMonsters;
        public static ConfigEntry<bool> CopyPlayerInventory;
        public static ConfigEntry<bool> ClearInventoryOnSpawn;
        public static ConfigEntry<float> ItemMultiplier;

        public static ConfigEntry<ItemGenerationType> ItemGenerationCount;

        public static ConfigEntry<bool> EnableTier1;
        public static ConfigEntry<bool> EnableTier2;
        public static ConfigEntry<bool> EnableTier3;
        public static ConfigEntry<bool> EnableTierBoss;
        public static ConfigEntry<bool> EnableTierLunar;
        public static ConfigEntry<bool> EnableTierVoid;
        public static ConfigEntry<bool> EnableTierFood;
        public static ConfigEntry<bool> EnableEquipment;
        public static ConfigEntry<bool> EnableCustomTiers;

        public static ConfigEntry<float> Tier1GenChance;
        public static ConfigEntry<float> Tier2GenChance;
        public static ConfigEntry<float> Tier3GenChance;
        public static ConfigEntry<float> TierBossGenChance;
        public static ConfigEntry<float> TierLunarGenChance;
        public static ConfigEntry<float> TierVoidGenChance;
        public static ConfigEntry<float> TierFoodGenChance;
        public static ConfigEntry<float> EquipmentGenChance;

        public static ConfigEntry<float> Tier1GenCap;
        public static ConfigEntry<float> Tier2GenCap;
        public static ConfigEntry<float> Tier3GenCap;
        public static ConfigEntry<float> TierBossGenCap;
        public static ConfigEntry<float> TierLunarGenCap;
        public static ConfigEntry<float> TierVoidGenCap;
        public static ConfigEntry<float> TierFoodGenCap;

        public static ConfigEntry<float> Tier1SSCPower;
        public static ConfigEntry<float> Tier2SSCPower;
        public static ConfigEntry<float> Tier3SSCPower;
        public static ConfigEntry<float> TierBossSSCPower;
        public static ConfigEntry<float> TierLunarSSCPower;
        public static ConfigEntry<float> TierVoidSSCPower;
        public static ConfigEntry<float> TierFoodSSCPower;

        public static ConfigEntry<bool> EnableItemDrop;
        public static ConfigEntry<float> DropChance;
        public static ConfigEntry<float> Tier1DropWeight;
        public static ConfigEntry<float> Tier2DropWeight;
        public static ConfigEntry<float> Tier3DropWeight;
        public static ConfigEntry<float> TierBossDropWeight;
        public static ConfigEntry<float> TierLunarDropWeight;
        public static ConfigEntry<float> TierVoidDropWeight;
        public static ConfigEntry<float> TierFoodDropWeight;
        public static ConfigEntry<float> TierCustomDropWeight;

        public static void Init(ConfigFile config)
        {
            ItemBlacklist = config.Bind(
                "Blacklist",
                "Item Blacklist",
                string.Join(",",
                    "StickyBomb",                   // Sticky Bomb
                    "StunChanceOnHit",              // Stun Grenade
                    "NovaOnHeal",                   // N'kuhana's Opinion
                    "ShockNearby",                  // Tesla Coil
                    "Mushroom",                     // Bustling Fungus
                    "ExplodeOnDeath",               // Genesis Loop
                    "LaserTurbine",                 // Resonance Disk
                    "ExecuteLowHealthElite",        // Old Guillotine
                    "TitanGoldDuringTP",            // Halcyon Seed
                    "TreasureCache",                // Rusted Key
                    "BossDamageBonus",              // Armor-Piercing Rounds
                    "ExtraLifeConsumed",            // Dio (consumed)
                    "Feather",                      // Hopoo Feather
                    "Firework",                     // Bundle of Fireworks
                    "SprintArmor",                  // Rose Buckler
                    "JumpBoost",                    // Wax Quail
                    "GoldOnHit",                    // Brittle Crown
                    "WardOnLevel",                  // Warbanner
                    "BeetleGland",                  // Queen's Gland
                    "TPHealingNova",                // Lepton Daisy
                    "LunarTrinket",                 // Beads of Fealty
                    "LunarPrimaryReplacement",      // Visions of Heresy
                    "LunarUtilityReplacement",      // Strides of Heresy
                    "LunarSecondaryReplacement",    // Hooks of Heresy
                    "LunarSpecialReplacement",      // Essense of Heresy
                    "BonusGoldPackOnKill",          // Ghor's Tome
                    "Squid",                        // Squid Polyp
                    "SprintWisp",                   // Little Disciple
                    "FocusConvergence",             // Focused Convergence
                    "MonstersOnShrineUse",          // Defiant Gouge
                    "ScrapWhite",                   // White Scrap
                    "ScrapGreen",                   // Green Scrap
                    "ScrapRed",                     // Red Scrap
                    "ScrapYellow",                  // Yellow Scrap
                    "SharedSuffering",              // Networked Suffering,
                    "Thorns"                        // Razorwire
                    ),
                "Blacklist of items that will never be given to AIs, separated by comas. Uses ItemDef names, you can get them in-game via list_item command with DebugToolkit.");

            ItemBlacklistTags = config.Bind(
                "Blacklist",
                "Blacklisted Tags",
                string.Join(",",
                    nameof(ItemTag.AIBlacklist),
                    nameof(ItemTag.Scrap),
                    nameof(ItemTag.ExtractorUnitBlacklist),
                    nameof(ItemTag.WorldUnique),
                    nameof(ItemTag.CannotSteal),
                    nameof(ItemTag.CannotCopy)
                    ),
                "List of blacklisted item tags, separated by coma.");

            EquipmentBlacklist = config.Bind(
                "Blacklist",
                "Equipment Blacklist",
                string.Join(",",
                    "Blackhole",
                    "BFG",
                    "Lightning",
                    "Scanner",
                    "CommandMissile",
                    "DroneBackup",
                    "Gateway",
                    "FireBallDash",
                    "Saw",
                    "Recycle",
                    "BossHunter"
                    ),
                "Blacklist of equipment that will never be given to AIs, separated by comas. Uses EquipmentDef names, you can get them in-game via list_equip command with DebugToolkit.");

            ItemCaps = config.Bind(
                "Blacklist",
                "Item Caps",
                string.Join(",",
                    "Bear:7",
                    "HealWhileSafe:30",
                    "EquipmentMagazine:3",
                    "SlowOnHit:1",
                    "Behemoth:2",
                    "BleedOnHit:3",
                    "IgniteOnKill:2",
                    "AutoCastEquipment:1",
                    "NearbyDamageBonus:3",
                    "DeathMark:1"
                    ),
                "Maximum number of item possible. Caps are applied after Item Multiplier. Format is ItemDef:Cap, separated by coma."
                );

            MinStageCompletion = config.Bind("General", "Min Stage Completion", 0, "Minimum stage completion before enemies start getting items.");
            OnlyAffectMonsters = config.Bind("General", "Only Affect Monster Team", true, "Items are added only to Monster team, Void and Lunar (and potentially modded teams) are unaffected. False to affect all teams except Player and Neutral.");
            CopyPlayerInventory = config.Bind("General", "Copy Random Player Inventory", false, "Copies items from a random player inventory. Option is exclusive, items are either copied from players or generated randomly.");
            ClearInventoryOnSpawn = config.Bind("General", "Clear Inventory on Card Spawn", false, "Clears inventory on SpawnCard spawn. This is a legacy option to support behaviour of the original mod, since it clears inventory on card spawn this means it removes all elite item bonuses from the inventory as well.");
            ItemMultiplier = config.Bind("General", "Item Multiplier", 1f, "Multiplies all items by this amount");

            EnableTier1 = config.Bind("General", "Give Tier 1", true, "Generate or inherit Tier 1 items.");
            EnableTier2 = config.Bind("General", "Give Tier 2", true, "Generate or inherit Tier 2 items.");
            EnableTier3 = config.Bind("General", "Give Tier 3", true, "Generate or inherit Tier 3 items.");
            EnableTierBoss = config.Bind("General", "Give Tier Boss", false, "Generate or inherit Tier Boss items.");
            EnableTierLunar = config.Bind("General", "Give Tier Lunar", false, "Generate or inherit Tier Lunar items.");
            EnableTierVoid = config.Bind("General", "Give Tier Void", false, "Generate or inherit Tier Void items.");
            EnableTierFood = config.Bind("General", "Give Tier Food", false, "Generate or inherit Tier Food items.");
            EnableEquipment = config.Bind("General", "Give Equipment", false, "Generate or inherit Equipment. Gesture of the Drown will also be given so enemies can actually use the equipment.");
            EnableCustomTiers = config.Bind("General", "Give Custom Tiers", false, "Inherit items of Custom tiers. Only inherit, they will not be generated.");

            ItemAdder.EnabledItemTierDictionary.Add(ItemTier.Tier1, EnableTier1.Value);
            ItemAdder.EnabledItemTierDictionary.Add(ItemTier.Tier2, EnableTier2.Value);
            ItemAdder.EnabledItemTierDictionary.Add(ItemTier.Tier3, EnableTier3.Value);
            ItemAdder.EnabledItemTierDictionary.Add(ItemTier.Boss, EnableTierBoss.Value);
            ItemAdder.EnabledItemTierDictionary.Add(ItemTier.Lunar, EnableTierLunar.Value);
            ItemAdder.EnabledItemTierDictionary.Add(ItemTier.FoodTier, EnableTierFood.Value);
            ItemAdder.EnabledItemTierDictionary.Add(ItemTier.VoidTier1, EnableTierVoid.Value);
            ItemAdder.EnabledItemTierDictionary.Add(ItemTier.VoidTier2, EnableTierVoid.Value);
            ItemAdder.EnabledItemTierDictionary.Add(ItemTier.VoidTier3, EnableTierVoid.Value);
            ItemAdder.EnabledItemTierDictionary.Add(ItemTier.VoidBoss, EnableTierVoid.Value);
            ItemAdder.EnabledItemTierDictionary.Add(ItemTier.AssignedAtRuntime, EnableCustomTiers.Value);

            ItemGenerationCount = config.Bind("Item Generation", "Item Generation Type", ItemGenerationType.Average, "What number of item to use as a basis. Average - average number of items across all players (total count divided by number of players). MaxOnePlayer - maximum number of items across all players. Total - total number of items across all players.");

            Tier1GenChance = config.Bind("Item Generation", "Tier 1 Generation Chance", 40f, "The percent chance for generating Tier 1 item.");
            Tier2GenChance = config.Bind("Item Generation", "Tier 2 Generation Chance", 20f, "The percent chance for generating Tier 2 item.");
            Tier3GenChance = config.Bind("Item Generation", "Tier 3 Generation Chance", 1f, "The percent chance for generating Tier 3 item.");
            TierBossGenChance = config.Bind("Item Generation", "Tier Boss Generation Chance", 3f, "The percent chance for generating Tier Boss item.");
            TierLunarGenChance = config.Bind("Item Generation", "Tier Lunar Generation Chance", 0.5f, "The percent chance for generating Tier Lunar item.");
            TierVoidGenChance = config.Bind("Item Generation", "Tier Void Generation Chance", 0.5f, "The percent chance for generating Tier Void item.");
            TierFoodGenChance = config.Bind("Item Generation", "Tier Food Generation Chance", 0.5f, "The percent chance for generating Tier Food item.");
            EquipmentGenChance = config.Bind("Item Generation", "Equipment Generation Chance", 10f, "The percent change for generating Equipment.");

            Tier1SSCPower = config.Bind("Item Generation", "Tier 1 Stage Clear Count Power", 2f, "Power (exponent) of stage clear count for Tier 1. Used when calculating max number of items that will be given per item of that tier.");
            Tier2SSCPower = config.Bind("Item Generation", "Tier 2 Stage Clear Count Power", 1.8f, "Power (exponent) of stage clear count for Tier 2. Used when calculating max number of items that will be given per item of that tier.");
            Tier3SSCPower = config.Bind("Item Generation", "Tier 3 Stage Clear Count Power", 1.5f, "Power (exponent) of stage clear count for Tier 3. Used when calculating max number of items that will be given per item of that tier.");
            TierBossSSCPower = config.Bind("Item Generation", "Tier Boss Stage Clear Count Power", 1.3f, "Power (exponent) of stage clear count for Tier Boss. Used when calculating max number of items that will be given per item of that tier.");
            TierLunarSSCPower = config.Bind("Item Generation", "Tier Lunar Stage Clear Count Power", 1.1f, "Power (exponent) of stage clear count for Tier Lunar. Used when calculating max number of items that will be given per item of that tier.");
            TierVoidSSCPower = config.Bind("Item Generation", "Tier Void Stage Clear Count Power", 1f, "Power (exponent) of stage clear count for Tier Void. Used when calculating max number of items that will be given per item of that tier.");
            TierFoodSSCPower = config.Bind("Item Generation", "Tier Food Stage Clear Count Power", 1f, "Power (exponent) of stage clear count for Tier Food. Used when calculating max number of items that will be given per item of that tier.");

            Tier1GenCap = config.Bind("Item Generation", "Tier 1 Gen Cap", 4f, "Generation cap for Tier 1 items. Basically this number is multiplied by stage clear count with exponent for this tier BEFORE MULTIPLIER. Multiplier is added after.");
            Tier2GenCap = config.Bind("Item Generation", "Tier 2 Gen Cap", 2f, "Generation cap for Tier 2 items. Basically this number is multiplied by stage clear count with exponent for this tier BEFORE MULTIPLIER. Multiplier is added after.");
            Tier3GenCap = config.Bind("Item Generation", "Tier 3 Gen Cap", 1f, "Generation cap for Tier 3 items. Basically this number is multiplied by stage clear count with exponent for this tier BEFORE MULTIPLIER. Multiplier is added after.");
            TierBossGenCap = config.Bind("Item Generation", "Tier Boss Gen Cap", 1f, "Generation cap for Tier Boss items. Basically this number is multiplied by stage clear count with exponent for this tier BEFORE MULTIPLIER. Multiplier is added after.");
            TierLunarGenCap = config.Bind("Item Generation", "Tier Lunar Gen Cap", 1f, "Generation cap for Tier Lunar items. Basically this number is multiplied by stage clear count with exponent for this tier BEFORE MULTIPLIER. Multiplier is added after.");
            TierVoidGenCap = config.Bind("Item Generation", "Tier Void Gen Cap", 1f, "Generation cap for Tier Void items. Basically this number is multiplied by stage clear count with exponent for this tier BEFORE MULTIPLIER. Multiplier is added after.");
            TierFoodGenCap = config.Bind("Item Generation", "Tier Food Gen Cap", 1f, "Generation cap for Tier Food items. Basically this number is multiplied by stage clear count with exponent for this tier BEFORE MULTIPLIER. Multiplier is added after.");

            EnableItemDrop = config.Bind("Item Drop", "Enable Item Drop", true, "Enables item drop from enemies. It assumes that you have item generation or inheritance enabled since dropped items will be from their inventories.");
            DropChance = config.Bind("Item Drop", "Item Drop Chance", 0.1f, "The percent chance of dropping an item. 0.1 means 1 in 1000 kills will result in a drop. Quality of said items is rolled after that");
            Tier1DropWeight = config.Bind("Item Drop", "Tier 1 Weight", 0.9f, "Weight of Tier 1. Used when dropping items, the higher the weight the higher the chance this item will be selected. Weights are relative to each other.");
            Tier2DropWeight = config.Bind("Item Drop", "Tier 2 Weight", 0.1f, "Weight of Tier 2. Used when dropping items, the higher the weight the higher the chance this item will be selected. Weights are relative to each other.");
            Tier3DropWeight = config.Bind("Item Drop", "Tier 3 Weight", 0.05f, "Weight of Tier 3. Used when dropping items, the higher the weight the higher the chance this item will be selected. Weights are relative to each other.");
            TierBossDropWeight = config.Bind("Item Drop", "Tier Boss Weight", 0.07f, "Weight of Tier Boss. Used when dropping items, the higher the weight the higher the chance this item will be selected. Weights are relative to each other.");
            TierLunarDropWeight = config.Bind("Item Drop", "Tier Lunar Weight", 0.01f, "Weight of Tier Lunar. Used when dropping items, the higher the weight the higher the chance this item will be selected. Weights are relative to each other.");
            TierVoidDropWeight = config.Bind("Item Drop", "Tier Void Weight", 0.1f, "Weight of Tier Void. Used when dropping items, the higher the weight the higher the chance this item will be selected. Weights are relative to each other.");
            TierFoodDropWeight = config.Bind("Item Drop", "Tier Food Weight", 0.001f, "Weight of Tier Food. Used when dropping items, the higher the weight the higher the chance this item will be selected. Weights are relative to each other.");
            TierCustomDropWeight = config.Bind("Item Drop", "Tier Custom Weight", 0.1f, "Weight of Custom Tier. Used when dropping items, the higher the weight the higher the chance this item will be selected. Weights are relative to each other.");

            ItemDropper.ItemTierDropWeightDictionary.Add(ItemTier.Tier1, Tier1DropWeight.Value);
            ItemDropper.ItemTierDropWeightDictionary.Add(ItemTier.Tier2, Tier2DropWeight.Value);
            ItemDropper.ItemTierDropWeightDictionary.Add(ItemTier.Tier3, Tier3DropWeight.Value);
            ItemDropper.ItemTierDropWeightDictionary.Add(ItemTier.Boss, TierBossDropWeight.Value);
            ItemDropper.ItemTierDropWeightDictionary.Add(ItemTier.Lunar, TierLunarDropWeight.Value);
            ItemDropper.ItemTierDropWeightDictionary.Add(ItemTier.FoodTier, TierFoodDropWeight.Value);
            ItemDropper.ItemTierDropWeightDictionary.Add(ItemTier.VoidTier1, TierVoidDropWeight.Value);
            ItemDropper.ItemTierDropWeightDictionary.Add(ItemTier.VoidTier2, TierVoidDropWeight.Value);
            ItemDropper.ItemTierDropWeightDictionary.Add(ItemTier.VoidTier3, TierVoidDropWeight.Value);
            ItemDropper.ItemTierDropWeightDictionary.Add(ItemTier.VoidBoss, TierVoidDropWeight.Value);
            ItemDropper.ItemTierDropWeightDictionary.Add(ItemTier.AssignedAtRuntime, TierCustomDropWeight.Value);
        }
    }
}
