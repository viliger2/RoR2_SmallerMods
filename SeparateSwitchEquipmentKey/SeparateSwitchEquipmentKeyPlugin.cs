using BepInEx;
using Rebindables;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SeparateSwitchEquipmentKey
{
    [BepInPlugin(GUID, ModName, Version)]
    [BepInDependency(Rebindables.Rebindables.PluginGUID)]
    public class SeparateSwitchEquipmentKeyPlugin : BaseUnityPlugin
    {
        public const string Author = "Viliger";
        public const string ModName = "SeparateSwitchEquipmentKey";
        public const string Version = "1.0.0";
        public const string GUID = "com." + Author + "." + ModName;

        public static ModKeybind SwitchEquipment;

        private void Awake()
        {
            RoR2.Language.collectLanguageRootFolders += Language_collectLanguageRootFolders;

            SwitchEquipment = RebindAPI.RegisterModKeybind(new ModKeybind("VILIGER_KEYBIND_SWITCH_EQUIPMENT", KeyCode.X, 14));

            On.RoR2.EquipmentSlot.Start += EquipmentSlot_Start;

            var hud = Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_UI.HUDSimple_prefab).WaitForCompletion();
            var nextIconTransform = hud.transform.Find("MainContainer/MainUIArea/SpringCanvas/BottomRightCluster/Scaler/AltEquipmentSlot/DisplayRoot/ExtraEquipmentKey/NextIcon");
            if (nextIconTransform)
            {
                nextIconTransform.gameObject.SetActive(false);
            }
            var prevIconTransform = hud.transform.Find("MainContainer/MainUIArea/SpringCanvas/BottomRightCluster/Scaler/AltEquipmentSlot/DisplayRoot/ExtraEquipmentKey/PrevIcon");
            if (prevIconTransform)
            {
                prevIconTransform.gameObject.SetActive(true);
                var equipmentKeyText = prevIconTransform.Find("EquipmentKeyText");
                if (equipmentKeyText)
                {
                    equipmentKeyText.gameObject.GetComponent<InputBindingDisplayController>().actionName = "VILIGERKEYBINDSWITCHEQUIPMENT";
                }
            }
        }

        private void EquipmentSlot_Start(On.RoR2.EquipmentSlot.orig_Start orig, EquipmentSlot self)
        {
            orig(self);
            self.gameObject.AddComponent<SwitchEquipmentInputHandler>().equipmentSlot = self;
        }

        private void Language_collectLanguageRootFolders(System.Collections.Generic.List<string> folders)
        {
            folders.Add(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(base.Info.Location), "Language"));
        }
    }
}
