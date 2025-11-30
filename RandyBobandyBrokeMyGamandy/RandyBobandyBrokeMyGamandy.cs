using BepInEx;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

[assembly: HG.Reflection.SearchableAttribute.OptInAttribute]
namespace RoR_RandyBobandyBrokeMyGamendy
{
    [BepInPlugin("com.Viliger.RandyBobandyBrokeMyGamandy", "RandyBobandyBrokeMyGamandy", "1.0.1")]
    [BepInIncompatibility("com.Wolfo.WolfFixes")]
    public class RandyBobandyBrokeMyGamandy : BaseUnityPlugin
    {
        public void Awake()
        {
            // RANDYYYYYYYYYYYYYYYYY
            var grandparentBoulder = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Grandparent/GrandparentMiniBoulderGhost.prefab").WaitForCompletion();
            var meshFilter = grandparentBoulder.GetComponentInChildren<MeshFilter>();
            if (meshFilter)
            {
                meshFilter.mesh = Addressables.LoadAssetAsync<Mesh>("RoR2/Base/blackbeach/mdlBBBoulderMediumRound1.fbx").WaitForCompletion();
                var rockTransform = grandparentBoulder.transform.Find("Rotator/RockMesh").transform;
                rockTransform.localScale *= 2.5f;
            }
        }
    }
}
