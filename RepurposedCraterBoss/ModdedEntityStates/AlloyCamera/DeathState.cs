using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RepurposedCraterBoss.ModdedEntityStates.AlloyCamera
{
    public class DeathState : BaseState
    {
        public static GameObject shockEffectPrefab = Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_Captain.CaptainTazerSupplyDropNova_prefab).WaitForCompletion();

        public static float shockRadius = 10f;

        public override void OnEnter()
        {
            base.OnEnter();

            Shock();

            if (modelLocator)
            {
                if (modelLocator.modelBaseTransform)
                {
                    EntityState.Destroy(modelLocator.modelBaseTransform.gameObject);
                }
                if (modelLocator.modelTransform)
                {
                    EntityState.Destroy(modelLocator.modelTransform.gameObject);
                }
            }
            EntityState.Destroy(gameObject);
        }

        private void Shock()
        {
            BlastAttack blastAttack = new BlastAttack();
            blastAttack.radius = shockRadius;
            blastAttack.baseDamage = 0f;
            blastAttack.damageType = DamageType.Silent | DamageType.Shock5s;
            blastAttack.falloffModel = BlastAttack.FalloffModel.None;
            blastAttack.attacker = null;
            blastAttack.teamIndex = TeamIndex.Player;
            blastAttack.position = base.transform.position;
            blastAttack.Fire();
            if ((bool)shockEffectPrefab)
            {
                EffectManager.SpawnEffect(shockEffectPrefab, new EffectData
                {
                    origin = base.transform.position,
                    scale = shockRadius
                }, transmit: false);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}
