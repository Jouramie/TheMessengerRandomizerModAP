using System;
using MessengerRando.Archipelago;
using MessengerRando.Lifecycle;
using Mod.Courier.Save;
using Newtonsoft.Json;
using WebSocketSharp;

namespace MessengerRando.Utils;

/// <summary>
/// CourierModSave object for the randomizer. Defines the values used for the mod save file.
/// Due to current limitations of the save file, a single string value is used to capture all of the needed save information.
/// Once Courier is able to support more complex object for the save file we can consider refactoring this.
/// </summary>
public class RandoSave : CourierModSave
{
    private static readonly Logger logger = Logger.GetLogger<RandoSave>();
    public string APSaveData = GetSaveData();

    public void Update()
    {
        if (Manager<LevelManager>.Instance.GetCurrentLevelEnum().Equals(ELevel.NONE))
            return;
        var randoStateManager = ServiceLocator.Get<RandomizerStateManager>();
        randoStateManager.APSave[randoStateManager.CurrentFileSlot] = ArchipelagoClient.ServerData;
        foreach (var handler in ServiceLocator.GetAll<ISaveLifecycleHandler>())
            handler.OnSave(randoStateManager.APSave[randoStateManager.CurrentFileSlot]);

        if (ArchipelagoClient.Authenticated)
            ArchipelagoClient.SyncLocations();
        UserConfig.UpdateConfig(APRandomizerMain.ModPath);
        APSaveData = GetSaveData();
    }

    public void ForceUpdate()
    {
        APSaveData = GetSaveData();
    }

    private static string GetSaveData()
    {
        var randoStateManager = ServiceLocator.Get<RandomizerStateManager>();
        var output =
            $"{randoStateManager.APSave[1]}|"
            + $"{randoStateManager.APSave[2]}|"
            + $"{randoStateManager.APSave[3]}|"
            + $"{SeedGenerator.ArchipelagoPath}";
        return output;
    }

    private const char SplitConst = '|';

    public static void TryLoad(string load)
    {
        logger.Log("loading rando save data...");
        if (string.IsNullOrEmpty(load))
        {
            logger.Log("unable to find rando save data to load");
            return;
        }
        try
        {
            var loadedData = load.Split(SplitConst);
            for (var i = 1; i < 4; i++)
            {
                var loadedAPData = JsonConvert.DeserializeObject<ArchipelagoData>(loadedData[i - 1]);
                ServiceLocator.Get<RandomizerStateManager>().APSave[i] = loadedAPData;
            }

            if (loadedData.Length > 3 && !loadedData[3].IsNullOrEmpty())
                SeedGenerator.ArchipelagoPath = loadedData[3];
        }
        catch (Exception e)
        {
            logger.Exception(e, "Failed to load AP Save Data");
        }
    }
}
