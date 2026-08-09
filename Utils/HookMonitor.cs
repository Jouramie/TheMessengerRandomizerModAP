using System;
using System.Linq;

namespace MessengerRando.Utils;

public static class HookMonitor
{
    private static readonly Logger logger = Logger.GetLogger(typeof(HookMonitor));

    public static T Debug<T>()
        where T : Delegate
    {
        return MonitorCalls<T>(Logger.LogLevel.Debug);
    }

    public static T Log<T>()
        where T : Delegate
    {
        return MonitorCalls<T>(Logger.LogLevel.Info);
    }

    public static T MonitorCalls<T>(Logger.LogLevel level)
        where T : Delegate
    {
        Type delegateType = typeof(T);
        var invoke = delegateType.GetMethod("Invoke");

        var parameters = invoke.GetParameters();

        // log level + method name + parameter names
        Type wrapperType = parameters.Length switch
        {
            2 => typeof(HookInvoker2<,,>).MakeGenericType(
                invoke.ReturnType,
                parameters[0].ParameterType,
                parameters[1].ParameterType
            ),
            3 => typeof(HookInvoker3<,,,>).MakeGenericType(
                invoke.ReturnType,
                parameters[0].ParameterType,
                parameters[1].ParameterType,
                parameters[2].ParameterType
            ),
            4 => typeof(HookInvoker4<,,,,>).MakeGenericType(
                invoke.ReturnType,
                parameters[0].ParameterType,
                parameters[1].ParameterType,
                parameters[2].ParameterType,
                parameters[3].ParameterType
            ),
            5 => typeof(HookInvoker5<,,,,,>).MakeGenericType(
                invoke.ReturnType,
                parameters[0].ParameterType,
                parameters[1].ParameterType,
                parameters[2].ParameterType,
                parameters[3].ParameterType,
                parameters[4].ParameterType
            ),
            6 => typeof(HookInvoker6<,,,,,,>).MakeGenericType(
                invoke.ReturnType,
                parameters[0].ParameterType,
                parameters[1].ParameterType,
                parameters[2].ParameterType,
                parameters[3].ParameterType,
                parameters[4].ParameterType,
                parameters[5].ParameterType
            ),
            7 => typeof(HookInvoker7<,,,,,,,>).MakeGenericType(
                invoke.ReturnType,
                parameters[0].ParameterType,
                parameters[1].ParameterType,
                parameters[2].ParameterType,
                parameters[3].ParameterType,
                parameters[4].ParameterType,
                parameters[5].ParameterType,
                parameters[6].ParameterType
            ),
            _ => throw new NotSupportedException(
                $"Unsupported delegate type: {delegateType.FullName} (parameter count: {parameters.Length})"
            ),
        };

        var hookIndex = delegateType.Name.IndexOf("hook_");
        var methodName = hookIndex >= 0 ? delegateType.Name.Substring(hookIndex + 5) : delegateType.Name;

        // Two first args are orig and self, skipping them.
        object[] args = [level, methodName, .. parameters.Skip(2).Select(p => p.Name)];

        var wrapper = Activator.CreateInstance(wrapperType, args);
        if (invoke.ReturnType == typeof(void))
        {
            return (T)Delegate.CreateDelegate(delegateType, wrapper, wrapperType.GetMethod("Invoke"));
        }
        else
        {
            return (T)Delegate.CreateDelegate(delegateType, wrapper, wrapperType.GetMethod("InvokeReturn"));
        }
    }

    private sealed class HookInvoker2<TReturn, TOrig, TSelf>(Logger.LogLevel level, string methodName)
        where TOrig : Delegate
    {
        private readonly string message = $"{typeof(TSelf).Name}.{methodName}()";

        public void Invoke(TOrig orig, TSelf self)
        {
            try
            {
                logger.Log(level, message);
                orig.DynamicInvoke([self]);
            }
            catch (Exception ex)
            {
                logger.Exception(ex);
                throw ex;
            }
        }

        public TReturn InvokeReturn(TOrig orig, TSelf self)
        {
            try
            {
                logger.Log(level, message);
                var result = orig.DynamicInvoke([self]);
                logger.Log(level, "Result: {0}", result);
                return (TReturn)result;
            }
            catch (Exception ex)
            {
                logger.Exception(ex);
                throw ex;
            }
        }
    }

    private sealed class HookInvoker3<TReturn, TOrig, TSelf, TArg1>(
        Logger.LogLevel level,
        string methodName,
        string arg1Name
    )
        where TOrig : Delegate
    {
        private readonly string format = $"{typeof(TSelf).Name}.{methodName}({arg1Name}: {{0}})";

        public void Invoke(TOrig orig, TSelf self, TArg1 arg1)
        {
            try
            {
                logger.Log(level, format, arg1);
                orig.DynamicInvoke([self, arg1]);
            }
            catch (Exception ex)
            {
                logger.Exception(ex);
                throw ex;
            }
        }

        public TReturn InvokeReturn(TOrig orig, TSelf self, TArg1 arg1)
        {
            try
            {
                logger.Log(level, format, arg1);
                var result = orig.DynamicInvoke([self, arg1]);
                logger.Log(level, "Result: {0}", result);
                return (TReturn)result;
            }
            catch (Exception ex)
            {
                logger.Exception(ex);
                throw ex;
            }
        }
    }

