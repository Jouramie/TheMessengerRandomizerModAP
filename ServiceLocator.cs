using System;
using System.Collections.Generic;
using System.Linq;

namespace MessengerRando;

public class ServiceLocator
{
    private static readonly Dictionary<Type, object> services = [];

    public static T Register<T>(T service)
    {
        services[typeof(T)] = service;
        return service;
    }

    public static T Get<T>()
    {
        return (T)services[typeof(T)];
    }

    public static IEnumerable<T> GetAll<T>()
    {
        foreach (var item in services)
            if (item.Key.GetInterfaces().Contains(typeof(T)))
                yield return (T)item.Value;
    }
}
