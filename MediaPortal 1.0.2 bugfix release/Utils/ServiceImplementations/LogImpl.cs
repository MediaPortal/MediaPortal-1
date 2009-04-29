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
using System.IO;
using System.Threading;
using MediaPortal.Configuration;
using MediaPortal.Profile;
using MediaPortal.Services;

namespace MediaPortal.ServiceImplementations
{
  /// <summary>
  /// An implementation of a log mechanism for the GUI library.
  /// </summary>
  internal class LogImpl : ILog
  {
    #region Variables

    private static DateTime _previousDate;
    private static Level _minLevel;
    private static string logDir;
    private static bool bConfiguration;

    #endregion

    // when Configuartion.exe is running the logging should take place in Configuration.log

    #region Constructors/Destructors

    /// <summary>
    /// Private constructor of the GUIPropertyManager. Singleton. Do not allow any instance of this class.
    /// </summary>
    public LogImpl()
    {
      _previousDate = DateTime.Now.Date;
      logDir = Config.GetFolder(Config.Dir.Log);
      if (!Directory.Exists(logDir))
      {
        Directory.CreateDirectory(logDir);
      }
      //BackupLogFiles();
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _minLevel =
          (Level) Enum.Parse(typeof (Level), xmlreader.GetValueAsString("general", "loglevel", "3"));
      }
      bConfiguration = false;
    }

    #endregion

    #region Private Methods

    private void Initialize(LogType type)
    {
      try
      {
        string name = GetFileName(type);
        string bakFile = name.Replace(".log", ".bak");
        if (File.Exists(bakFile))
        {
          File.Delete(bakFile);
        }
        if (File.Exists(name))
        {
          File.Move(name, bakFile);
        }
      }
      catch (Exception ex)
      {
        Write(ex);
      }
    }

    private string GetFileName(LogType type)
    {
      string fname = "MediaPortal.log";
      if (bConfiguration)
      {
        fname = "Configuration.log";
      }
      else
      {
        switch (type)
        {
          case LogType.Recorder:
            fname = "recorder.log";
            break;
          case LogType.Error:
            fname = "error.log";
            break;
          case LogType.EPG:
            fname = "epg.log";
            break;
          case LogType.VMR9:
            fname = "vmr9.log";
            break;
          case LogType.MusicShareWatcher:
            fname = "MusicshareWatcher.log";
            break;
          case LogType.WebEPG:
            fname = "webEPG.log";
            break;
        }
      }
      return Config.GetFile(Config.Dir.Log, fname);
    }

    private string GetLevelName(Level logLevel)
    {
      switch (logLevel)
      {
        case Level.Error:
          return "ERROR";

        case Level.Warning:
          return "Warn.";

        case Level.Information:
          return "Info.";

        case Level.Debug:
          return "Debug";
      }

      return "Unknown";
    }

    #endregion

    #region ILog Implementations

    public void BackupLogFiles()
    {
      BackupLogFile(LogType.Log);
      BackupLogFile(LogType.Recorder);
      BackupLogFile(LogType.Error);
      BackupLogFile(LogType.EPG);
      BackupLogFile(LogType.VMR9);
      BackupLogFile(LogType.WebEPG);
    }

    public void BackupLogFile(LogType logType)
    {
      Initialize(logType);
    }

    public void Write(Exception ex)
    {
      WriteFile(LogType.Log, true, "Exception   :{0}", ex.ToString());
      WriteFile(LogType.Log, true, "Exception   :{0}", ex.Message);
      WriteFile(LogType.Log, true, "  site      :{0}", ex.TargetSite);
      WriteFile(LogType.Log, true, "  source    :{0}", ex.Source);
      WriteFile(LogType.Log, true, "  stacktrace:{0}", ex.StackTrace);
    }

    /// <summary>
    /// Write a string to the logfile.
    /// </summary>
    /// <param name="format">The format of the string.</param>
    /// <param name="arg">An array containing the actual data of the string.</param>
    public void Write(string format, params object[] arg)
    {
      // uncomment the following four lines to help identify the calling method, this
      // is useful in situations where an unreported exception causes problems
      //		StackTrace stackTrace = new StackTrace();
      //		StackFrame stackFrame = stackTrace.GetFrame(1);
      //		MethodBase methodBase = stackFrame.GetMethod();
      //		WriteFile(LogType.Log, "{0}", methodBase.Name);

      WriteFile(LogType.Log, format, arg);
    }

