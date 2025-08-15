using BepInEx;
using RoR2;
using UnityEngine.AddressableAssets;

// TODO:
// 5. write propper contentpack instead of relying on ContentAddition
namespace FathomlessVoidling
{
    [BepInDependency(R2API.ContentManagement.R2APIContentManager.PluginGUID)]
    [BepInDependency(R2API.PrefabAPI.PluginGUID)]
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(GUID, ModName, Version)]
    public class FathomlessVoidlingPlugin : BaseUnityPlugin
    {
        public const string ModName = "FathomlessVoidling";
        public const string Version = "0.10.1";
        public const string GUID = "com.Nuxlar.FathomlessVoidling";

        public void Awake()
        {
            Log.Init(Logger);
            ModConfig.InitConfig(base.Config);

            SetupVoidling.SetupStuff();
            SetupStage.SetupStuff();

            var voidEnding = Addressables.LoadAssetAsync<GameEndingDef>("RoR2/DLC1/GameModes/VoidEnding.asset").WaitForCompletion();
            voidEnding.gameOverControllerState = new EntityStates.SerializableEntityStateType(typeof(VoidEnding.VoidEndingStart));

            RoR2.Language.collectLanguageRootFolders += Language_collectLanguageRootFolders;
        }

        private void Language_collectLanguageRootFolders(System.Collections.Generic.List<string> folders)
        {
            folders.Add(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(base.Info.Location), "Language"));
        }
    }
}
