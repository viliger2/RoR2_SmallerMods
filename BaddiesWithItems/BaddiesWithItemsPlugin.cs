using BepInEx;
using System;

[assembly: HG.Reflection.SearchableAttribute.OptInAttribute]
namespace BaddiesWithItems
{
    [BepInPlugin(GUID, ModName, Version)]
    public class BaddiesWithItemsPlugin : BaseUnityPlugin
    {
        public const string Author = "viliger";
        public const string ModName = "BaddiesWithItems";
        public const string Version = "1.0.0";
        public const string GUID = "com." + Author + "." + ModName;

        private void Awake()
        {
            Log.Init(Logger);
            Configuration.Init(Config);
            ItemAdder.Hooks();
            ItemDropper.Hooks();
        }
    }
}
