using UnityEngine;
using UnityEngine.AddressableAssets;

namespace FathomlessVoidling.VoidlingEntityStates.Phase3
{
    public class Disillusion : BaseDisillusion
    {
        public static GameObject staticBombPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidJailer/VoidJailerDeathBombProjectile.prefab").WaitForCompletion();

        internal override GameObject bombPrefab => staticBombPrefab;
    }
}
