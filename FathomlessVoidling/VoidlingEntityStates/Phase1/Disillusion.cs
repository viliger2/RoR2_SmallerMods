using UnityEngine;
using UnityEngine.AddressableAssets;

namespace FathomlessVoidling.VoidlingEntityStates.Phase1
{
    public class Disillusion : BaseDisillusion
    {
        public static GameObject staticBombPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathBombProjectile.prefab").WaitForCompletion(); // TODO

        internal override GameObject bombPrefab => staticBombPrefab;
    }
}