    public void Info(string format, params object[] arg)
    {
      WriteFile(LogType.Log, Level.Information, format, arg);
    }

    public void Info(LogType type, string format, params object[] arg)
    {
      WriteFile(type, Level.Information, format, arg);
    }

    public void Warn(string format, params object[] arg)
    {
      WriteFile(LogType.Log, Level.Warning, format, arg);
    }

    public void Warn(LogType type, string format, params object[] arg)
    {
      WriteFile(type, Level.Warning, format, arg);
    }

    public void Debug(string format, params object[] arg)
    {
      WriteFile(LogType.Log, Level.Debug, format, arg);
    }

    public void Debug(LogType type, string format, params object[] arg)
    {
      WriteFile(type, Level.Debug, format, arg);
    }

    public void Error(string format, params object[] arg)
    {
      WriteFile(LogType.Log, true, format, arg);
    }

    public void Error(LogType type, string format, params object[] arg)
    {
      WriteFile(type, true, format, arg);
    }

    public void Error(Exception ex)
    {
      Write(ex);
    }

    /// <summary>
    /// Write a string to the logfile.
    /// </summary>
    /// <param name="format">The format of the string.</param>
    /// <param name="arg">An array containing the actual data of the string.</param>
    public void WriteThreadId(string format, params object[] arg)
    {
      // uncomment the following four lines to help identify the calling method, this
      // is useful in situations where an unreported exception causes problems
      //		StackTrace stackTrace = new StackTrace();
      //		StackFrame stackFrame = stackTrace.GetFrame(1);
      //		MethodBase methodBase = stackFrame.GetMethod();
      //		WriteFile(LogType.Log, "{0}", methodBase.Name);
      String log = String.Format("{0:X} {1}",
                                 Thread.CurrentThread.ManagedThreadId, String.Format(format, arg));
      WriteFile(LogType.Log, log);
    }

    public void WriteThreadId(LogType type, string format, params object[] arg)
    {
      String log = String.Format("{0:X} {1}",
                                 Thread.CurrentThread.ManagedThreadId, String.Format(format, arg));
      WriteFile(type, log);
    }

    public void WriteFileThreadId(LogType type, bool isError, string format, params object[] arg)
    {
      WriteThreadId(type, format, arg);
      if (isError)
      {
        WriteThreadId(LogType.Error, format, arg);
      }
    }

    public void InfoThread(string format, params object[] arg)
    {
      WriteThreadId(format, arg);
    }

    public void WarnThread(string format, params object[] arg)
    {
      WriteThreadId(format, arg);
    }

    public void ErrorThread(string format, params object[] arg)
    {
      WriteThreadId(format, arg);
    }

    public void SetConfigurationMode()
    {
      bConfiguration = true;
      // Always use Debug Level in Configuration
      _minLevel = Level.Debug;
    }

    public void WriteFile(LogType type, bool isError, string format, params object[] arg)
    {
      WriteFile(type, format, arg);
      if (isError)
      {
        WriteFile(LogType.Error, Level.Error, format, arg);
      }
    }

    public void WriteFile(LogType type, string format, params object[] arg)
    {
      WriteFile(type, Level.Information, format, arg);
    }

    public void WriteFile(LogType type, Level logLevel, string format, params object[] arg)
    {
      lock (typeof (Log))
      {
        try
        {
          if (_previousDate != DateTime.Now.Date)
          {
            _previousDate = DateTime.Now.Date;
            BackupLogFiles();
          }

          if (logLevel <= _minLevel)
          {
            using (StreamWriter writer = new StreamWriter(GetFileName(type), true))
            {
              string thread = Thread.CurrentThread.Name;
              if (thread == null)
              {
                thread = Thread.CurrentThread.ManagedThreadId.ToString();
              }
              // Write message to log stream
              writer.WriteLine("{0:yyyy-MM-dd HH:mm:ss.ffffff} [{1}][{2}]: {3}", DateTime.Now, GetLevelName(logLevel),
                               thread, string.Format(format, arg));
              writer.Close();
            }
          }
        }
        catch (Exception)
        {
        }
      }

      //
      if (type != LogType.Log && type != LogType.Error && type != LogType.EPG &&
          type != LogType.MusicShareWatcher && type != LogType.WebEPG)
      {
        WriteFile(LogType.Log, format, arg);
      }
    } //public static void WriteFile(LogType type, string format, params object[] arg)

    public void SetLogLevel(Level logLevel)
    {
      _minLevel = logLevel;
    }

    #endregion
  }
}