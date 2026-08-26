using System;
using System.Collections.Generic;
using UnityEngine;

namespace MessengerRando.Data;

public static class LevelData
{
    /// <summary>
    /// Represents a destination level for teleportation. Should either contain a LevelEntrance or a PlayerPos, but not both.
    /// Dimension is optional, only present when a specific dimension need to be enforced in the destination level.
    /// </summary>
    public readonly struct DestinationLevel : IEquatable<DestinationLevel>, IEquatable<LevelExit>
    {
        public ELevel LevelName { get; }
        public ELevelEntranceID LevelEntrance { get; }
        public Vector3 PlayerPos { get; }
        public EBits Dimension { get; }
        public Type EntranceCutscene { get; }

        public DestinationLevel(
            ELevel levelName,
            ELevelEntranceID levelEntrance,
            EBits dimension = EBits.NONE,
            Type entranceCutscene = null
        )
        {
            LevelName = levelName;
            LevelEntrance = levelEntrance;
            PlayerPos = Vector3.zero;
            Dimension = dimension;
            EntranceCutscene = entranceCutscene;
        }

        public DestinationLevel(ELevel levelName, Vector3 playerPos, EBits dimension = EBits.NONE)
        {
            LevelName = levelName;
            LevelEntrance = ELevelEntranceID.NONE;
            PlayerPos = playerPos;
            Dimension = dimension;
            EntranceCutscene = null;
        }

        public LevelExit AsLevelExit()
        {
            return new LevelExit(LevelName, LevelEntrance);
        }

        public bool Equals(DestinationLevel other)
        {
            return LevelName == other.LevelName && PlayerPos.Equals(other.PlayerPos);
        }

        public bool Equals(LevelExit other)
        {
            return LevelName == other.NextLevel && LevelEntrance.Equals(other.EntranceID);
        }

        public bool Equals(ELevel level)
        {
            return LevelName == level;
        }

        public override bool Equals(object obj)
        {
            return obj is DestinationLevel other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 27;
                hash = (hash * 13) + LevelName.GetHashCode();
                hash = (hash * 13) + LevelEntrance.GetHashCode();
                hash = (hash * 13) + PlayerPos.GetHashCode();
                hash = (hash * 13) + Dimension.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            if (LevelEntrance == ELevelEntranceID.NONE)
                return $"{LevelName} {PlayerPos}" + (Dimension == EBits.NONE ? "" : $" {Dimension}");

            return $"{LevelName} {LevelEntrance}" + (Dimension == EBits.NONE ? "" : $" {Dimension}");
        }
    }

    /// <summary>
    /// Represents the exit of a level by its vanilla destination.
    /// </summary>
    public readonly struct LevelExit(ELevel nextLevel, ELevelEntranceID entranceID)
        : IEquatable<LevelExit>,
            IEquatable<DestinationLevel>
    {
        public ELevel NextLevel { get; } = nextLevel;
        public ELevelEntranceID EntranceID { get; } = entranceID;

        public bool Equals(LevelExit other)
        {
            return NextLevel == other.NextLevel && EntranceID == other.EntranceID;
        }

        public bool Equals(DestinationLevel other)
        {
            return NextLevel == other.LevelName && EntranceID == other.LevelEntrance;
        }

        public override bool Equals(object obj)
        {
            return obj is LevelExit other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 27;
                hash = (hash * 13) + NextLevel.GetHashCode();
                hash = (hash * 13) + EntranceID.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return $"{NextLevel} {EntranceID}";
        }
    }

