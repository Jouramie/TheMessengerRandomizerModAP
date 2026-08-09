using System;
using MessengerRando.Archipelago;
using Logger = MessengerRando.Utils.Logger;
using Object = UnityEngine.Object;

namespace MessengerRando.GameOverrideManagers;

public class RandoPowerSealManager
{
    private static readonly Logger logger = Logger.GetLogger<RandoPowerSealManager>();

    public RandoPowerSealManager(int requiredPowerSeals)
    {
        if (requiredPowerSeals == 0)
            requiredPowerSeals = 45;
        Manager<ProgressionManager>.Instance.powerSealTotal = requiredPowerSeals;
    }

    public void AddPowerSeal() => ArchipelagoClient.ServerData.PowerSealsCollected++;

    public void OnShopChestOpen(On.ShopChestOpenCutscene.orig_OnChestOpened orig, ShopChestOpenCutscene self)
    {
        try
        {
            //going to attempt to teleport the player to the ending sequence when they open the chest
            OnShopChestOpen();
            self.EndCutScene();
        }
        catch (Exception e)
        {
            logger.Exception(e);
            orig(self);
        }
    }

    public void OnShopChestOpen(On.ShopChestChangeShurikenCutscene.orig_Play orig, ShopChestChangeShurikenCutscene self)
    {
        try
        {
            //going to attempt to teleport the player to the ending sequence when they open the chest
            OnShopChestOpen();
            self.EndCutScene();
        }
        catch (Exception e)
        {
            logger.Exception(e);
            orig(self);
        }
    }

    private void OnShopChestOpen()
    {
        try
        {
            logger.Log("Opening the shop chest...");
            Object.FindObjectOfType<Shop>().LeaveToCurrentLevel();
            RandoLevelManager.SkipMusicBox();
        }
        catch (Exception e)
        {
            logger.Exception(e);
        }
    }

    /// <summary>
    /// Assigns our total power seal count to the game and then returns the value. Unsure if the assignment is safe
    /// here, but trying it so it'll show the required count in the dialog.
    /// </summary>
    /// <returns></returns>
    public int AmountPowerSealsCollected() => ArchipelagoClient.ServerData.PowerSealsCollected;
}
