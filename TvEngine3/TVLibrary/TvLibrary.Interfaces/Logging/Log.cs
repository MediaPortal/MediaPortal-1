#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using Mediaportal.TV.Server.TVLibrary.IntegrationProvider.Interfaces;
using TvLibrary.Interfaces;

namespace TvLibrary.Log
{
  public static class Log
  {
    public static void SetLogLevel(LogLevel level)
    {
      // Not supported here, defined by the hosting environment
    }

    public static string GetPathName()
    {
      return GlobalServiceProvider.Instance.Get<IIntegrationProvider>().PathManager.GetPath("<DATA>");
    }

    public static void Write(Exception ex)
    {
      Error(ex);
    }

    public static void Write(string message, params object[] args)
    {
      Debug(message, args);
    }

    public static void Epg(string message, params object[] args)
    {
      Debug(message, args);
    }

    public static void WriteFile(string message, params object[] args)
    {
      Debug(message, args);
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
      GlobalServiceProvider.Instance.Get<IIntegrationProvider>().Logger.Error("", exception);
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