    public static readonly Dictionary<string, DestinationLevel> EntranceNameToDestinationLevel = new()
    {
        { "Ninja Village - Left", new DestinationLevel(ELevel.Level_01_NinjaVillage, ELevelEntranceID.ENTRANCE_A) }, // Used when after the first quest I assume?
        { "Ninja Village - Right", new DestinationLevel(ELevel.Level_01_NinjaVillage, ELevelEntranceID.ENTRANCE_B) },
        { "Autumn Hills - Left", new DestinationLevel(ELevel.Level_02_AutumnHills, ELevelEntranceID.ENTRANCE_A) },
        { "Autumn Hills - Right", new DestinationLevel(ELevel.Level_02_AutumnHills, ELevelEntranceID.ENTRANCE_B) },
        {
            "Autumn Hills - Portal",
            new DestinationLevel(ELevel.Level_02_AutumnHills, ELevelEntranceID.ENTRANCE_C, EBits.BITS_16)
        },
        {
            "Autumn Hills - Bottom",
            new DestinationLevel(ELevel.Level_02_AutumnHills, ELevelEntranceID.ENTRANCE_D, EBits.BITS_16)
        },
        { "Forlorn Temple - Left", new DestinationLevel(ELevel.Level_03_ForlornTemple, ELevelEntranceID.ENTRANCE_A) },
        {
            "Forlorn Temple - Right",
            new DestinationLevel(ELevel.Level_03_ForlornTemple, ELevelEntranceID.ENTRANCE_B, EBits.BITS_16)
        },
        {
            "Forlorn Temple - Bottom",
            new DestinationLevel(
                ELevel.Level_03_ForlornTemple,
                ELevelEntranceID.ENTRANCE_C,
                EBits.BITS_16,
                entranceCutscene: typeof(ClimbUpFromCatacombsCutscene)
            )
        },
        {
            "Catacombs - Top Left",
            new DestinationLevel(
                ELevel.Level_04_Catacombs,
                ELevelEntranceID.ENTRANCE_A,
                entranceCutscene: typeof(CatacombEntranceFromForlorn)
            )
        },
        { "Catacombs - Right", new DestinationLevel(ELevel.Level_04_Catacombs, ELevelEntranceID.ENTRANCE_B) },
        {
            "Catacombs - Bottom",
            new DestinationLevel(ELevel.Level_04_Catacombs, ELevelEntranceID.ENTRANCE_C, EBits.BITS_16)
        },
        {
            "Catacombs - Bottom Left",
            new DestinationLevel(ELevel.Level_04_Catacombs, ELevelEntranceID.ENTRANCE_D, EBits.BITS_16)
        },
        {
            "Dark Cave - Right",
            new DestinationLevel(ELevel.Level_04_B_DarkCave, ELevelEntranceID.ENTRANCE_A, EBits.BITS_16)
        },
        {
            "Riviere Turquoise - Right",
            new DestinationLevel(ELevel.Level_04_C_RiviereTurquoise, ELevelEntranceID.ENTRANCE_A, EBits.BITS_8)
        },
        {
            "Riviere Turquoise - Portal",
            new DestinationLevel(ELevel.Level_04_C_RiviereTurquoise, ELevelEntranceID.ENTRANCE_B, EBits.BITS_16)
        },
        { "Howling Grotto - Left", new DestinationLevel(ELevel.Level_05_A_HowlingGrotto, ELevelEntranceID.ENTRANCE_A) },
        {
            "Howling Grotto - Right",
            new DestinationLevel(ELevel.Level_05_A_HowlingGrotto, ELevelEntranceID.ENTRANCE_B)
        },
        {
            "Howling Grotto - Portal",
            new DestinationLevel(ELevel.Level_05_A_HowlingGrotto, ELevelEntranceID.ENTRANCE_C, EBits.BITS_16)
        },
        {
            "Howling Grotto - Bottom",
            new DestinationLevel(ELevel.Level_05_A_HowlingGrotto, ELevelEntranceID.ENTRANCE_D, EBits.BITS_16)
        },
        {
            "Howling Grotto - Top",
            new DestinationLevel(ELevel.Level_05_A_HowlingGrotto, ELevelEntranceID.ENTRANCE_E, EBits.BITS_16)
        },
        {
            "Sunken Shrine - Left",
            new DestinationLevel(ELevel.Level_05_B_SunkenShrine, ELevelEntranceID.ENTRANCE_A, EBits.BITS_16)
        },
        {
            "Sunken Shrine - Portal",
            new DestinationLevel(ELevel.Level_05_B_SunkenShrine, ELevelEntranceID.ENTRANCE_B, EBits.BITS_16)
        },
        {
            "Bamboo Creek - Bottom Left",
            new DestinationLevel(ELevel.Level_06_A_BambooCreek, ELevelEntranceID.ENTRANCE_A)
        },
        { "Bamboo Creek - Right", new DestinationLevel(ELevel.Level_06_A_BambooCreek, ELevelEntranceID.ENTRANCE_B) },
        {
            "Bamboo Creek - Top Left",
            new DestinationLevel(ELevel.Level_06_A_BambooCreek, ELevelEntranceID.ENTRANCE_C, EBits.BITS_16)
        },
        {
            "Quillshroom Marsh - Top Left",
            new DestinationLevel(ELevel.Level_07_QuillshroomMarsh, ELevelEntranceID.ENTRANCE_A)
        },
        {
            "Quillshroom Marsh - Top Right",
            new DestinationLevel(ELevel.Level_07_QuillshroomMarsh, ELevelEntranceID.ENTRANCE_B)
        },
        {
            "Quillshroom Marsh - Bottom Left",
            new DestinationLevel(ELevel.Level_07_QuillshroomMarsh, ELevelEntranceID.ENTRANCE_C, EBits.BITS_16)
        },
        {
            "Quillshroom Marsh - Bottom Right",
            new DestinationLevel(ELevel.Level_07_QuillshroomMarsh, ELevelEntranceID.ENTRANCE_D, EBits.BITS_16)
        },
        { "Searing Crags - Left", new DestinationLevel(ELevel.Level_08_SearingCrags, ELevelEntranceID.ENTRANCE_A) },
        // There is also an entrance B, but its only active during the first quest before the rope from Glacial Peak is added.
        {
            "Searing Crags - Right",
            new DestinationLevel(ELevel.Level_08_SearingCrags, ELevelEntranceID.ENTRANCE_C, EBits.BITS_8)
        },
        {
            "Searing Crags - Bottom",
            new DestinationLevel(ELevel.Level_08_SearingCrags, ELevelEntranceID.ENTRANCE_D, EBits.BITS_16)
        },
        {
            "Searing Crags - Top",
            new DestinationLevel(
                ELevel.Level_08_SearingCrags,
                ELevelEntranceID.ENTRANCE_E,
                entranceCutscene: typeof(SearingEntranceFromGlouciousCutscene)
            )
        },
        {
            "Searing Crags - Portal",
            new DestinationLevel(ELevel.Level_08_SearingCrags, ELevelEntranceID.ENTRANCE_F, EBits.BITS_16)
        },
        {
            "Glacial Peak - Bottom",
            new DestinationLevel(
                ELevel.Level_09_A_GlacialPeak,
                ELevelEntranceID.ENTRANCE_A,
                entranceCutscene: typeof(GlouciousEntranceFromSearingRopeCutscene)
            )
        },
        {
            "Glacial Peak - Left",
            new DestinationLevel(
                ELevel.Level_09_A_GlacialPeak,
                ELevelEntranceID.ENTRANCE_B,
                entranceCutscene: typeof(SkylandsToGlacialPeakSecondTimeCutscene)
            )
        },
        {
            "Glacial Peak - Top",
            new DestinationLevel(
                ELevel.Level_09_A_GlacialPeak,
                ELevelEntranceID.ENTRANCE_C,
                entranceCutscene: typeof(GlouciousStaffTeleportEntranceCutscene)
            )
        },
        {
            "Glacial Peak - Portal",
            new DestinationLevel(ELevel.Level_09_A_GlacialPeak, ELevelEntranceID.ENTRANCE_D, EBits.BITS_16)
        },
        {
            "Elemental Skylands - Air Shmup",
            new DestinationLevel(ELevel.Level_09_B_ElementalSkylands, ELevelEntranceID.ENTRANCE_A, EBits.BITS_16)
        },
        {
            "Tower of Time - Left",
            new DestinationLevel(ELevel.Level_10_A_TowerOfTime, ELevelEntranceID.ENTRANCE_A, EBits.BITS_8)
        },
        {
            "Artificer's Challenge",
            new DestinationLevel(ELevel.Level_10_A_TowerOfTime, ELevelEntranceID.ENTRANCE_A, EBits.BITS_8)
        },
        {
            // Entrance A is for the first quest.
            "Cloud Ruins - Left",
            new DestinationLevel(
                ELevel.Level_11_A_CloudRuins,
                ELevelEntranceID.ENTRANCE_B,
                EBits.BITS_16,
                entranceCutscene: typeof(CloudRuinsStaffTeleportEntranceCutscene)
            )
        },
        { "Music Box - Left", new DestinationLevel(ELevel.Level_11_B_MusicBox, ELevelEntranceID.ENTRANCE_A) },
        {
            "Underworld - Left",
            new DestinationLevel(ELevel.Level_12_UnderWorld, ELevelEntranceID.ENTRANCE_B, EBits.BITS_8)
        },
        {
            "Corrupted Future",
            new DestinationLevel(
                ELevel.Level_14_CorruptedFuture,
                ELevelEntranceID.ENTRANCE_C,
                EBits.BITS_16,
                entranceCutscene: typeof(CorruptedFutureIntroCutscene)
            )
        },
        {
            "Artificer's Portal",
            new DestinationLevel(ELevel.Level_14_CorruptedFuture, ELevelEntranceID.ENTRANCE_C, EBits.BITS_16)
        },
        { "Ruxxtin Surfin'", new DestinationLevel(ELevel.Level_15_Surf, new Vector3(-472.191f, 472f), EBits.BITS_8) }, // No entrance here?
        { "Beach - Left", new DestinationLevel(ELevel.Level_16_Beach, ELevelEntranceID.ENTRANCE_A) },
        { "Fire Mountain - Bottom", new DestinationLevel(ELevel.Level_17_Volcano, ELevelEntranceID.ENTRANCE_A) },
        {
            "Voodoo Heart - Left",
            new DestinationLevel(ELevel.Level_18_Volcano_Chase, ELevelEntranceID.ENTRANCE_A, EBits.BITS_8)
        },
    };

