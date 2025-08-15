using UnityEngine;
using UnityEngine.AddressableAssets;

namespace FathomlessVoidling.VoidlingEntityStates.Phase2
{
    public class Disillusion : BaseDisillusion
    {
        public static GameObject staticBombPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Nullifier/NullifierDeathBombProjectile.prefab").WaitForCompletion();

        internal override GameObject bombPrefab => staticBombPrefab;
    }
}
