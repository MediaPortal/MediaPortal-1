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
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using MediaPortal.Common.Utils.Logger;

namespace TvLibrary.Log
{
  /// <summary>
  /// Type of log (default is "Log")
  /// </summary>
  public enum LogType
  {
    Log,
    Recorder,
    Error,
    EPG,
    VMR9,
    Config,
    MusicShareWatcher,
    WebEPG,
    PS
  }

  /// <summary>
  /// An implementation of a log mechanism for the GUI library.
  /// </summary>
  public class Log
  {
    /// <summary>
    /// The maximum count of identic messages to be logged in a row
    /// </summary>
    private const int _maxRepetitions = 5;

    /// <summary>
    /// The last log n lines to compare for repeated entries.
    /// </summary>
    private static readonly List<string> _lastLogLines = new List<string>(_maxRepetitions);

    #region Constructors

    /// <summary>
    /// Private singleton constructor . Do not allow any instance of this class.
    /// </summary>
    private Log() {}

    /// <summary>
    /// Static constructor
    /// </summary>
    static Log()
    {
      _lastLogLines.Clear();
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Backups the log files.
    /// </summary>
    public static void BackupLogFiles()
    {
      _lastLogLines.Clear();
    }

    /// <summary>
    /// Writes the specified exception to the log file
    /// </summary>
    /// <param name="ex">The ex.</param>
    public static void Write(Exception ex)
    {
      Error("Exception   :{0}\n", ex);
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

    /// <summary>
    /// Write a string to the logfile.
    /// </summary>
    /// <param name="format">The format of the string.</param>
    /// <param name="arg">An array containing the actual data of the string.</param>
    public static void Write(string format, params object[] arg)
    {
      // uncomment the following four lines to help identify the calling method, this
      // is useful in situations where an unreported exception causes problems
      //		StackTrace stackTrace = new StackTrace();
      //		StackFrame stackFrame = stackTrace.GetFrame(1);
      //		MethodBase methodBase = stackFrame.GetMethod();
      //		WriteFile(LogType.Log, "{0}", methodBase.Name);

      WriteToFile(LogLevel.Information, format, arg);
    }

    /// <summary>
    /// Write a string to the logfile.
    /// </summary>
    /// <param name="format">The format of the string.</param>
    /// <param name="arg">An array containing the actual data of the string.</param>
    public static void WriteThreadId(string format, params object[] arg)
    {
      // uncomment the following four lines to help identify the calling method, this
      // is useful in situations where an unreported exception causes problems
      //		StackTrace stackTrace = new StackTrace();
      //		StackFrame stackFrame = stackTrace.GetFrame(1);
      //		MethodBase methodBase = stackFrame.GetMethod();
      //		WriteFile(LogType.Log, "{0}", methodBase.Name);
      string log = String.Format("{0:X} {1}", Thread.CurrentThread.ManagedThreadId, String.Format(format, arg));
      WriteToFile(LogLevel.Information, log);
    }

    /// <summary>
    /// Logs the message to the error file
    /// </summary>
    /// <param name="format">The format.</param>
    /// <param name="arg">The arg.</param>
    public static void Error(string format, params object[] arg)
    {
      WriteToFile(LogLevel.Error, format, arg);
    }

    /// <summary>
    /// Logs an error message to the log specified
    /// </summary>
    /// <param name="logType">logger</param>
    /// <param name="format">message format string</param>
    /// <param name="arg">message arguments</param>
    public static void Error(LogType logType, string format, params object[] arg)
    {
      WriteToFile(logType, LogLevel.Error, format, arg);
    }

    /// <summary>
    /// Logs the message to the warning file
    /// </summary>
    /// <param name="format">The format.</param>
    /// <param name="arg">The arg.</param>
    public static void Warn(string format, params object[] arg)
    {
      WriteToFile(LogLevel.Warning, format, arg);
    }

    /// <summary>
    /// Logs a warn message to the log specified
    /// </summary>
    /// <param name="logType">logger</param>
    /// <param name="format">message format string</param>
    /// <param name="arg">message arguments</param>
    public static void Warn(LogType logType, string format, params object[] arg)
    {
      WriteToFile(logType, LogLevel.Warning, format, arg);
    }

    /// <summary>
    /// Logs the message to the info file
    /// </summary>
    /// <param name="format">The format.</param>
    /// <param name="arg">The arg.</param>
    public static void Info(string format, params object[] arg)
    {
      WriteToFile(LogLevel.Information, format, arg);
    }

    /// <summary>
    /// Logs an info message to the log specified
    /// </summary>
    /// <param name="logType">logType</param>
    /// <param name="format">message format string</param>
    /// <param name="arg">message arguments</param>
    public static void Info(LogType logType, string format, params object[] arg)
    {
      WriteToFile(logType, LogLevel.Information, format, arg);
    }

    /// <summary>
    /// Logs the message to the debug file
    /// </summary>
    /// <param name="format">The format.</param>
    /// <param name="arg">The arg.</param>
    public static void Debug(string format, params object[] arg)
    {
      WriteToFile(LogLevel.Debug, format, arg);
    }

    /// <summary>
    /// Logs a debug message to the log specified
    /// </summary>
    /// <param name="logType">logType</param>
    /// <param name="format">message format string</param>
    /// <param name="arg">message arguments</param>
    public static void Debug(LogType logType, string format, params object[] arg)
    {
      WriteToFile(logType, LogLevel.Debug, format, arg);
    }

    /// <summary>
    /// Logs the message to the epg file
    /// </summary>
    /// <param name="format">The format.</param>
    /// <param name="arg">The arg.</param>
    public static void Epg(string format, params object[] arg)
    {
      WriteToFile(LogType.EPG, LogLevel.Information, format, arg);
    }

    /// <summary>
    /// Logs the message to the info file
    /// </summary>
    /// <param name="format">The format.</param>
    /// <param name="arg">The arg.</param>
    public static void WriteFile(string format, params object[] arg)
    {
      WriteToFile(LogLevel.Information, format, arg);
    }

    ///<summary>
    /// Returns the path the Application data location
    ///</summary>
    ///<returns>Application data path of TvServer</returns>
    public static string GetPathName()
    {
      return String.Format(@"{0}\Team MediaPortal\MediaPortal TV Server",
                           Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
    }

    /// <summary>
    /// Set the log level
    /// </summary>
    /// <param name="level">level to set</param>
    public static void SetLogLevel(LogLevel level)
    {
      Log.Info("Set loglevel to: {0}", level.ToString());
      CommonLogger.Instance.LogLevel = ConvertToCommonLogLevel(level);
    }

    /// <summary>
    /// Set the log level for the log type
    /// </summary>
    /// <param name="type">log type</param>
    /// <param name="level">level to set</param>
    public static void SetLogLevel(LogType type, LogLevel level)
    {
      Log.Info(type, "Set loglevel for {0} to: {1}", type.ToString(), level.ToString());
      CommonLogger.Instance.SetLogLevel(ConvertToCommonLogType(type), ConvertToCommonLogLevel(level));
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Compares the cache's last log entries to check whether we have repeating lines that should not be logged
    /// </summary>
    /// <param name="aLogLine">A new log line</param>
    /// <returns>True if the cache only contains the exact lines as given by parameter</returns>
    private static bool IsRepetition(IComparable<string> aLogLine)
    {
      bool result = true;
      // as long as the cache is not full we have no repetitions
      if (_lastLogLines.Count == _maxRepetitions)
      {
        foreach (string singleLine in _lastLogLines)
        {
          if (aLogLine.CompareTo(singleLine) != 0)
          {
            result = false;
            break;
          }
        }
      }
      else
      {
        result = false;
      }

      return result;
    }

    private static void CacheLogLine(string aLogLine)
    {
      if (!string.IsNullOrEmpty(aLogLine))
      {
        if (_lastLogLines.Count == _maxRepetitions)
        {
          _lastLogLines.RemoveAt(0);
        }

        _lastLogLines.Add(aLogLine);
      }
    }


    /// <summary>
    /// Writes the file.
    /// </summary>
    /// <param name="logType">the type of logging.</param>
    /// <param name="format">The format.</param>
    /// <param name="arg">The arg.</param>
    private static void WriteToFile(LogLevel logLevel, string format, params object[] arg)
    {
      WriteToFile(LogType.Log, logLevel, format, arg);
    }

    /// <summary>
    /// Writes the file.
    /// </summary>
    /// <param name="logType">the type of logging.</param>
    /// <param name="format">The format.</param>
    /// <param name="arg">The arg.</param>
    private static void WriteToFile(LogType logType, LogLevel logLevel, string format, params object[] arg)
    {
      lock (typeof(Log))
      {
        try
        {
          string logLine = string.Format(format, arg);

          if (IsRepetition(logLine))
          {
            return;
          }
          CacheLogLine(logLine);

          // implementation
          switch (logLevel)
          {
            case LogLevel.Debug:
              CommonLogger.Instance.Debug(ConvertToCommonLogType(logType), format, arg);
              break;
            case LogLevel.Information:
              CommonLogger.Instance.Info(ConvertToCommonLogType(logType), format, arg);
              break;
            case LogLevel.Warning:
              CommonLogger.Instance.Warn(ConvertToCommonLogType(logType), format, arg);
              break;
            case LogLevel.Error:
              CommonLogger.Instance.Error(ConvertToCommonLogType(logType), format, arg);
              break;
          }
        }
        catch (Exception ex)
        {
          CommonLogger.Instance.Error(CommonLogType.Error, "Error in writing log entry", ex);
        }
      }
    }

    private static CommonLogType ConvertToCommonLogType(LogType logType)
    {
      switch (logType)
      {
        case LogType.Recorder:
          return CommonLogType.Recorder;
        case LogType.Error:
          return CommonLogType.Error;
        case LogType.EPG:
          return CommonLogType.EPG;
        case LogType.VMR9:
          return CommonLogType.VMR9;
        case LogType.Config:
          return CommonLogType.Config;
        case LogType.MusicShareWatcher:
          return CommonLogType.MusicShareWatcher;
        case LogType.WebEPG:
          return CommonLogType.WebEPG;
        case LogType.PS:
          return CommonLogType.PS;
        default:
          return CommonLogType.Log;
      }
    }

    private static CommonLogLevel ConvertToCommonLogLevel(LogLevel logLevel)
    {
      switch (logLevel)
      {
        case LogLevel.Debug: return CommonLogLevel.Debug;
        case LogLevel.Error: return CommonLogLevel.Error;
        case LogLevel.Information: return CommonLogLevel.Information;
        case LogLevel.Warning: return CommonLogLevel.Warning;
        default: return CommonLogLevel.All;
      }
    }

    #endregion
  }
}