    public static readonly Dictionary<LevelExit, string> LevelExitToExitName = new()
    {
        { new LevelExit(ELevel.Level_02_AutumnHills, ELevelEntranceID.ENTRANCE_A), "Ninja Village - Right" },
        { new LevelExit(ELevel.Level_01_NinjaVillage, ELevelEntranceID.ENTRANCE_B), "Autumn Hills - Left" },
        { new LevelExit(ELevel.Level_04_Catacombs, ELevelEntranceID.ENTRANCE_D), "Autumn Hills - Bottom" },
        { new LevelExit(ELevel.Level_03_ForlornTemple, ELevelEntranceID.ENTRANCE_A), "Autumn Hills - Right" },
        { new LevelExit(ELevel.Level_06_A_BambooCreek, ELevelEntranceID.ENTRANCE_C), "Forlorn Temple - Right" },
        { new LevelExit(ELevel.Level_02_AutumnHills, ELevelEntranceID.ENTRANCE_B), "Forlorn Temple - Left" },
        { new LevelExit(ELevel.Level_04_Catacombs, ELevelEntranceID.ENTRANCE_A), "Forlorn Temple - Bottom" },
        { new LevelExit(ELevel.Level_02_AutumnHills, ELevelEntranceID.ENTRANCE_D), "Catacombs - Bottom Left" },
        { new LevelExit(ELevel.Level_06_A_BambooCreek, ELevelEntranceID.ENTRANCE_A), "Catacombs - Right" },
        { new LevelExit(ELevel.Level_04_B_DarkCave, ELevelEntranceID.ENTRANCE_A), "Catacombs - Bottom" },
        { new LevelExit(ELevel.Level_03_ForlornTemple, ELevelEntranceID.ENTRANCE_C), "Catacombs - Top Left" },
        { new LevelExit(ELevel.Level_04_Catacombs, ELevelEntranceID.ENTRANCE_C), "Dark Cave - Right" },
        { new LevelExit(ELevel.Level_04_C_RiviereTurquoise, ELevelEntranceID.ENTRANCE_A), "Dark Cave - Left" },
        { new LevelExit(ELevel.Level_04_Catacombs, ELevelEntranceID.ENTRANCE_B), "Bamboo Creek - Bottom Left" },
        { new LevelExit(ELevel.Level_03_ForlornTemple, ELevelEntranceID.ENTRANCE_B), "Bamboo Creek - Top Left" },
        { new LevelExit(ELevel.Level_05_A_HowlingGrotto, ELevelEntranceID.ENTRANCE_A), "Bamboo Creek - Right" },
        { new LevelExit(ELevel.Level_07_QuillshroomMarsh, ELevelEntranceID.ENTRANCE_A), "Howling Grotto - Right" },
        { new LevelExit(ELevel.Level_06_A_BambooCreek, ELevelEntranceID.ENTRANCE_B), "Howling Grotto - Left" },
        { new LevelExit(ELevel.Level_07_QuillshroomMarsh, ELevelEntranceID.ENTRANCE_C), "Howling Grotto - Top" },
        { new LevelExit(ELevel.Level_05_B_SunkenShrine, ELevelEntranceID.ENTRANCE_A), "Howling Grotto - Bottom" },
        { new LevelExit(ELevel.Level_05_A_HowlingGrotto, ELevelEntranceID.ENTRANCE_D), "Sunken Shrine - Left" },
        { new LevelExit(ELevel.Level_05_A_HowlingGrotto, ELevelEntranceID.ENTRANCE_B), "Quillshroom Marsh - Top Left" },
        {
            new LevelExit(ELevel.Level_05_A_HowlingGrotto, ELevelEntranceID.ENTRANCE_E),
            "Quillshroom Marsh - Bottom Left"
        },
        { new LevelExit(ELevel.Level_08_SearingCrags, ELevelEntranceID.ENTRANCE_A), "Quillshroom Marsh - Top Right" },
        {
            new LevelExit(ELevel.Level_08_SearingCrags, ELevelEntranceID.ENTRANCE_D),
            "Quillshroom Marsh - Bottom Right"
        },
        { new LevelExit(ELevel.Level_09_A_GlacialPeak, ELevelEntranceID.ENTRANCE_A), "Searing Crags - Top" },
        { new LevelExit(ELevel.Level_07_QuillshroomMarsh, ELevelEntranceID.ENTRANCE_B), "Searing Crags - Left" },
        { new LevelExit(ELevel.Level_07_QuillshroomMarsh, ELevelEntranceID.ENTRANCE_D), "Searing Crags - Bottom" },
        { new LevelExit(ELevel.Level_12_UnderWorld, ELevelEntranceID.ENTRANCE_B), "Searing Crags - Right" },
        { new LevelExit(ELevel.Level_08_SearingCrags, ELevelEntranceID.ENTRANCE_B), "Glacial Peak - Bottom" },
        { // Not an actual exit, its the cutscene that triggers it.
            new LevelExit(ELevel.Level_08_SearingCrags, ELevelEntranceID.ENTRANCE_E),
            "Glacial Peak - Bottom"
        },
        { // Not an actual exit, its the cutscene that triggers it.
            new LevelExit(ELevel.Level_11_A_CloudRuins, ELevelEntranceID.ENTRANCE_B),
            "Glacial Peak - Top"
        },
        { // Not an actual exit, its the cutscene that triggers it.
            new LevelExit(ELevel.Level_09_B_ElementalSkylands, ELevelEntranceID.ENTRANCE_A),
            "Glacial Peak - Left"
        },
        { // Not an actual exit, its the cutscene that triggers it.
            new LevelExit(ELevel.Level_09_A_GlacialPeak, ELevelEntranceID.ENTRANCE_B),
            "Elemental Skylands - Right"
        },
        { new LevelExit(ELevel.Level_09_A_GlacialPeak, ELevelEntranceID.ENTRANCE_C), "Cloud Ruins - Left" },
        { new LevelExit(ELevel.Level_08_SearingCrags, ELevelEntranceID.ENTRANCE_C), "Underworld - Left" },
        { new LevelExit(ELevel.Level_02_AutumnHills, ELevelEntranceID.ENTRANCE_C), "HQ - Autumn Hills Portal" },
        {
            new LevelExit(ELevel.Level_04_C_RiviereTurquoise, ELevelEntranceID.ENTRANCE_B),
            "HQ - Riviere Turquoise Portal"
        },
        { new LevelExit(ELevel.Level_09_A_GlacialPeak, ELevelEntranceID.ENTRANCE_D), "HQ - Glacial Peak Portal" },
        { new LevelExit(ELevel.Level_05_B_SunkenShrine, ELevelEntranceID.ENTRANCE_B), "HQ - Sunken Shrine Portal" },
        { new LevelExit(ELevel.Level_05_A_HowlingGrotto, ELevelEntranceID.ENTRANCE_C), "HQ - Howling Grotto Portal" },
        { new LevelExit(ELevel.Level_08_SearingCrags, ELevelEntranceID.ENTRANCE_F), "HQ - Searing Crags Portal" },
        { new LevelExit(ELevel.Level_10_A_TowerOfTime, ELevelEntranceID.ENTRANCE_A), "Artificer's Challenge" },
        { new LevelExit(ELevel.Level_14_CorruptedFuture, ELevelEntranceID.ENTRANCE_C), "Artificer's Portal" },
    };

