using System.Reflection;
using UnityEngine;

namespace MessengerRando.Extensions;

public static class ReflectionHelpers
{
    public static T GetPrivateField<T>(this MonoBehaviour o, string fieldName)
    {
        var field = o.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        return (T)field.GetValue(o);
    }

    public static void SetPrivateField(this MonoBehaviour o, string fieldName, object value)
    {
        var field = o.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(o, value);
    }

    public static T GetPrivateProperty<T>(this MonoBehaviour o, string propertyName)
    {
        var property = o.GetType().GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);
        return (T)property.GetValue(o, null);
    }

    public static void InvokeMethod(this MonoBehaviour o, string methodName, params object[] parameters)
    {
        var method = o.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        method.Invoke(o, parameters);
    }

    public static T InvokeMethod<T>(this MonoBehaviour o, string methodName, params object[] parameters)
    {
        var method = o.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        return (T)method.Invoke(o, parameters);
    }

    public static T InvokeStaticMethod<T>(this MonoBehaviour o, string methodName, params object[] parameters)
    {
        var method = o.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
        return (T)method.Invoke(null, parameters);
    }
}
