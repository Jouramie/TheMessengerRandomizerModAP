using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Archipelago.MultiClient.Net.Enums;
using MessengerRando.Archipelago;
using MessengerRando.Extensions;
using MessengerRando.GameOverrideManagers;
using MessengerRando.Lifecycle;
using MessengerRando.Overrides;
using MessengerRando.Utils;
using MessengerRando.Utils.Menus;
using Mod.Courier;
using Mod.Courier.Module;
using Mod.Courier.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using WebSocketSharp;
using static Mod.Courier.UI.TextEntryButtonInfo;
using Logger = MessengerRando.Utils.Logger;
using Object = UnityEngine.Object;

namespace MessengerRando;

/// <summary>
/// Where it all begins! This class defines and injects all the necessary for the mod.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class APRandomizerMain : CourierModule
{
    private static readonly Logger logger = Logger.GetLogger<APRandomizerMain>();
    private float updateTimer;
    public static float UpdateTime = 3.0f;

    private TextMeshProUGUI apTextDisplay8;
    private TextMeshProUGUI apTextDisplay16;
    private TextMeshProUGUI apMessagesDisplay8;
    private TextMeshProUGUI apMessagesDisplay16;
    public static string ModPath = Courier.ModsFolder.Replace("/Mods", "/");

    private TextEntryPopup closeLater;
    private float closeLaterTimer;

    //Set up save data
    public override Type ModuleSaveType => typeof(RandoSave);

    // ReSharper disable once MemberCanBePrivate.Global
    public RandoSave Save => (RandoSave)ModuleSave;

    public override void Load()
    {
        Thread.CurrentThread.Name = "GameThread";
        logger.Log("Randomizer loading and ready to try things!");

        // Client
        var trackerManager = ServiceLocator.Register(new TrackerManager());

        // RandoManagers
        var skylandsGeneratorManager = ServiceLocator.Register(new SkylandsGeneratorManager());
        var randoStateManager = ServiceLocator.Register(new RandomizerStateManager());
        var randoBossManager = ServiceLocator.Register(new RandoBossManager());
        var randoGoalManager = ServiceLocator.Register(new RandoGoalManager());
        var lostWoodsManager = ServiceLocator.Register(new LostWoodsManager());

        // Overrides
        var autumnHillsOverrides = ServiceLocator.Register(new AutumnHillsOverrides());
        var catacombsOverrides = ServiceLocator.Register(new CatacombsOverrides());

        // Tooling
        var prefabHunter = ServiceLocator.Register(new PrefabHunter());
        var teleporter = ServiceLocator.Register(new Teleporter());

        foreach (var item in ServiceLocator.GetAll<IOnModLoadHandler>())
            item.OnModLoad();

        //Plug in my code :3
        On.InventoryManager.AddItem += SafeHook.Wrap<On.InventoryManager.hook_AddItem>(InventoryManager_AddItem);
        On.ProgressionManager.SetChallengeRoomAsCompleted +=
            SafeHook.Wrap<On.ProgressionManager.hook_SetChallengeRoomAsCompleted>(
                ProgressionManager_SetChallengeRoomAsCompleted
            );
        On.HasItem.IsTrue += SafeHook.Wrap<On.HasItem.hook_IsTrue>(HasItem_IsTrue);
        On.AwardNoteCutscene.ShouldPlay += SafeHook.Wrap<On.AwardNoteCutscene.hook_ShouldPlay>(
            AwardNoteCutscene_ShouldPlay
        );
        On.CutsceneHasPlayed.IsTrue += SafeHook.Wrap<On.CutsceneHasPlayed.hook_IsTrue>(CutsceneHasPlayed_IsTrue);
        On.SaveGameSelectionScreen.OnLoadGame += SafeHook.Wrap<On.SaveGameSelectionScreen.hook_OnLoadGame>(
            SaveGameSelectionScreen_OnLoadGame
        );
        On.SaveGameSelectionScreen.OnNewGame += SafeHook.Wrap<On.SaveGameSelectionScreen.hook_OnNewGame>(
            SaveGameSelectionScreen_OnNewGame
        );
        On.SaveGameSelectionScreen.ConfirmSaveDelete +=
            HookMonitor.Log<On.SaveGameSelectionScreen.hook_ConfirmSaveDelete>();
        On.SaveGameSelectionScreen.OnDeleteChoiceDone +=
            SafeHook.Wrap<On.SaveGameSelectionScreen.hook_OnDeleteChoiceDone>(SaveGameSelectionScreen_OnDelete);
        On.SaveGameSelectionScreen.Update += SafeHook.Wrap<On.SaveGameSelectionScreen.hook_Update>(
            SaveSelectionScreen_OnUpdate
        );
        On.NameSavePopup.Update += SafeHook.Wrap<On.NameSavePopup.hook_Update>(OnNameSaveUpdate);
        On.BackToTitleScreen.GoBackToTitleScreen += SafeHook.Wrap<On.BackToTitleScreen.hook_GoBackToTitleScreen>(
            PauseScreen_OnQuitToTitle
        );

        On.DialogCutscene.Play += SafeHook.Wrap<On.DialogCutscene.hook_Play>(DialogCutscene_Play);
        On.DialogManager.LoadDialogs_ELanguage += SafeHook.Wrap<On.DialogManager.hook_LoadDialogs_ELanguage>(
            DialogChanger.LoadDialogs_Elanguage
        );
        On.OptionScreen.OnEnable += SafeHook.Wrap<On.OptionScreen.hook_OnEnable>(OnOptionScreenEnable);
        // shop management
        On.UpgradeButtonData.GetPrice += SafeHook.Wrap<On.UpgradeButtonData.hook_GetPrice>(RandoShopManager.GetPrice);
        On.LocalizationManager.GetText += SafeHook.Wrap<On.LocalizationManager.hook_GetText>(RandoShopManager.GetText);
        // On.UpgradeButton.Refresh += SafeHook.WrapSafe<On.UpgradeButton.hook_Refresh>(RandoShopManager.UpgradeButton_Refresh);
        On.BuyMoneyWrenchCutscene.OnBuyWrenchChoice += SafeHook.Wrap<On.BuyMoneyWrenchCutscene.hook_OnBuyWrenchChoice>(
            RandoShopManager.BuyMoneyWrench
        );
        On.BuyMoneyWrenchCutscene.EndCutsceneOnDialogDone +=
            SafeHook.Wrap<On.BuyMoneyWrenchCutscene.hook_EndCutsceneOnDialogDone>(
                RandoShopManager.EndMoneyWrenchCutscene
            );
        On.MoneySinkUnclogCutscene.OnDialogOutDone += SafeHook.Wrap<On.MoneySinkUnclogCutscene.hook_OnDialogOutDone>(
            RandoShopManager.UnclogSink
        );
        On.GoToSousSolCutscene.EndCutScene += SafeHook.Wrap<On.GoToSousSolCutscene.hook_EndCutScene>(
            RandoShopManager.GoToSousSol
        );
        On.IronHoodShopScreen.GetFigurineData += SafeHook.Wrap<On.IronHoodShopScreen.hook_GetFigurineData>(
            RandoShopManager.GetFigurineData
        );
        On.SousSol.UnlockFigurine += SafeHook.Wrap<On.SousSol.hook_UnlockFigurine>(RandoShopManager.UnlockFigurine);
        On.Shop.Init += SafeHook.Wrap<On.Shop.hook_Init>(RandoShopManager.ShopInit);
        On.JukeboxTrack.IsUnlocked += SafeHook.Wrap<On.JukeboxTrack.hook_IsUnlocked>((orig, self) => true);
        On.UpgradeButtonData.IsStoryUnlocked += SafeHook.Wrap<On.UpgradeButtonData.hook_IsStoryUnlocked>(
            RandoShopManager.IsStoryUnlocked
        );
        On.AudioManager.PlayMusic += SafeHook.Wrap<On.AudioManager.hook_PlayMusic>(RandoMusicManager.OnPlayMusic);
        // boss management
        On.ProgressionManager.HasDefeatedBoss += SafeHook.Wrap<On.ProgressionManager.hook_HasDefeatedBoss>(
            (orig, self, bossName) => ServiceLocator.Get<RandoBossManager>().HasBossDefeated(bossName)
        );
        On.ProgressionManager.HasEverDefeatedBoss += SafeHook.Wrap<On.ProgressionManager.hook_HasEverDefeatedBoss>(
            (orig, self, bossName) => ServiceLocator.Get<RandoBossManager>().HasBossDefeated(bossName)
        );
        On.ProgressionManager.SetBossAsDefeated += SafeHook.Wrap<On.ProgressionManager.hook_SetBossAsDefeated>(
            (orig, self, bossName) => ServiceLocator.Get<RandoBossManager>().SetBossAsDefeated(bossName)
        );
        // level teleporting etc management
        On.Level.ChangeRoom += SafeHook.Wrap<On.Level.hook_ChangeRoom>(RandoRoomManager.Level_ChangeRoom);

        RandoLevelManager.ApplyHooks();
        RandoPortalManager.ApplyHooks();

        //update loops for Archipelago
        Courier.Events.PlayerController.OnUpdate += SafeHook.Wrap(PlayerController_OnUpdate);
        On.InGameHud.OnGUI += SafeHook.Wrap<On.InGameHud.hook_OnGUI>(InGameHud_OnGUI);
        On.SaveManager.DoActualSaving += SafeHook.Wrap<On.SaveManager.hook_DoActualSaving>(SaveManager_DoActualSave);
        On.PlayerController.Die += SafeHook.Wrap<On.PlayerController.hook_Die>(OnPlayerDie);
        On.Quarble.OnDeathScreenDone += SafeHook.Wrap<On.Quarble.hook_OnDeathScreenDone>(OnDeathScreenDone);
        // On.MegaTimeShard.NextState += RandoTimeShardManager.NextState;
        // On.MegaTimeShard.ReceiveHit += RandoTimeShardManager.ReceiveHit;
        On.MegaTimeShard.OnBreakDone += SafeHook.Wrap<On.MegaTimeShard.hook_OnBreakDone>(MegaTimeShard_OnBreakDone);
        On.DialogSequence.GetDialogList += SafeHook.Wrap<On.DialogSequence.hook_GetDialogList>(
            DialogSequence_GetDialogList
        );
        On.Cutscene.Play += SafeHook.Wrap<On.Cutscene.hook_Play>(Cutscene_Play);

#if DEBUG
        On.PhantomIntroCutscene.OnEnterRoom += SafeHook.Wrap<On.PhantomIntroCutscene.hook_OnEnterRoom>(
            PhantomIntro_OnEnterRoom
        );
#endif

        On.UIManager.ShowView += HookMonitor.Debug<On.UIManager.hook_ShowView>();
        On.MusicBox.SetNotesState += HookMonitor.Debug<On.MusicBox.hook_SetNotesState>();

        logger.Log("Randomizer finished loading!");
    }

    private void SaveSelectionScreen_OnUpdate(On.SaveGameSelectionScreen.orig_Update orig, SaveGameSelectionScreen self)
    {
        orig(self);
        if (closeLater)
        {
            closeLaterTimer += Time.deltaTime;
            if (closeLaterTimer >= 3)
            {
                closeLater.gameObject.SetActive(false);
                closeLater = null;
            }
        }
    }

    private void OnNameSaveUpdate(On.NameSavePopup.orig_Update orig, NameSavePopup self)
    {
        self.OnLetterErased();
    }

    private void OnOptionScreenEnable(On.OptionScreen.orig_OnEnable orig, OptionScreen self)
    {
        if (ServiceLocator.Get<RandomizerStateManager>().APSave == null)
            RandoSave.TryLoad(Save.APSaveData);
        orig(self);
    }

    public override void Initialize()
    {
#if DEBUG
        SceneManager.sceneLoaded += OnSceneLoadedRando;
#endif

        //load config
        logger.Log("Loading config from APConfig.toml");
        try
        {
            UserConfig.ReadConfig(ModPath);
        }
        catch (Exception e)
        {
            logger.Exception(e);
        }
        ArchipelagoMenu.BuildArchipelagoMenu();
        RandoMenu.BuildRandoMenu();
        HintMenu.BuildHintMenu();
    }

    List<DialogInfo> DialogSequence_GetDialogList(On.DialogSequence.orig_GetDialogList orig, DialogSequence self)
    {
        //Using this function to add some of my own dialog stuff to the game.
        if (ArchipelagoClient.HasConnected && new[] { "ARCHIPELAGO_ITEM", "DEATH_LINK" }.Contains(self.dialogID))
        {
            var dialogInfoList = new List<DialogInfo>();
            var dialog = new DialogInfo();
            switch (self.dialogID)
            {
                case "ARCHIPELAGO_ITEM":
                    dialog.text = self.name;
                    break;
                case "DEATH_LINK":
                    dialog.text = $"Deathlink: {self.name}";
                    break;
            }

            dialogInfoList.Add(dialog);

            return dialogInfoList;
        }

        return orig(self);
    }

    void InventoryManager_AddItem(
        On.InventoryManager.orig_AddItem orig,
        InventoryManager self,
        EItems itemId,
        int quantity
    )
    {
        if (!itemId.Equals(EItems.TIME_SHARD))
        {
            logger.Log(
                "Called InventoryManager_AddItem method. Looking to give x{0} amount of item '{1}'.",
                quantity,
                itemId
            );
            if (quantity == ItemsAndLocationsHandler.APQuantity)
            {
                orig(self, itemId, 1);
                return;
            }
            var randoStateManager = ServiceLocator.Get<RandomizerStateManager>();
            if (randoStateManager.IsLocationRandomized(itemId, out var randoItemCheck))
            {
                if (
                    itemId.Equals(EItems.CANDLE)
                    && randoStateManager.IsLocationRandomized(EItems.TEA_SEED, out var seedCheck)
                    && !RandomizerStateManager.HasCompletedCheck(seedCheck)
                )
                {
                    ItemsAndLocationsHandler.SendLocationCheck(seedCheck);
                }
                ItemsAndLocationsHandler.SendLocationCheck(randoItemCheck);
                return;
            }
        }
        //Call original add with items
        orig(self, itemId, quantity);
    }

    void ProgressionManager_SetChallengeRoomAsCompleted(
        On.ProgressionManager.orig_SetChallengeRoomAsCompleted orig,
        ProgressionManager self,
        string roomKey
    )
    {
        //if this is a rando file, go ahead and give the item we expect to get
        if (ArchipelagoClient.HasConnected)
        {
            var powerSealLocation = ItemsAndLocationsHandler.ArchipelagoLocations.Find(loc => loc.Equals(roomKey));
            ItemsAndLocationsHandler.SendLocationCheck(powerSealLocation);
        }
        //For now calling the orig method once we are done so the game still things we are collecting seals. We can change this later.
        orig(self, roomKey);
    }

    bool HasItem_IsTrue(On.HasItem.orig_IsTrue orig, HasItem self)
    {
        bool hasItem = false;
        //Check to make sure this is an item that was randomized and make sure we are not ignoring this specific trigger check
        var randoStateManager = ServiceLocator.Get<RandomizerStateManager>();
        if (
            ArchipelagoClient.HasConnected
            && randoStateManager.IsLocationRandomized(self.item, out var check)
            && !RandomizerConstants.GetSpecialTriggerNames().Contains(self.Owner.name)
        )
        {
            if (
                self.transform.parent != null
                && "InteractionZone".Equals(self.Owner.name)
                && RandomizerConstants.GetSpecialTriggerNames().Contains(self.transform.parent.name)
                && EItems.KEY_OF_LOVE != self.item
            )
            {
                //Special triggers that need to use normal logic, call orig method. This also includes the trigger check for the key of love on the sunken door because yeah.
                logger.Log(
                    $"While checking if player HasItem in an interaction zone, found parent object '{self.transform.parent.name}' in ignore logic. Calling orig HasItem logic."
                );
                return orig(self);
            }

            //OLD WAY
            //Don't actually check for the item i have, check to see if I have the item that was at it's location.
            //int itemQuantity = Manager<InventoryManager>.Instance.GetItemQuantity(randoStateManager.CurrentLocationToItemMapping[check].Item);

            //NEW WAY
            //Don't actually check for the item I have, check to see if I have done this check before. We'll do this by seeing if the item at its location has been collected yet or not
            int itemQuantity = RandomizerStateManager.HasCompletedCheck(check) ? 1 : 0;
            if (self.item.Equals(EItems.CANDLE) && itemQuantity == 1)
            {
                var seed = ItemsAndLocationsHandler.LocationFromEItem(EItems.TEA_SEED);
                var leaves = ItemsAndLocationsHandler.LocationFromEItem(EItems.TEA_LEAVES);
                itemQuantity =
                    RandomizerStateManager.HasCompletedCheck(seed) && RandomizerStateManager.HasCompletedCheck(leaves)
                        ? 1
                        : 0;
            }

            switch (self.conditionOperator)
            {
                case EConditionOperator.LESS_THAN:
                    hasItem = itemQuantity < self.quantityToHave;
                    break;
                case EConditionOperator.LESS_OR_EQUAL:
                    hasItem = itemQuantity <= self.quantityToHave;
                    break;
                case EConditionOperator.EQUAL:
                    hasItem = itemQuantity == self.quantityToHave;
                    break;
                case EConditionOperator.GREATER_OR_EQUAL:
                    hasItem = itemQuantity >= self.quantityToHave;
                    break;
                case EConditionOperator.GREATER_THAN:
                    hasItem = itemQuantity > self.quantityToHave;
                    break;
            }
            return hasItem;
        }
        logger.Log("HasItem check was not randomized. Doing vanilla checks.");
        logger.Log(
            $"Is randomized file : '{ArchipelagoClient.HasConnected}' | Is location '{self.item}' randomized: '{randoStateManager.IsLocationRandomized(self.item, out check)}' | Not in the special triggers list: '{!RandomizerConstants.GetSpecialTriggerNames().Contains(self.Owner.name)}'|"
        );
        return orig(self);
    }

    bool AwardNoteCutscene_ShouldPlay(On.AwardNoteCutscene.orig_ShouldPlay orig, AwardNoteCutscene self)
    {
        //Need to handle note cutscene triggers so they will play as long as I dont have the actual item it grants
        if (!ServiceLocator.Get<RandomizerStateManager>().IsLocationRandomized(self.noteToAward, out var noteCheck))
            return orig(self);
        var shouldPlay = !ArchipelagoClient.ServerData.CheckedLocations.Contains(noteCheck);
        if (shouldPlay)
            ItemsAndLocationsHandler.SendLocationCheck(noteCheck);
        return shouldPlay;
    }

    bool CutsceneHasPlayed_IsTrue(On.CutsceneHasPlayed.orig_IsTrue orig, CutsceneHasPlayed self)
    {
        if (
            RandomizerConstants.GetCutsceneMappings().ContainsKey(self.cutsceneId)
            && ServiceLocator
                .Get<RandomizerStateManager>()
                .IsLocationRandomized(RandomizerConstants.GetCutsceneMappings()[self.cutsceneId], out var cutsceneCheck)
        )
        {
            return RandomizerStateManager.HasCompletedCheck(cutsceneCheck);
        }
        return orig(self);
    }

    void SaveGameSelectionScreen_OnLoadGame(
        On.SaveGameSelectionScreen.orig_OnLoadGame orig,
        SaveGameSelectionScreen self,
        int slotIndex
    )
    {
        //slotIndex is 0-based, going to increment it locally to keep things simple.
        var randoStateManager = ServiceLocator.Get<RandomizerStateManager>();
        randoStateManager.CurrentFileSlot = slotIndex + 1;

        //This is probably a bad way to do this
        try
        {
            RandoSave.TryLoad(Save.APSaveData);
            if (ArchipelagoData.LoadData(randoStateManager.CurrentFileSlot))
            {
                if (!ArchipelagoClient.Offline)
                {
                    // need to wait for the scout response from the server
                    while (
                        ArchipelagoClient.Authenticated
                        && (randoStateManager.ScoutedLocations == null || randoStateManager.ScoutedLocations.Count < 1)
                    )
                    {
                        logger.Log("locations not scouted yet. waiting...");
                        Thread.Sleep(100);
                    }
                }
                Manager<DialogManager>.Instance.LoadDialogs(Manager<LocalizationManager>.Instance.CurrentLanguage);
                //The player is connected to an Archipelago server and trying to load a save file so check it's valid
                logger.Log($"Successfully loaded Archipelago seed {randoStateManager.CurrentFileSlot}");
                logger.Log("Current Inventory:");
                foreach (var item in randoStateManager.APSave[randoStateManager.CurrentFileSlot].ReceivedItems.Keys)
                {
                    logger.Log(
                        $"{item}: {randoStateManager.APSave[randoStateManager.CurrentFileSlot].ReceivedItems[item]}"
                    );
                }

                foreach (var handler in ServiceLocator.GetAll<ISaveLifecycleHandler>())
                    handler.OnLoad(randoStateManager.APSave[randoStateManager.CurrentFileSlot]);
            }
            else if (
                ArchipelagoClient.Authenticated
                && string.IsNullOrEmpty(randoStateManager.APSave[randoStateManager.CurrentFileSlot].SlotName)
            )
            {
                ArchipelagoClient.ServerData.StartNewSeed();
                //We force a reload of all dialog when loading the game
                try
                {
                    Manager<DialogManager>.Instance.LoadDialogs(Manager<LocalizationManager>.Instance.CurrentLanguage);
                }
                catch (Exception e)
                {
                    logger.Exception(e);
                }

                foreach (var handler in ServiceLocator.GetAll<ISaveLifecycleHandler>())
                    handler.OnLoad(randoStateManager.APSave[randoStateManager.CurrentFileSlot]);
            }
            else if (ArchipelagoClient.Offline)
            {
                if (ItemsAndLocationsHandler.ItemsLookup == null)
                    ItemsAndLocationsHandler.Initialize();
                Manager<DialogManager>.Instance.LoadDialogs(Manager<LocalizationManager>.Instance.CurrentLanguage);
            }
        }
        catch (Exception e)
        {
            logger.Exception(e);
        }
        if (!ArchipelagoClient.HasConnected)
        {
            logger.Log(
                $"This file slot ({randoStateManager.CurrentFileSlot}) has no seed generated or is not "
                    + "a randomized file. Resetting the mappings and putting game items back to normal."
            );
            ArchipelagoClient.ServerData = new ArchipelagoData();
        }
        else
        {
            RandomizerStateManager.OnMainMenu = false;
        }

        orig(self, slotIndex);
        Manager<AudioManager>.Instance.levelMusicShuffle = RandoMusicManager.ShuffleMusic;
        RandoMusicManager.BuildMusicLibrary();
    }

    void SaveGameSelectionScreen_OnNewGame(
        On.SaveGameSelectionScreen.orig_OnNewGame orig,
        SaveGameSelectionScreen self,
        SaveSlotUI slot
    )
    {
        logger.Log("trying to load new game");
        if (ArchipelagoClient.Authenticated)
            RandomizerStateManager.InitializeNewSecondQuest(self, slot.slotIndex);
        else if (ArchipelagoClient.Offline)
        {
            try
            {
                RandomizerStateManager.InitializeNewSecondQuest(self, slot.slotIndex);
            }
            catch (Exception e)
            {
                logger.Exception(e);
            }
        }
        else if (Environment.GetCommandLineArgs().Length > 1)
        {
            logger.Log("loading new game save... found command line args");
            foreach (var arg in Environment.GetCommandLineArgs())
            {
                logger.Log(arg);
                if (!arg.Contains("archipelago"))
                    continue;
                var uri = new Uri(arg);
                ArchipelagoClient.ServerData = new ArchipelagoData();
                var userInfo = uri.UserInfo.Split(':');
                ArchipelagoClient.ServerData.SlotName = userInfo[0];
                ArchipelagoClient.ServerData.Password = userInfo[1] == "None" ? "" : userInfo[1];
                ArchipelagoClient.ServerData.Uri = uri.Host;
                ArchipelagoClient.ServerData.Port = uri.Port;
                logger.Log(ArchipelagoClient.ServerData.SlotName);
                logger.Log(ArchipelagoClient.ServerData.Password);
                logger.Log(ArchipelagoClient.ServerData.Uri);
                logger.Log(ArchipelagoClient.ServerData.Port);
                var result = ArchipelagoClient.Connect();
                if (ArchipelagoClient.Authenticated)
                {
                    RandomizerStateManager.InitializeNewSecondQuest(self, slot.slotIndex);
                    return;
                }
                break;
            }
            orig(self, slot);
            self.nameSavePopup.OnLetterErased();
            closeLater = InitTextEntryPopup(
                self,
                "Not connected to an Archipelago server. Please connect before continuing.",
                _ => true,
                0,
                null,
                CharsetFlags.Space
            );
            closeLater.Init("");
            closeLater.gameObject.SetActive(true);
            closeLaterTimer = 0f;
        }
        else
        {
            orig(self, slot);
            self.nameSavePopup.OnLetterErased();
            closeLater = InitTextEntryPopup(self, "", _ => true, 0, null, CharsetFlags.Space);
            closeLater.Init("Not connected to an Archipelago server. Please connect before continuing.");
            closeLater.gameObject.SetActive(true);
            closeLaterTimer = 0f;
        }
    }

    private void SaveGameSelectionScreen_OnDelete(
        On.SaveGameSelectionScreen.orig_OnDeleteChoiceDone orig,
        SaveGameSelectionScreen self,
        bool delete
    )
    {
        if (delete)
        {
            RandoSave.TryLoad(Save.APSaveData);
            var slotIndex = self.GetPrivateField<SaveSlotUI>("focusedSlot").slotIndex + 1;
            ServiceLocator.Get<RandomizerStateManager>().APSave[slotIndex] = new ArchipelagoData();
            Save?.ForceUpdate();
        }
        orig(self, delete);
    }

    void PauseScreen_OnQuitToTitle(On.BackToTitleScreen.orig_GoBackToTitleScreen orig)
    {
        if (ArchipelagoClient.HasConnected)
        {
            ArchipelagoClient.Disconnect();
            ArchipelagoClient.HasConnected = false;
            ArchipelagoClient.Offline = false;
            ArchipelagoClient.OfflineReceivedItems = 0;
            RandoBossManager.DefeatedBosses = [];
            RandoPortalManager.StartingPortals = null;
            RandoLevelManager.Reset();
            Manager<ProgressionManager>.Instance.powerSealTotal = 0;
            HintMenu.ReBuildHintMenu();

            foreach (var handler in ServiceLocator.GetAll<IOnBackToTitleHandler>())
                handler.OnBackToTitle();
        }

        ServiceLocator.Register(new RandomizerStateManager());
        ArchipelagoClient.ServerData = new ArchipelagoData();
        RandomizerStateManager.OnMainMenu = true;
        orig();
        logger.Log("returned to title");
        Manager<UIManager>.Instance.GetView<InGameHud>().UpdateShurikenVisibility();
    }

    void MegaTimeShard_OnBreakDone(On.MegaTimeShard.orig_OnBreakDone orig, MegaTimeShard self)
    {
        var currentLevel = Manager<LevelManager>.Instance.GetCurrentLevelEnum();
        var currentRoom = Manager<Level>.Instance.CurrentRoom.roomKey;
        RandoTimeShardManager.BreakShard(new RandoTimeShardManager.MegaShard(currentLevel, currentRoom));
        orig(self);
    }

    void Cutscene_Play(On.Cutscene.orig_Play orig, Cutscene self)
    {
        var eventName = self.GetType().ToString();
        logger.Log($"Playing cutscene: {eventName}");
        if (eventName == "PortalOpeningCutscene")
        {
            if (RandoPortalManager.StartingPortals != null)
            {
                var progManager = Manager<ProgressionManager>.Instance;
                foreach (
                    var portal in RandoPortalManager.StartingPortals.Where(portal =>
                        !portal.StartsWith("Autumn") && !portal.StartsWith("Howling") && !portal.StartsWith("Glacial")
                    )
                )
                {
                    var cutsceneName = $"{portal.Replace(" ", "")}OpeningCutscene";
                    progManager.cutscenesPlayed.Add(cutsceneName);
                }
            }
        }

        if (ArchipelagoClient.EventsICareAbout.Contains(eventName) && ArchipelagoClient.Authenticated)
        {
            ArchipelagoClient.Session.DataStorage[Scope.Slot, "Events"] += new List<string> { eventName };
        }

        if (eventName.EndsWith("PortalOpeningCutscene"))
        {
            self.onDone += OnAnyPortalOpeningCutsceneDone;
        }

        orig(self);
    }

    private void OnAnyPortalOpeningCutsceneDone(Cutscene cutscene)
    {
        Manager<LevelManager>.Instance.GetCurrentLevelEnum();
        cutscene.onDone -= OnAnyPortalOpeningCutsceneDone;
        ServiceLocator.Get<TrackerManager>().ReconciliateUnlockedPortals();
    }

    void PhantomIntro_OnEnterRoom(
        On.PhantomIntroCutscene.orig_OnEnterRoom orig,
        PhantomIntroCutscene self,
        bool teleportedInRoom
    )
    {
        if (ServiceLocator.Get<RandomizerStateManager>().SkipPhantom)
        {
            Manager<AudioManager>.Instance.StopMusic();
            Object.FindObjectOfType<PhantomOutroCutscene>().Play();
        }
        else
        {
            orig(self, teleportedInRoom);
        }
    }

    void DialogCutscene_Play(On.DialogCutscene.orig_Play orig, DialogCutscene self)
    {
        //ruxxtin cutscene is being a bitch so just gonna hard code around it here.
        if (ArchipelagoClient.HasConnected && self.name.Equals("ReadNote"))
        {
            if (ServiceLocator.Get<RandomizerStateManager>().IsLocationRandomized(EItems.RUXXTIN_AMULET, out var locID))
            {
                if (!RandomizerStateManager.HasCompletedCheck(locID))
                {
                    ItemsAndLocationsHandler.SendLocationCheck(locID);
                }
            }
        }
        orig(self);
    }

    public static void OnToggleWindmillShuriken()
    {
        Manager<ProgressionManager>.Instance.useWindmillShuriken = !Manager<ProgressionManager>
            .Instance
            .useWindmillShuriken;
        InGameHud view = Manager<UIManager>.Instance.GetView<InGameHud>();
        view?.UpdateShurikenVisibility();
    }

    public static bool OnSelectArchipelagoHost(string answer)
    {
        if (answer == null)
            return true;
        if (ArchipelagoClient.ServerData == null)
            ArchipelagoClient.ServerData = new ArchipelagoData();
        var uri = answer;
        ArchipelagoClient.ServerData.Uri = uri;
        return true;
    }

    public static bool OnSelectArchipelagoPort(string answer)
    {
        if (answer == null)
            return true;
        if (ArchipelagoClient.ServerData == null)
            ArchipelagoClient.ServerData = new ArchipelagoData();
        int.TryParse(answer, out var port);
        ArchipelagoClient.ServerData.Port = port;
        return true;
    }

    public static bool ArchipelagoPortEnabled()
    {
        if (Manager<LevelManager>.Instance.GetCurrentLevelEnum().Equals(ELevel.NONE))
        {
            return !ArchipelagoClient.HasConnected;
        }

        return ArchipelagoClient.HasConnected && !ArchipelagoClient.Authenticated && !ArchipelagoClient.Offline;
    }

    public static bool OnSelectArchipelagoName(string answer)
    {
        if (answer == null)
            return true;
        if (ArchipelagoClient.ServerData == null)
            ArchipelagoClient.ServerData = new ArchipelagoData();
        ArchipelagoClient.ServerData.SlotName = answer;
        return true;
    }

    public static bool OnSelectArchipelagoPass(string answer)
    {
        if (answer == null)
            return true;
        if (ArchipelagoClient.ServerData == null)
            ArchipelagoClient.ServerData = new ArchipelagoData();
        ArchipelagoClient.ServerData.Password = answer;
        return true;
    }

    public static void OnSelectArchipelagoConnect()
    {
        ArchipelagoClient.ServerData ??= new ArchipelagoData();
        if (!UserConfig.SlotName.IsNullOrEmpty())
        {
            ArchipelagoClient.ServerData.Uri = UserConfig.HostName;
            ArchipelagoClient.ServerData.Port = UserConfig.Port;
            ArchipelagoClient.ServerData.SlotName = UserConfig.SlotName;
            ArchipelagoClient.ServerData.Password = UserConfig.Password;
        }
        else if (ArchipelagoClient.ServerData.SlotName.IsNullOrEmpty())
        {
            return;
        }

        ArchipelagoClient.ConnectAsync(ArchipelagoMenu.ArchipelagoConnectButton);
    }

    public static void OnSelectArchipelagoRelease()
    {
        ArchipelagoClient.SendSayPacket("!release");
    }

    public static void OnSelectArchipelagoCollect()
    {
        ArchipelagoClient.SendSayPacket("!collect");
    }

    public static bool OnSelectArchipelagoHint(string answer)
    {
        ArchipelagoClient.SendSayPacket($"!hint {answer}");
        return true;
    }

    public static void OnToggleAPStatus()
    {
        ArchipelagoClient.DisplayStatus = !ArchipelagoClient.DisplayStatus;
    }

    public static void OnToggleAPMessages()
    {
        ArchipelagoClient.DisplayAPMessages = !ArchipelagoClient.DisplayAPMessages;
    }

    public static void OnToggleDeathLink()
    {
        ArchipelagoData.DeathLink = !ArchipelagoData.DeathLink;
        if (ArchipelagoData.DeathLink)
            ArchipelagoClient.DeathLinkHandler.DeathLinkService.EnableDeathLink();
        else
            ArchipelagoClient.DeathLinkHandler.DeathLinkService.DisableDeathLink();
    }

    public static bool OnSelectMessageTimer(string answer)
    {
        if (answer == null)
            return true;
        if (ArchipelagoClient.ServerData == null)
            ArchipelagoClient.ServerData = new ArchipelagoData();
        int.TryParse(answer, out var newTime);
        UpdateTime = newTime;
        return true;
    }

    public static string GetCurrentSeedNum()
    {
        string seedNum = "Unknown";

        if (ArchipelagoClient.HasConnected)
        {
            seedNum = ArchipelagoClient.ServerData.SeedName;
        }

        return seedNum;
    }

    private void OnSceneLoadedRando(Scene scene, LoadSceneMode mode)
    {
        logger.Log($"Scene loaded: '{scene.name}'");
    }

    private void PlayerController_OnUpdate(PlayerController controller)
    {
        if (!ArchipelagoClient.HasConnected || ServiceLocator.Get<RandomizerStateManager>().CurrentFileSlot == 0)
        {
            return;
        }
        if (ArchipelagoClient.Authenticated)
        {
            if (ArchipelagoClient.DeathLinkHandler.Player == null)
            {
                ArchipelagoClient.DeathLinkHandler.Player = controller;
            }

            if (RandomizerStateManager.IsSafeTeleportState() && !Manager<PauseManager>.Instance.IsPaused)
            {
                ArchipelagoClient.DeathLinkHandler.KillPlayer();
            }
        }
        //This updates every {updateTime} seconds
        updateTimer += Time.deltaTime;
        TrapManager.TrapTimer += Time.deltaTime;
        if (!(updateTimer >= UpdateTime))
            return;
        updateTimer = 0;
        UpdateArchipelagoState();
        apMessagesDisplay16.text = apMessagesDisplay8.text = ArchipelagoClient.UpdateMessagesText();
    }

    public void UpdateArchipelagoState()
    {
        while (ArchipelagoClient.ItemQueue.Count > 0)
        {
            ItemsAndLocationsHandler.Unlock((long)ArchipelagoClient.ItemQueue.Dequeue());
        }

        if (ArchipelagoClient.DialogQueue.Count > 0)
        {
            var message = (string)ArchipelagoClient.DialogQueue.Dequeue();
            logger.Log(message);
            DialogChanger.CreateDialogBox(message);
        }

        TrapManager.UpdateTrapStatus();
        if (ArchipelagoClient.Offline)
            return;
        if (!ArchipelagoClient.Authenticated)
        {
            logger.Log("Attempting to reconnect to Archipelago Server...");
            ThreadPool.QueueUserWorkItem(_ => ArchipelagoClient.ConnectAsync());
            return;
        }

        if (ArchipelagoClient.ServerData.Index < ArchipelagoClient.Session.Items.AllItemsReceived.Count)
        {
            ItemsAndLocationsHandler.UnlockItems();
            return;
        }

        if (!ItemsAndLocationsHandler.Synced)
            ItemsAndLocationsHandler.ReSync();
        var trackerManager = ServiceLocator.Get<TrackerManager>();
        if (!trackerManager.Synced)
            trackerManager.ReSync();
    }

    private void InGameHud_OnGUI(On.InGameHud.orig_OnGUI orig, InGameHud self)
    {
        orig(self);
        if (apTextDisplay8 == null)
        {
            apTextDisplay8 = Object.Instantiate(self.hud_8.coinCount, self.hud_8.gameObject.transform);
            apTextDisplay16 = Object.Instantiate(self.hud_16.coinCount, self.hud_16.gameObject.transform);
            apTextDisplay8.rectTransform.Translate(0f, -110f, 0f);
            apTextDisplay16.rectTransform.Translate(0f, -110f, 0f);
            apTextDisplay8.rectTransform.sizeDelta = new Vector2(
                apTextDisplay8.rectTransform.sizeDelta.x + 100f,
                apTextDisplay8.rectTransform.sizeDelta.y
            );
            apTextDisplay16.rectTransform.sizeDelta = new Vector2(
                apTextDisplay16.rectTransform.sizeDelta.x + 100f,
                apTextDisplay16.rectTransform.sizeDelta.y
            );
            apTextDisplay16.fontSize = apTextDisplay8.fontSize = UserConfig.StatusTextSize;
            apTextDisplay16.alignment = apTextDisplay8.alignment = TextAlignmentOptions.TopRight;
            apTextDisplay16.enableWordWrapping = apTextDisplay8.enableWordWrapping = true;
            apTextDisplay16.color = apTextDisplay8.color = Color.white;

            apMessagesDisplay8 = Object.Instantiate(self.hud_8.coinCount, self.hud_8.gameObject.transform);
            apMessagesDisplay16 = Object.Instantiate(self.hud_16.coinCount, self.hud_16.gameObject.transform);
            apMessagesDisplay8.rectTransform.Translate(0f, -200f, 0f);
            apMessagesDisplay16.rectTransform.Translate(0f, -200f, 0f);
            apMessagesDisplay16.fontSize = apMessagesDisplay8.fontSize = UserConfig.MessageTextSize;
            apMessagesDisplay16.alignment = apMessagesDisplay8.alignment = TextAlignmentOptions.BottomRight;
            apMessagesDisplay16.enableWordWrapping = apMessagesDisplay16.enableWordWrapping = true;
            apMessagesDisplay16.color = apMessagesDisplay8.color = Color.green;
            apMessagesDisplay16.text = apMessagesDisplay8.text = string.Empty;
        }

        if (ArchipelagoClient.Offline)
            return;
        //This updates every frame
        apTextDisplay16.fontSize = apTextDisplay8.fontSize = UserConfig.StatusTextSize;
        apMessagesDisplay16.fontSize = apMessagesDisplay8.fontSize = UserConfig.MessageTextSize;
        apTextDisplay16.text = apTextDisplay8.text = ArchipelagoClient.UpdateStatusText();
    }

    private void SaveManager_DoActualSave(
        On.SaveManager.orig_DoActualSaving orig,
        SaveManager self,
        bool applySaveDelay = true
    )
    {
        orig(self, applySaveDelay);
        if (!ArchipelagoClient.HasConnected)
            return;
        Save?.Update();

        // The game calls the save method after the ending cutscene before rolling credits
        if (
            ArchipelagoClient.Authenticated
            && Manager<LevelManager>.Instance.GetCurrentLevelEnum().Equals(ELevel.Level_Ending)
        )
        {
            ArchipelagoClient.UpdateClientStatus(ArchipelagoClientState.ClientGoal);
        }
    }

    [SafeHook(callOrigOnError: true)]
    private void OnPlayerDie(
        On.PlayerController.orig_Die orig,
        PlayerController self,
        EDeathType deathType,
        Transform killedBy
    )
    {
        TrapManager.ResetPlayerState();
        Manager<UIManager>.Instance.CloseAllScreensOfType<AwardItemPopup>(false);
        ArchipelagoClient.DeathLinkHandler?.SendDeathLink(deathType, killedBy);
        orig(self, deathType, killedBy);
    }

    private void OnDeathScreenDone(On.Quarble.orig_OnDeathScreenDone orig, Quarble self)
    {
        orig(self);
        // this code doesn't expect music shuffle lmao
        Manager<AudioManager>.Instance.StopMusic();
    }
}
