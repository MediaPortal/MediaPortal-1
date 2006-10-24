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
      Error
    }
    static DateTime _previousDate;
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
      //System.IO.Directory.CreateDirectory("log");
      BackupLogFiles();
    }
    /// <summary>
    /// Backups the log files.
    /// </summary>
    public static void BackupLogFiles()
    {
      BackupLogFile();
    }

    /// <summary>
    /// Backups the log file.
    /// </summary>
    public static void BackupLogFile()
    {
      Initialize();
    }

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    static void Initialize()
    {
      try
      {
        string name = GetFileName(LogType.Info);
        string bakFile = name.Replace(".log", ".bak");
        if (File.Exists(bakFile))
          File.Delete(bakFile);
        if (File.Exists(name))
          File.Move(name, bakFile);

        name = GetFileName(LogType.Error);
        bakFile = name.Replace(".log", ".bak");
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

    /// <summary>
    /// Writes the specified exception to the log file
    /// </summary>
    /// <param name="ex">The ex.</param>
    static public void Write(Exception ex)
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
    static public void Write(string format, params object[] arg)
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
      WriteToFile(LogType.Info, log);
    }
    static string GetFileName(LogType logType)
    {
      switch (logType)
      {
        case LogType.Debug:
          return String.Format(@"{0}\MediaPortal TV Server\log\tv.log", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));

        case LogType.Info:
          return String.Format(@"{0}\MediaPortal TV Server\log\tv.log", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));

        case LogType.Error:
          return String.Format(@"{0}\MediaPortal TV Server\log\error.log", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));

        default:
          return String.Format(@"{0}\MediaPortal TV Server\log\tv.log", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));


      }
    }

    /// <summary>
    /// Logs the message to the error file
    /// </summary>
    /// <param name="format">The format.</param>
    /// <param name="arg">The arg.</param>
    static public void Error(string format, params object[] arg)
    {
      WriteToFile(LogType.Error, format, arg);
    }
    /// <summary>
    /// Logs the message to the info file
    /// </summary>
    /// <param name="format">The format.</param>
    /// <param name="arg">The arg.</param>
    static public void Info(string format, params object[] arg)
    {
      WriteToFile(LogType.Info, format, arg);
    }
    /// <summary>
    /// Logs the message to the debug file
    /// </summary>
    /// <param name="format">The format.</param>
    /// <param name="arg">The arg.</param>
    static public void Debug(string format, params object[] arg)
    {
      WriteToFile(LogType.Debug, format, arg);
    }
    /// <summary>
    /// Logs the message to the info file
    /// </summary>
    /// <param name="format">The format.</param>
    /// <param name="arg">The arg.</param>
    static public void WriteFile(string format, params object[] arg)
    {
      WriteToFile(LogType.Info, format, arg);
    }

    /// <summary>
    /// Writes the file.
    /// </summary>
    /// <param name="logType">the type of logging.</param>
    /// <param name="format">The format.</param>
    /// <param name="arg">The arg.</param>
    static void WriteToFile(LogType logType, string format, params object[] arg)
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

          using (StreamWriter writer = new StreamWriter(GetFileName(logType), true))
          {
            writer.BaseStream.Seek(0, SeekOrigin.End); // set the file pointer to the end of 
            writer.Write(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " ");
            writer.WriteLine(format, arg);
            writer.Close();
          }
        }
        catch (Exception)
        {
        }
      }
      //
    }//static public void WriteFile(string format, params object[] arg)
  }
}
