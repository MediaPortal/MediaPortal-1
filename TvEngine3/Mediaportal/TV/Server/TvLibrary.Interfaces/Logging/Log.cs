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
using System.IO;
using System.Text.RegularExpressions;
using MediaPortal.Common.Utils;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Integration;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Logging
{
  /// <summary>
  /// An implementation of a log mechanism for the GUI library.
  /// </summary>
  public class Log
  {
    private enum LogType
    {
      /// <summary>
      /// Debug logging
      /// </summary>
      Debug,
      /// <summary>
      /// normal logging
      /// </summary>
      Info,
      /// <summary>
      /// error logging
      /// </summary>
      Error,
      /// <summary>
      /// epg logging
      /// </summary>
      Epg
    }

    /// <summary>
    /// Replaces a password inside the string by stars
    /// </summary>
    /// <param name="Logtext">String to replace</param>
    /// <returns>String without password</returns>
    public static String SafeString(String Logtext)
    {
      return new Regex(@"Password=[^;]*;", RegexOptions.IgnoreCase).Replace(Logtext, "Password=***;");
    }

    #region Private methods

    private static string GetFileName(LogType logType)
    {
      string Path = GetPathName();
      switch (logType)
      {
        case LogType.Debug:
        case LogType.Info:
          return String.Format(@"{0}\log\tv.log", Path);

        case LogType.Error:
          return String.Format(@"{0}\log\error.log", Path);

        case LogType.Epg:
          return String.Format(@"{0}\log\epg.log", Path);

        default:
          return String.Format(@"{0}\log\tv.log", Path);
      }
    }

    private static string GetPathName ()
    {
      return GlobalServiceProvider.Instance.Get<IIntegrationProvider>().PathManager.GetPath("<LOG>");
    }

    /// <summary>
    /// Since Windows caches API calls to the FileSystem a simple FileInfo.CreationTime will be wrong when replacing files (even after refresh).
    /// Therefore we set it manually.
    /// </summary>
    /// <param name="aFileName"></param>
    private static void CreateBlankFile(string aFileName)
    {
      try
      {
        using (StreamWriter sw = File.CreateText(aFileName))
        {                    
        }
        try
        {
          File.SetCreationTime(aFileName, DateTime.Now);
        }
        catch (Exception) { }
      }
      catch (Exception) {}
    }

    #endregion

    public static void Debug(string message, params object[] args)
    {
     GlobalServiceProvider.Instance.Get<IIntegrationProvider>().Logger.Debug(message, args);
    }

    public static void Info(string message, params object[] args)
    {
     GlobalServiceProvider.Instance.Get<IIntegrationProvider>().Logger.Info(message, args);
    }

    public static void Warn(string message, params object[] args)
    {
     GlobalServiceProvider.Instance.Get<IIntegrationProvider>().Logger.Warn(message, args);
    }

    public static void Error(string message, params object[] args)
    {
     GlobalServiceProvider.Instance.Get<IIntegrationProvider>().Logger.Error(message, args);
    }

    public static void Error(Exception exception)
    {
      GlobalServiceProvider.Instance.Get<IIntegrationProvider>().Logger.Error(string.Empty, exception);
    }

    public static void Error(Exception exception, string message, params object[] args)
    {
      GlobalServiceProvider.Instance.Get<IIntegrationProvider>().Logger.Error(message, exception, args);
    }

    public static void WriteFile(string message, params object[] args)
    {
      Info(message, args);
    }

    public static void Write(string message, params object[] args)
    {
      Info(message, args);
    }

    public static void Epg(string message, params object[] args)
    {
      // TODO: support different logfiles
      Debug("EPG: "+ message, args);
    }
  }
}