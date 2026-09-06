using System;
using System.Collections.Generic;
using MessengerRando.Archipelago;
using MessengerRando.Data;
using MessengerRando.Extensions;
using MessengerRando.Utils;
using Logger = MessengerRando.Utils.Logger;

namespace MessengerRando.GameOverrideManagers;

public static class RandoLevelManager
{
    private static readonly Logger logger = Logger.GetLogger(typeof(RandoLevelManager));

    private static readonly Dictionary<LevelData.LevelExit, LevelData.DestinationLevel> DestinationReplacements = [];

    // Ongoing transition state
    private static LevelData.DestinationLevel? _ongoingTransitionShuffle = null;
    private static bool _startOnManfred = false;

    public static bool IsTransitionShuffled { get; private set; } = false;
    public static bool IsPortalShuffled { get; private set; } = false;

    public static void ApplyHooks()
    {
        // For current region tracker
        On.TotHQ.OnInitDone += SafeHook.Wrap<On.TotHQ.hook_OnInitDone>(TotHQ_OnInitDone);
        On.Shop.Init += SafeHook.Wrap<On.Shop.hook_Init>(Shop_Init);
        On.Shop.LeaveToCurrentLevel += SafeHook.Wrap<On.Shop.hook_LeaveToCurrentLevel>(Shop_LeaveToCurrentLevel);

        // Portals
        On.TowerOfTimePortal.LoadLevel += SafeHook.Wrap<On.TowerOfTimePortal.hook_LoadLevel>(
            TowerOfTimePortal_LoadLevel
        );

        On.LevelManager.ReinitCurrentLevel += SafeHook.Wrap<On.LevelManager.hook_ReinitCurrentLevel>(
            LevelManager_ReinitCurrentLevel
        );
        On.LevelManager.LoadLevel += SafeHook.Wrap<On.LevelManager.hook_LoadLevel>(LevelManager_LoadLevel);

        On.ElementalSkylandsLevelInitializer.OnBeforeInitDone +=
            SafeHook.Wrap<On.ElementalSkylandsLevelInitializer.hook_OnBeforeInitDone>(
                ElementalSkylandsLevelInitializer_OnBeforeInitDone
            );

        On.LevelInitializer.InitDone += HookMonitor.Debug<On.LevelInitializer.hook_InitDone>();
        On.ExitPortalCutscene.Play += HookMonitor.Debug<On.ExitPortalCutscene.hook_Play>();
    }

    public static void Reset()
    {
        DestinationReplacements.Clear();
        _ongoingTransitionShuffle = null;
        _startOnManfred = false;

        IsTransitionShuffled = false;
        IsPortalShuffled = false;
    }

    public static void SetTransitionMapping(Dictionary<LevelData.LevelExit, LevelData.DestinationLevel> mapping)
    {
        DestinationReplacements.AddRange(mapping);
        IsTransitionShuffled = true;
    }

    public static void SetPortalMapping(Dictionary<LevelData.LevelExit, LevelData.DestinationLevel> mapping)
    {
        DestinationReplacements.AddRange(mapping);
        IsPortalShuffled = true;
    }

    public static void SetSkipMusicBox()
    {
        DestinationReplacements.Add(
            LevelData.EntranceNameToDestinationLevel["Music Box - Left"].AsLevelExit(),
            LevelData.MusicBoxSkip
        );
    }

    private static void TotHQ_OnInitDone(On.TotHQ.orig_OnInitDone orig, TotHQ self)
    {
        orig(self);
        ServiceLocator.Get<TrackerManager>().SetCurrentRegion(ELevel.Level_13_TowerOfTimeHQ);
    }

    private static void Shop_Init(On.Shop.orig_Init orig, Shop self, ShopParameters shopParameters, bool fakeShop)
    {
        orig(self, shopParameters, fakeShop);
        ServiceLocator.Get<TrackerManager>().SetCurrentRegion(ELevel.Level_13_TowerOfTimeHQ);
    }

    private static void Shop_LeaveToCurrentLevel(On.Shop.orig_LeaveToCurrentLevel orig, Shop self)
    {
        orig(self);
        ServiceLocator.Get<TrackerManager>().SetCurrentRegion(Manager<LevelManager>.Instance.GetCurrentLevelEnum());
    }

    [SafeHook(callOrigOnError: true)]
    private static void LevelManager_ReinitCurrentLevel(
        On.LevelManager.orig_ReinitCurrentLevel orig,
        LevelManager self,
        bool showTransition,
        ELevelEntranceID levelEntrance,
        Type reinitCutscene,
        EBits dimension,
        bool playMusic
    )
    {
        if (_ongoingTransitionShuffle is not null)
        {
            var destination = _ongoingTransitionShuffle.Value;
            logger.Log("Reinitializing level after taking portal; Overriding dimension to {0}", destination.Dimension);
            dimension = destination.Dimension == EBits.NONE ? dimension : destination.Dimension;
            levelEntrance = destination.LevelEntrance;
            _ongoingTransitionShuffle = null;
        }

        ServiceLocator.Get<TrackerManager>().SetCurrentRegion(self.GetCurrentLevelEnum());

        orig(self, showTransition, levelEntrance, reinitCutscene, dimension, playMusic);
    }

