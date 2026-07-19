using System;

namespace MessengerRando.Utils;

public static class SafeHook
{
    private static readonly Logger logger = Logger.GetLogger(typeof(SafeHook));

    public static T Wrap<T>(T hook) where T : Delegate
    {
        Type delegateType = typeof(T);
        var invoke = delegateType.GetMethod("Invoke");

        if (invoke.ReturnType != typeof(void))
        {
            logger.Warning("Unsupported delegate type: {0} (return type is not void)", delegateType.FullName);
            return hook;
        }

        var parameters = invoke.GetParameters();

        Type wrapperType = parameters.Length switch
        {
            2 => typeof(HookInvoker2<,>).MakeGenericType(parameters[0].ParameterType, parameters[1].ParameterType),
            3 => typeof(HookInvoker3<,,>).MakeGenericType(parameters[0].ParameterType, parameters[1].ParameterType, parameters[2].ParameterType),
            4 => typeof(HookInvoker4<,,,>).MakeGenericType(parameters[0].ParameterType, parameters[1].ParameterType, parameters[2].ParameterType, parameters[3].ParameterType),
            _ => null,
        };

        if (wrapperType == null)
        {
            logger.Warning("Unsupported delegate type: {0} (parameter count: {1})", delegateType.FullName, parameters.Length);
            return hook;
        }

        var wrapper = Activator.CreateInstance(wrapperType, [hook]);
        return (T)Delegate.CreateDelegate(delegateType, wrapper, wrapperType.GetMethod("Invoke"));
    }

    private static bool ShouldCallOrigOnError(Delegate handler)
    {
        var attributes = handler.Method.GetCustomAttributes(typeof(SafeHookAttribute), false);
        if (attributes.Length == 0)
        {
            return false;
        }

        return ((SafeHookAttribute)attributes[0]).CallOrigOnError;
    }

    private sealed class HookInvoker2<TOrig, TSelf>(Delegate handler)
    {
        private readonly bool callOrigOnError = ShouldCallOrigOnError(handler);

        public void Invoke(TOrig orig, TSelf self)
        {
            try
            {
                handler.DynamicInvoke([orig, self]);
            }
            catch (Exception ex)
            {
                logger.Exception(ex, "[{0}]", handler.Method.Name);
                if (callOrigOnError)
                {
                    ((Delegate)(object)orig).DynamicInvoke([self]);
                }
            }
        }
    }

    private sealed class HookInvoker3<TOrig, TSelf, TArg1>(Delegate handler)
    {
        private readonly bool callOrigOnError = ShouldCallOrigOnError(handler);

        public void Invoke(TOrig orig, TSelf self, TArg1 arg1)
        {
            try
            {
                handler.DynamicInvoke([orig, self, arg1]);
            }
            catch (Exception ex)
            {
                logger.Exception(ex, "[{0}]", handler.Method.Name);
                if (callOrigOnError)
                {
                    ((Delegate)(object)orig).DynamicInvoke([self, arg1]);
                }
            }
        }
    }

    private sealed class HookInvoker4<TOrig, TSelf, TArg1, TArg2>(Delegate handler)
    {
        private readonly bool callOrigOnError = ShouldCallOrigOnError(handler);

        public void Invoke(TOrig orig, TSelf self, TArg1 arg1, TArg2 arg2)
        {
            try
            {
                handler.DynamicInvoke([orig, self, arg1, arg2]);
            }
            catch (Exception ex)
            {
                logger.Exception(ex, "[{0}]", handler.Method.Name);
                if (callOrigOnError)
                {
                    ((Delegate)(object)orig).DynamicInvoke([self, arg1, arg2]);
                }
            }
        }
    }
}

