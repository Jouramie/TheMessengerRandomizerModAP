using System.Collections.Generic;
using UnityEngine;

namespace MessengerRando.Data;

public static class PortalData
{
    public readonly struct Portal
    {
        private readonly int Region;
        private readonly int PortalType;
        private readonly int Index;

        public Portal(int portalWarp)
        {
            var modWarp = portalWarp.ToString();
            if (modWarp.Length == 4)
            {
                Region = int.Parse(modWarp.Substring(0, 2));
                PortalType = int.Parse(modWarp[2].ToString());
                Index = int.Parse(modWarp[3].ToString());
            }
            else if (modWarp.Length == 3)
            {
                Region = int.Parse(modWarp[0].ToString());
                PortalType = int.Parse(modWarp[1].ToString());
                Index = int.Parse(modWarp[2].ToString());
            }
            else if (modWarp.Length == 2)
            {
                Region = 0;
                PortalType = int.Parse(modWarp[0].ToString());
                Index = int.Parse(modWarp[1].ToString());
            }
            else
            {
                Region = 0;
                PortalType = 0;
                Index = int.Parse(modWarp);
            }
        }

        public LevelData.DestinationLevel Destination
        {
            get
            {
                var destinations = PortalDestinationsByArea[Region];
                return PortalType switch
                {
                    0 => destinations.portals[Index],
                    1 => destinations.shops[Index],
                    2 => destinations.checkpoints[Index],
                    _ => throw new System.Exception($"Invalid portal type: {PortalType}"),
                };
            }
        }
    }

    public readonly struct AreaCheckpointData(
        List<LevelData.DestinationLevel> portals = null,
        List<LevelData.DestinationLevel> shops = null,
        List<LevelData.DestinationLevel> checkpoints = null
    )
    {
        public readonly List<LevelData.DestinationLevel> portals = portals ?? [];
        public readonly List<LevelData.DestinationLevel> shops = shops ?? [];
        public readonly List<LevelData.DestinationLevel> checkpoints = checkpoints ?? [];
    }

