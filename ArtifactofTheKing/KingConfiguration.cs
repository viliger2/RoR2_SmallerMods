using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArtifactofTheKing
{
    public static class KingConfiguration
    {
        public static ConfigEntry<bool> EnableFixes;

        public static ConfigEntry<int> PrimStocks { get; set; }

        public static ConfigEntry<int> SecStocks { get; set; }

        public static ConfigEntry<int> UtilStocks { get; set; }
        
        public static ConfigEntry<int> SpecialStocks { get; set; }

        public static ConfigEntry<float> PrimCD { get; set; }

        public static ConfigEntry<float> SecCD { get; set; }

        public static ConfigEntry<float> UtilCD { get; set; }

        public static ConfigEntry<float> SpecialCD { get; set; }

        public static ConfigEntry<float> basehealth { get; set; }

        public static ConfigEntry<float> levelhealth { get; set; }

        public static ConfigEntry<float> basearmor { get; set; }

        public static ConfigEntry<float> baseattackspeed { get; set; }

        public static ConfigEntry<float> basedamage { get; set; }

        public static ConfigEntry<float> leveldamage { get; set; }

        public static ConfigEntry<float> basespeed { get; set; }

        public static ConfigEntry<float> mass { get; set; }

        public static ConfigEntry<float> turningspeed { get; set; }

        public static ConfigEntry<float> jumpingpower { get; set; }

        public static ConfigEntry<float> acceleration { get; set; }

        public static ConfigEntry<int> jumpcount { get; set; }

        public static ConfigEntry<float> aircontrol { get; set; }

        public static ConfigEntry<float> SlamOrbCount { get; set; }

        public static ConfigEntry<int> SecondaryFan { get; set; }

        public static ConfigEntry<int> UtilityShotgun { get; set; }

        public static ConfigEntry<int> LunarShardAdd { get; set; }

        public static ConfigEntry<int> UltimateWaves { get; set; }

        public static ConfigEntry<int> UltimateCount { get; set; }

        public static ConfigEntry<float> UltimateDuration { get; set; }

        public static ConfigEntry<int> clonecount { get; set; }

        public static ConfigEntry<int> cloneduration { get; set; }

        public static ConfigEntry<float> JumpRecast { get; set; }

        public static ConfigEntry<float> JumpPause { get; set; }

        public static ConfigEntry<float> ShardHoming { get; set; }

        public static ConfigEntry<float> ShardRange { get; set; }

        public static ConfigEntry<float> ShardCone { get; set; }

        public static void PopulateConfig(ConfigFile config)
        {
            EnableFixes = config.Bind<bool>("Update", "Enabled quote fixes unquote", true, "Enables fixes to what I (viliger) believe to be bugs. This ammounts to two more wave attacks as Mithix lands. Leave disabled if you want it to function as it was originally.");

            basehealth = config.Bind<float>("Stats", "BaseHealth", 1400f, "base health");
            levelhealth = config.Bind<float>("Stats", "LevelHealth", 420f, "level health");
            basedamage = config.Bind<float>("Stats", "BaseDamage", 16f, "base damage");
            leveldamage = config.Bind<float>("Stats", "LevelDamage", 3.2f, "level damage");
            basearmor = config.Bind<float>("Stats", "BaseArmor", 20f, "base armor");
            baseattackspeed = config.Bind<float>("Stats", "BaseAttackSpeed", 1f, "base attack speed");
            basespeed = config.Bind<float>("Movement", "BaseSpeed", 18f, "Mithrix's base movement speed");
            mass = config.Bind<float>("Movement", "Mass", 1200f, "mass, recommended to increase if you increase his movement speed");
            turningspeed = config.Bind<float>("Movement", "TurnSpeed", 900f, "how fast mithrix turns");
            jumpingpower = config.Bind<float>("Movement", "MoonShoes", 75f, "how hard mithrix jumps, vanilla is 25 for context");
            acceleration = config.Bind<float>("Movement", "Acceleration", 180f, "acceleration");
            jumpcount = config.Bind<int>("Movement", "JumpCount", 3, "jump count, probably doesn't do anything");
            aircontrol = config.Bind<float>("Movement", "Aircontrol", 1f, "air control");
            PrimStocks = config.Bind<int>("Skills", "PrimStocks", 2, "Max Stocks for Mithrix's Weapon Slam");
            SecStocks = config.Bind<int>("Skills", "SecondaryStocks", 2, "Max Stocks for Mithrix's Dash Attack");
            UtilStocks = config.Bind<int>("Skills", "UtilStocks", 4, "Max Stocks for Mithrix's Dash");
            SpecialStocks = config.Bind<int>("Skills", "SpecialStocks", 5, "Max Stocks for Mithrix's Sky Leap");
            PrimCD = config.Bind<float>("Skills", "PrimCD", 3f, "Cooldown for Mithrix's Weapon Slam");
            SecCD = config.Bind<float>("Skills", "SecCD", 3f, "Cooldown for Mithrix's Dash Attack");
            UtilCD = config.Bind<float>("Skills", "UtilCD", 1f, "Cooldown for Mithrix's Dash");
            SpecialCD = config.Bind<float>("Skills", "SpecialCD", 30f, "Cooldown for Mithrix's Jump Attack");
            SlamOrbCount = config.Bind<float>("Skillmods", "OrbCount", 16f, "Orbs fired by weapon slam in a circle, note, set this to an integer");
            SecondaryFan = config.Bind<int>("Skillmods", "FanCount", 5, "half the shards fired in a fan by the secondary skill");
            UtilityShotgun = config.Bind<int>("Skillmods", "ShotgunCount", 5, "shots fired in a shotgun by utility");
            LunarShardAdd = config.Bind<int>("Skillmods", "ShardAddCount", 5, "Bonus shards added to each shot of lunar shards");
            UltimateWaves = config.Bind<int>("Skillmods", "WavePerShot", 16, "waves fired by ultimate per shot");
            UltimateCount = config.Bind<int>("Skillmods", "WaveShots", 6, "Total shots of ultimate");
            UltimateDuration = config.Bind<float>("Skillmods", "WaveDuration", 5.5f, "how long ultimate lasts");
            clonecount = config.Bind<int>("Skillmods", "CloneCount", 2, "clones spawned in phase 3 by jump attack");
            cloneduration = config.Bind<int>("Skillmods", "CloneDuration", 30, "how long clones take to despawn (like happiest mask)");
            JumpRecast = config.Bind<float>("Skillmods", "RecastChance", 0f, "chance mithrix has to recast his jump skill. USE WITH CAUTION.");
            JumpPause = config.Bind<float>("Skillmods", "JumpDelay", 0.2f, "How long Mithrix spends in the air when using his jump special");
            ShardHoming = config.Bind<float>("Skillmods", "ShardHoming", 30f, "How strongly lunar shards home in to targets. Vanilla is 20f");
            ShardRange = config.Bind<float>("Skillmods", "ShardRange", 60f, "Range (distance) in which shards look for targets");
            ShardCone = config.Bind<float>("Skillmods", "ShardCone", 180f, "Cone (Angle) in which shards look for targets");
        }

    }
}
