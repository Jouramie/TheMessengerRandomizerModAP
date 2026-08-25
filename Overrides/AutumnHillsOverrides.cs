using MessengerRando.Extensions;
using MessengerRando.GameOverrideManagers;
using MessengerRando.Lifecycle;
using MessengerRando.Utils;
using UnityEngine;

namespace MessengerRando.Overrides;

public class AutumnHillsOverrides : IOnModLoadHandler
{
    public void OnModLoad()
    {
        On.Level.Start += SafeHook.Wrap<On.Level.hook_Start>(Level_Start);
    }

    private void Level_Start(On.Level.orig_Start orig, Level self)
    {
        orig(self);
        if (Manager<LevelManager>.Instance.CurrentSceneName != ELevel.Level_02_AutumnHills.SceneName)
            return;

        if (!RandoLevelManager.IsTransitionShuffled)
            return;

        var spawner = ServiceLocator.Get<PrefabHunter>().CreateDimensionPortalSpawner();
        spawner.transform.position = new Vector3(963, -22, 0);
        var room = Manager<Level>.Instance.LevelRooms["940972-28-12"];
        room.roomObjects.Add(spawner);

        Manager<MapManager>
            .Instance.LevelMapByLevelName[ELevel.Level_02_AutumnHills][0]
            .mapData.AddRoom(room, spawner.GetComponent<MapRoomObject>());
    }
}
