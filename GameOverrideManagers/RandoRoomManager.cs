using System.Collections.Generic;
using MessengerRando.Archipelago;
using MessengerRando.Utils.Constants;
using WebSocketSharp;
using Logger = MessengerRando.Utils.Logger;

namespace MessengerRando.GameOverrideManagers;

public static class RandoRoomManager
{
    private static readonly Logger logger = Logger.GetLogger(typeof(RandoRoomManager));
    public static bool RoomRando;
    private static bool roomOverride;
    private static Dictionary<string, string> roomMap; // old room name - new room name

    private static readonly List<string> TeleportRoomKeys = ["-500 -148 -60 -44", "364 396 308 324"];

    static string GetRoomKey(ScreenEdge left, ScreenEdge right, ScreenEdge bottom, ScreenEdge top)
    {
        return $"{left.edgeIdX} {right.edgeIdX} {bottom.edgeIdY} {top.edgeIdY}";
    }

    static void SetRoomKey(ScreenEdge left, ScreenEdge right, ScreenEdge bottom, ScreenEdge top, string roomKey)
    {
        var edges = roomKey.Split(' ');
        left.edgeIdX = edges[0];
        right.edgeIdX = edges[1];
        bottom.edgeIdY = edges[2];
        top.edgeIdY = edges[3];
    }

    static bool IsBossRoom(string roomKey, out string bossName)
    {
        logger.Log("Checking if {0} is a boss room", roomKey);
        bossName = string.Empty;
        return roomKey != null
            && BossConstants.RoomToVanillaBoss.TryGetValue(roomKey, out bossName)
            && BossConstants.BossLocations.TryGetValue(bossName, out var bossLocation)
            && bossLocation.BossRegion.Equals(Manager<LevelManager>.Instance.GetCurrentLevelEnum());
    }

    public static void Level_ChangeRoom(
        On.Level.orig_ChangeRoom orig,
        Level self,
        ScreenEdge leftEdge,
        ScreenEdge rightEdge,
        ScreenEdge bottomEdge,
        ScreenEdge topEdge,
        bool teleportedInRoom
    )
    {
        var oldRoomKey = GetRoomKey(leftEdge, rightEdge, bottomEdge, topEdge);
#if DEBUG
        logger.Log("Changing rooms.");
        logger.Log("Last Level: {0}", Manager<LevelManager>.Instance.lastLevelLoaded);
        logger.Log("Current Level: {0}", Manager<LevelManager>.Instance.GetCurrentLevelEnum());
        logger.Log("new roomKey: {0}", oldRoomKey);
        logger.Log(
            self.CurrentRoom != null
                ? $"currentRoom roomKey: {self.CurrentRoom.roomKey}"
                : "currentRoom does not exist."
        );
        logger.Log("teleported: {0}", teleportedInRoom);
        var position = Manager<PlayerManager>.Instance.Player.transform.position;
        logger.Log("Player position: ({0} {1} {2})", position.x, position.y, position.z);
#endif
        //This func checks if the new roomKey exists within levelRooms before changing and checks if currentRoom exists
        //if we're in a room, it leaves the current room then enters the new room with the teleported bool
        //no idea what the teleported bool does currently
        orig(self, leftEdge, rightEdge, bottomEdge, topEdge, teleportedInRoom);
        if (TeleportRoomKeys.Contains(oldRoomKey))
        {
            var index = TeleportRoomKeys.IndexOf(oldRoomKey);
            ArchipelagoClient.ServerData.AvailableTeleports[index] = true;
        }
        if (roomOverride)
        {
            roomOverride = false;
            return;
        }

        var currentLevel = Manager<LevelManager>.Instance.GetCurrentLevelEnum();
        var bossRoomKey = oldRoomKey.Replace(" ", string.Empty);
        if (IsBossRoom(bossRoomKey, out var bossName))
        {
            ServiceLocator.Get<RandoBossManager>().ShouldFightBoss(bossName);
        }
        else if (RoomRando)
        {
            var newRoom = PlaceInRoom(oldRoomKey, currentLevel, out var transition);
            if (newRoom.RoomKey.IsNullOrEmpty() || transition.Direction.IsNullOrEmpty())
                return;

            SetRoomKey(leftEdge, rightEdge, bottomEdge, topEdge, newRoom.RoomKey);
            roomOverride = true;
            if (newRoom.Region.Equals(currentLevel))
                Manager<Level>.Instance.ChangeRoom(leftEdge, rightEdge, bottomEdge, topEdge, teleportedInRoom);
            else
                RandoLevelManager.TeleportInArea(newRoom.Region, transition.Position, transition.Dimension);
        }
    }

    static bool WithinRange(float pos1, float pos2)
    {
        var comparison = pos2 - pos1;
        if (comparison < 0)
            comparison *= -1;
        return comparison <= 10;
    }

    static RoomConstants.RandoRoom PlaceInRoom(
        string oldRoomKey,
        ELevel currentLevel,
        out RoomConstants.RoomTransition newTransition
    )
    {
        newTransition = new RoomConstants.RoomTransition();
        if (
            !RoomConstants.RoomNameLookup.TryGetValue(
                new RoomConstants.RandoRoom(oldRoomKey, currentLevel),
                out var oldRoomName
            )
            || !RoomConstants.TransitionLookup.TryGetValue(oldRoomName, out var oldTransitions)
            || !roomMap.TryGetValue(oldRoomName, out var newName)
            || !RoomConstants.TransitionLookup.TryGetValue(newName, out var newTransitions)
        )
            return new RoomConstants.RandoRoom();

        var currentPosX = Manager<PlayerManager>.Instance.Player.transform.position.x;
        var currentTransition = oldTransitions.Find(transition => WithinRange(currentPosX, transition.Position.x));
        newTransition = newTransitions.Find(transition => currentTransition.Equals(transition));

        return !RoomConstants.RoomLookup.TryGetValue(newName, out var newRoom)
            ? new RoomConstants.RandoRoom()
            : newRoom;
    }
}