    [SafeHook(callOrigOnError: true)]
    public static void LevelManager_LoadLevel(
        On.LevelManager.orig_LoadLevel orig,
        LevelManager self,
        LevelLoadingInfo levelInfo
    )
    {
        if (_ongoingTransitionShuffle is not null)
        {
            var destination = _ongoingTransitionShuffle.Value;
            logger.Log("Loading level with transition shuffle; Overriding dimension to {0}", destination.Dimension);
            levelInfo.dimension = destination.Dimension == EBits.NONE ? levelInfo.dimension : destination.Dimension;
            levelInfo.levelEntranceId = destination.LevelEntrance;
            _ongoingTransitionShuffle = null;

            ServiceLocator.Get<TrackerManager>().SetCurrentRegion(ELevel.FromSceneName(levelInfo.levelName));
            orig(self, levelInfo);
            return;
        }

        logger.Log("Current Level: {0}", Manager<LevelManager>.Instance.GetCurrentLevelEnum());
        logger.Log(
            "Loading Level: {0}, Entrance: {1}, Dimension: {2}",
            levelInfo.levelName,
            levelInfo.levelEntranceId,
            levelInfo.dimension
        );
        logger.Log(
            "Transition type: {0}, EntranceCutscene: {1}",
            levelInfo.transitionType?.Name,
            levelInfo.levelInitializerParams?.entranceCutsceneType
        );

        TryOverrideWithTransitionRando(levelInfo);
        ServiceLocator.Get<TrackerManager>().SetCurrentRegion(ELevel.FromSceneName(levelInfo.levelName));

        orig(self, levelInfo);
    }

    private static bool TryOverrideWithTransitionRando(LevelLoadingInfo levelLoadingInfo)
    {
        var targetLevel = ELevel.FromSceneName(levelLoadingInfo.levelName);
        var targetEntrance = levelLoadingInfo.levelEntranceId;
        var originalDestination = new LevelData.LevelExit(targetLevel, targetEntrance);

        if (LevelData.LevelExitToExitName.TryGetValue(originalDestination, out var exitName))
        {
            ServiceLocator.Get<TrackerManager>().AddVisitedEntrance(exitName + " exit");
        }
        else
        {
            exitName = originalDestination.ToString();
        }

        if (!DestinationReplacements.TryGetValue(originalDestination, out var destination))
        {
            logger.Log("No transition rando mapping found for exit {0}. Not applying transition rando.", exitName);
            return false;
        }

        logger.Log(
            "Applying transition rando override. Exit: {0} will go to {1} {2}",
            exitName,
            destination.LevelName,
            destination.LevelEntrance
        );

        levelLoadingInfo.levelName = destination.LevelName.SceneName;

        levelLoadingInfo.levelEntranceId = destination.LevelEntrance;
        if (levelLoadingInfo.levelEntranceId is ELevelEntranceID.NONE)
            Manager<ProgressionManager>.Instance.checkpointSaveInfo.loadedLevelPlayerPosition = destination.PlayerPos;

        if (destination.Dimension is not EBits.NONE)
            levelLoadingInfo.dimension = destination.Dimension;

        // Not removing if cutscene is null because the portal animation is kinda cool. Will see if that causes issues.
        if (destination.EntranceCutscene is not null)
        {
            levelLoadingInfo.levelInitializerParams ??= new LevelInitializerParams();
            levelLoadingInfo.levelInitializerParams.entranceCutsceneType = destination.EntranceCutscene;
        }

        if (destination.Equals(LevelData.EntranceNameToDestinationLevel["Elemental Skylands - Air Shmup"]))
            _startOnManfred = true;
        if (destination.Equals(LevelData.EntranceNameToDestinationLevel["Howling Grotto - Bottom"]))
            // FIXME This won't handle the case where player could die/exit the game and have their game saved there.
            ServiceLocator.Get<LostWoodsManager>().SolveLostWoodsUntilExit();

        return true;
    }

    [SafeHook(callOrigOnError: true)]
    private static void TowerOfTimePortal_LoadLevel(On.TowerOfTimePortal.orig_LoadLevel orig, TowerOfTimePortal self)
    {
        var levelExit = new LevelData.LevelExit(self.nextLevel, self.levelEntrance);
        if (!DestinationReplacements.TryGetValue(levelExit, out var destination))
        {
            logger.Log("Found no replacement destination portal going to {0}", levelExit);
            orig(self);
            return;
        }

        if (destination.LevelName is ELevel.Level_09_B_ElementalSkylands)
        {
            // Reinit does not work very well with Elemental Skylands. There are two specific bugs
            // 1- Reinit cutscene is set to null, so the portal cutscene does not trigger.
            // 2- When Reinit to Air Shmup, player is not put on Manfred.
            //    Or sometimes the Camera moves to Manfred but not the player.
            // By not handling it here, it forces the portal to reload the scene and reset everything, and it works.

            logger.Log("Found shuffled destination for portal {0}, but it's going to Elemental Skylands.", levelExit);
            orig(self);
            return;
        }

        ServiceLocator.Get<TrackerManager>().AddVisitedEntrance(LevelData.LevelExitToExitName[levelExit]);
        logger.Log(
            "Player entered {0}, overriding destination to {1} {2}",
            self.name,
            destination.LevelName,
            destination.LevelEntrance is ELevelEntranceID.NONE ? destination.PlayerPos : destination.LevelEntrance
        );

        self.nextLevel = destination.LevelName;
        if (destination.LevelEntrance is ELevelEntranceID.NONE)
            Manager<ProgressionManager>.Instance.checkpointSaveInfo.loadedLevelPlayerPosition = destination.PlayerPos;

        _ongoingTransitionShuffle = destination;

        orig(self);

        self.nextLevel = levelExit.NextLevel;
        return;
    }

    [SafeHook(callOrigOnError: true)]
    private static void ElementalSkylandsLevelInitializer_OnBeforeInitDone(
        On.ElementalSkylandsLevelInitializer.orig_OnBeforeInitDone orig,
        ElementalSkylandsLevelInitializer self
    )
    {
        self.startOnManfred = _startOnManfred;
        logger.Log("Starting on Manfred {0}", self.startOnManfred);
        orig(self);
        _startOnManfred = false;
    }
}
