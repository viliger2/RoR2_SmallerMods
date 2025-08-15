using EntityStates;
using RoR2;
using RoR2.Navigation;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace FathomlessVoidling.VoidlingEntityStates
{
    public class Transpose : BaseState
    {
        public static GameObject deathBombPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathBombExplosion.prefab").WaitForCompletion();

        public static float duration = 2f;

        public static float blinkDistance = 500f;

        public static Material destealthMaterial = Addressables.LoadAssetAsync<Material>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Imp.matImpDissolve_mat).WaitForCompletion();

        private Transform modelTransform;

        private Animator animator;

        private CharacterModel characterModel;

        private HurtBoxGroup hurtboxGroup;

        private Vector3 blinkStart;

        private Vector3 blinkDestination;

        public override void OnEnter()
        {
            base.OnEnter();
            modelTransform = GetModelTransform();
            if (modelTransform)
            {
                animator = modelTransform.GetComponent<Animator>();
                characterModel = modelTransform.GetComponent<CharacterModel>();
                hurtboxGroup = modelTransform.GetComponent<HurtBoxGroup>();
            }
            if (characterModel)
            {
                characterModel.invisibilityCount++;
            }
            if (hurtboxGroup)
            {
                hurtboxGroup.hurtBoxesDeactivatorCounter++;
            }
            if (rigidbodyMotor)
            {
                rigidbodyMotor.enabled = false;
            }
            Util.PlaySound("Play_voidRaid_snipe_shoot_final", gameObject);
            if (isAuthority)
            {
                inputBank.moveVector *= blinkDistance;
                CalculateBlinkDestination();
                CreateBlinkEffect(Util.GetCorePosition(gameObject));
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= duration || isAuthority)
            {
                rigidbodyMotor.AddDisplacement(blinkDestination);
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            Util.PlaySound("Play_voidRaid_snipe_shoot_final", gameObject);
            if (isAuthority)
            {
                CreateBlinkEffect(characterBody.corePosition);
            }
            if (modelTransform && destealthMaterial)
            {
                TemporaryOverlay temporaryOverlay = animator.gameObject.AddComponent<TemporaryOverlay>();
                temporaryOverlay.duration = 1f;
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = destealthMaterial;
                temporaryOverlay.inspectorCharacterModel = animator.gameObject.GetComponent<CharacterModel>();
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay.animateShaderAlpha = true;
            }
            if (characterModel)
            {
                characterModel.invisibilityCount--;
            }
            if (hurtboxGroup)
            {
                HurtBoxGroup hurtboxGroup = this.hurtboxGroup;
                int num = hurtboxGroup.hurtBoxesDeactivatorCounter - 1;
                hurtboxGroup.hurtBoxesDeactivatorCounter = num;
            }
            if (rigidbodyMotor)
            {
                rigidbodyMotor.enabled = true;
            }
        }

        private void CalculateBlinkDestination()
        {
            Vector3 vector = Vector3.zero;
            Ray aimRay = GetAimRay();
            BullseyeSearch bullseyeSearch = new BullseyeSearch
            {
                searchOrigin = aimRay.origin,
                searchDirection = aimRay.direction,
                maxDistanceFilter = blinkDistance,
                teamMaskFilter = TeamMask.allButNeutral,
                filterByLoS = false,
                sortMode = BullseyeSearch.SortMode.Angle
            };
            bullseyeSearch.teamMaskFilter.RemoveTeam(TeamComponent.GetObjectTeam(gameObject));
            bullseyeSearch.RefreshCandidates();

            HurtBox hurtBox = bullseyeSearch.GetResults().FirstOrDefault();
            if (hurtBox)
            {
                vector = hurtBox.transform.position - transform.position;
            }
            blinkDestination = transform.position;
            blinkStart = transform.position;
            NodeGraph groundNodes = SceneInfo.instance.groundNodes;
            groundNodes.GetNodePosition(groundNodes.FindClosestNodeWithFlagConditions(vector, characterBody.hullClassification, NodeFlags.None, NodeFlags.NoCharacterSpawn, true), out blinkDestination);
        }

        private void CreateBlinkEffect(Vector3 origin1)
        {
            EffectManager.SpawnEffect(deathBombPrefab, new EffectData
            {
                rotation = Util.QuaternionSafeLookRotation(blinkDestination - blinkStart),
                origin = origin1,
                scale = 75f
            }, true);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
