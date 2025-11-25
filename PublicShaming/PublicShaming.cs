using BepInEx;
using RoR2;
using UnityEngine;

namespace PublicShaming
{
    [BepInPlugin(GUID, ModName, Version)]
    public class PublicShaming : BaseUnityPlugin
    {
        public const string Author = "Viliger";
        public const string ModName = "PublicShaming";
        public const string Version = "1.0.1";
        public const string GUID = "com." + Author + "." + ModName;

        public const string LanguageFolder = "Language";

        private void Awake()
        {
            RoR2.GlobalEventManager.OnInteractionsGlobal += GlobalEventManager_OnInteractionsGlobal;
            On.RoR2.ShrineRebirthController.StoreRebirthItemChoice += ShrineRebirthController_StoreRebirthItemChoice;
            RoR2.Language.collectLanguageRootFolders += Language_collectLanguageRootFolders;
        }

        private void ShrineRebirthController_StoreRebirthItemChoice(On.RoR2.ShrineRebirthController.orig_StoreRebirthItemChoice orig, ShrineRebirthController self, int itemChoice)
        {
            orig(self, itemChoice);
            if (!self.rebirthCandidate)
            {
                return;
            }

            SendChatMessage(self.rebirthCandidate, self.gameObject);
        }

        private void GlobalEventManager_OnInteractionsGlobal(Interactor interactor, IInteractable interactable, GameObject interactableObject)
        {
            if(interactable is TeleporterInteraction)
            {
                SendChatMessage(interactor, interactableObject);
            } else if(interactable is GenericInteraction)
            {
                if (!interactableObject.GetComponent<SceneExitController>())
                {
                    return;
                }

                SendChatMessage(interactor, interactableObject);
            }
        }

        private static void SendChatMessage(Interactor activator, GameObject gameObject)
        {
            string interactableName = gameObject.name;
            if (gameObject.TryGetComponent<IDisplayNameProvider>(out var provider))
            {
                interactableName = provider.GetDisplayName();
            }

            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = "PUBLIC_SHAMING_INTERACTABLE_ACTIVATED",
                paramTokens = new string[] { Util.GetBestBodyName(activator.gameObject), interactableName }
            });
        }

        private void Language_collectLanguageRootFolders(System.Collections.Generic.List<string> folders)
        {
            folders.Add(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(base.Info.Location), LanguageFolder));
        }
    }
}