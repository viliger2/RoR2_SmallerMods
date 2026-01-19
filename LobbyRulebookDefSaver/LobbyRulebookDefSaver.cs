using BepInEx;
using BepInEx.Configuration;
using RoR2;
using RoR2.UI;
using System;
using UnityEngine;
using UnityEngine.Events;
using static RoR2.PreGameRuleVoteController;

namespace LobbyRulebookDefSaver
{
    [BepInPlugin(GUID, ModName, Version)]
    public class LobbyRulebookDefSaver : BaseUnityPlugin
    {
        public const string Author = "viliger";
        public const string ModName = "LobbyRulebookDefSaver";
        public const string Version = "1.0.0";
        public const string GUID = "com." + Author + "." + ModName;

        private static ConfigEntry<string> SavedRulebookRules;
        private static ConfigEntry<bool> SaveOnRuleChanges;
        private static ConfigEntry<bool> AddSaveButton;

        private void Awake()
        {
            SavedRulebookRules = Config.Bind<string>("Rulebook Rules", "Saved Rulebook Rules", "", "Saved Rulebook rules. It is not recommended to modify this value manually.");
            SaveOnRuleChanges = Config.Bind("Rulebook Rules", "Save on Rulebook Changes", false, "Saves Rulebook rules to config on any changes to rules.");
            AddSaveButton = Config.Bind("Rulebook Rules", "Add Save to Config button", true, "Adds Save to config button that allows for manual rulebook saving to config.");

            On.RoR2.PreGameRuleVoteController.LocalUserBallotPersistenceManager.OnLocalUserSignIn += LocalUserBallotPersistenceManager_OnLocalUserSignIn;
            if (SaveOnRuleChanges.Value)
            {
                On.RoR2.PreGameRuleVoteController.LocalUserBallotPersistenceManager.OnVotesUpdated += LocalUserBallotPersistenceManager_OnVotesUpdated;
            }
            if (AddSaveButton.Value)
            {
                On.RoR2.UI.CharacterSelectController.Awake += CharacterSelectController_Awake;
            }
            RoR2.Language.collectLanguageRootFolders += Language_collectLanguageRootFolders;

        }

        private void Language_collectLanguageRootFolders(System.Collections.Generic.List<string> folders)
        {
            folders.Add(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(base.Info.Location), "Language"));
        }

        private void CharacterSelectController_Awake(On.RoR2.UI.CharacterSelectController.orig_Awake orig, RoR2.UI.CharacterSelectController self)
        {
            // shamelessly copypasted from ProperSave
            try
            {
                var randomButton = self.transform.Find("SafeArea/RightHandPanel/PopoutPanelContainer/PopoutPanelPrefab/Canvas/Main/RandomButtonContainer/RandomButton");
                var quitButton = self.transform.Find("SafeArea/FooterPanel/NakedButton (Quit)");
                var lobbyButton = GameObject.Instantiate(quitButton, randomButton.transform.parent).gameObject;

                foreach (var filter in self.GetComponents<InputSourceFilter>())
                {
                    if (filter.requiredInputSource == MPEventSystem.InputSource.MouseAndKeyboard)
                    {
                        Array.Resize(ref filter.objectsToFilter, filter.objectsToFilter.Length + 1);
                        filter.objectsToFilter[filter.objectsToFilter.Length - 1] = lobbyButton;
                        break;
                    }
                }

                lobbyButton.name = "LobbyRulebookDefSaver_SaveToConfig";

                var tooltipProvider = lobbyButton.AddComponent<TooltipProvider>();

                var rectTransform = lobbyButton.GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(1F, 1.5F);
                rectTransform.anchorMax = new Vector2(1F, 1.5F);

                var buttonComponent = lobbyButton.GetComponent<HGButton>();
                buttonComponent.hoverToken = "LOBBYRULEBOOKDEFSAVER_SAVEBUTTON_HOVER";

                var languageComponent = lobbyButton.GetComponent<LanguageTextMeshController>();
                languageComponent.token = "LOBBYRULEBOOKDEFSAVER_SAVEBUTTON_DESC";

                buttonComponent.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                AddPersistentListener(buttonComponent.onClick, SaveToConfigInputEvent);
            }
            catch (Exception e)
            {
                Logger.LogWarning("Couldn't add SaveToConfig button.");
                Logger.LogError(e);
            }
            orig(self);
        }

        private void LocalUserBallotPersistenceManager_OnLocalUserSignIn(On.RoR2.PreGameRuleVoteController.LocalUserBallotPersistenceManager.orig_OnLocalUserSignIn orig, LocalUser localUser)
        {
            orig(localUser);

            var strings = SavedRulebookRules.Value.Split(';', StringSplitOptions.RemoveEmptyEntries);
            if (RoR2.PreGameRuleVoteController.LocalUserBallotPersistenceManager.votesCache[localUser] == null)
            {
                RoR2.PreGameRuleVoteController.LocalUserBallotPersistenceManager.votesCache[localUser] = PreGameRuleVoteController.CreateBallot();
            }

            foreach (var ruleChoiceDefName in strings)
            {
                var ruleChoiceDef = RuleCatalog.FindChoiceDef(ruleChoiceDefName);
                if (ruleChoiceDef != null)
                {
                    var ruleDef = ruleChoiceDef.ruleDef;
                    RoR2.PreGameRuleVoteController.LocalUserBallotPersistenceManager.votesCache[localUser][ruleDef.globalIndex].choiceValue = ruleChoiceDef.localIndex;
                }
            }

        }

        private void SaveToConfigInputEvent()
        {
            SaveToConfig();
        }

        private void LocalUserBallotPersistenceManager_OnVotesUpdated(On.RoR2.PreGameRuleVoteController.LocalUserBallotPersistenceManager.orig_OnVotesUpdated orig)
        {
            orig();

            SaveToConfig();
        }

        private void SaveToConfig()
        {
            string result = "";

            foreach (var whatever in RoR2.PreGameRuleVoteController.LocalUserBallotPersistenceManager.votesCache)
            {
                var rows = whatever.Value;
                if (rows != null)
                {
                    for (int j = 0; j < RuleCatalog.ruleCount; j++)
                    {
                        RuleDef ruleDef = RuleCatalog.GetRuleDef(j);
                        int count = ruleDef.choices.Count;

                        Vote vote = rows[j];
                        if (vote.hasVoted && vote.choiceValue < count)
                        {
                            RuleChoiceDef ruleChoiceDef = ruleDef.choices[vote.choiceValue];
                            result += ruleChoiceDef.globalName + ";";
                        }
                    }
                }
            }

            SavedRulebookRules.Value = result;
        }

        public static void AddPersistentListener(UnityEvent unityEvent, UnityAction action)
        {
            unityEvent.m_PersistentCalls.AddListener(new PersistentCall
            {
                m_Target = action.Target as UnityEngine.Object,
                m_TargetAssemblyTypeName = UnityEventTools.TidyAssemblyTypeName(action.Method.DeclaringType.AssemblyQualifiedName),
                m_MethodName = action.Method.Name,
                m_CallState = UnityEventCallState.RuntimeOnly,
                m_Mode = PersistentListenerMode.EventDefined,
            });
        }
    }
}
