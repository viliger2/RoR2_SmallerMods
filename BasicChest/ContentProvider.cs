using R2API;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace BasicChest
{
    public class ContentProvider : IContentPackProvider
    {
        public static GameObject basicChestPrefab;

        public static InteractableSpawnCard iscBasicChest;

        //private static Material basicChestMaterial;

        public string identifier => BasicChestPlugin.GUID + "." + nameof(ContentPack);

        private readonly ContentPack _contentPack = new ContentPack();

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(_contentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            _contentPack.identifier = identifier;

            var chest2Prefab = Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_Chest2.Chest2_prefab).WaitForCompletion();
            basicChestPrefab = chest2Prefab.InstantiateClone("viliger_BasicChest", true);

            var purchaseInteraction = basicChestPrefab.GetComponent<PurchaseInteraction>();
            purchaseInteraction.displayNameToken = "VILIGER_BASIC_CHEST_NAME";
            purchaseInteraction.cost = BasicChestPlugin.GoldCost.Value;

            var inspectDef = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<InspectDef>(RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_Chest2.Chest2InspectDef_asset).WaitForCompletion());
            inspectDef.name = "idBasicChest";
            inspectDef.Info.TitleToken = "VILIGER_BASIC_CHEST_NAME";
            inspectDef.Info.DescriptionToken = "VILIGER_BASIC_CHEST_DESC";
            inspectDef.Info.FlavorToken = "VILIGER_BASIC_CHEST_LORE";

            basicChestPrefab.GetComponent<GenericInspectInfoProvider>().InspectInfo = inspectDef;

            basicChestPrefab.GetComponent<GenericDisplayNameProvider>().displayToken = "VILIGER_BASIC_CHEST_NAME";

            CustomExplicitDropTable cedt = new CustomExplicitDropTable();
            cedt.entries = BasicChestPlugin.DropList.Value.Split(',');
            cedt.tier1Weight = BasicChestPlugin.Tier1Weight.Value;
            cedt.tier2Weight = BasicChestPlugin.Tier2Weight.Value;
            cedt.tier3Weight = BasicChestPlugin.Tier3Weight.Value;
            cedt.everythingElseWeight = BasicChestPlugin.EverythingElseWeight.Value;

            basicChestPrefab.GetComponent<ChestBehavior>().dropTable = cedt;

            basicChestPrefab.GetComponentInChildren<SkinnedMeshRenderer>().material = CreateBasicChestMaterial(); 

            _contentPack.networkedObjectPrefabs.Add(new GameObject[] { basicChestPrefab });

            iscBasicChest = ScriptableObject.CreateInstance<InteractableSpawnCard>();
            (iscBasicChest as ScriptableObject).name = "iscBasicChest";
            iscBasicChest.prefab = basicChestPrefab;
            iscBasicChest.sendOverNetwork = true;
            iscBasicChest.hullSize = HullClassification.Human;
            iscBasicChest.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;
            iscBasicChest.requiredFlags = RoR2.Navigation.NodeFlags.None;
            iscBasicChest.forbiddenFlags = RoR2.Navigation.NodeFlags.NoChestSpawn;
            iscBasicChest.directorCreditCost = BasicChestPlugin.DirectorCost.Value;
            iscBasicChest.occupyPosition = true;
            iscBasicChest.eliteRules = SpawnCard.EliteRules.Default;
            iscBasicChest.orientToFloor = true;
            iscBasicChest.slightlyRandomizeOrientation = true;
            iscBasicChest.skipSpawnWhenSacrificeArtifactEnabled = true;
            iscBasicChest.weightScalarWhenSacrificeArtifactEnabled = 1;
            iscBasicChest.skipSpawnWhenDevotionArtifactEnabled = false;
            iscBasicChest.maxSpawnsPerStage = -1;
            iscBasicChest.prismaticTrialSpawnChance = 1;

            yield break;
        }

        private Material CreateBasicChestMaterial()
        {
            var newMaterial = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<Material>(RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_Chest1.matChest1_mat).WaitForCompletion());
            newMaterial.name = "matBasicChest";
            newMaterial.SetTexture("_MainTex", Addressables.LoadAssetAsync<Texture2D>(RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_Common_TrimSheets.texTrimSheetMedical_png).WaitForCompletion());

            return newMaterial;
        }

    }
}
