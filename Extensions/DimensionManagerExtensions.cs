using System.Collections;
using UnityEngine;
using Logger = MessengerRando.Utils.Logger;

namespace MessengerRando.Extensions;

public static class DimensionManagerExtensions
{
    private static readonly Logger logger = Logger.GetLogger(typeof(DimensionManagerExtensions));

    public static void ChangeDimensionWithPortalAnimation(
        this DimensionManager manager,
        EBits bossDimension,
        float delay = 0f
    )
    {
        if (manager.CurrentDimension == bossDimension)
            return;

        var portal = Manager<PoolManager>.Instance.GetObjectInstance("DimensionPortal");

        if (portal == null)
        {
            logger.Warning("Failed to spawn portal. Switching dimension directly.");
            manager.SetDimension(bossDimension);
            return;
        }
        var component = portal.GetComponent<DimensionPortal>();
        component.StartCoroutine(WaitAndTriggerAnimation(component, delay));
    }

    private static IEnumerator WaitAndTriggerAnimation(DimensionPortal portal, float delay)
    {
        yield return new WaitForSeconds(delay);
        portal.transform.position = Manager<PlayerManager>.Instance.Player.transform.position;
        portal.Initialize();
        portal.OnTriggered();
    }
}
