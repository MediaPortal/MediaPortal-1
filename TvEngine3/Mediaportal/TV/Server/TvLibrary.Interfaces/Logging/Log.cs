using System;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.IntegrationProvider.Interfaces;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Logging
{
  public static class Log
  {
    public static LogLevel Level
    {
      get
      {
        return GlobalServiceProvider.Instance.Get<IIntegrationProvider>().Logger.LogLevel;
      }
    }

    public static void Debug(Type callerType, string message, params object[] args)
    {
      GlobalServiceProvider.Instance.Get<IIntegrationProvider>().Logger.Debug(callerType, message, args);
    }

    public static void Debug(Type callerType, Exception exception, string message, params object[] args)
    {
      GlobalServiceProvider.Instance.Get<IIntegrationProvider>().Logger.Debug(callerType, message, exception, args);
    }

    public static void Debug(Exception exception, string message, params object[] args)
    {
      GlobalServiceProvider.Instance.Get<IIntegrationProvider>().Logger.Debug(message, exception, args);
    }

    public static void Debug(string message, params object[] args)
    {
      GlobalServiceProvider.Instance.Get<IIntegrationProvider>().Logger.Debug(message, args);
    }

    public static void Info(Type callerType, string message, params object[] args)
    {
      GlobalServiceProvider.Instance.Get<IIntegrationProvider>().Logger.Info(callerType, message, args);
    }

    public static void Info(Type callerType, Exception exception, string message, params object[] args)
    {
      GlobalServiceProvider.Instance.Get<IIntegrationProvider>().Logger.Info(callerType, message, exception, args);
    }

    public static void Info(Exception exception, string message, params object[] args)
    {
      GlobalServiceProvider.Instance.Get<IIntegrationProvider>().Logger.Info(message, exception, args);
    }


    public static void Info(string message, params object[] args)
    {
      GlobalServiceProvider.Instance.Get<IIntegrationProvider>().Logger.Info(message, args);
    }

    public static void Critical(Type callerType, string message, params object[] args)
    {
      GlobalServiceProvider.Instance.Get<IIntegrationProvider>().Logger.Critical(callerType, message, args);
    }

    public static void Critical(string message, params object[] args)
    {
      GlobalServiceProvider.Instance.Get<IIntegrationProvider>().Logger.Critical(message, args);
    }

    public static void Critical(Type callerType, Exception exception, string message, params object[] args)
    {
      GlobalServiceProvider.Instance.Get<IIntegrationProvider>().Logger.Critical(callerType, message, exception, args);
    }

    public static void Critical(Exception exception, string message, params object[] args)
    {
      GlobalServiceProvider.Instance.Get<IIntegrationProvider>().Logger.Critical(message, exception, args);
    }

    public static void Warn(Type callerType, string message, params object[] args)
    {
      GlobalServiceProvider.Instance.Get<IIntegrationProvider>().Logger.Warn(callerType, message, args);
    }

    public static void Warn(string message, params object[] args)
    {
      GlobalServiceProvider.Instance.Get<IIntegrationProvider>().Logger.Warn(message, args);
    }

    public static void Warn(Type callerType, Exception exception, string message, params object[] args)
    {
      GlobalServiceProvider.Instance.Get<IIntegrationProvider>().Logger.Warn(callerType, message, exception, args);
    }

    public static void Warn(Exception exception, string message, params object[] args)
    {
      GlobalServiceProvider.Instance.Get<IIntegrationProvider>().Logger.Warn(message, exception, args);
    }

    public static void Error(Type callerType, string message, params object[] args)
    {
      GlobalServiceProvider.Instance.Get<IIntegrationProvider>().Logger.Error(callerType, message, args);
    }

    public static void Error(string message, params object[] args)
    {
      GlobalServiceProvider.Instance.Get<IIntegrationProvider>().Logger.Error(message, args);
    }

    public static void Error(Exception exception)
    {
      GlobalServiceProvider.Instance.Get<IIntegrationProvider>().Logger.Error(string.Empty, exception);
    }

    public static void Error(Type callerType, Exception exception, string message, params object[] args)
    {
      GlobalServiceProvider.Instance.Get<IIntegrationProvider>().Logger.Error(callerType, message, exception, args);
    }

    public static void Error(Exception exception, string message, params object[] args)
    {
      GlobalServiceProvider.Instance.Get<IIntegrationProvider>().Logger.Error(message, exception, args);
    }

  }
}
