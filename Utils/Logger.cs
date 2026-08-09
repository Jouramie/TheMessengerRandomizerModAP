using System;
using System.Threading;

namespace MessengerRando.Utils;

public class Logger(string Tag)
{
    public static Logger GetLogger(String tag)
    {
        return new Logger(tag);
    }

    public static Logger GetLogger(Type type)
    {
        return new Logger(type.Name);
    }

    public static Logger GetLogger<T>()
    {
        return new Logger(typeof(T).Name);
    }

    public enum LogLevel
    {
        Error,
        Assert,
        Warning,
        Info,
        Debug,
        Exception,
    }

    public void Debug(string message)
    {
#if DEBUG
        LogFormat(LogLevel.Debug, null, message);
#endif
    }

    public void Debug(object message)
    {
#if DEBUG
        LogFormat(LogLevel.Debug, null, "{0}", message);
#endif
    }

    public void Debug(string format, params object[] args)
    {
#if DEBUG
        LogFormat(LogLevel.Debug, null, format, args);
#endif
    }

    public void Log(string message)
    {
        LogFormat(LogLevel.Info, null, message);
    }

    public void Log(object message)
    {
        LogFormat(LogLevel.Info, null, "{0}", message);
    }

    public void Log(string format, params object[] args)
    {
        LogFormat(LogLevel.Info, null, format, args);
    }

    public void Warning(string message)
    {
        LogFormat(LogLevel.Warning, null, message);
    }

    public void Warning(string format, params object[] args)
    {
        LogFormat(LogLevel.Warning, null, format, args);
    }

    public void Error(string message)
    {
        LogFormat(LogLevel.Error, null, message);
    }

    public void Error(string format, params object[] args)
    {
        LogFormat(LogLevel.Error, null, format, args);
    }

    public void Exception(Exception exception)
    {
        LogFormat(LogLevel.Exception, null, "{0}", exception);
    }

    public void Exception(Exception exception, string message)
    {
        LogFormat(LogLevel.Exception, null, "{0}\n{1}", message, exception);
    }

    public void Exception(Exception exception, string format, params object[] args)
    {
        LogFormat(LogLevel.Exception, null, "{0}\n{1}", string.Format(format, args), exception);
    }

    public void Log(LogLevel logType, string message)
    {
        LogFormat(logType, null, message);
    }

    public void Log(LogLevel logType, string format, params object[] args)
    {
        LogFormat(logType, null, format, args);
    }

    public void LogFormat(LogLevel logType, UnityEngine.Object context, string format, params object[] args)
    {
        var threadName = Thread.CurrentThread.Name ?? Thread.CurrentThread.ManagedThreadId.ToString();
        if (context == null)
            Console.Write("({0}) [AP] [{1}] {2} [{3}] ", DateTime.Now, logType, threadName, Tag);
        else
            Console.Write("({0}) [AP] [{1}] {2} [{3}: {4}] ", DateTime.Now, logType, threadName, Tag, context.name);
        Console.WriteLine(format, args);
    }
}
