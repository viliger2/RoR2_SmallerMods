using ProperSave.Data;
using RoR2;
using System.Collections.Generic;

namespace ArtifactOfLimit
{
    internal static class ProperSaveCompat
    {
        private static bool? _enabled;

        private const string SAVE_DATA_NAME = "viliger_artifactoflimit_save";

        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.KingEnderBrine.ProperSave");
                }
                return (bool)_enabled;
            }
        }

        public static bool IsFirstRun()
        {
            return ProperSave.Loading.FirstRunStage;
        }

        public static bool IsLoading()
        {
            return ProperSave.Loading.IsLoading;
        }

        public static void RegisterSaveData()
        {
            ProperSave.SaveFile.OnGatherSaveData += SaveFile_OnGatherSaveData;
        }

        public static void LoadItemMask(out ItemMask itemMask)
        {
            var data = ProperSave.Loading.CurrentSave.GetModdedData<ItemMaskData>(SAVE_DATA_NAME);
            data.LoadDataOut(out itemMask);
        }

        private static void SaveFile_OnGatherSaveData(Dictionary<string, object> saveData)
        {
            saveData.Add(SAVE_DATA_NAME, new ProperSave.Data.ItemMaskData(ArtifactOfLimitManager.newAvailableItems));
        }


    }
}
