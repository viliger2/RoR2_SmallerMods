using BepInEx;
using BepInEx.Configuration;
using RoR2.ContentManagement;
using System.Security.Permissions;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace BasicChest
{
    [BepInPlugin(GUID, ModName, Version)]
    [BepInDependency(R2API.PrefabAPI.PluginGUID)]
    public class BasicChestPlugin : BaseUnityPlugin
    {
        public const string Author = "viliger";
        public const string ModName = "BasicChest";
        public const string Version = "1.0.0";
        public const string GUID = "com." + Author + "." + ModName;

        public static ConfigEntry<string> DropList;
        public static ConfigEntry<int> GoldCost;
        public static ConfigEntry<int> DirectorCost;

        public static ConfigEntry<float> Tier1Weight;
        public static ConfigEntry<float> Tier2Weight;
        public static ConfigEntry<float> Tier3Weight;
        public static ConfigEntry<float> EverythingElseWeight;

        public void Awake()
        {
            DropList = Config.Bind(
                "Basic Chest", 
                "Drop Table", 
                string.Join(",",
                    "CritGlasses",
                    "Hoof",
                    "Feather",
                    "SprintBonus",
                    "Syringe",
                    "BossDamageBonus",
                    "StickyBomb",
                    "SecondarySkillMagazine",
                    "Missile",
                    "ChainLightning",
                    "ExplodeOnDeath",
                    "FireRing",
                    "IceRing",
                    "Behemoth",
                    "AlienHead",
                    "UtilitySkillMagazine",
                    "Dagger"), 
                "Drop table, separated by coma (,). You can get internal item names by using DebugToolkit's list_item command.");
            GoldCost = Config.Bind("Basic Chest", "Gold Cost", 40, "Cost of Basic Chest.");
            DirectorCost = Config.Bind("Basic Chest", "Director Cost", 30, "Director cost of Basic Chest.");

            Tier1Weight = Config.Bind("Basic Chest", "Tier 1 Weight", 75f, "Weight of Tier 1 items. Weights are relative to each other.");
            Tier2Weight = Config.Bind("Basic Chest", "Tier 2 Weight", 20f, "Weight of Tier 2 items. Weights are relative to each other.");
            Tier3Weight = Config.Bind("Basic Chest", "Tier 3 Weight", 5f, "Weight of Tier 3 items. Weights are relative to each other.");
            EverythingElseWeight = Config.Bind("Basic Chest", "Everything Else Weight", 1f, "Weight of item of everyother tier. Weights are relative to each other.");

            ContentManager.collectContentPackProviders += ContentManager_collectContentPackProviders;
            RoR2.Language.collectLanguageRootFolders += Language_collectLanguageRootFolders;
            On.RoR2.SceneDirector.GenerateInteractableCardSelection += SceneDirector_GenerateInteractableCardSelection;
        }

        private WeightedSelection<RoR2.DirectorCard> SceneDirector_GenerateInteractableCardSelection(On.RoR2.SceneDirector.orig_GenerateInteractableCardSelection orig, RoR2.SceneDirector self)
        {
            var result = orig(self);

            if(result == null)
            {
                return result;
            }

            if (ContentProvider.iscBasicChest) 
            {
                // first iteration - check that it has a director card with our spawncard
                for(int i = 0; i < result.Count; i++)
                {
                    var choice = result.choices[i];
                    if(choice.value.spawnCard == ContentProvider.iscBasicChest)
                    {
                        return result;
                    }
                }

                for (int i = 0; i < result.Count; i++)
                {
                    var choice = result.choices[i];
                    if (choice.value.spawnCard.name == "iscChest2")
                    {
                        var directorCard = new RoR2.DirectorCard()
                        {
                            spawnCard = ContentProvider.iscBasicChest,
                            selectionWeight = choice.value.selectionWeight,
                            spawnDistance = choice.value.spawnDistance,
                            minimumStageCompletions = choice.value.minimumStageCompletions,
                        };

                        result.AddChoice(directorCard, choice.weight);
                        break;
                    }
                }
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