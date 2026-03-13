using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace BaddiesWithItems
{
    public static class ConsoleCommands
    {
        [ConCommand(commandName = "bwi_regenerate_blacklist", flags = ConVarFlags.SenderMustBeServer, helpText = "Regenerates blacklist from config")]
        private static void CCRegenerateBlacklist(ConCommandArgs args)
        {
            ItemAdder.RegenerateBlacklist();
        }
    }
}
