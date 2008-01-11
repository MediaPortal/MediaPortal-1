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
using System.IO;
using System.Threading;
using MediaPortal.Services;

namespace MediaPortal.EPG.WebEPGTester
{
  public class TestLog : ILog
  {
    #region Variables
    private Level _minLevel;
    private TextWriter _logStream;
    #endregion

    #region Constructors/Destructors
    public TestLog(TextWriter stream, Level minLevel)
    {
      _minLevel = minLevel;
      _logStream = stream;
    }

    public TestLog(string name, Level minLevel)
    {
      _minLevel = minLevel;
      LogFile file = new LogFile(name);
      _logStream = file.GetStream();
    }
    #endregion

    public void Dispose()
    {
      if (_logStream == null)
      {
        return;
      }
      _logStream.Close();
      _logStream.Dispose();
      _logStream = null;
    }

    #region Private Methods
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

    private void Write(Level logLevel, string format, params object[] arg)
    {
      if (logLevel <= _minLevel)
      {
        string thread = Thread.CurrentThread.Name;
        if (thread == null)
          thread = Thread.CurrentThread.ManagedThreadId.ToString();
        // Write message to log stream
        _logStream.WriteLine("{0:yyyy-MM-dd HH:mm:ss.fffffff} [{1}][{2}]: {3}", DateTime.Now, GetLevelName(logLevel), thread, string.Format(format, arg));
      }
    }
    #endregion

    #region ILog Implementations
    public void SetLogLevel(MediaPortal.Services.Level level)
    { }

    public void BackupLogFiles()
    { }

    public void BackupLogFile(LogType logType)
    { }

    public void Info(LogType type, string format, params object[] arg)
    {
      Info(format, arg);
    }
    public void Info(string format, params object[] arg)
    {
      Write(Level.Information, format, arg);
    }

    public void Warn(LogType type, string format, params object[] arg)
    {
      Warn(format, arg);
    }
    public void Warn(string format, params object[] arg)
    {
      Write(Level.Warning, format, arg);
    }

    public void Error(LogType type, string format, params object[] arg)
    {
      Error(format, arg);
    }
    public void Error(string format, params object[] arg)
    {
      Write(Level.Error, format, arg);
    }

    public void Debug(LogType type, string format, params object[] arg)
    {
      Debug(format, arg);
    }

    public void Debug(string format, params object[] arg)
    {
      Write(Level.Debug, format, arg);
    }

    public void Error(Exception ex)
    {
      Write(Level.Error, "Exception   :{0}", ex.ToString());
      Write(Level.Error, "Exception   :{0}", ex.Message);
      Write(Level.Error, "  site      :{0}", ex.TargetSite);
      Write(Level.Error, "  source    :{0}", ex.Source);
      Write(Level.Error, "  stacktrace:{0}", ex.StackTrace);
    }

    public void Write(Exception ex)
    {
      Error(ex);
    }


    public void SetConfigurationMode()
    { }

    public void Write(string format, params object[] arg)
    { }
    public void WriteFile(LogType type, bool isError, string format, params object[] arg)
    { }
    public void WriteFile(LogType type, string format, params object[] arg)
    { }
    public void WriteFile(LogType type, Level logLevel, string format, params object[] arg)
    { }
    #endregion
  }
}
