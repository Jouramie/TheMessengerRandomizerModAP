using MessengerRando.Archipelago;
using MessengerRando.Extensions;
using MessengerRando.Lifecycle;
using MessengerRando.RO;
using MessengerRando.Utils;
using MonoMod.Cil;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MessengerRando.Overrides;

public class CatacombsOverrides : IOnModLoadHandler
{
    public void OnModLoad()
    {
        On.CatacombLevelInitializer.OnBeforeInitDone +=
            SafeHook.Wrap<On.CatacombLevelInitializer.hook_OnBeforeInitDone>(CatacombLevelInitializer_OnBeforeInitDone);

        IL.RuxxtinNoteAndAwardAmuletCutscene.Play += RuxxtinNoteAndAwardAmuletCutscene_Play;
    }

    private static void CatacombLevelInitializer_OnBeforeInitDone(
        On.CatacombLevelInitializer.orig_OnBeforeInitDone orig,
        CatacombLevelInitializer self
    )
    {
        if (!ArchipelagoClient.HasConnected)
            orig(self);

        long locationID = ItemsAndLocationsHandler.LocationsLookup[new LocationRO("Necro")];
        if (!RandomizerStateManager.HasCompletedCheck(locationID))
        {
            Manager<ProgressionManager>.Instance.cutscenesPlayed.Remove("NecrophobicWorkerCutscene");
            var phobekin = self.necrophobicWorkerCutscene.phobekin;
            var chatPrompt = Object.Instantiate(
                GameObject
                    .Find("/ToForlornSecondQuest/ToForlornCutscene")
                    .GetComponent<CutsceneInteractionZone>()
                    .interactionPrompt
            );

            chatPrompt.transform.SetParent(phobekin);
            chatPrompt.transform.localPosition = new Vector3(-0.5f, 2, 0);

            var lookController = phobekin.gameObject.AddComponent<LookDirectionController>();
            lookController.defaultLookDir = ELeftRightDirection.LEFT;
            lookController.flipMethod = LookDirectionController.EFlipMethod.FLIP_SPRITE;
            lookController.toFlip = [phobekin.GetComponent<SpriteRenderer>()];

            var collider = phobekin.gameObject.AddComponent<BoxCollider2D>();
            collider.offset = new Vector2(0, 1);
            collider.size = new Vector2(4.5f, 2f);

            var cutsceneInteractionZone = phobekin.gameObject.AddComponent<CutsceneInteractionZone>();
            cutsceneInteractionZone.interactionPrompt = chatPrompt;
            cutsceneInteractionZone.singleDimension = true;
            cutsceneInteractionZone.dimensions = EBits.BITS_8;

            var phobekinCutscene = phobekin.gameObject.AddComponent<PhobekinCollectCutscene>();
            phobekinCutscene.phobekinDialogId = "NECRO_PHOBEKIN_DIALOG";
            phobekinCutscene.phobekinAnimator = phobekin.GetComponent<Animator>();
            phobekinCutscene.phobekinType = EItems.NECROPHOBIC_WORKER;
            phobekinCutscene.phobekinLookDirectionController = lookController;
            phobekinCutscene.onDone += MarkNecrophobicWorkerCutsceneAsPlayed;

            var cutsceneConditionGroup = new CutsceneConditionGroup();
            cutsceneInteractionZone.cutscenes = [cutsceneConditionGroup];
            var condition = phobekin.gameObject.AddComponent<CutsceneHasPlayed>();
            condition.mustHavePlayed = false;
            condition.cutsceneId = typeof(NecrophobicWorkerCutscene).ToString();
            cutsceneConditionGroup.conditions = [condition];
            cutsceneConditionGroup.cutscene = phobekinCutscene;
        }
        else
        {
            self.necrophobicWorkerCutscene.gameObject.SetActive(false);
        }

        self.InvokeMethod("FixPlayerStuckInChallengeRoom");
    }

    private static void MarkNecrophobicWorkerCutsceneAsPlayed(Cutscene cutscene)
    {
        cutscene.onDone -= MarkNecrophobicWorkerCutsceneAsPlayed;
        Manager<ProgressionManager>.Instance.SetCutsceneAsPlayed<NecrophobicWorkerCutscene>();
    }

    // ?????
    private static void RuxxtinNoteAndAwardAmuletCutscene_Play(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);

        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(55)))
        {
            cursor.EmitDelegate(GetRandoItemByItem);
        }
    }

    private static EItems GetRandoItemByItem(EItems item)
    {
        return !ServiceLocator.Get<RandomizerStateManager>().IsLocationRandomized(item, out var ruxxAmuletLocation)
            ? item
            : EItems.POTION;
    }
}
