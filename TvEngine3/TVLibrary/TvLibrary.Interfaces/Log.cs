#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace TvLibrary.Log
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
    /// Configure after how many days the log file shall be rotated when a new line is added
    /// </summary>
    private static readonly TimeSpan _logDaysToKeep = new TimeSpan(1, 0, 0, 0);

    /// <summary>
    /// The maximum size of each log file in Megabytes
    /// </summary>
    private const int _maxLogSizeMb = 100;

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
    private Log()
    {
    }

    /// <summary>
    /// Static constructor
    /// </summary>
    static Log()
    {
      //BackupLogFiles(); <-- do not rotate logs when e.g. SetupTv is started.
      _lastLogLines.Clear();
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Backups the log files.
    /// </summary>
    public static void BackupLogFiles()
    {
      RotateLogs();
      _lastLogLines.Clear();
    }

    /// <summary>
    /// Writes the specified exception to the log file
    /// </summary>
    /// <param name="ex">The ex.</param>
    public static void Write(Exception ex)
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendFormat("Exception   :{0}\n", ex);
      Error(SafeString(sb.ToString()));
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

      WriteToFile(LogType.Info, format, arg);
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
      WriteToFile(LogType.Info, log);
    }

    /// <summary>
    /// Logs the message to the error file
    /// </summary>
    /// <param name="format">The format.</param>
    /// <param name="arg">The arg.</param>
    public static void Error(string format, params object[] arg)
    {
      WriteToFile(LogType.Error, format, arg);
    }

    /// <summary>
    /// Logs the message to the info file
    /// </summary>
    /// <param name="format">The format.</param>
    /// <param name="arg">The arg.</param>
    public static void Info(string format, params object[] arg)
    {
      WriteToFile(LogType.Info, format, arg);
    }

    /// <summary>
    /// Logs the message to the debug file
    /// </summary>
    /// <param name="format">The format.</param>
    /// <param name="arg">The arg.</param>
    public static void Debug(string format, params object[] arg)
    {
      WriteToFile(LogType.Debug, format, arg);
    }

    /// <summary>
    /// Logs the message to the epg file
    /// </summary>
    /// <param name="format">The format.</param>
    /// <param name="arg">The arg.</param>
    public static void Epg(string format, params object[] arg)
    {
      WriteToFile(LogType.Epg, format, arg);
    }

    /// <summary>
    /// Logs the message to the info file
    /// </summary>
    /// <param name="format">The format.</param>
    /// <param name="arg">The arg.</param>
    public static void WriteFile(string format, params object[] arg)
    {
      WriteToFile(LogType.Info, format, arg);
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

    #endregion

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
          sw.Close();
          try
          {
            File.SetCreationTime(aFileName, DateTime.Now);
          }
          catch (Exception)
          {
          }
        }
      }
      catch (Exception)
      {
      }
    }

    /// <summary>
    /// Deletes .bak file, moves .log to .bak for every LogType
    /// </summary>
    private static void RotateLogs()
    {
      try
      {
        List<string> physicalLogFiles = new List<string>(3);
        // Get all log types
        foreach (LogType logtype in Enum.GetValues(typeof (LogType)))
        {
          // Get full path for log
          string name = GetFileName(logtype);
          // Since e.g. debug and info might share the same file make sure we only rotate once
          if (!physicalLogFiles.Contains(name))
          {
            physicalLogFiles.Add(name);
          }
        }

        foreach (string logFileName in physicalLogFiles)
        {
          // make sure other files will be rotated even if one file fails
          try
          {
            string bakFileName = logFileName.Replace(".log", ".bak");
            // Delete outdated log
            if (File.Exists(bakFileName))
            {
              File.Delete(bakFileName);
            }
            // Rotate current log
            if (File.Exists(logFileName))
            {
              File.Move(logFileName, bakFileName);
            }
            // Create a new log file with correct timestamps
            CreateBlankFile(logFileName);
          }
          catch (UnauthorizedAccessException)
          {
          }
          catch (ArgumentException)
          {
          }
          catch (IOException)
          {
          }
        }
      }
      catch (Exception)
      {
        // Maybe add EventLog here...
      }
    }

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
    /// Does pre-logging tasks - like check for rotation, oversize, etc
    /// </summary>
    /// <param name="aLogFileName">The file to be checked</param>
    /// <returns>False if logging must not go on</returns>
    private static bool CheckLogPrepared(string aLogFileName)
    {
      bool result = true;
      try
      {
        // If the user or some other event deleted the dir make sure to recreate it.
        Directory.CreateDirectory(Path.GetDirectoryName(aLogFileName));
        if (File.Exists(aLogFileName))
        {
          DateTime checkDate = DateTime.Now - _logDaysToKeep;
          // Set the file date to a default which would NOT rotate for the case that FileInfo fetching will fail
          DateTime fileDate = DateTime.Now;
          try
          {
            FileInfo logFi = new FileInfo(aLogFileName);
            // The information is retrieved from a cache and might be outdated.
            logFi.Refresh();
            fileDate = logFi.CreationTime;

            // Some log source went out of control here - do not log until out of disk space!
            if (logFi.Length > _maxLogSizeMb*1000*1000)
            {
              result = false;
            }
          }
          catch (Exception)
          {
          }
          // File is older than today - _logDaysToKeep = rotate
          if (checkDate.CompareTo(fileDate) > 0)
          {
            BackupLogFiles();
          }
        }
      }
      catch (Exception)
      {
      }
      return result;
    }

    /// <summary>
    /// Writes the file.
    /// </summary>
    /// <param name="logType">the type of logging.</param>
    /// <param name="format">The format.</param>
    /// <param name="arg">The arg.</param>
    private static void WriteToFile(LogType logType, string format, params object[] arg)
    {
      lock (typeof (Log))
      {
        try
        {
          string logFileName = GetFileName(logType);
          string logLine = string.Format(format, arg);

          if (IsRepetition(logLine))
          {
            return;
          }
          CacheLogLine(logLine);

          if (CheckLogPrepared(logFileName))
          {
            using (StreamWriter writer = new StreamWriter(logFileName, true))
            {
              string thread = Thread.CurrentThread.Name;
              if (string.IsNullOrEmpty(thread))
              {
                thread = Thread.CurrentThread.ManagedThreadId.ToString();
              }

              writer.BaseStream.Seek(0, SeekOrigin.End); // set the file pointer to the end of file
              writer.WriteLine("{0:yyyy-MM-dd HH:mm:ss.ffffff} [{1}]: {2}", DateTime.Now, thread, logLine);
              writer.Close();
            }
          }
        }
        catch (Exception)
        {
        }
      }
    }

    #endregion
  }
}