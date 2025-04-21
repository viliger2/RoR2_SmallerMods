using EntityStates;
using EntityStates.ImpBossMonster;
using EntityStates.VoidRaidCrab.Weapon;
using RoR2;
using RoR2.Navigation;
using System.Linq;
using UnityEngine;

namespace FathomlessVoidling.VoidlingEntityStates.Phase1
{
    public class Transpose : BlinkState
    {
        public override void OnEnter()
        {
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
                HurtBoxGroup hurtboxGroup = this.hurtboxGroup;
                int num = hurtboxGroup.hurtBoxesDeactivatorCounter + 1;
                hurtboxGroup.hurtBoxesDeactivatorCounter = num;
            }
            if (rigidbodyMotor)
            {
                rigidbodyMotor.enabled = false;
            }
            inputBank.moveVector *= blinkDistance;
            CalculateBlinkDestination();
            CreateBlinkEffect(Util.GetCorePosition(gameObject));
        }

        private new void CreateBlinkEffect(Vector3 origin1)
        {
            Util.PlaySound(new FireMultiBeamFinale().enterSoundString, gameObject);
            EffectManager.SpawnEffect(FathomlessVoidling.deathBombPrefab, new EffectData
            {
                rotation = Util.QuaternionSafeLookRotation(blinkDestination - blinkStart),
                origin = origin1,
                scale = 75f
            }, false);
        }

        public override void FixedUpdate()
        {
            stopwatch += Time.fixedDeltaTime;
            if (stopwatch < (double)duration || !isAuthority)
            {
                return;
            }
            rigidbodyMotor.AddDisplacement(blinkDestination);
            outer.SetNextStateToMain();
        }

        private new void CalculateBlinkDestination()
        {
            Vector3 vector = Vector3.zero;
            Ray aimRay = GetAimRay();
            BullseyeSearch bullseyeSearch = new BullseyeSearch();
            bullseyeSearch.searchOrigin = aimRay.origin;
            bullseyeSearch.searchDirection = aimRay.direction;
            bullseyeSearch.maxDistanceFilter = blinkDistance;
            bullseyeSearch.teamMaskFilter = TeamMask.allButNeutral;
            bullseyeSearch.filterByLoS = false;
            bullseyeSearch.teamMaskFilter.RemoveTeam(TeamComponent.GetObjectTeam(gameObject));
            bullseyeSearch.sortMode = BullseyeSearch.SortMode.Angle;
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

        public override void OnExit()
        {
            Util.PlaySound(new BlinkState().endSoundString, gameObject);
            CreateBlinkEffect(characterBody.corePosition);
            modelTransform = GetModelTransform();
            if (modelTransform && new BlinkState().destealthMaterial)
            {
                TemporaryOverlay temporaryOverlay = animator.gameObject.AddComponent<TemporaryOverlay>();
                temporaryOverlay.duration = 1f;
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = new BlinkState().destealthMaterial;
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

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public new int blinkDistance = 500;

        public new float duration = 2f;

        private float stopwatch;
    }
}
