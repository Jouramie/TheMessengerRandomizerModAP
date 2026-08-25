using MessengerRando.Extensions;
using MessengerRando.Lifecycle;
using UnityEngine;

namespace MessengerRando.Utils;

/// <summary>
/// This class is responsible for preloading prefabs by finding them in different game objects.
/// Also allows finding objects to copy at different places in loaded scenes.
/// </summary>
public class PrefabHunter : IOnModLoadHandler
{
    public void OnModLoad()
    {
        On.PortalSpawner.Start += SafeHook.Wrap<On.PortalSpawner.hook_Start>(PortalSpawner_Start);
    }

    private void PortalSpawner_Start(On.PortalSpawner.orig_Start orig, PortalSpawner self)
    {
        orig(self);
        Manager<PoolManager>.Instance.Preload(self.portalPrefab.GetComponent<PoolableObject>(), 1);
    }

    public GameObject CreateChatPrompt(Transform parent = null)
    {
        string path;
        if (Manager<LevelManager>.Instance.CurrentSceneName == ELevel.Level_04_Catacombs.SceneName)
            path = "/ToForlornSecondQuest/ToForlornCutscene/ChatPrompt";
        else
            throw new RandomizerException("Could not find chat prompt to copy.");

        if (parent != null)
            return Object.Instantiate(GameObject.Find(path), parent);
        else
            return Object.Instantiate(GameObject.Find(path));
    }

    public GameObject CreateDimensionPortalSpawner(Transform parent = null)
    {
        string path;
        if (Manager<LevelManager>.Instance.CurrentSceneName == ELevel.Level_02_AutumnHills.SceneName)
            path = "/DimensionZones/Portals/DimensionPortalSpawner (12)";
        else
            throw new RandomizerException("Could not find portal spawner to copy.");

        if (parent != null)
            return Object.Instantiate(GameObject.Find(path), parent);
        else
            return Object.Instantiate(GameObject.Find(path));
    }
}
