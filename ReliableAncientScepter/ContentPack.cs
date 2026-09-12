using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.ExpansionManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ReliableAncientScepter
{
    public class ContentProvider : IContentPackProvider
    {
        public string identifier => ReliableAncientScepterPlugin.GUID + "." + nameof(ContentPack);

        private readonly ContentPack _contentPack = new ContentPack();

        public static InteractableSpawnCard iscScepterGem;

        public static ItemDef BrokenScepterShaft;

        public static ItemDef BrokenScepterGem;

        public static GameObject voidCampGemInteractable;

        public static readonly Dictionary<string, string> ShaderLookup = new Dictionary<string, string>()
        {
            {"stubbedror2/base/shaders/hgstandard", "RoR2/Base/Shaders/HGStandard.shader"}
        };

        private Material[] materialCache;

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

            string assetBundleFolderPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(ContentProvider).Assembly.Location), "AssetBundles");
            AssetBundle assetbundle = null;
            yield return LoadAssetBundle(System.IO.Path.Combine(assetBundleFolderPath, "reliablescepter"), args.progressReceiver, (resultAssetBundle) => assetbundle = resultAssetBundle);

            BrokenScepterShaft = CreateItem("ViligerBrokenScepterShaft", assetbundle.LoadAsset<GameObject>("Assets/Scepter/PickupScepterShaft.prefab"), assetbundle.LoadAsset<Sprite>("Assets/Scepter/texScepterShaftIcon.png"));
            BrokenScepterGem = CreateItem("ViligerBrokenScepterGem", assetbundle.LoadAsset<GameObject>("Assets/Scepter/PickupScepterGem.prefab"), assetbundle.LoadAsset<Sprite>("Assets/Scepter/texScepterGemIcon.png"));

            _contentPack.itemDefs.Add(new ItemDef[] { BrokenScepterGem, BrokenScepterShaft });

            var recipe = CreateRecipe(AncientScepter.AncientScepterItem.myDef, BrokenScepterShaft, BrokenScepterGem);
            _contentPack.craftableDefs.Add(new CraftableDef[] { recipe });

            voidCampGemInteractable = CreateVoidCampGemInteractable(assetbundle.LoadAsset<GameObject>("Assets/Scepter/ScepterGemInteractable.prefab"), BrokenScepterGem);
            _contentPack.networkedObjectPrefabs.Add(new GameObject[] { voidCampGemInteractable });
            iscScepterGem = CreateInteractableSpawnCard(voidCampGemInteractable);

            materialCache = assetbundle.LoadAllAssets<Material>();
            SwapMaterials(materialCache);

            var voidCamp = Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_DLC1_VoidCamp.VoidCamp_prefab).WaitForCompletion();
            var interactables = voidCamp.transform.Find("Camp 1 - Void Monsters & Interactables");
            if (interactables)
            {
                var spawner = interactables.gameObject.AddComponent<ScepterVoidCampSpawner>();
                spawner.iscScepterGem = iscScepterGem;
            }

            yield break;
        }

        private InteractableSpawnCard CreateInteractableSpawnCard(GameObject prefab)
        {
            var isc = ScriptableObject.CreateInstance<InteractableSpawnCard>();

            isc.prefab = prefab;
            isc.sendOverNetwork = true;
            isc.hullSize = HullClassification.Human;
            isc.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;
            isc.requiredFlags = RoR2.Navigation.NodeFlags.None;
            isc.forbiddenFlags = RoR2.Navigation.NodeFlags.NoChestSpawn;
            isc.directorCreditCost = ReliableAncientScepterPlugin.ScepterGemVoidCampCost.Value;
            isc.occupyPosition = true;
            isc.eliteRules = SpawnCard.EliteRules.Default;
            isc.orientToFloor = true;
            isc.maxSpawnsPerStage = 1; // it doesn't matter but still

            return isc;
        }

        private CraftableDef CreateRecipe(ItemDef target, ItemDef reg1, ItemDef reg2)
        {
            var craftableDef = ScriptableObject.CreateInstance<CraftableDef>();
            (craftableDef as ScriptableObject).name = "viligerReliableScepter" + target.name;
            craftableDef.pickup = target;
            craftableDef.recipes = new Recipe[]
            {
                new Recipe()
                {
                    ingredients = new RecipeIngredient[]
                    {
                        new RecipeIngredient()
                        {
                            pickup = reg1,
                            type = IngredientTypeIndex.AssetReference
                        },
                        new RecipeIngredient()
                        {
                            pickup = reg2,
                            type = IngredientTypeIndex.AssetReference
                        }
                    }
                }
            };

            return craftableDef;
        }

        private GameObject CreateVoidCampGemInteractable(GameObject prefab, ItemDef itemDefToDrop)
        {
            var gem = prefab.transform.Find("ModelBase/Gem");

            prefab.AddComponent<NetworkIdentity>();

            var highlight = prefab.AddComponent<Highlight>();
            highlight.strength = 1f;
            highlight.targetRenderer = gem.GetComponent<Renderer>();
            highlight.highlightColor = Highlight.HighlightColor.interactive;

            var modelLocator = prefab.AddComponent<ModelLocator>();
            modelLocator.modelTransform = gem;
            modelLocator.modelBaseTransform = prefab.transform.Find("ModelBase");
            modelLocator.dontDetatchFromParent = true;
            modelLocator.modelScaleCompensation = gem.localScale.x; // sure

            var expansion = prefab.AddComponent<ExpansionRequirementComponent>();
            expansion.requiredExpansion = Addressables.LoadAssetAsync<ExpansionDef>(RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_DLC1_Common.DLC1_asset).WaitForCompletion();

            var dropItem = prefab.AddComponent<DropItem>();
            dropItem.itemToDrop = itemDefToDrop;
            dropItem.dropletOrigin = prefab.transform.Find("ModelBase/DropletOrigin");

            var purchase = prefab.AddComponent<PurchaseInteraction>();
            purchase.displayNameToken = "VILIGER_RELIABLE_SCEPTER_INTERACABLE_GEM_NAME";
            purchase.contextToken = "VILIGER_RELIABLE_SCEPTER_INTERACABLE_GEM_CONTEXT";
            purchase.costType = CostTypeIndex.None;
            purchase.setUnavailableOnTeleporterActivated = false;
            purchase.isShrine = false;
            purchase.shouldProximityHighlight = true;
#pragma warning disable CS0618 // Type or member is obsolete
            purchase.onPurchase = new PurchaseEvent();
            purchase.onPurchase.AddPersistentListener<Interactor>(dropItem.DropAndDestroySelf);
#pragma warning restore CS0618 // Type or member is obsolete

            var locator = gem.gameObject.AddComponent<EntityLocator>();
            locator.entity = prefab;

            prefab.RegisterNetworkPrefab();

            return prefab;
        }

        private ItemDef CreateItem(string name, GameObject pickupModel, Sprite icon)
        {
            var itemDef = ScriptableObject.CreateInstance<ItemDef>();

            (itemDef as ScriptableObject).name = name;
            itemDef.tier = ItemTier.NoTier;
#pragma warning disable CS0618 // Type or member is obsolete
            itemDef.deprecatedTier = ItemTier.NoTier;
            itemDef.name = name;
            itemDef.nameToken = "VILIGER_RELIABLE_SCEPTER_ITEM_" + name.ToUpper() + "_NAME";
            itemDef.pickupToken = "VILIGER_RELIABLE_SCEPTER_ITEM_" + name.ToUpper() + "_PICKUP";
            itemDef.descriptionToken = "VILIGER_RELIABLE_SCEPTER_ITEM_" + name.ToUpper() + "_DESC";
            itemDef.loreToken = "VILIGER_RELIABLE_SCEPTER_ITEM_" + name.ToUpper() + "_LORE";
            itemDef.pickupModelPrefab = pickupModel;
#pragma warning restore CS0618 // Type or member is obsolete
            itemDef.pickupIconSprite = icon;
            itemDef.canRemove = false;
            itemDef.isConsumed = true;
            itemDef.tags = new ItemTag[] { ItemTag.AllowedForUseAsCraftingIngredient };

            return itemDef;
        }

        private void SwapMaterials(Material[] assets)
        {
            var materials = assets;

            if (materials != null)
            {
                foreach (Material material in materials)
                {
                    if (!ShaderLookup.TryGetValue(material.shader.name.ToLower(), out var matName))
                    {
                        Log.Info($"Couldn't find replacement shader for {material.shader.name.ToLower()} in dictionary for material {material.name}.");
                        continue;
                    }
                    var replacementShader = Addressables.LoadAssetAsync<Shader>(matName).WaitForCompletion();
                    if (replacementShader)
                    {
                        var renderQueue = material.renderQueue;
                        material.shader = replacementShader;
                        material.renderQueue = renderQueue;
                    }
                    else
                    {
                        Log.Info("Couldn't find replacement shader for " + material.shader.name.ToLower());
                    }
                }
            }
        }

        private IEnumerator LoadAssetBundle(string assetBundleFullPath, IProgress<float> progress, Action<AssetBundle> onAssetBundleLoaded)
        {
            var assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(assetBundleFullPath);
            while (!assetBundleCreateRequest.isDone)
            {
                progress.Report(assetBundleCreateRequest.progress);
                yield return null;
            }

            onAssetBundleLoaded(assetBundleCreateRequest.assetBundle);

            yield break;
        }
    }
}
