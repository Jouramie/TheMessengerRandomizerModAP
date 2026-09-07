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
        portal.Initialize();
        portal.transform.position = Manager<PlayerManager>.Instance.Player.transform.position + (Vector3.up * 2);
        // Pooling creates a bug where the reused portal is already placed in the scene and triggers two times
        //  which invokes the `Kill()` method two times and delete the current dimension.
        portal.SetPrivateField("pool", false);
        portal.OnTriggered();
    }
}
