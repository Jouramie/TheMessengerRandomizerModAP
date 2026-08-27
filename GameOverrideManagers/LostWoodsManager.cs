using MessengerRando.Archipelago;
using MessengerRando.Extensions;
using MessengerRando.Lifecycle;
using MessengerRando.Utils;

namespace MessengerRando.GameOverrideManagers;

public class LostWoodsManager : IOnModLoadHandler, IOnBackToTitleHandler, ISaveLifecycleHandler
{
    private static readonly Logger logger = Logger.GetLogger(typeof(LostWoodsManager));

    private bool tempSolved = false;
    private bool solved = false;

    public void OnModLoad()
    {
        On.LostWoods.Start += SafeHook.Wrap<On.LostWoods.hook_Start>(LostWoods_Start);
        On.LostWoods.Exit += SafeHook.Wrap<On.LostWoods.hook_Exit>(LostWoods_Exit);
        On.LostWoods.OpenAccesToSunkenShrine += SafeHook.Wrap<On.LostWoods.hook_OpenAccesToSunkenShrine>(
            LostWoods_OpenAccesToSunkenShrine
        );
        On.LostWoods.SetAsSolved += HookMonitor.Debug<On.LostWoods.hook_SetAsSolved>();
    }

    public void OnBackToTitle()
    {
        tempSolved = false;
    }

    public void OnLoad(ArchipelagoData save)
    {
        solved = save.LostWoodsSolved;
    }

    public void OnSave(ArchipelagoData save)
    {
        save.LostWoodsSolved = solved;
    }

    public void SolveLostWoodsUntilExit()
    {
        if (solved)
            return;

        logger.Log("Solving Lost Woods temporarily until exit");
        tempSolved = true;
        Manager<LostWoods>.Instance?.SetAsSolved();
    }

    private void LostWoods_Start(On.LostWoods.orig_Start orig, LostWoods self)
    {
        // Not managing the LostWoods when transition are not randoed.
        // TODO use data from save if needed instead of nothing so we can enable that properly.
        if (!RandoLevelManager.IsTransitionShuffled)
            orig(self);

        if (tempSolved || solved)
            self.SetAsSolved();
    }

    private void LostWoods_Exit(On.LostWoods.orig_Exit orig, LostWoods self)
    {
        // There is some weird stuff going on here. If you don't have the Sea Shell, there is a bug when the wrong
        //  music starts when you try to exit the solved lost woods.

        if (tempSolved)
        {
            logger.Debug("Resetting Lost Woods to unsolved");
            self.SetAsUnsolved();
        }

        orig(self);
        tempSolved = false;
    }

    private void LostWoods_OpenAccesToSunkenShrine(On.LostWoods.orig_OpenAccesToSunkenShrine orig, LostWoods self)
    {
        logger.Log("Lost Woods solved.");
        tempSolved = false;
        solved = true;
        orig(self);
    }
}
