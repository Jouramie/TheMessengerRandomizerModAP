using System.Collections.Generic;
using MessengerRando.Utils;
using Logger = MessengerRando.Utils.Logger;

namespace MessengerRando.GameOverrideManagers;

public static class RandoPortalManager
{
    private static readonly Logger logger = Logger.GetLogger(typeof(RandoPortalManager));

    public static List<string> StartingPortals;

    public static HashSet<string> UnlockedPortals
    {
        get
        {
            var unlockedPortals = new HashSet<string>();

            var progressManager = Manager<ProgressionManager>.Instance;
            if (progressManager.HasCutscenePlayed<PortalOpeningCutscene>())
            {
                unlockedPortals.Add("Autumn Hills Portal");
                unlockedPortals.Add("Howling Grotto Portal");
                unlockedPortals.Add("Glacial Peak Portal");
            }
            if (progressManager.HasCutscenePlayed<SunkenShrinePortalOpeningCutscene>())
            {
                unlockedPortals.Add("Sunken Shrine Portal");
            }
            if (progressManager.HasCutscenePlayed<SearingCragsPortalOpeningCutscene>())
            {
                unlockedPortals.Add("Searing Crags Portal");
            }
            if (progressManager.HasCutscenePlayed<RiviereTurquoisePortalOpeningCutscene>())
            {
                unlockedPortals.Add("Riviere Turquoise Portal");
            }

            return unlockedPortals;
        }
    }

    public static void ApplyHooks()
    {
        On.LevelInitializer.InitDone += SafeHook.Wrap<On.LevelInitializer.hook_InitDone>(LevelInitializer_InitDone);

        // TODO the portal unlock logic should move here.
    }

    [SafeHook(callOrigOnError: true)]
    private static void LevelInitializer_InitDone(On.LevelInitializer.orig_InitDone orig, LevelInitializer self)
    {
        if (self.GetEntranceCutscene(typeof(ExitPortalCutscene)) == null)
        {
            logger.Log("Adding entrance cutscene {0}", typeof(ExitPortalCutscene));
            var exitCutscene = self.gameObject.AddComponent(typeof(ExitPortalCutscene));
            self.entranceCutscenes.Add((Cutscene)exitCutscene);
        }

        orig(self);
        return;
    }
}
