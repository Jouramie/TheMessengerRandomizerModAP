using MessengerRando.Archipelago;
using MessengerRando.Data;
using MessengerRando.Lifecycle;
using MessengerRando.Utils;
using Logger = MessengerRando.Utils.Logger;

namespace MessengerRando.GameOverrideManagers;

public class RandoGoalManager : IOnModLoadHandler
{
    private static readonly Logger logger = Logger.GetLogger<RandoGoalManager>();

    public void OnModLoad()
    {
        On.ShopChestOpenCutscene.OnChestOpened += SafeHook.Wrap<On.ShopChestOpenCutscene.hook_OnChestOpened>(
            ShopChestOpenCutscene_OnChestOpened
        );
        On.ShopChestChangeShurikenCutscene.Play += SafeHook.Wrap<On.ShopChestChangeShurikenCutscene.hook_Play>(
            ShopChestChangeShurikenCutscene_Play
        );

        On.ProgressionManager.TotalPowerSealCollected +=
            SafeHook.Wrap<On.ProgressionManager.hook_TotalPowerSealCollected>(
                ProgressionManager_TotalPowerSealCollected
            );
    }

    public void SetPowerSealGoal(int requiredPowerSeals)
    {
        if (requiredPowerSeals == 0)
            requiredPowerSeals = 45;
        Manager<ProgressionManager>.Instance.powerSealTotal = requiredPowerSeals;
    }

    public void AddPowerSeal() => ArchipelagoClient.ServerData.PowerSealsCollected++;

    [SafeHook(callOrigOnError: true)]
    private void ShopChestOpenCutscene_OnChestOpened(
        On.ShopChestOpenCutscene.orig_OnChestOpened orig,
        ShopChestOpenCutscene self
    )
    {
        TeleportInMusicBoxFromShopChest();
        self.EndCutScene();
    }

    [SafeHook(callOrigOnError: true)]
    private void ShopChestChangeShurikenCutscene_Play(
        On.ShopChestChangeShurikenCutscene.orig_Play orig,
        ShopChestChangeShurikenCutscene self
    )
    {
        TeleportInMusicBoxFromShopChest();
        self.EndCutScene();
    }

    private int ProgressionManager_TotalPowerSealCollected(
        On.ProgressionManager.orig_TotalPowerSealCollected orig,
        ProgressionManager self
    )
    {
        return ArchipelagoClient.ServerData.PowerSealsCollected;
    }

    private void TeleportInMusicBoxFromShopChest()
    {
        logger.Log("All the power seals are reunited, teleporting to music box");
        Manager<Shop>.Instance.LeaveToCurrentLevel();
        TeleportToMusicBox();
    }

    public void TeleportToMusicBox()
    {
        ServiceLocator.Get<Teleporter>().TeleportTo(LevelData.EntranceNameToDestinationLevel["Music Box - Left"]);
    }
}
