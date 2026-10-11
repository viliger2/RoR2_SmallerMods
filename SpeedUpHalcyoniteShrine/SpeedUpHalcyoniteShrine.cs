using BepInEx;
using BepInEx.Configuration;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SpeedUpHalcyoniteShrine
{
    [BepInPlugin(GUID, ModName, Version)]
    public class SpeedUpHalcyoniteShrine : BaseUnityPlugin
    {
        public const string Author = "Viliger";
        public const string ModName = "SpeedUpHalcyoniteShrine";
        public const string Version = "1.0.2";
        public const string GUID = "com." + Author + "." + ModName;

        public static ConfigEntry<float> GoldTickRate;

        private void Awake()
        {
            GoldTickRate = Config.Bind("Gold Tick Rate", "Gold Tick Rate", 10f, "Tick rate (times gold gets drained per second) of the shrine. Vanilla values is 7, so 10 would make shrine charge twice as fast.");

            var shrineHalcyonite = Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC2.ShrineHalcyonite_prefab).WaitForCompletion();
            var interactable = shrineHalcyonite.GetComponent<HalcyoniteShrineInteractable>();
            interactable.tickRate = GoldTickRate.Value;
        }
    }
}