    private sealed class HookInvoker4<TReturn, TOrig, TSelf, TArg1, TArg2>(
        Logger.LogLevel level,
        string methodName,
        string arg1Name,
        string arg2Name
    )
        where TOrig : Delegate
    {
        private readonly string format = $"{typeof(TSelf).Name}.{methodName}({arg1Name}: {{0}}, {arg2Name}: {{1}})";

        public void Invoke(TOrig orig, TSelf self, TArg1 arg1, TArg2 arg2)
        {
            try
            {
                logger.Log(level, format, arg1, arg2);
                orig.DynamicInvoke([self, arg1, arg2]);
            }
            catch (Exception ex)
            {
                logger.Exception(ex);
                throw ex;
            }
        }

        public TReturn InvokeReturn(TOrig orig, TSelf self, TArg1 arg1, TArg2 arg2)
        {
            try
            {
                logger.Log(level, format, arg1, arg2);
                var result = orig.DynamicInvoke([self, arg1, arg2]);
                logger.Log(level, "Result: {0}", result);
                return (TReturn)result;
            }
            catch (Exception ex)
            {
                logger.Exception(ex);
                throw ex;
            }
        }
    }

    private sealed class HookInvoker5<TReturn, TOrig, TSelf, TArg1, TArg2, TArg3>(
        Logger.LogLevel level,
        string methodName,
        string arg1Name,
        string arg2Name,
        string arg3Name
    )
        where TOrig : Delegate
    {
        private readonly string format =
            $"{typeof(TSelf).Name}.{methodName}({arg1Name}: {{0}}, {arg2Name}: {{1}}, {arg3Name}: {{2}})";

        public void Invoke(TOrig orig, TSelf self, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            try
            {
                logger.Log(level, format, arg1, arg2, arg3);
                orig.DynamicInvoke([self, arg1, arg2, arg3]);
            }
            catch (Exception ex)
            {
                logger.Exception(ex);
                throw ex;
            }
        }

        public TReturn InvokeReturn(TOrig orig, TSelf self, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            try
            {
                logger.Log(level, format, arg1, arg2, arg3);
                var result = orig.DynamicInvoke([self, arg1, arg2, arg3]);
                logger.Log(level, "Result: {0}", result);
                return (TReturn)result;
            }
            catch (Exception ex)
            {
                logger.Exception(ex);
                throw ex;
            }
        }
    }

    private sealed class HookInvoker6<TReturn, TOrig, TSelf, TArg1, TArg2, TArg3, TArg4>(
        Logger.LogLevel level,
        string methodName,
        string arg1Name,
        string arg2Name,
        string arg3Name,
        string arg4Name
    )
        where TOrig : Delegate
    {
        private readonly string format =
            $"{typeof(TSelf).Name}.{methodName}({arg1Name}: {{0}}, {arg2Name}: {{1}}, {arg3Name}: {{2}}, {arg4Name}: {{3}})";

        public void Invoke(TOrig orig, TSelf self, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            try
            {
                logger.Log(level, format, arg1, arg2, arg3, arg4);
                orig.DynamicInvoke([self, arg1, arg2, arg3, arg4]);
            }
            catch (Exception ex)
            {
                logger.Exception(ex);
                throw ex;
            }
        }

        public TReturn InvokeReturn(TOrig orig, TSelf self, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            try
            {
                logger.Log(level, format, arg1, arg2, arg3, arg4);
                var result = orig.DynamicInvoke([self, arg1, arg2, arg3, arg4]);
                logger.Log(level, "Result: {0}", result);
                return (TReturn)result;
            }
            catch (Exception ex)
            {
                logger.Exception(ex);
                throw ex;
            }
        }
    }

    private sealed class HookInvoker7<TReturn, TOrig, TSelf, TArg1, TArg2, TArg3, TArg4, TArg5>(
        Logger.LogLevel level,
        string methodName,
        string arg1Name,
        string arg2Name,
        string arg3Name,
        string arg4Name,
        string arg5Name
    )
        where TOrig : Delegate
    {
        private readonly string format =
            $"{typeof(TSelf).Name}.{methodName}({arg1Name}: {{0}}, {arg2Name}: {{1}}, {arg3Name}: {{2}}, {arg4Name}: {{3}}, {arg5Name}: {{4}})";

        public void Invoke(TOrig orig, TSelf self, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
        {
            try
            {
                logger.Log(level, format, arg1, arg2, arg3, arg4, arg5);
                orig.DynamicInvoke([self, arg1, arg2, arg3, arg4, arg5]);
            }
            catch (Exception ex)
            {
                logger.Exception(ex);
                throw ex;
            }
        }

        public TReturn InvokeReturn(TOrig orig, TSelf self, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
        {
            try
            {
                logger.Log(level, format, arg1, arg2, arg3, arg4, arg5);
                var result = orig.DynamicInvoke([self, arg1, arg2, arg3, arg4, arg5]);
                logger.Log(level, "Result: {0}", result);
                return (TReturn)result;
            }
            catch (Exception ex)
            {
                logger.Exception(ex);
                throw ex;
            }
        }
    }
}
