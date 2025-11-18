using BepInEx.Configuration;

namespace ArtifactOfLimit
{
    public static class Config
    {
        public static ConfigEntry<int> Tier1ItemCount;
        public static ConfigEntry<float> Tier1DamageTag;
        public static ConfigEntry<float> Tier1HealingTag;
        public static ConfigEntry<float> Tier1UtilityTag;

        public static ConfigEntry<int> Tier2ItemCount;
        public static ConfigEntry<float> Tier2DamageTag;
        public static ConfigEntry<float> Tier2HealingTag;
        public static ConfigEntry<float> Tier2UtilityTag;

        public static ConfigEntry<int> Tier3ItemCount;
        public static ConfigEntry<float> Tier3DamageTag;
        public static ConfigEntry<float> Tier3HealingTag;
        public static ConfigEntry<float> Tier3UtilityTag;

        public static ConfigEntry<bool> AffectVoidItems;
        public static ConfigEntry<int> PityVoidItemCount;

        public static void PopulateConfig(ConfigFile config)
        {
            Tier1ItemCount = config.Bind("Tier 1", "Total Number of Items", 24, "Total number of Tier 1 items to be used each run.");
            Tier1DamageTag = config.Bind("Tier 1", "Share of Damage Items", 10f, "Share of Tier 1 items with Damage tag. You can use any value you want, since all Share will be added together and then relative will be calculated.");
            Tier1HealingTag = config.Bind("Tier 1", "Share of Healing Items", 6f, "Share of Tier 1 items with Healing tag. You can use any value you want, since all Share will be added together and then relative will be calculated.");
            Tier1UtilityTag = config.Bind("Tier 1", "Share of Utility Items", 8f, "Share of Tier 1 items with Utility tag. You can use any value you want, since all Share will be added together and then relative will be calculated.");

            Tier2ItemCount = config.Bind("Tier 2", "Total Number of Items", 24, "Total number of Tier 2 items to be used each run.");
            Tier2DamageTag = config.Bind("Tier 2", "Share of Damage Items", 12f, "Share of Tier 2 items with Damage tag. You can use any value you want, since all Share will be added together and then relative will be calculated.");
            Tier2HealingTag = config.Bind("Tier 2", "Share of Healing Items", 4f, "Share of Tier 2 items with Healing tag. You can use any value you want, since all Share will be added together and then relative will be calculated.");
            Tier2UtilityTag = config.Bind("Tier 2", "Share of Utility Items", 8f, "Share of Tier 2 items with Utility tag. You can use any value you want, since all Share will be added together and then relative will be calculated.");

            Tier3ItemCount = config.Bind("Tier 3", "Total Number of Items", 21, "Total number of Tier 3 items to be used each run.");
            Tier3DamageTag = config.Bind("Tier 3", "Share of Damage Items", 11f, "Share of Tier 3 items with Damage tag. You can use any value you want, since all Share will be added together and then relative will be calculated.");
            Tier3HealingTag = config.Bind("Tier 3", "Share of Healing Items", 3f, "Share of Tier 3 items with Healing tag. You can use any value you want, since all Share will be added together and then relative will be calculated.");
            Tier3UtilityTag = config.Bind("Tier 3", "Share of Utility Items", 7f, "Share of Tier 3 items with Utility tag. You can use any value you want, since all Share will be added together and then relative will be calculated.");

            AffectVoidItems = config.Bind("Void Items", "Affect Void Items", true, "Only those Void Items that can be converted from selected Tier1-3 item will be selected. Newly Hatched Zoea (and all VoidBoss items) will be always included.");
            PityVoidItemCount = config.Bind("Void Items", "Pity Void Items Count", 5, "Count of Void Items that will be used if selected item pool has zero corruptable items. Values below 3 will most likely lead to а bad time.");
        }
    }
}
