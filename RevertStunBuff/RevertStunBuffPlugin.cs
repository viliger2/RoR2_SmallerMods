using BepInEx;
using EntityStates;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace RevertStunBuff
{
    [BepInPlugin(GUID, ModName, Version)]
    public class RevertStunBuffPlugin : BaseUnityPlugin
    {
        public const string Author = "Viliger";
        public const string ModName = "RevertStunBuff";
        public const string Version = "1.0.0";
        public const string GUID = "com." + Author + "." + ModName;

        private void Awake()
        {
            IL.RoR2.SetStateOnHurt.SetStunInternal += SetStateOnHurt_SetStunInternal;
            //On.EntityStates.StunState.ExtendStun += StunState_ExtendStun;
            //On.EntityStates.StunState.OnExit += StunState_OnExit;
        }

        //private void StunState_OnExit(On.EntityStates.StunState.orig_OnExit orig, StunState self)
        //{
        //    orig(self);
        //    Logger.LogMessage($"fixedAge: {self.fixedAge}, we left stun");
        //}

        //private void StunState_ExtendStun(On.EntityStates.StunState.orig_ExtendStun orig, StunState self, float durationDelta)
        //{
        //    Logger.LogMessage($"fixedAge: {self.fixedAge}, pre duration: {self.duration}, durationDelta: {durationDelta}");
        //    orig(self, durationDelta);
        //    Logger.LogMessage($"fixedAge: {self.fixedAge}, post duration: {self.duration}, durationDelta: {durationDelta}");
        //}

        private void SetStateOnHurt_SetStunInternal(MonoMod.Cil.ILContext il)
        {
            ILLabel test1 = null;
            ILCursor c = new ILCursor(il);
            if(c.TryGotoNext(MoveType.Before,
                x => x.MatchBrfalse(out test1)))
            {
                // do nothing lmoa
            } else
            {
                Logger.LogWarning("Failed to IL hook RoR2::SetStateOnHurt::SetStunInternal: couldn't get jump index.");
            }
            c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdarg(out _),
                x => x.MatchLdfld<RoR2.SetStateOnHurt>("targetStateMachine"),
                x => x.MatchCallvirt<RoR2.EntityStateMachine>("get_state"),
                x => x.MatchIsinst<EntityStates.StunState>(),
                x => x.MatchLdarg(out _)))
            {
                c.Emit(OpCodes.Ldarg_0); // self
                c.Emit(OpCodes.Ldarg_1); // stun duration
                c.EmitDelegate<Action<RoR2.SetStateOnHurt, float>>((self, stunDuration) =>
                {
                    StunState stunState = self.targetStateMachine.state as StunState;
                    if(stunState.timeRemaining < stunDuration)
                    {
                        stunState.ExtendStun(stunDuration - stunState.timeRemaining);
                    }
                });
                c.Emit(OpCodes.Br_S, test1);
            }
            else
            {
                Logger.LogWarning("Failed to IL hook RoR2::SetStateOnHurt::SetStunInternal: couldn't apply stun mod.");
            }
        }
    }
}
