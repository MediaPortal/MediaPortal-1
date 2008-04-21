#region Copyright (C) 2005-2008 Team MediaPortal
/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Text;

namespace TvLibrary.Log
{
  /// <summary>
  /// An implementation of a log mechanism for the GUI library.
  /// </summary>
  public class Log
  {
    enum LogType
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
    static TimeSpan _logDaysToKeep = new TimeSpan(1, 0, 0, 0);

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
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Backups the log files.
    /// </summary>
    public static void BackupLogFiles()
    {
      RotateLogs();
    }

    /// <summary>
    /// Writes the specified exception to the log file
    /// </summary>
    /// <param name="ex">The ex.</param>
    public static void Write(Exception ex)
    {
      WriteToFile(LogType.Error, "Exception   :{0}", ex.ToString());
      WriteToFile(LogType.Error, "Exception   :{0}", ex.Message);
      WriteToFile(LogType.Error, "  site      :{0}", ex.TargetSite);
      WriteToFile(LogType.Error, "  source    :{0}", ex.Source);
      WriteToFile(LogType.Error, "  stacktrace:{0}", ex.StackTrace);
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
      String log = String.Format("{0:X} {1}", Thread.CurrentThread.ManagedThreadId, String.Format(format, arg));
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

    public static string GetPathName()
    {
      return String.Format(@"{0}\Team MediaPortal\MediaPortal TV Server", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
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
          catch (Exception) { }
        }
      }
      catch (Exception) { }
    }

    /// <summary>
    /// Deletes .bak file, moves .log to .bak for every LogType
    /// </summary>
    private static void RotateLogs()
    {
      try
      {
        // Get full path for tv.log
        string name = GetFileName(LogType.Info);
        string bakFile = name.Replace(".log", ".bak");
        // Delete outdated log
        if (File.Exists(bakFile))
          File.Delete(bakFile);
        // Rotate current log
        if (File.Exists(name))
          File.Move(name, bakFile);
        // Create a new tv.log with correct timestamps
        CreateBlankFile(name);

        name = GetFileName(LogType.Error);
        bakFile = name.Replace(".log", ".bak");
        if (File.Exists(bakFile))
          File.Delete(bakFile);
        if (File.Exists(name))
          File.Move(name, bakFile);
        CreateBlankFile(name);

        name = GetFileName(LogType.Epg);
        bakFile = name.Replace(".log", ".bak");
        if (File.Exists(bakFile))
          File.Delete(bakFile);
        if (File.Exists(name))
          File.Move(name, bakFile);
        CreateBlankFile(name);
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
    }

    /// <summary>
    /// Writes the file.
    /// </summary>
    /// <param name="logType">the type of logging.</param>
    /// <param name="format">The format.</param>
    /// <param name="arg">The arg.</param>
    private static void WriteToFile(LogType logType, string format, params object[] arg)
    {
      lock (typeof(Log))
      {
        try
        {
          string logFileName = GetFileName(logType);
          try
          {
            // If the user or some other event deleted the dir make sure to recreate it.
            Directory.CreateDirectory(Path.GetDirectoryName(logFileName));
            if (File.Exists(logFileName))
            {
              DateTime checkDate = DateTime.Now - _logDaysToKeep;
              // Set the file date to a default which would NOT rotate for the case that FileInfo fetching will fail
              DateTime fileDate = DateTime.Now;
              try
              {
                FileInfo logFi = new FileInfo(logFileName);
                // The information is retrieved from a cache and might be outdated.
                logFi.Refresh();
                fileDate = logFi.CreationTime;
              }
              catch (Exception) { }
              // File is older than today - _logDaysToKeep = rotate
              if (checkDate.CompareTo(fileDate) > 0)
                BackupLogFiles();
            }
          }
          catch (Exception) { }

          using (StreamWriter writer = new StreamWriter(logFileName, true))
          {
            string thread = Thread.CurrentThread.Name;
            if (thread == null)
            {
              thread = Thread.CurrentThread.ManagedThreadId.ToString();
            }
            writer.BaseStream.Seek(0, SeekOrigin.End); // set the file pointer to the end of 
            writer.WriteLine("{0:yyyy-MM-dd HH:mm:ss.ffffff} [{1}]: {2}", DateTime.Now, thread, string.Format(format, arg));
            writer.Close();
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
