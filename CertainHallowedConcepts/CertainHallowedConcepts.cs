using BepInEx;
using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static RoR2.Navigation.NodeGraph;

namespace CertainHallowedConcepts
{
    [BepInPlugin(GUID, ModName, Version)]
    public class CertainHallowedConcepts : BaseUnityPlugin
    {
        public const string Author = "viliger";
        public const string ModName = "CertainHallowedConcepts";
        public const string Version = "1.0.0";
        public const string GUID = "com." + Author + "." + ModName;

        public static ConfigEntry<bool> WakeEnableFlowerAdvancement;
        public static ConfigEntry<float> SacrificeHealthFraction;
        public static ConfigEntry<float> SacrificeHoldoutPercent;

        public static ConfigEntry<bool> WakeDropsSacrificeOnAllSouls;

        public static ConfigEntry<bool> GongSpawnPortalOnHitter;

        public static ConfigEntry<bool> GuaranteedWake;
        public static ConfigEntry<float> GuaranteedWakePercentPerStage;

        public static ConfigEntry<bool> CleansedHexesExtendTrialTimer;
        public static ConfigEntry<float> CleansedHexesBonusSeconds;

        private void Awake()
        {
            Log.Init(Logger);
            InitiateConfigValue();

            if (WakeEnableFlowerAdvancement.Value)
            {
                On.RoR2.WakeDonationController.DonatePeaceFlowersServer += WakeDonationController_DonatePeaceFlowersServer;
            }
            if (WakeDropsSacrificeOnAllSouls.Value)
            {
                AddDrops(Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC4_Wake.WakeAltar_prefab).WaitForCompletion());
                AddDrops(Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC4_Wake.WakeAltarPage_prefab).WaitForCompletion());
            }
            if (GongSpawnPortalOnHitter.Value)
            {
                IL.RoR2.TrialGongInteraction.OnIncomingDamageServer += TrialGongInteraction_OnIncomingDamageServer;
            }
            if (GuaranteedWake.Value)
            {
                RoR2.Run.onRunStartGlobal += Run_onRunStartGlobal;
            }

