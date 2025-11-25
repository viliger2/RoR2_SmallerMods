using BepInEx;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections;
using UnityEngine;

[assembly: HG.Reflection.SearchableAttribute.OptInAttribute]
namespace ArtifactOfLimit
{
    [BepInPlugin(GUID, ModName, Version)]
    [BepInDependency("com.KingEnderBrine.ProperSave", BepInDependency.DependencyFlags.SoftDependency)]
    public class ArtifactOfLimitPlugin : BaseUnityPlugin
    {
        public const string Author = "viliger";
        public const string ModName = "ArtifactOfLimit";
        public const string Version = "1.0.2";
        public const string GUID = "com." + Author + "." + ModName;

        private void Awake()
        {
            ArtifactOfLimit.Config.PopulateConfig(this.Config);
            ContentManager.collectContentPackProviders += ContentManager_collectContentPackProviders;
            RoR2.Language.collectLanguageRootFolders += Language_collectLanguageRootFolders;

            if (ProperSaveCompat.enabled)
            {
                ProperSaveCompat.RegisterSaveData();
            }

            On.RoR2.Run.Start += Run_Start;
        }

        private void Run_Start(On.RoR2.Run.orig_Start orig, Run self)
        {
            orig(self);
            if (RunArtifactManager.instance.IsArtifactEnabled(ArtifactOfLimitManager.myArtifact))
            {
                PickupDropTable.RegenerateAll(self);
            }
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

    public class ContentProvider : IContentPackProvider
    {
        public string identifier => ArtifactOfLimitPlugin.GUID + "." + nameof(ContentPack);

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

            string assetBundleFolderPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(ContentProvider).Assembly.Location), "AssetBundles");
            AssetBundle assetbundle = null;
            yield return LoadAssetBundle(System.IO.Path.Combine(assetBundleFolderPath, "artifactoflimit"), args.progressReceiver, (resultAssetBundle) => assetbundle = resultAssetBundle);

            var artifactDef = ScriptableObject.CreateInstance<RoR2.ArtifactDef>();
            (artifactDef as ScriptableObject).name = "viliger_ArtifactOfLimit";
            artifactDef.nameToken = "VILIGER_ARTIFACT_OF_LIMIT_NAME";
            artifactDef.descriptionToken = "VILIGER_ARTIFACT_OF_LIMIT_NAME_DESC";
            artifactDef.smallIconSelectedSprite = assetbundle.LoadAsset<Sprite>("Assets/ArtifactOfLimit/limiton.png");
            artifactDef.smallIconDeselectedSprite = assetbundle.LoadAsset<Sprite>("Assets/ArtifactOfLimit/limitoff.png");

            ArtifactOfLimitManager.myArtifact = artifactDef;

            _contentPack.artifactDefs.Add(new RoR2.ArtifactDef[] { artifactDef });
            yield break;
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
