using BepInEx;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

[assembly: HG.Reflection.SearchableAttribute.OptInAttribute]
namespace RoR2_PrintersIcon
{
    [BepInPlugin("com.Viliger.ActualPrinterIcon", "ActualPrinterIcon", "1.0.2")]
    public class ActualPrinterIcon : BaseUnityPlugin
    {

        public void Awake()
        {
            AssetBundle bundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Info.Location), "actualprintericon"));

            Sprite sprite = bundle.LoadAsset<Sprite>("texDuplicatorIconOutline");

            AddPingInfoOverride(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Duplicator/Duplicator.prefab").WaitForCompletion(), sprite);
            AddPingInfoOverride(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/DuplicatorLarge/DuplicatorLarge.prefab").WaitForCompletion(), sprite);
            AddPingInfoOverride(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/DuplicatorMilitary/DuplicatorMilitary.prefab").WaitForCompletion(), sprite);
            AddPingInfoOverride(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/DuplicatorWild/DuplicatorWild.prefab").WaitForCompletion(), sprite);
        }

        private void AddPingInfoOverride(GameObject duplicator, Sprite sprite)
        {
            if (duplicator)
            {
                if (!duplicator.TryGetComponent<PingInfoProvider>(out var pingInfo))
                {
                    pingInfo = duplicator.AddComponent<PingInfoProvider>();
                };
                pingInfo.pingIconOverride = sprite;
            }
        }
    }
}
