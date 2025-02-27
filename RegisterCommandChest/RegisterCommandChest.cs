using BepInEx;
using RoR2.ContentManagement;
using System;
using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine;

namespace RegisterCommandChest
{
    [BepInPlugin(GUID, Name, Version)]
    public class RegisterCommandChest : BaseUnityPlugin
    {
        public const string Author = "Viliger";
        public const string Name = nameof(RegisterCommandChest);
        public const string Version = "1.0.1";
        public const string GUID = Author + "." + Name;

        public const string LanguageFolder = "Language";

        private void Awake()
        {
            ContentManager.collectContentPackProviders += ContentManager_collectContentPackProviders;
            RoR2.Language.collectLanguageRootFolders += Language_collectLanguageRootFolders;
        }

        private void ContentManager_collectContentPackProviders(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(new ContentProvider());
        }

        private void Language_collectLanguageRootFolders(System.Collections.Generic.List<string> folders)
        {
            folders.Add(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(base.Info.Location), LanguageFolder));
        }
    }

    public class ContentProvider : IContentPackProvider
    {
        public string identifier => RegisterCommandChest.GUID + "." + nameof(ContentPack);

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

            var inspectDef = ScriptableObject.CreateInstance<RoR2.InspectDef>();
            (inspectDef as ScriptableObject).name = "CommandChestInspectDef";
            inspectDef.Info = new RoR2.UI.InspectInfo()
            {
                Visual = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/ChestIcon_1.png").WaitForCompletion(),
                TitleToken = "COMMANDCHEST_NAME",
                DescriptionToken = "COMMANDCHEST_DESCRIPTION",
                FlavorToken = "COMMANDCHEST_LORE"
            };

            var commandChestPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/CommandChest/CommandChest.prefab").WaitForCompletion();
            commandChestPrefab.AddComponent<GenericInspectInfoProvider>().InspectInfo = inspectDef;

            _contentPack.networkedObjectPrefabs.Add(new GameObject[] { commandChestPrefab });
            yield break;
        }
    }
}