    public static readonly List<string> TransitionNames =
    [
        "Ninja Village - Right",
        "Autumn Hills - Left",
        "Autumn Hills - Right",
        "Autumn Hills - Bottom",
        "Forlorn Temple - Left",
        "Forlorn Temple - Bottom",
        "Forlorn Temple - Right",
        "Catacombs - Top Left",
        "Catacombs - Right",
        "Catacombs - Bottom",
        "Catacombs - Bottom Left",
        "Dark Cave - Right",
        "Dark Cave - Left",
        "Riviere Turquoise - Right",
        "Howling Grotto - Left",
        "Howling Grotto - Right",
        "Howling Grotto - Top",
        "Howling Grotto - Bottom",
        "Sunken Shrine - Left",
        "Bamboo Creek - Top Left",
        "Bamboo Creek - Bottom Left",
        "Bamboo Creek - Right",
        "Quillshroom Marsh - Top Left",
        "Quillshroom Marsh - Bottom Left",
        "Quillshroom Marsh - Top Right",
        "Quillshroom Marsh - Bottom Right",
        "Searing Crags - Left",
        "Searing Crags - Bottom",
        "Searing Crags - Right",
        "Searing Crags - Top",
        "Glacial Peak - Bottom",
        "Glacial Peak - Top",
        "Glacial Peak - Left",
        "Elemental Skylands - Air Shmup",
        "Elemental Skylands - Right",
        "Artificer's Challenge",
        "Tower of Time - Left",
        "Corrupted Future",
        "Cloud Ruins - Left",
        "Underworld - Left",
        "Artificer's Portal",
    ];

    public static DestinationLevel MusicBoxSkip = new(ELevel.Level_11_B_MusicBox, new Vector2(125, 40), EBits.BITS_16);
}