    // Order is fixed to match order in the APWorld.
    public static readonly List<AreaCheckpointData> PortalDestinationsByArea =
    [
        new AreaCheckpointData(
            portals: [new(ELevel.Level_02_AutumnHills, ELevelEntranceID.ENTRANCE_C, EBits.BITS_16)],
            shops:
            [
                new(ELevel.Level_02_AutumnHills, new Vector3(-45.5f, -89)),
                new(ELevel.Level_02_AutumnHills, new Vector3(68.5f, -111)),
                new(ELevel.Level_02_AutumnHills, new Vector3(407.5f, -74)),
                new(ELevel.Level_02_AutumnHills, new Vector3(892.5f, -27)),
            ],
            checkpoints:
            [
                new(ELevel.Level_02_AutumnHills, new Vector3(175, -148)),
                new(ELevel.Level_02_AutumnHills, new Vector3(91.5f, -87)),
                new(ELevel.Level_02_AutumnHills, new Vector3(238.5f, -74)),
                new(ELevel.Level_02_AutumnHills, new Vector3(607.48f, -35)),
                new(ELevel.Level_02_AutumnHills, new Vector3(718.5f, -73), EBits.BITS_8),
            ]
        ),
        new AreaCheckpointData(
            shops:
            [
                new(ELevel.Level_03_ForlornTemple, new Vector3(0.5f, -11)),
                new(ELevel.Level_03_ForlornTemple, new Vector3(88.5f, -10)),
                new(ELevel.Level_03_ForlornTemple, new Vector3(251.5f, 53), EBits.BITS_8),
                new(ELevel.Level_03_ForlornTemple, new Vector3(156, 85)),
                new(ELevel.Level_03_ForlornTemple, new Vector3(271.5f, 61)),
                new(ELevel.Level_03_ForlornTemple, new Vector3(347.5f, 31)),
                new(ELevel.Level_03_ForlornTemple, new Vector3(354.5f, -11)),
            ],
            checkpoints:
            [
                new(ELevel.Level_03_ForlornTemple, new Vector3(124.5f, 47)),
                new(ELevel.Level_03_ForlornTemple, new Vector3(260.5f, 24)),
            ]
        ),
        new AreaCheckpointData(
            shops:
            [
                new(ELevel.Level_04_Catacombs, new Vector3(241.5f, -25)),
                new(ELevel.Level_04_Catacombs, new Vector3(731.5f, -75)),
            ],
            checkpoints:
            [
                new(ELevel.Level_04_Catacombs, new Vector3(379.5f, -23)),
                new(ELevel.Level_04_Catacombs, new Vector3(529.5f, -75), EBits.BITS_16),
                new(ELevel.Level_04_Catacombs, new Vector3(499.5f, -43)),
            ]
        ),
        new AreaCheckpointData(
            shops:
            [
                new(ELevel.Level_06_A_BambooCreek, new Vector3(-28.5f, -19)),
                new(ELevel.Level_06_A_BambooCreek, new Vector3(92.5f, 25)),
                new(ELevel.Level_06_A_BambooCreek, new Vector3(379.5f, 25)),
            ],
            checkpoints:
            [
                new(ELevel.Level_06_A_BambooCreek, new Vector3(227.5f, -41)),
                new(ELevel.Level_06_A_BambooCreek, new Vector3(210.5f, 29)),
            ]
        ),
        new AreaCheckpointData(
            portals: [new(ELevel.Level_05_A_HowlingGrotto, ELevelEntranceID.ENTRANCE_C, EBits.BITS_16)],
            shops:
            [
                new(ELevel.Level_05_A_HowlingGrotto, new Vector3(26.5f, -27)),
                new(ELevel.Level_05_A_HowlingGrotto, new Vector3(310.5f, -115)),
                new(ELevel.Level_05_A_HowlingGrotto, new Vector3(541.5f, -123)),
            ],
            checkpoints:
            [
                new(ELevel.Level_05_A_HowlingGrotto, new Vector3(138.5f, -90)),
                new(ELevel.Level_05_A_HowlingGrotto, new Vector3(439, -170)),
            ]
        ),
        new AreaCheckpointData(
            shops:
            [
                new(ELevel.Level_07_QuillshroomMarsh, new Vector3(193.5f, -37)),
                new(ELevel.Level_07_QuillshroomMarsh, new Vector3(663.5f, -27)),
                new(ELevel.Level_07_QuillshroomMarsh, new Vector3(1085.5f, -43)),
            ],
            checkpoints:
            [
                new(ELevel.Level_07_QuillshroomMarsh, new Vector3(161.5f, -54)),
                new(ELevel.Level_07_QuillshroomMarsh, new Vector3(409.5f, -42)),
                new(ELevel.Level_07_QuillshroomMarsh, new Vector3(916.5f, -26)),
            ]
        ),
        new AreaCheckpointData(
            portals: [new(ELevel.Level_08_SearingCrags, ELevelEntranceID.ENTRANCE_F, EBits.BITS_16)],
            shops:
            [
                new(ELevel.Level_08_SearingCrags, new Vector3(61, -27)),
                new(ELevel.Level_08_SearingCrags, new Vector3(147.5f, 69)),
                new(ELevel.Level_08_SearingCrags, new Vector3(226.5f, 151)),
                new(ELevel.Level_08_SearingCrags, new Vector3(282, 237)),
                new(ELevel.Level_08_SearingCrags, new Vector3(380.5f, 309)),
                new(ELevel.Level_08_SearingCrags, new Vector3(119.5f, 248), EBits.BITS_8),
            ],
            checkpoints:
            [
                new(ELevel.Level_08_SearingCrags, new Vector3(109.5f, 63)),
                new(ELevel.Level_08_SearingCrags, new Vector3(296.5f, 189f), EBits.BITS_8),
            ]
        ),
        new AreaCheckpointData(
            portals: [new(ELevel.Level_09_A_GlacialPeak, ELevelEntranceID.ENTRANCE_D, EBits.BITS_16)],
            shops:
            [
                new(ELevel.Level_09_A_GlacialPeak, new Vector3(216.5f, -456)),
                new(ELevel.Level_09_A_GlacialPeak, new Vector3(259.5f, -297)),
                new(ELevel.Level_09_A_GlacialPeak, new Vector3(156.5f, -27)),
            ],
            checkpoints:
            [
                new(ELevel.Level_09_A_GlacialPeak, new Vector3(227.5f, -405)),
                new(ELevel.Level_09_A_GlacialPeak, new Vector3(251.5f, -235)),
                new(ELevel.Level_09_A_GlacialPeak, new Vector3(195.5f, -131)),
            ]
        ),
        new AreaCheckpointData(
            shops:
            [
                new(ELevel.Level_10_A_TowerOfTime, new Vector3(71.5f, -11), EBits.BITS_8),
                new(ELevel.Level_10_A_TowerOfTime, new Vector3(84.5f, 237), EBits.BITS_8),
            ],
            checkpoints:
            [
                new(ELevel.Level_10_A_TowerOfTime, new Vector3(38.5f, 21.5f), EBits.BITS_8),
                new(ELevel.Level_10_A_TowerOfTime, new Vector3(5.5f, 37), EBits.BITS_8),
                new(ELevel.Level_10_A_TowerOfTime, new Vector3(50.5f, 77), EBits.BITS_8),
                new(ELevel.Level_10_A_TowerOfTime, new Vector3(57.5f, 85), EBits.BITS_8),
                new(ELevel.Level_10_A_TowerOfTime, new Vector3(31.5f, 133), EBits.BITS_8),
                new(ELevel.Level_10_A_TowerOfTime, new Vector3(58.5f, 165), EBits.BITS_8),
            ]
        ),
        new AreaCheckpointData(
            shops:
            [
                new(ELevel.Level_11_A_CloudRuins, new Vector3(-368.5f, -26), EBits.BITS_8),
                new(ELevel.Level_11_A_CloudRuins, new Vector3(-140.5f, -26), EBits.BITS_8),
                new(ELevel.Level_11_A_CloudRuins, new Vector3(116.5f, -25)),
                new(ELevel.Level_11_A_CloudRuins, new Vector3(366.5f, -27), EBits.BITS_8),
                new(ELevel.Level_11_A_CloudRuins, new Vector3(721.5f, -22), EBits.BITS_8),
                new(ELevel.Level_11_A_CloudRuins, new Vector3(816.5f, -26), EBits.BITS_8),
                new(ELevel.Level_11_A_CloudRuins, new Vector3(1148.5f, -27), EBits.BITS_8),
            ],
            checkpoints:
            [
                new(ELevel.Level_11_A_CloudRuins, new Vector3(-146, 24), EBits.BITS_8),
                new(ELevel.Level_11_A_CloudRuins, new Vector3(164, -21)),
                new(ELevel.Level_11_A_CloudRuins, new Vector3(769.5f, -26)),
                new(ELevel.Level_11_A_CloudRuins, new Vector3(-251, 13)),
            ]
        ),
        new AreaCheckpointData(
            shops:
            [
                new(ELevel.Level_12_UnderWorld, new Vector3(-305, -51), EBits.BITS_8),
                new(ELevel.Level_12_UnderWorld, new Vector3(-186.5f, -24), EBits.BITS_8),
                new(ELevel.Level_12_UnderWorld, new Vector3(-125.5f, -91), EBits.BITS_8),
                //new LevelConstants.RandoLevel(ELevel.Level_12_UnderWorld, new Vector3(124.5f, -43)), // barm'athazel which isn't accessible in second quest
                new(ELevel.Level_12_UnderWorld, new Vector3(132.5f, -130)),
            ],
            checkpoints:
            [
                new(ELevel.Level_12_UnderWorld, new Vector3(-226.5f, -89), EBits.BITS_8),
                new(ELevel.Level_12_UnderWorld, new Vector3(0.5f, 72), EBits.BITS_8),
                new(ELevel.Level_12_UnderWorld, new Vector3(-110.5f, -98), EBits.BITS_8),
            ]
        ),
        new AreaCheckpointData(
            portals: [new(ELevel.Level_04_C_RiviereTurquoise, ELevelEntranceID.ENTRANCE_B, EBits.BITS_16)],
            shops:
            [
                new(ELevel.Level_04_C_RiviereTurquoise, new Vector3(804.5f, -40)),
                new(ELevel.Level_04_C_RiviereTurquoise, new Vector3(499, -132), EBits.BITS_8),
                new(ELevel.Level_04_C_RiviereTurquoise, new Vector3(337.5f, -131)),
                new(ELevel.Level_04_C_RiviereTurquoise, new Vector3(149.5f, -89)),
                new(ELevel.Level_04_C_RiviereTurquoise, new Vector3(-8.5f, 13)),
                new(ELevel.Level_04_C_RiviereTurquoise, new Vector3(-259, 7)),
            ],
            checkpoints: [new(ELevel.Level_04_C_RiviereTurquoise, new Vector3(648.5f, -83), EBits.BITS_8)]
        ),
        new AreaCheckpointData(
            shops:
            [
                new(ELevel.Level_09_B_ElementalSkylands, new Vector3(-35, 359)),
                new(ELevel.Level_09_B_ElementalSkylands, new Vector3(87.5f, 407)),
                new(ELevel.Level_09_B_ElementalSkylands, new Vector3(864.5f, 381)),
                new(ELevel.Level_09_B_ElementalSkylands, new Vector3(966.3f, 409.4f)),
                new(ELevel.Level_09_B_ElementalSkylands, new Vector3(1763.5f, 381)),
                new(ELevel.Level_09_B_ElementalSkylands, new Vector3(1909, 411)),
                new(ELevel.Level_09_B_ElementalSkylands, new Vector3(2755.5f, 376), EBits.BITS_8),
                new(ELevel.Level_09_B_ElementalSkylands, new Vector3(2926.5f, 406)),
            ],
            checkpoints: [new(ELevel.Level_09_B_ElementalSkylands, new Vector3(-22.5f, 417))]
        ),
        new AreaCheckpointData(
            portals: [new(ELevel.Level_05_B_SunkenShrine, ELevelEntranceID.ENTRANCE_B, EBits.BITS_16)],
            shops:
            [
                new(ELevel.Level_05_B_SunkenShrine, new Vector3(36.5f, -41)),
                new(ELevel.Level_05_B_SunkenShrine, new Vector3(186.5f, -9), EBits.BITS_8),
                new(ELevel.Level_05_B_SunkenShrine, new Vector3(8, -65), EBits.BITS_8),
                new(ELevel.Level_05_B_SunkenShrine, new Vector3(-102.5f, -121), EBits.BITS_8),
                new(ELevel.Level_05_B_SunkenShrine, new Vector3(100, -81), EBits.BITS_8),
            ],
            checkpoints:
            [
                new(ELevel.Level_05_B_SunkenShrine, new Vector3(53.5f, -25), EBits.BITS_8),
                new(ELevel.Level_05_B_SunkenShrine, new Vector3(29.5f, -87), EBits.BITS_8),
                new(ELevel.Level_05_B_SunkenShrine, new Vector3(166, -178), EBits.BITS_8),
                new(ELevel.Level_05_B_SunkenShrine, new Vector3(92.5f, -100), EBits.BITS_8),
            ]
        ),
    ];

    public static LevelData.LevelExit AutumnHillsPortal = new(ELevel.Level_02_AutumnHills, ELevelEntranceID.ENTRANCE_C);
    public static LevelData.LevelExit RiviereTurquoisePortal = new(
        ELevel.Level_04_C_RiviereTurquoise,
        ELevelEntranceID.ENTRANCE_B
    );
    public static LevelData.LevelExit HowlingGrottoPortal = new(
        ELevel.Level_05_A_HowlingGrotto,
        ELevelEntranceID.ENTRANCE_C
    );
    public static LevelData.LevelExit SunkenShrinePortal = new(
        ELevel.Level_05_B_SunkenShrine,
        ELevelEntranceID.ENTRANCE_B
    );
    public static LevelData.LevelExit SearingCragsPortal = new(
        ELevel.Level_08_SearingCrags,
        ELevelEntranceID.ENTRANCE_F
    );
    public static LevelData.LevelExit GlacialPeakPortal = new(
        ELevel.Level_09_A_GlacialPeak,
        ELevelEntranceID.ENTRANCE_D
    );

    public static LevelData.DestinationLevel DecodePortalDestination(int encodedDestination)
    {
        return new Portal(encodedDestination).Destination;
    }
}