            if (CleansedHexesExtendTrialTimer.Value)
            {
                On.EntityStates.HallowedCauldron.DrainingItems.OnExit += DrainingItems_OnExit;
            }
        }

        private void DrainingItems_OnExit(On.EntityStates.HallowedCauldron.DrainingItems.orig_OnExit orig, EntityStates.HallowedCauldron.DrainingItems self)
        {
            orig(self);
            if(!DLC4ProgressionController.instance || !DLC4ProgressionController.instance.trialSelector)
            {
                return;
            }

            if (self.itemsTaken == 0)
            {
                return;
            }

            var timer = DLC4ProgressionController.instance.trialSelector.GetRemainingTrialTimeServer();
            DLC4ProgressionController.instance.trialSelector.SetRemainingTrialTimeServer(timer + self.itemsTaken * CleansedHexesBonusSeconds.Value);
        }

        private void Run_onRunStartGlobal(Run obj)
        {
            obj.gameObject.AddComponent<GuaranteedWakeController>();
        }

        private void TrialGongInteraction_OnIncomingDamageServer(MonoMod.Cil.ILContext il)
        {
            ILCursor c1 = new ILCursor(il);
            var match1 = c1.TryGotoNext(MoveType.After,
                x => x.MatchDup());

            ILCursor c = new ILCursor(il);

            ILLabel label = c.DefineLabel();
            var match2 = c.TryGotoNext(MoveType.Before,
                x => x.MatchLdarg(out _),
                x => x.MatchCallvirt<EntityStates.Interactables.TrialGong.BaseTrialGongState>("OnStruck"));
            if (match1 && match2)
            {
                c.MarkLabel(label);
                c.Emit(OpCodes.Ldarg, 0); // self
                c.Emit(OpCodes.Ldarg, 1); // damageInfo
                c.EmitDelegate <System.Action<TrialGongInteraction, DamageInfo>>(AdjustSpawnPosition);

                c1.Emit(OpCodes.Brtrue, label);
                c1.Emit(OpCodes.Dup);
            } else
            {
                Log.Warning($"IL Hook Failed - RoR2.TrialGongInteraction.OnIncomingDamageServer: Gong teleporter won't spawn on attacker's position.");
            }

        }

        private void AdjustSpawnPosition(TrialGongInteraction interaction, DamageInfo damageInfo)
        {
            if(!SceneInfo.instance || !SceneInfo.instance.groundNodes)
            {
                return;
            }

            if (!damageInfo.attacker.TryGetComponent<CharacterBody>(out var body))
            {
                return;
            }

            var corePosition = body.corePosition;
            var groundNodes = SceneInfo.instance.groundNodes;
            var nodeIndex = groundNodes.FindClosestNode(corePosition, HullClassification.Human, 50f);
            if(nodeIndex == NodeIndex.invalid)
            {
                return;
            }
            var node = groundNodes.nodes[nodeIndex.nodeIndex];
            // holy shit this is garbage but its better than adding like 3 more hooks
            var gameObject = new GameObject();
            gameObject.name = "CertainHallowedConcepts_GongPortalLocation";
            gameObject.transform.position = node.position;
            interaction.preferredSpawnPosition = gameObject.transform;
        }


        private void WakeDonationController_DonatePeaceFlowersServer(On.RoR2.WakeDonationController.orig_DonatePeaceFlowersServer orig, RoR2.WakeDonationController self, RoR2.Interactor donator)
        {
            if (!WakeController.instance || !WakeController.instance.holdoutZoneController)
            {
                orig(self, donator);
                return;
            }

            int flowerCount = 0;
            float healthFraction = 0f;
            if(WakeController.instance && donator.TryGetComponent<CharacterBody>(out var charBody) && charBody.inventory)
            {
                flowerCount = charBody.inventory.GetItemCountEffective(DLC4Content.Items.HelpWake);
            }
            if(self.instanceData.isValid && self.instanceData.isAlive)
            {
                healthFraction = self.instanceData.body.healthComponent.healthFraction;
            }

            orig(self, donator);

            if(flowerCount == 0)
            {
                return;
            }

            if(healthFraction < SacrificeHealthFraction.Value)
            {
                return;
            }

            var charge = WakeController.instance.holdoutZoneController.charge + (Mathf.Clamp(SacrificeHoldoutPercent.Value, 0, 100f) / 100f);
            WakeController.instance.holdoutZoneController.RaiseCharge(charge);
        }

        private void AddDrops(GameObject prefab)
        {
            var component = prefab.AddComponent<DropSacrificePage>();
            component.itemToDrop = Addressables.LoadAssetAsync<ItemDef>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC4_CursedBook.PageDie_asset).WaitForCompletion();

            var eventCaller = prefab.GetComponent<WakeEventCaller>();
            eventCaller.onWakeSuccessServer.AddPersistentListener(component.DropItemServer);
        }

        private void InitiateConfigValue()
        {
            WakeEnableFlowerAdvancement = Config.Bind("Ancient Sepulcher", "Enable Flowers Advancing Charging", true, "Donating flowers to Soul Conduit will advance charge zone by configured ammount.");
            SacrificeHealthFraction = Config.Bind("Ancient Sepulcher", "Soul Conduit Health Fraction", 0.9f, "At what health fraction Soul Conduit has to be to give to charge zone.");
            SacrificeHoldoutPercent = Config.Bind("Ancient Sepulcher", "Soul Conduit Holdout Fraction Charge", 10f, "How much Ancient Sepulcher hold out zone gains from giving Soul Conduit flower.");

            WakeDropsSacrificeOnAllSouls = Config.Bind("Ancient Sepulcher", "Drop Reincarnation on All Souls Alive", true, "Ancient Sepulcher drops guaranteed Reincarnation page if it is charged with all Soul Conduits alive.");

            GongSpawnPortalOnHitter = Config.Bind("Ritual Gong", "Ritual Gong Spawns Portal on Interactor", true, "Ritual Gong spawns its portal on interactor instead of preditermined locations in front of it. This allow for hitting the gong to be actually useful.");

            GuaranteedWake = Config.Bind("Ancient Sepulcher", "Guaranteed Ancient Sepulcher within first four stages", true, "Enables guaranteed Ancient Sepulcher within first four stages, resets on loop, doesn't spawn if one spawns naturally.");
            GuaranteedWakePercentPerStage = Config.Bind("Ancient Sepulcher", "Guaranteed Ancient Sepulcher Spawn Chance Per Stage", 25f, "Spawn chance of Ancient Sepulcher per stage. Chances are rolled only on stages 1-4 and reset on loop. If one spawns naturally then chances are no longer rolled on this loop.");

            CleansedHexesExtendTrialTimer = Config.Bind("Hexes", "Cleansed Hexes Add To Trial Timer", true, "Hexes removed by Life hand give additional seconds to trial timer.");
            CleansedHexesBonusSeconds = Config.Bind("Hexes", "Bonus Seconds From Cleansed Hexes", 5f, "How many seconds each cleansed Hex gives to Trial timer.");
        }

    }
}