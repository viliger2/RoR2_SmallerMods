using BepInEx;
using BepInEx.Configuration;
using RoR2;
using RoR2.ContentManagement;
using System;

namespace ReliableAncientScepter
{
    [BepInPlugin(GUID, Name, Version)]
    [BepInDependency(AncientScepter.AncientScepterMain.ModGuid, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(R2API.PrefabAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    public class ReliableAncientScepterPlugin : BaseUnityPlugin
    {
        public const string Author = "viliger";
        public const string Name = nameof(ReliableAncientScepterPlugin);
        public const string Version = "1.0.0";
        public const string GUID = Author + "." + Name;

        public static ConfigEntry<float> HalcyoniteRewardChance;
        public static ConfigEntry<int> ScepterGemVoidCampCost;
        public static ConfigEntry<float> VoidCampGemSpawnChance;

        public void Awake()
        {
            Log.Init(Logger);

            HalcyoniteRewardChance = Config.Bind("Reliable Ancient Scepter", "Halcyonite Reward Spawn Chance", 50f, "Percent chance that Scepter Shaft will replace one of the items from Halcyonite Shrine rewards (and the one you get at meridian but shhhh)");
            ScepterGemVoidCampCost = Config.Bind("Reliable Ancient Scepter", "Void Camp Spawn Cost", 30, "How much, in credits, it costs Void Camp to spawn Scepter Gem interactable.");
            VoidCampGemSpawnChance = Config.Bind("Reliable Ancient Scepter", "Void Camp Spawn Chance", 50f, "Percent chance that Scepter Gem will spawn in Void Camp.");

            ContentManager.collectContentPackProviders += ContentManager_collectContentPackProviders;
            RoR2.Language.collectLanguageRootFolders += Language_collectLanguageRootFolders;

            // its not ideal but I cba to IL hook HalcyoniteShrineController
            On.RoR2.PickupPickerController.GenerateOptionsFromDropTablePlusForcedStorm += PickupPickerController_GenerateOptionsFromDropTablePlusForcedStorm;
        }

        private RoR2.PickupPickerController.Option[] PickupPickerController_GenerateOptionsFromDropTablePlusForcedStorm(On.RoR2.PickupPickerController.orig_GenerateOptionsFromDropTablePlusForcedStorm orig, int numOptions, RoR2.PickupDropTable dropTable, RoR2.PickupDropTable stormDropTable, Xoroshiro128Plus rng)
        {
            var result = orig(numOptions, dropTable, stormDropTable, rng);

            var roll = rng.RangeFloat(0, 100);
            if(roll > HalcyoniteRewardChance.Value && ContentProvider.BrokenScepterShaft && result.Length > 0)
            {
                var replacementIndex = rng.RangeInt(0, result.Length);
                result[replacementIndex] = new RoR2.PickupPickerController.Option
                {
                    available = true,
                    pickup = new RoR2.UniquePickup(PickupCatalog.FindPickupIndex(ContentProvider.BrokenScepterShaft.itemIndex))
                };
            }

            return result;
        }

        private void Language_collectLanguageRootFolders(System.Collections.Generic.List<string> folders)
        {
            folders.Add(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(base.Info.Location), "Language"));
        }

        private void ContentManager_collectContentPackProviders(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(new ContentProvider());
        }
    }
}
