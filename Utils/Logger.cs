using System;
using UnityEngine;

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

    public void Log(string message)
    {
        LogFormat(LogType.Log, null, message);
    }

    public void Log(object message)
    {
        LogFormat(LogType.Log, null, "{0}", message);
    }

    public void Log(string format, params object[] args)
    {
        LogFormat(LogType.Log, null, format, args);
    }

    public void Warning(string message)
    {
        LogFormat(LogType.Warning, null, message);
    }

    public void Warning(string format, params object[] args)
    {
        LogFormat(LogType.Warning, null, format, args);
    }

    public void Error(string message)
    {
        LogFormat(LogType.Error, null, message);
    }

    public void Error(string format, params object[] args)
    {
        LogFormat(LogType.Error, null, format, args);
    }

    public void Exception(Exception exception)
    {
        LogFormat(LogType.Exception, null, "{0}", exception);
    }

    public void Exception(Exception exception, string message)
    {
        LogFormat(LogType.Exception, null, "{0}\n{1}", message, exception);
    }

    public void Exception(Exception exception, string format, params object[] args)
    {
        LogFormat(LogType.Exception, null, "{0}\n{1}", string.Format(format, args), exception);
    }

    public void Log(LogType logType, string message)
    {
        LogFormat(logType, null, message);
    }


    public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
    {
        if (context == null) Console.Write("({0}) [AP] [{1}] [{2}] ", DateTime.Now, logType, Tag);
        else Console.Write("({0}) [AP] [{1}] [{2}: {3}] ", DateTime.Now, logType, Tag, context.name);
        Console.WriteLine(format, args);
    }
}
