using System;

namespace MessengerRando.Utils;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class SafeHookAttribute(bool callOrigOnError = false) : Attribute
{
    public bool CallOrigOnError { get; } = callOrigOnError;
}
