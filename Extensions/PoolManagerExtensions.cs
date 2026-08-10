using System;
using System.Collections.Generic;
using UnityEngine;
using Logger = MessengerRando.Utils.Logger;

namespace MessengerRando.Extensions;

public static class PoolManagerExtensions
{
    private static readonly Logger logger = Logger.GetLogger(typeof(PoolManagerExtensions));

    /// <summary>
    /// This will only work if the pool is already created. It will not create a new pool if it doesn't exist.
    /// </summary>
    public static GameObject GetObjectInstance(this PoolManager poolManager, string poolName)
    {
#if DEBUG
        List<string> testedPools = [];
#endif
        foreach (
            var item in Manager<PoolManager>.Instance.GetPrivateField<Dictionary<GameObject, Pool>>("poolByGameObject")
        )
        {
            try
            {
                if (item.Key == null)
                {
                    continue;
                }

                var name = item.Key?.name;
                if (name == poolName)
                {
                    return poolManager.GetObjectInstance(item.Key);
                }
#if DEBUG
                testedPools.Add(name);
#endif
            }
            catch (Exception)
            {
                logger.Warning("Failed to get object instance for pool {0}", item.Key);
            }
        }
#if DEBUG
        logger.Warning("Could not find pool: {0}. Tested pools: {1}", poolName, string.Join(", ", [.. testedPools]));
#endif
        return null;
    }
}
