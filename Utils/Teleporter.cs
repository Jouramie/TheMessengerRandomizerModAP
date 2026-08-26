using MessengerRando.Archipelago;
using MessengerRando.Data;
using MessengerRando.Extensions;
using MessengerRando.Utils.Menus;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MessengerRando.Utils;

public class Teleporter
{
    private static readonly Logger logger = Logger.GetLogger<Teleporter>();

    public void TeleportToTHQ()
    {
        logger.Log("Teleporting to HQ!");

        CleanupBeforeTeleport();
        Manager<TowerOfTimeHQManager>.Instance.TeleportInToTHQ(true, ELevelEntranceID.ENTRANCE_A, null);
        ServiceLocator.Get<TrackerManager>().SetCurrentRegion(ELevel.Level_13_TowerOfTimeHQ);
        CleanupAfterTeleport();
    }

    public void TeleportTo(ELevel area, Vector2 position, EBits dimension = EBits.NONE, bool updateTracker = true)
    {
        logger.Log("Teleporting to {0}, {1}, {2}", area, position, dimension);

        CleanupBeforeTeleport();
        Manager<ProgressionManager>.Instance.checkpointSaveInfo.loadedLevelPlayerPosition = position;
        if (updateTracker)
            ServiceLocator.Get<TrackerManager>().SetCurrentRegion(area);

        if (dimension.Equals(EBits.NONE))
            dimension = Manager<DimensionManager>.Instance.currentDimension;

        var levelLoadingInfo = new LevelLoadingInfo(
            area.SceneName,
            showTransition: true,
            closeTransitionOnLevelLoaded: true,
            LoadSceneMode.Single,
            dimension
        );
        Manager<LevelManager>.Instance.LoadLevel(levelLoadingInfo);
    }

    public void TeleportTo(LevelData.DestinationLevel teleportLocation)
    {
        TeleportTo(teleportLocation.LevelName, teleportLocation.PlayerPos, teleportLocation.Dimension);
    }

    public void CleanupBeforeOptionsTeleport()
    {
        CleanupBeforeTeleport();
        Manager<PauseManager>.Instance.Resume();
        ArchipelagoMenu.archipelagoScreen.Close(false);
        Manager<UIManager>.Instance.CloseAllScreensOfType<OptionScreen>(false);
    }

    private void CleanupBeforeTeleport()
    {
        Manager<AudioManager>.Instance.StopMusic();
    }

    public void CleanupAfterTeleport()
    {
        Manager<UIManager>.Instance.CloseAllScreensOfType<CinematicBordersScreen>(false);
        Manager<UIManager>.Instance.CloseAllScreensOfType<TransitionScreen>(false);
        Manager<UIManager>.Instance.CloseAllScreensOfType<SavingScreen>(false);
        Manager<UIManager>.Instance.CloseAllScreensOfType<LoadingAnimation>(false);
    }
}
