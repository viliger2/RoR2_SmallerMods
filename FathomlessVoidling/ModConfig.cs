using BepInEx.Configuration;

namespace FathomlessVoidling
{
    internal class ModConfig
    {
        public static void InitConfig(ConfigFile config)
        {
            ModConfig.enableAltMoon = config.Bind<bool>("General", "Alt Moon", true, "Toggle Void Locus as an alternative to the moon. Adds stage 5 Locus portal and item cauldrons.");
            ModConfig.enableVoidFog = config.Bind<bool>("General", "Pillar Fog", false, "Toggle void pillar fog.");
            ModConfig.baseHealth = config.Bind<float>("Stats", "Base Health", 1250f, "Vanilla: 2000");
            ModConfig.levelHealth = config.Bind<float>("Stats", "Level Health", 350f, "Health gained per level. Vanilla: 600");
            ModConfig.baseDamage = config.Bind<float>("Stats", "Base Damage", 15f, "Vanilla: 15");
            ModConfig.levelDamage = config.Bind<float>("Stats", "Level Damage", 3f, "Damage gained per level. Vanilla: 3");
            ModConfig.baseArmor = config.Bind<float>("Stats", "Base Armor", 30f, "Vanilla: 20");
            ModConfig.baseAtkSpd = config.Bind<float>("Stats", "Base Attack Speed", 1.25f, "Vanilla: 1");
            ModConfig.baseSpd = config.Bind<float>("Stats", "Base Move Speed", 90f, "Vanilla: 45");
            ModConfig.acceleration = config.Bind<float>("Stats", "Acceleration", 45f, "Vanilla: 20");
            ModConfig.primCD = config.Bind<int>("Skills", "Primary Cooldown", 10, "Cooldown for Disillusion (Main missile attack).");
            ModConfig.secCD = config.Bind<int>("Skills", "Secondary Cooldown", 40, "Cooldown for Secondary (Vacuum, Singularity, Crush).");
            ModConfig.utilCD = config.Bind<int>("Skills", "Util Cooldown", 20, "Cooldown for Transpose (Blink).");
            ModConfig.specCD = config.Bind<int>("Skills", "Special Cooldown", 30, "Cooldown for Special (Rend, SpinBeam, Reap).");
            ModConfig.Phase2Vacuum = config.Bind<bool>("Skills", "Phase 2 Singularity Changes", true, "Enables changes to Vaccum/Singularity attack in P2 so it spawns in the center of the arena instead of under Voidling.");

            if (ModCompat.RiskOfOptionsCompat.enabled)
            {
                ModCompat.RiskOfOptionsCompat.CreateNewOption(ModConfig.enableAltMoon);
                ModCompat.RiskOfOptionsCompat.CreateNewOption(ModConfig.enableVoidFog);

                ModCompat.RiskOfOptionsCompat.CreateNewOption(baseHealth, 1000f, 2000f, 50f);
                ModCompat.RiskOfOptionsCompat.CreateNewOption(levelHealth, 100f, 500f, 25f);
                ModCompat.RiskOfOptionsCompat.CreateNewOption(baseDamage, 10f, 20f, 0.5f);
                ModCompat.RiskOfOptionsCompat.CreateNewOption(levelDamage, 1f, 6f, 0.25f);
                ModCompat.RiskOfOptionsCompat.CreateNewOption(baseArmor, 20f, 60f, 5f);
                ModCompat.RiskOfOptionsCompat.CreateNewOption(baseAtkSpd, 0.5f, 2f, 0.25f);
                ModCompat.RiskOfOptionsCompat.CreateNewOption(baseSpd, 45f, 135f, 5f);
                ModCompat.RiskOfOptionsCompat.CreateNewOption(acceleration, 20f, 70f, 5f);

                ModCompat.RiskOfOptionsCompat.CreateNewOption(primCD, 1, 10);
                ModCompat.RiskOfOptionsCompat.CreateNewOption(secCD, 30, 50);
                ModCompat.RiskOfOptionsCompat.CreateNewOption(utilCD, 10, 30);
                ModCompat.RiskOfOptionsCompat.CreateNewOption(specCD, 20, 40);

                ModCompat.RiskOfOptionsCompat.SetDescription();
            }
        }

        public static ConfigEntry<bool> enableAltMoon;

        public static ConfigEntry<bool> enableVoidFog;

        public static ConfigEntry<float> baseHealth;

        public static ConfigEntry<float> levelHealth;

        public static ConfigEntry<float> baseDamage;

        public static ConfigEntry<float> levelDamage;

        public static ConfigEntry<float> baseArmor;

        public static ConfigEntry<float> baseAtkSpd;

        public static ConfigEntry<float> baseSpd;

        public static ConfigEntry<float> acceleration;

        public static ConfigEntry<int> primCD;

        public static ConfigEntry<int> secCD;

        public static ConfigEntry<int> utilCD;

        public static ConfigEntry<int> specCD;

        public static ConfigEntry<bool> Phase2Vacuum;
    }
}
