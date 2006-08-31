/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using MediaPortal.Util;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// An implementation of a log mechanism for the GUI library.
  /// </summary>
  public class Log
  {
    static DateTime _previousDate;
    static Level _minLevel;
    static string logDir;
    static bool bConfiguration;   // when Configuartion.exe is running the logging should take place in Configuration.log

    public enum LogType
    {
      Log,
      Recorder,
      Error,
      EPG,
      VMR9,
      Config,
      MusicShareWatcher
    }

    public enum Level
    {
      Error = 0,
      Warning = 1,
      Information = 2,
      Debug = 3
    }

    /// <summary>
    /// Private constructor of the GUIPropertyManager. Singleton. Do not allow any instance of this class.
    /// </summary>
    private Log()
    {
    }

    /// <summary>
    /// Deletes the logfile.
    /// </summary>
    static Log()
    {
      _previousDate = DateTime.Now.Date;
      logDir = Config.Get(Config.Dir.Log);
      System.IO.Directory.CreateDirectory(logDir);
      //BackupLogFiles();
      using (MediaPortal.Profile.Settings xmlReader = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
      {
        _minLevel = (Level)Enum.Parse(typeof(Level), xmlReader.GetValueAsString("general", "loglevel", "3"));
      }
      bConfiguration = false; 
    }

    public static void BackupLogFiles()
    {
      BackupLogFile(LogType.Log);
      BackupLogFile(LogType.Recorder);
      BackupLogFile(LogType.Error);
      BackupLogFile(LogType.EPG);
      BackupLogFile(LogType.VMR9);
    }

    public static void BackupLogFile(LogType logType)
    {
      Initialize(logType);
    }

    static void Initialize(LogType type)
    {
      try
      {
        string name = GetFileName(type);
        string bakFile = name.Replace(".log", ".bak");
        if (File.Exists(bakFile))
          File.Delete(bakFile);
        if (File.Exists(name))
          File.Move(name, bakFile);
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
    }

    static public void Write(Exception ex)
    {
      Log.WriteFile(Log.LogType.Log, true, "Exception   :{0}", ex.ToString());
      Log.WriteFile(Log.LogType.Log, true, "Exception   :{0}", ex.Message);
      Log.WriteFile(Log.LogType.Log, true, "  site      :{0}", ex.TargetSite);
      Log.WriteFile(Log.LogType.Log, true, "  source    :{0}", ex.Source);
      Log.WriteFile(Log.LogType.Log, true, "  stacktrace:{0}", ex.StackTrace);
    }

    /// <summary>
    /// Write a string to the logfile.
    /// </summary>
    /// <param name="format">The format of the string.</param>
    /// <param name="arg">An array containing the actual data of the string.</param>
    static public void Write(string format, params object[] arg)
    {
      // uncomment the following four lines to help identify the calling method, this
      // is useful in situations where an unreported exception causes problems
      //		StackTrace stackTrace = new StackTrace();
      //		StackFrame stackFrame = stackTrace.GetFrame(1);
      //		MethodBase methodBase = stackFrame.GetMethod();
      //		WriteFile(LogType.Log, "{0}", methodBase.Name);

      WriteFile(LogType.Log, format, arg);
    }

    static public void Info(string format, params object[] arg)
    {
      WriteFile(LogType.Log, Level.Information, format, arg);
    }

    static public void Info(LogType type, string format, params object[] arg)
    {
      WriteFile(type, Level.Information, format, arg);
    }

    static public void Warn(string format, params object[] arg)
    {
      WriteFile(LogType.Log, Level.Warning, format, arg);
    }

    static public void Warn(LogType type, string format, params object[] arg)
    {
      WriteFile(type, Level.Warning, format, arg);
    }

    static public void Debug(string format, params object[] arg)
    {
      WriteFile(LogType.Log, Level.Debug, format, arg);
    }

    static public void Debug(LogType type, string format, params object[] arg)
    {
      WriteFile(type, Level.Debug, format, arg);
    }

    static public void Error(string format, params object[] arg)
    {
      WriteFile(LogType.Log, true, format, arg);
    }

    static public void Error(LogType type, string format, params object[] arg)
    {
      WriteFile(type, true, format, arg);
    }

    static public void Error(Exception ex)
    {
      Write(ex);
    }

    /// <summary>
    /// Write a string to the logfile.
    /// </summary>
    /// <param name="format">The format of the string.</param>
    /// <param name="arg">An array containing the actual data of the string.</param>
    static public void WriteThreadId(string format, params object[] arg)
    {
      // uncomment the following four lines to help identify the calling method, this
      // is useful in situations where an unreported exception causes problems
      //		StackTrace stackTrace = new StackTrace();
      //		StackFrame stackFrame = stackTrace.GetFrame(1);
      //		MethodBase methodBase = stackFrame.GetMethod();
      //		WriteFile(LogType.Log, "{0}", methodBase.Name);
      String log = String.Format("{0:X} {1}", 
          System.Threading.Thread.CurrentThread.ManagedThreadId, String.Format(format, arg));
      WriteFile(LogType.Log, log);
    }

    static public void WriteThreadId(LogType type, string format, params object[] arg)
    {
      String log = String.Format("{0:X} {1}",
          System.Threading.Thread.CurrentThread.ManagedThreadId, String.Format(format, arg));
      WriteFile(type, log);
    }

    static public void WriteFileThreadId(LogType type, bool isError, string format, params object[] arg)
    {
      WriteThreadId(type, format, arg);
      if (isError)
        WriteThreadId(LogType.Error, format, arg);
    }

    static public void InfoThread(string format, params object[] arg)
    {
      WriteThreadId(format, arg);
    }

    static public void WarnThread(string format, params object[] arg)
    {
      WriteThreadId(format, arg);
    }

    static public void ErrorThread(string format, params object[] arg)
    {
      WriteThreadId(format, arg);
    }

    static string GetFileName(LogType type)
    {
      string fname = logDir + "MediaPortal.log";
      if (bConfiguration)
      {
        fname = logDir + "Configuration.log";
        return fname;
      }
      switch (type)
      {
        case LogType.Recorder:
          fname = logDir + "recorder.log";
          break;
        case LogType.Error:
          fname = logDir + "error.log";
          break;
        case LogType.EPG:
          fname = logDir + "epg.log";
          break;
        case LogType.VMR9:
          fname = logDir + "vmr9.log";
          break;
        case LogType.MusicShareWatcher:
          fname = logDir + "MusicshareWatcher.log";
          break;
      }
      return fname;
    }
    
    static string GetLevelName(Level logLevel)
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

    static public void SetConfigurationMode()
    {
      bConfiguration = true;
      // Always use Debug Level in Configuration
      _minLevel = Level.Debug;
    }

    static public void WriteFile(LogType type, bool isError, string format, params object[] arg)
    {
      WriteFile(type, format, arg);
      if (isError)
        WriteFile(LogType.Error, Level.Error, format, arg);
    }

    static public void WriteFile(LogType type, string format, params object[] arg)
    {
      WriteFile(type, Level.Information, format, arg);
    }

    static public void WriteFile(LogType type, Level logLevel, string format, params object[] arg)
    {
      lock (typeof(Log))
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
              writer.BaseStream.Seek(0, SeekOrigin.End); // set the file pointer to the end of 
              writer.Write(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " " + String.Format("{0:000}", DateTime.Now.Millisecond));
              writer.Write(" [" + GetLevelName(logLevel) + "] ");
              writer.WriteLine(format, arg);
              writer.Close();
            }
          }
        }
        catch (Exception)
        {
        }
      }

      //
      if (type != LogType.Log && type != LogType.Error && type != LogType.EPG && type != LogType.MusicShareWatcher)
        WriteFile(LogType.Log, format, arg);
    }//static public void WriteFile(LogType type, string format, params object[] arg)
  }
}
